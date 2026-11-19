param(
    [Parameter(Mandatory)]
    [string]$Path
)

$ErrorActionPreference = 'Stop'

try {
    $file = Get-Item -LiteralPath $Path
    $report = [System.Xml.XmlDocument]::new()
    $settings = [System.Xml.XmlReaderSettings]::new()
    $settings.DtdProcessing = [System.Xml.DtdProcessing]::Prohibit
    $reader = [System.Xml.XmlReader]::Create($file.FullName, $settings)
    try {
        $report.Load($reader)
    } finally {
        $reader.Dispose()
    }

    $packages = $report.SelectNodes('/coverage/packages/package[@name="Canvas.Domain"]')
    if ($packages.Count -ne 1) {
        throw 'El informe debe contener exactamente un paquete Canvas.Domain.'
    }

    $lines = [System.Collections.Generic.HashSet[System.ValueTuple[string,long]]]::new()
    $coveredLines = [System.Collections.Generic.HashSet[System.ValueTuple[string,long]]]::new()
    foreach ($class in $packages[0].SelectNodes('classes/class')) {
        $filename = $class.GetAttribute('filename')
        foreach ($line in $class.SelectNodes('lines/line')) {
            $number = 0L
            $hits = 0L
            if ([string]::IsNullOrWhiteSpace($filename) -or
                -not [long]::TryParse($line.GetAttribute('number'), [ref]$number) -or $number -le 0 -or
                -not [long]::TryParse($line.GetAttribute('hits'), [ref]$hits) -or $hits -lt 0) {
                throw 'El informe contiene una línea con archivo, número o cantidad de ejecuciones inválidos.'
            }

            $key = [System.ValueTuple[string,long]]::new($filename, $number)
            [void]$lines.Add($key)
            if ($hits -gt 0) {
                [void]$coveredLines.Add($key)
            }
        }
    }

    $valid = [long]$lines.Count
    $covered = [long]$coveredLines.Count
    if ($valid -eq 0) {
        Write-Output 'Canvas.Domain: 0/0 líneas cubiertas, porcentaje indefinido.'
        throw 'Canvas.Domain no contiene líneas válidas.'
    }

    $percentage = 100 * $covered / $valid
    Write-Output ('Canvas.Domain: {0}/{1} líneas cubiertas ({2:0.##} %).' -f $covered, $valid, $percentage)
    if ($covered * 100 -lt $valid * 60) {
        throw 'La cobertura de líneas es inferior al 60 %.'
    }
} catch {
    [Console]::Error.WriteLine("Error de cobertura: $($_.Exception.Message)")
    exit 1
}

exit 0
