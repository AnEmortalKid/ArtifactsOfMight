@echo off
setlocal

:: Where visual studio places things
set SOURCE_DIR=%~1
:: How things are named
set PROJECT_NAME=%~2

:: === Source Root (relative to this script) ===
set SOURCE_ROOT=%~dp0..

:: Wherever you want your plugin to live
set DEST_DIR=%APPDATA%\r2modmanPlus-local\RiskOfRain2\profiles\ModTest\BepInEx\plugins\%PROJECT_NAME%

:: === Local assetbundles folder  ===
set ASSET_BUNDLE_SRC=%SOURCE_ROOT%\assetbundles
set ASSET_BUNDLE_DEST=%DEST_DIR%\assetbundles

echo Deploying %PROJECT_NAME% from "%SOURCE_DIR%" to "%DEST_DIR%"
if not exist "%DEST_DIR%" (
    mkdir "%DEST_DIR%"
)

xcopy /Y /R "%SOURCE_DIR%%PROJECT_NAME%.dll" "%DEST_DIR%\"
xcopy /Y /R "%SOURCE_DIR%%PROJECT_NAME%.pdb" "%DEST_DIR%\"

:: === Copy assetbundles folder (if it exists) ===
if exist "%ASSET_BUNDLE_SRC%" (
    echo Copying assetbundles...
    xcopy /E /Y /R "%ASSET_BUNDLE_SRC%" "%ASSET_BUNDLE_DEST%\"
) else (
    echo No assetbundles folder found at "%ASSET_BUNDLE_SRC%"
)

echo Done.
endlocal
