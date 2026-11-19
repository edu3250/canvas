$ErrorActionPreference = 'Stop'
$dll = Join-Path $PSScriptRoot '../src/Canvas.Console/bin/Release/net10.0/Canvas.Console.dll'
if (-not (Test-Path -LiteralPath $dll -PathType Leaf)) {
    throw 'Falta compilar Canvas.Console en Release.'
}

function Test-ConsoleCase {
    param(
        [string]$Name,
        [string]$Arguments,
        [int]$ExpectedCode,
        [string[]]$ExpectedOutput = @(),
        [string[]]$ExpectedError = @()
    )

    $startInfo = [Diagnostics.ProcessStartInfo]::new('dotnet', ('"{0}" {1}' -f $dll, $Arguments))
    $startInfo.UseShellExecute = $false
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    $startInfo.StandardOutputEncoding = [Text.Encoding]::UTF8
    $startInfo.StandardErrorEncoding = [Text.Encoding]::UTF8

    $process = [Diagnostics.Process]::Start($startInfo)
    try {
        $outputTask = $process.StandardOutput.ReadToEndAsync()
        $errorTask = $process.StandardError.ReadToEndAsync()
        $process.WaitForExit()
        $code = $process.ExitCode
        $output = $outputTask.GetAwaiter().GetResult()
        $errorOutput = $errorTask.GetAwaiter().GetResult()
    } finally {
        $process.Dispose()
    }

    $newline = [Environment]::NewLine
    $expectedStdout = (@($ExpectedOutput) + '') -join $newline
    $expectedStderr = (@($ExpectedError) + '') -join $newline
    Write-Output "Caso ${Name} · código $code"
    Write-Output 'stdout:'
    Write-Output $output
    Write-Output 'stderr:'
    Write-Output $errorOutput
    if ($code -ne $ExpectedCode -or
        -not [string]::Equals($output, $expectedStdout, [StringComparison]::Ordinal) -or
        -not [string]::Equals($errorOutput, $expectedStderr, [StringComparison]::Ordinal)) {
        throw "El caso '$Name' difiere del contrato. Código esperado: $ExpectedCode."
    }
}

$programming = 'Espacio: Programación | Cámara: (120; 80; 0,75) | Retorno: vacío'
$games = 'Espacio: Juegos | Cámara: (0; 0; 1) | Retorno: vacío'

Test-ConsoleCase -Name 'Recorrido automático' -ExpectedCode 0 -ExpectedOutput @(
    $programming
    'Espacio: Programación | Cámara: (0; 0; 1) | Retorno: (120; 80; 0,75)'
    $programming
    $games
    'Espacio: Juegos | Cámara: (60; 40; 1) | Retorno: vacío'
    $programming
)
Test-ConsoleCase -Name 'Selección de Juegos' -Arguments '--espacio Juegos' -ExpectedCode 0 -ExpectedOutput $games
Test-ConsoleCase -Name 'Selección de Programación' -Arguments '--espacio Programación' -ExpectedCode 0 -ExpectedOutput $programming
Test-ConsoleCase -Name 'Espacio desconocido' -Arguments '--espacio Desconocido' -ExpectedCode 1 -ExpectedError 'Espacio desconocido: Desconocido'
Test-ConsoleCase -Name 'Sintaxis inválida' -Arguments '--espacio' -ExpectedCode 2 -ExpectedError 'Uso: Canvas.Console [--espacio NOMBRE]'
