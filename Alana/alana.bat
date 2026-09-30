@echo off
setlocal

set "ALANA_HOME=%~dp0source"

if "%~1"=="" (
    echo Usage: alana ^<file.alana^>
    exit /b 1
)

if not exist "%~1" (
    echo Alana error: file '%~1' not found.
    exit /b 1
)

if not exist "%ALANA_HOME%\Alana.exe" (
    echo Alana error: runtime not found in "%ALANA_HOME%".
    exit /b 1
)

pushd "%~dp1"
"%ALANA_HOME%\Alana.exe" "%~f1"
set "CODE=%ERRORLEVEL%"
popd

exit /b %CODE%
