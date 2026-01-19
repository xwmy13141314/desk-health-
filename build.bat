@echo off
REM DeskHealth Build Script

echo ========================================
echo DeskHealth Build Script
echo ========================================
echo.

echo [1/4] Restoring NuGet packages...
dotnet restore
if %errorlevel% neq 0 (
    echo Error: Failed to restore packages
    exit /b %errorlevel%
)
echo OK - Packages restored
echo.

echo [2/4] Building solution...
dotnet build --configuration Release
if %errorlevel% neq 0 (
    echo Error: Failed to build solution
    exit /b %errorlevel%
)
echo OK - Solution built
echo.

echo [3/4] Publishing single file...
dotnet publish src/DeskHealth.App -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
if %errorlevel% neq 0 (
    echo Error: Failed to publish
    exit /b %errorlevel%
)
echo OK - Published to: src\DeskHealth.App\bin\Release\net8.0-windows\win-x64\publish\
echo.

echo [4/4] Build completed successfully!
echo.
echo Output location:
echo src\DeskHealth.App\bin\Release\net8.0-windows\win-x64\publish\DeskHealth.exe
echo.

pause
