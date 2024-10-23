@echo off
REM 
title Service Manager

REM
cd /d "%~dp0"

REM
set "project_relative_path=src/Common/Common.Persistence/Common.Persistence.csproj"
set "startup_project_relative_path=src/API/API.csproj"

REM
set "project_path=%~dp0%project_relative_path%"
set "startup_project_path=%~dp0%startup_project_relative_path%"

:MENU
cls
echo 1) Add migration
echo 2) Remove migration
echo 3) Run project
echo 4) Exit
set /p choice="Choose an option: "

if "%choice%"=="1" goto ADD_MIGRATION
if "%choice%"=="2" goto REMOVE_MIGRATION
if "%choice%"=="3" goto RUN_PROJECT
if "%choice%"=="4" exit /b 0
goto MENU

:ADD_MIGRATION
REM
for /f "tokens=2 delims==" %%I in ('"wmic os get localdatetime /value"') do set datetime=%%I
set timestamp=%datetime:~0,8%_%datetime:~8,6%

set "random_name=Migration%timestamp%"

REM
dotnet ef migrations add %random_name% -c HVODbContext -p "%project_path%" -s "%startup_project_path%"
set /p DUMMY=Hit ENTER to continue...
goto MENU

:REMOVE_MIGRATION
REM
dotnet ef migrations remove -c HVODbContext -p "%project_path%" -s "%startup_project_path%"
set /p DUMMY=Hit ENTER to continue...
goto MENU

:RUN_PROJECT
REM Ensure the script uses the directory where the .bat file is located
cd /d "%~dp0"

REM Define the relative paths to the projects
set "instance_title=HVO_BE"
set "api_project_path=src\API\API.csproj"
set "portal_path=src\Portal"

REM Construct the absolute paths
set "api_project_full_path=%~dp0%api_project_path%"
set "portal_full_path=%~dp0%portal_path%"

REM Close previous instances of cmd windows with the specific title
taskkill /F /IM cmd.exe /FI "WINDOWTITLE eq %instance_title%"

REM Check if the portal directory exists
if not exist "%portal_full_path%" (
    echo Error: The directory for yarn run dev does not exist at %portal_full_path%
    exit /b 1
)

REM Open a new Command Prompt window for the yarn run dev command
start "%instance_title%" cmd /k "cd /d %portal_full_path% && yarn run build"

REM Check if the .csproj file exists
if not exist "%api_project_full_path%" (
    echo Error: The project file for dotnet run does not exist at %api_project_full_path%
    exit /b 1
)

REM Open a new Command Prompt window for the dotnet run command
start "%instance_title%" cmd /k "cd /d %~dp0src\API && dotnet run --project %api_project_full_path%"

goto MENU
