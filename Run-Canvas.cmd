@echo off
setlocal
pushd "%~dp0" || exit /b 1
dotnet run --project "src\Canvas.Desktop\Canvas.Desktop.csproj"
set "canvasExitCode=%errorlevel%"
popd
if not "%canvasExitCode%"=="0" pause
exit /b %canvasExitCode%
