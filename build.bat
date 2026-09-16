@echo off
REM One-click-ish build for Curiosity.Plugin.
REM What this does automatically: finds your AutoCAD install, points the build at it, builds the DLL.
REM What it can't do automatically: install the build tool itself (msbuild) — that's the one manual
REM step. See docs/PLUGIN_SETUP.md "Prerequisites" if this script says msbuild wasn't found.

setlocal enabledelayedexpansion

echo Curiosity.Plugin build helper
echo ==============================
echo.

REM --- Step 1: find AutoCAD install dir (look for AcMgd.dll under common install roots) ---
set "FOUND_DIR="
for %%R in ("%ProgramFiles%\Autodesk" "%ProgramFiles(x86)%\Autodesk") do (
    if exist "%%~R" (
        for /d %%D in ("%%~R\AutoCAD*") do (
            if exist "%%~D\AcMgd.dll" (
                set "FOUND_DIR=%%~D"
            )
        )
    )
)

if defined AUTOCAD_INSTALL_DIR (
    echo Using AUTOCAD_INSTALL_DIR already set in your environment: %AUTOCAD_INSTALL_DIR%
) else if defined FOUND_DIR (
    echo Found AutoCAD install automatically: !FOUND_DIR!
    set "AUTOCAD_INSTALL_DIR=!FOUND_DIR!"
) else (
    echo Could not auto-find your AutoCAD install.
    echo Please enter the full path to your AutoCAD folder ^(the one containing AcMgd.dll^),
    echo for example: C:\Program Files\Autodesk\AutoCAD 2026
    set /p AUTOCAD_INSTALL_DIR="AutoCAD folder: "
)

if not exist "%AUTOCAD_INSTALL_DIR%\AcMgd.dll" (
    echo.
    echo ERROR: AcMgd.dll not found in "%AUTOCAD_INSTALL_DIR%".
    echo That means AutoCAD isn't actually installed there, or this isn't the right folder.
    echo Fix: find the real folder yourself in File Explorer ^(search for AcMgd.dll^) and re-run this script.
    pause
    exit /b 1
)

echo.
echo Using AutoCAD install: %AUTOCAD_INSTALL_DIR%
echo.

REM --- Step 2: find msbuild ---
where msbuild >nul 2>nul
if %errorlevel% neq 0 (
    echo ERROR: msbuild was not found on this machine.
    echo.
    echo You need Visual Studio Build Tools installed once first ^(free, smaller than full Visual Studio^):
    echo   https://visualstudio.microsoft.com/downloads/  -^> scroll to "Tools for Visual Studio"
    echo   -^> "Build Tools for Visual Studio" -^> during install, check ".NET desktop build tools"
    echo After installing that, close this window and double-click build.bat again.
    pause
    exit /b 1
)

REM --- Step 3: build ---
echo Building...
echo.
msbuild "%~dp0src\Curiosity.Plugin\Curiosity.Plugin.csproj" /p:Configuration=Debug /nologo /v:minimal

if %errorlevel% neq 0 (
    echo.
    echo BUILD FAILED. Copy the red/error text above and send it back for a fix.
    pause
    exit /b 1
)

echo.
echo BUILD SUCCEEDED.
echo Your DLL is at: %~dp0src\Curiosity.Plugin\bin\Debug\net48\Curiosity.Plugin.dll
echo.
echo Next: open AutoCAD, run the NETLOAD command, browse to that file, then run the CURIOSITY command.
pause
