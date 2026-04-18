@echo off
REM =======================================================
REM PARÇA ALIM SİSTEMİ - KURULUM SCRIPTI (Windows Batch)
REM =======================================================
REM Bu dosyayı çalıştırmadan önce:
REM 1. SQL Server kurulu olmalı
REM 2. Server adını değiştirin (localhost yerine)
REM 3. Veritabanı adını değiştirin
REM =======================================================

setlocal enabledelayedexpansion

echo.
echo =========================================
echo PARÇA ALIM SİSTEMİ KURULUMU
echo =========================================
echo.

REM Veritabanı parametreleri
set SERVER=localhost
set DATABASE=YourDatabaseName
set SCRIPT_FILE=%~dp0KURULUM_COMPLETE.sql

echo Sunucu: %SERVER%
echo Veritabanı: %DATABASE%
echo Script: %SCRIPT_FILE%
echo.

REM sqlcmd ile script'i çalıştır
echo [!] Script çalıştırılıyor...
sqlcmd -S %SERVER% -d %DATABASE% -i "%SCRIPT_FILE%" -U sa -P your_password

if %ERRORLEVEL% EQU 0 (
    echo.
    echo =========================================
    echo ✅ KURULUM BAŞARILI!
    echo =========================================
    echo.
    echo Tablolar başarıyla oluşturuldu.
    echo Uygulamayı yeniden başlatın.
    echo.
) else (
    echo.
    echo =========================================
    echo ❌ KURULUM BAŞARISIZ!
    echo =========================================
    echo.
    echo Hata: %ERRORLEVEL%
    echo Kontrol edin:
    echo - SQL Server açık mı?
    echo - Sunucu adı doğru mu? (%SERVER%)
    echo - Veritabanı adı doğru mu? (%DATABASE%)
    echo - Şifre doğru mu?
    echo.
)

pause
