@echo off

REM Build the full path to the link target.
set "LINK_TARGET=%~dp0Packages\com.nueruyu.descrio\Samples~"

REM Build the full path to where the link will be created.
set "LINK_LOCATION=%~dp0Assets\Descrio\Samples"

REM If a link already exists, remove it.
if exist "%LINK_LOCATION%" (
    echo Existing link found. Removing it.
    rmdir "%LINK_LOCATION%"
)

REM Create the symbolic link.
echo Creating symbolic link:
echo  - From: %LINK_LOCATION%
echo  - To:   %LINK_TARGET%
mklink /D "%LINK_LOCATION%" "%LINK_TARGET%"

echo.
echo Done.
pause