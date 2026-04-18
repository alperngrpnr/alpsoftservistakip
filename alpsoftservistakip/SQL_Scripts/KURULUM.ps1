# =======================================================
# PARÇA ALIM SİSTEMİ - KURULUM SCRIPTI (PowerShell)
# =======================================================
# Bu scripti PowerShell'de çalıştırın:
# 1. PowerShell'i Yönetici Olarak Açın
# 2. Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope CurrentUser
# 3. .\KURULUM.ps1
# =======================================================

param(
    [string]$Server = "localhost",
    [string]$Database = "YourDatabaseName",
    [string]$Username = "sa",
    [string]$Password = "your_password"
)

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "PARÇA ALIM SİSTEMİ KURULUMU" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Sunucu: $Server" -ForegroundColor Yellow
Write-Host "Veritabanı: $Database" -ForegroundColor Yellow
Write-Host "Kullanıcı: $Username" -ForegroundColor Yellow
Write-Host ""

# Script dosyasının yolunu al
$ScriptPath = Join-Path (Split-Path $MyInvocation.MyCommand.Path) "KURULUM_COMPLETE.sql"

if (-not (Test-Path $ScriptPath)) {
    Write-Host "❌ Hata: Script dosyası bulunamadı!" -ForegroundColor Red
    Write-Host "Beklenen yol: $ScriptPath" -ForegroundColor Red
    exit 1
}

try {
    Write-Host "[*] SQL Server'a bağlanılıyor..." -ForegroundColor Cyan

    # SQL Server bağlantısı
    $ConnectionString = "Server=$Server;Database=$Database;User Id=$Username;Password=$Password;"
    $Connection = New-Object System.Data.SqlClient.SqlConnection
    $Connection.ConnectionString = $ConnectionString
    $Connection.Open()

    Write-Host "✅ Bağlantı başarılı" -ForegroundColor Green

    # Script'i oku
    Write-Host "[*] Script okunuyor..." -ForegroundColor Cyan
    $SqlScript = Get-Content $ScriptPath -Raw

    # Script'i GO ile böl ve çalıştır
    $Batches = $SqlScript -split "GO\s*`r?`n"

    $BatchCount = 0
    foreach ($Batch in $Batches) {
        if ($Batch.Trim().Length -gt 0) {
            $BatchCount++
            Write-Host "[*] Batch $BatchCount çalıştırılıyor..." -ForegroundColor Cyan

            $Command = $Connection.CreateCommand()
            $Command.CommandText = $Batch
            $Command.ExecutionTimeout = 300

            try {
                $Result = $Command.ExecuteNonQuery()
                Write-Host "✅ Batch $BatchCount tamam" -ForegroundColor Green
            } catch {
                Write-Host "❌ Batch $BatchCount hata: $_" -ForegroundColor Red
                throw
            }
        }
    }

    # Sorguları çalıştır
    Write-Host "[*] Kontrol sorguları çalıştırılıyor..." -ForegroundColor Cyan

    $CheckQueries = @(
        @{Name = "Toptancilar"; Query = "SELECT COUNT(*) as Toptancilar FROM sys.objects WHERE name = 'Toptancilar'"},
        @{Name = "ParcaAlimlar"; Query = "SELECT COUNT(*) as ParcaAlimlar FROM sys.objects WHERE name = 'ParcaAlimlar'"},
        @{Name = "KasaIslemleri"; Query = "SELECT COUNT(*) as KasaIslemleri FROM sys.objects WHERE name = 'KasaIslemleri'"}
    )

    foreach ($CheckQuery in $CheckQueries) {
        $Command = $Connection.CreateCommand()
        $Command.CommandText = $CheckQuery.Query
        $Result = $Command.ExecuteScalar()

        if ($Result -gt 0) {
            Write-Host "✅ $($CheckQuery.Name) tablosu - OK" -ForegroundColor Green
        } else {
            Write-Host "❌ $($CheckQuery.Name) tablosu - HATA!" -ForegroundColor Red
        }
    }

    $Connection.Close()

    Write-Host ""
    Write-Host "=========================================" -ForegroundColor Green
    Write-Host "✅ KURULUM BAŞARILI!" -ForegroundColor Green
    Write-Host "=========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Tablolar başarıyla oluşturuldu." -ForegroundColor Yellow
    Write-Host "Uygulamayı yeniden başlatın." -ForegroundColor Yellow
    Write-Host ""

} catch {
    Write-Host ""
    Write-Host "=========================================" -ForegroundColor Red
    Write-Host "❌ KURULUM BAŞARISIZ!" -ForegroundColor Red
    Write-Host "=========================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "Hata: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "Kontrol edin:" -ForegroundColor Yellow
    Write-Host "- SQL Server açık mı?" -ForegroundColor Yellow
    Write-Host "- Sunucu adı doğru mu? ($Server)" -ForegroundColor Yellow
    Write-Host "- Veritabanı adı doğru mu? ($Database)" -ForegroundColor Yellow
    Write-Host "- Kullanıcı adı/şifre doğru mu?" -ForegroundColor Yellow
    Write-Host ""
    exit 1
}
