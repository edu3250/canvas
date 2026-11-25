$ErrorActionPreference = 'Stop'
$chrome = @(
    "$env:ProgramFiles\Google\Chrome\Application\chrome.exe"
    "${env:ProgramFiles(x86)}\Google\Chrome\Application\chrome.exe"
    "$env:LOCALAPPDATA\Google\Chrome\Application\chrome.exe"
) | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
if (!$chrome) { throw 'No se encontró Chrome.' }

$tempRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
$name = 'canvas-zoom-' + [Guid]::NewGuid().ToString('N')
$profile = Join-Path $tempRoot $name
$page = [Uri]::new((Resolve-Path (Join-Path $PSScriptRoot 'index.html')).Path)
New-Item -ItemType Directory -Path $profile | Out-Null
try {
    $arguments = @("--user-data-dir=`"$profile`"", '--no-first-run', '--disable-sync', '--no-default-browser-check', "--app=`"$($page.AbsoluteUri)`"")
    Start-Process -FilePath $chrome -ArgumentList $arguments -Wait
} finally {
    $resolved = (Resolve-Path -LiteralPath $profile).Path
    if ($resolved -eq [IO.Path]::Combine($tempRoot, $name)) {
        Remove-Item -LiteralPath $resolved -Recurse -Force
    }
}
