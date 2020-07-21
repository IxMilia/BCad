@echo off

set version=42.42.42

:parseargs
if "%1" == "" goto argsdone
if /i "%1" == "--version" goto set_version

echo Unsupported argument: %1
goto error

:set_version
set version=%2
shift
shift
goto parseargs

:argsdone

call npm i
if errorlevel 1 echo Error restoring npm packages && goto error

call npm version %version% --allow-same-version
if errorlevel 1 echo Error setting package version && goto error

call npm run compile
if errorlevel 1 echo Error building client && goto error

call npm version 42.42.42 --allow-same-version
if errorlevel 1 echo Error restoring package version && goto error

exit /b 0

:error
echo Build failed with errors.
exit /b 1
