@echo off
setlocal

set "ALANA_DLL="

cls

echo [36m========================================[0m
echo              [34mALANA CONSOLE[0m
echo [36m========================================[0m
echo Type "exit" to quit.
echo.

:console

set /p "command=[32mAlana[0m [31m|[0m %cd%> "

if /i "%command%"=="exit" goto exit

if /i "%command%"=="cd" (
    echo Current directory: %cd%
    goto console
)

if /i "%command:~0,3%"=="cd " (
    cd /d "%command:~3%"
    goto console
)

if /i "%command:~0,6%"=="alana " (
    if not exist "%command:~6%" (
        echo Alana error: File "%command:~6%" not found.
        goto console
    )

    dotnet "%ALANA_DLL%" "%command:~6%"
    goto console
)

echo Unknown command: %command%
goto console

:exit

endlocal