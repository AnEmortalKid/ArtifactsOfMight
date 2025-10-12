@echo off
setlocal

:: Where visual studio places things
set SOURCE_DIR=%~1
:: How things are named
set PROJECT_NAME=%~2

:: Wherever you want your plugin to live
set DEST_DIR=%APPDATA%\r2modmanPlus-local\RiskOfRain2\profiles\ModTest\BepInEx\plugins\%PROJECT_NAME%

echo Deploying %PROJECT_NAME% from "%SOURCE_DIR%" to "%DEST_DIR%"
if not exist "%DEST_DIR%" (
    mkdir "%DEST_DIR%"
)

xcopy /Y /R "%SOURCE_DIR%%PROJECT_NAME%.dll" "%DEST_DIR%\"
xcopy /Y /R "%SOURCE_DIR%%PROJECT_NAME%.pdb" "%DEST_DIR%\"

echo Done.
endlocal
