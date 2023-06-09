@echo off

REM Check for adb
WHERE adb >nul 2>&1
if %errorlevel% neq 0 (
    echo adb not found. Please install Android SDK Platform-tools and set them to your PATH.
    exit /b 1
)

REM Check if parameter is provided
if "%~1"=="" (
    echo Usage: manageADB.bat [start|kill]
    exit /b 1
)

REM Handle the parameter
if /I "%~1"=="start" (
    REM Start adb server if not already running
    echo Starting adb server...
    adb start-server
    echo adb server started.
) else if /I "%~1"=="kill" (
    REM Kill adb server if running
    echo Killing adb server...
    adb kill-server
    echo adb server killed.
) else (
    echo Invalid parameter. Usage: manageADB.bat [start|kill]
)