@echo off
setlocal

set "ALANA_HOME=%~dp0"
if "%ALANA_HOME:~-1%"=="\" set "ALANA_HOME=%ALANA_HOME:~0,-1%"
set "ALANA_BIN=%ALANA_HOME%\source"

if not exist "%ALANA_BIN%\Alana.exe" (
    echo Alana error: runtime not found in "%ALANA_BIN%".
    exit /b 1
)

if "%~1"=="--version" (
    "%ALANA_BIN%\Alana.exe" --version
    exit /b %ERRORLEVEL%
)

if /I "%~1"=="start" (
    if "%~2"=="" (
        echo Usage: alana start ^<path^>
        exit /b 1
    )
    "%ALANA_BIN%\Alana.exe" start "%~2"
    exit /b %ERRORLEVEL%
)

if "%~1"=="" (
    echo Usage: alana ^<file.alana^>
    echo        alana start ^<path^>
    echo        alana --version
    exit /b 1
)

if not exist "%~1" (
    echo Alana error: file '%~1' not found.
    exit /b 1
)

pushd "%~dp1"
"%ALANA_BIN%\Alana.exe" "%~f1"
set "CODE=%ERRORLEVEL%"
popd

exit /b %CODE%
