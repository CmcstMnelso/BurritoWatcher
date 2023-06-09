@echo off

REM Check for adb
WHERE adb >nul 2>&1
if %errorlevel% neq 0 (
    echo adb not found. Please install Android SDK Platform-tools and set them to your PATH.
    exit /b 1
)

REM Check if two parameters are provided
if "%~2"=="" (
    echo Usage: sendSMS.bat [phone_number] [message]
    exit /b 1
)

set phone_number=%1
set message=%2

echo sending %message% to %phone_number%

REM Start adb server
REM adb start-server

REM Sending the SMS
adb shell "service call isms 5 i32 1 s16 \"com.android.mms\" s16 \"null\" s16 \"%phone_number%\" s16 "null" s16 \"%message%\" s16 \"null\" s16 \"null\" i32 0 i64 0"

REM Stopping adb server
REM adb kill-server

echo SMS sent successfully!