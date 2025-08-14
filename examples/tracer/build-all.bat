@echo off
echo Building all Vampire Survivors diagnostic tools...
echo.

REM Check if VAMPIRE_SURVIVORS_PATH is set
if "%VAMPIRE_SURVIVORS_PATH%"=="" (
    echo ERROR: VAMPIRE_SURVIVORS_PATH environment variable is not set!
    echo.
    echo Please set it to your Vampire Survivors installation directory:
    echo.
    echo For Command Prompt ^(current session^):
    echo   set VAMPIRE_SURVIVORS_PATH=F:\vampire\VampireSurvivors
    echo.
    echo For PowerShell ^(current session^):
    echo   $env:VAMPIRE_SURVIVORS_PATH = "F:\vampire\VampireSurvivors"
    echo.
    echo To set permanently ^(requires new terminal after^):
    echo   setx VAMPIRE_SURVIVORS_PATH "F:\vampire\VampireSurvivors"
    echo.
    echo Note: If you just used setx, close and reopen your terminal for it to take effect.
    exit /b 1
)

echo Using game path: %VAMPIRE_SURVIVORS_PATH%
echo.

REM Build TracerMod
echo [1/3] Building TracerMod...
dotnet build TracerMod.csproj -c Release
if %ERRORLEVEL% NEQ 0 (
    echo Failed to build TracerMod!
    exit /b 1
)
echo TracerMod built successfully.
echo.

REM Build DLCInspector
echo [2/3] Building DLCInspector...
dotnet build DLCInspector.csproj -c Release
if %ERRORLEVEL% NEQ 0 (
    echo Failed to build DLCInspector!
    exit /b 1
)
echo DLCInspector built successfully.
echo.

REM Build AssetDumper
echo [3/3] Building AssetDumper...
dotnet build AssetDumper.csproj -c Release
if %ERRORLEVEL% NEQ 0 (
    echo Failed to build AssetDumper!
    exit /b 1
)
echo AssetDumper built successfully.
echo.

echo ========================================
echo All tools built successfully!
echo.
echo Output DLLs are in: bin\Release\net6.0\
echo.
echo To install, copy the DLLs to: %VAMPIRE_SURVIVORS_PATH%\Mods\
echo.
echo Would you like to copy them now? (Y/N)
set /p COPY_MODS=

if /i "%COPY_MODS%"=="Y" (
    echo.
    echo Copying mods to game directory...
    
    if not exist "%VAMPIRE_SURVIVORS_PATH%\Mods" (
        echo Creating Mods directory...
        mkdir "%VAMPIRE_SURVIVORS_PATH%\Mods"
    )
    
    copy /Y "bin\Release\net6.0\TracerMod.dll" "%VAMPIRE_SURVIVORS_PATH%\Mods\" >nul 2>&1
    if %ERRORLEVEL% EQU 0 echo - TracerMod.dll copied
    
    copy /Y "bin\Release\net6.0\DLCInspector.dll" "%VAMPIRE_SURVIVORS_PATH%\Mods\" >nul 2>&1
    if %ERRORLEVEL% EQU 0 echo - DLCInspector.dll copied
    
    copy /Y "bin\Release\net6.0\AssetDumper.dll" "%VAMPIRE_SURVIVORS_PATH%\Mods\" >nul 2>&1
    if %ERRORLEVEL% EQU 0 echo - AssetDumper.dll copied
    
    echo.
    echo Mods installed successfully!
)

echo.
echo Done!
pause