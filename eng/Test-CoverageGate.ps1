$ErrorActionPreference = 'Stop'
$gate = Join-Path $PSScriptRoot 'Check-Coverage.ps1'
$engine = (Get-Process -Id $PID).Path
$directory = Join-Path ([IO.Path]::GetTempPath()) ('coverage-gate-' + [guid]::NewGuid())
$report = Join-Path $directory 'coverage.xml'

function New-ClassXml {
    param([string]$Filename, [int[]]$Hits)

    $lines = for ($index = 0; $index -lt $Hits.Count; $index++) {
        '<line number="{0}" hits="{1}" />' -f ($index + 1), $Hits[$index]
    }
    '<class filename="{0}"><lines>{1}</lines></class>' -f $Filename, ($lines -join '')
}

function New-PackageXml {
    param([string]$Name, [string[]]$Classes)

    '<package name="{0}"><classes>{1}</classes></package>' -f $Name, ($Classes -join '')
}

function Test-Gate {
    param(
        [string]$Name,
        [int]$ExpectedCode,
        [string]$Xml,
        [string]$ExpectedSummary
    )

    if ($PSBoundParameters.ContainsKey('Xml')) {
        [IO.File]::WriteAllText($report, $Xml)
    }
    $ErrorActionPreference = 'Continue'
    $output = & $engine -NoProfile -File $gate -Path $report 2>&1
    $code = $LASTEXITCODE
    $summary = $output -join ' '
    if ($code -ne $ExpectedCode -or ($ExpectedSummary -and $summary -notlike "*$ExpectedSummary*")) {
        throw "$Name esperaba código $ExpectedCode y resumen '$ExpectedSummary'. Obtuvo código $code. $summary"
    }
    Write-Output ("Verificado {0} (código {1})." -f $Name, $code)
}

[void][IO.Directory]::CreateDirectory($directory)
try {
    Test-Gate -Name 'archivo ausente' -ExpectedCode 1
    Test-Gate -Name 'XML inválido' -ExpectedCode 1 -Xml '<coverage>'

    $domainName = 'Canvas.Domain'
    $otherPackage = New-PackageXml 'canvas.domain' (New-ClassXml 'Other.cs' @(1, 1, 1, 1, 1))
    $emptyPackage = New-PackageXml $domainName
    Test-Gate -Name 'paquete ausente' -ExpectedCode 1 -Xml "<coverage><packages>$otherPackage</packages></coverage>"
    Test-Gate -Name 'paquete ambiguo' -ExpectedCode 1 -Xml "<coverage><packages>$emptyPackage$emptyPackage</packages></coverage>"
    Test-Gate -Name 'cero líneas' -ExpectedCode 1 -Xml "<coverage><packages>$emptyPackage$otherPackage</packages></coverage>" -ExpectedSummary '0/0'

    $lowPackage = New-PackageXml $domainName (New-ClassXml 'Panel.cs' @(1, 1, 0, 0, 0))
    $limitPackage = New-PackageXml $domainName (New-ClassXml 'Panel.cs' @(1, 1, 1, 0, 0))
    $highPackage = New-PackageXml $domainName (New-ClassXml 'Panel.cs' @(1, 1, 1, 1, 0))
    Test-Gate -Name '2 de 5 con otro paquete cubierto' -ExpectedCode 1 -Xml "<coverage><packages>$lowPackage$otherPackage</packages></coverage>" -ExpectedSummary '2/5'
    Test-Gate -Name '3 de 5' -ExpectedCode 0 -Xml "<coverage><packages>$limitPackage</packages></coverage>" -ExpectedSummary '3/5'
    Test-Gate -Name '4 de 5' -ExpectedCode 0 -Xml "<coverage><packages>$highPackage</packages></coverage>" -ExpectedSummary '4/5'

    $duplicatePackage = New-PackageXml $domainName @(
        (New-ClassXml 'Panel.cs' @(1, 0, 0))
        (New-ClassXml 'Panel.cs' @(0, 1, 0))
        (New-ClassXml 'Space.cs' @(1, 0))
    )
    Test-Gate -Name 'duplicados por archivo y línea' -ExpectedCode 0 -Xml "<coverage><packages>$duplicatePackage</packages></coverage>" -ExpectedSummary '3/5'
} finally {
    [IO.File]::Delete($report)
    [IO.Directory]::Delete($directory)
}
