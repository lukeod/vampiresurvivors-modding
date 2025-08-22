@echo off
echo Building Signal Inspector Mod...
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

dotnet build SignalInspectorMod.csproj -c Release

if %ERRORLEVEL% NEQ 0 (
    echo Failed to build SignalInspectorMod!
    exit /b 1
)

echo SignalInspectorMod built successfully.
echo.

echo Output DLL is in: bin\Release\net6.0\
echo.
echo Would you like to copy it to the Mods folder? (Y/N)
set /p COPY_MOD=

if /i "%COPY_MOD%"=="Y" (
    echo.
    echo Copying mod to game directory...
    
    if not exist "%VAMPIRE_SURVIVORS_PATH%\Mods" (
        echo Creating Mods directory...
        mkdir "%VAMPIRE_SURVIVORS_PATH%\Mods"
    )
    
    copy /Y "bin\Release\net6.0\SignalInspectorMod.dll" "%VAMPIRE_SURVIVORS_PATH%\Mods\" >nul 2>&1
    if %ERRORLEVEL% EQU 0 (
        echo - SignalInspectorMod.dll copied
        echo.
        echo Mod installed successfully to: %VAMPIRE_SURVIVORS_PATH%\Mods\
    ) else (
        echo Failed to copy mod!
    )
)

echo.
echo Done!
pause