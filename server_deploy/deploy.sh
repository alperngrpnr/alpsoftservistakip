#!/bin/bash
set -e

echo "=========================================="
echo " AlpSoft Servis Takip Otomatik Güncelleme"
echo "=========================================="

BASE_URL="https://raw.githubusercontent.com/alperngrpnr/alpsoftservistakip/main/server_deploy"

# 1. API Güncellemesi
echo "[1/3] Yeni API (TakipController & CORS) indiriliyor..."
mkdir -p /root/app/api
curl -sL "${BASE_URL}/AlpSoftConApi.dll" -o /root/app/api/AlpSoftConApi.dll

# SQL Bildirim Tablosu ve İzinleri
CREATE_SQL="USE alpsoftservistakip; IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MusteriOnayBildirimleri') CREATE TABLE MusteriOnayBildirimleri (Id INT IDENTITY(1,1) PRIMARY KEY, ServisId INT NOT NULL, CompanyId INT NOT NULL, MusteriAdi NVARCHAR(200), Cihaz NVARCHAR(200), FiyatBilgisi NVARCHAR(100), Karar NVARCHAR(50), Tarih DATETIME DEFAULT GETDATE(), Okundu BIT DEFAULT 0, Ip NVARCHAR(100)); GRANT SELECT, INSERT, UPDATE, DELETE ON MusteriOnayBildirimleri TO alpsoft_api;"
/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'Alperengurpinar4160552009.' -C -Q "$CREATE_SQL" 2>/dev/null || \
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'Alperengurpinar4160552009.' -Q "$CREATE_SQL" 2>/dev/null || \
docker exec -i $(docker ps -q -f name=sql | head -n 1) /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'Alperengurpinar4160552009.' -C -Q "$CREATE_SQL" 2>/dev/null || true

systemctl restart alpsoft-api
echo ">>> API servisi güncellendi ve yeniden başlatıldı."

# 2. Web Sitesi Güncellemesi
echo "[2/3] Web Sitesi (Takip Barı & Buton) güncelleniyor..."
mkdir -p /root/app/web
curl -sL "${BASE_URL}/index.html" -o /root/app/web/index.html
curl -sL "${BASE_URL}/style.css" -o /root/app/web/style.css
docker exec web nginx -s reload 2>/dev/null || true
echo ">>> Web sayfaları güncellendi ve Nginx yenilendi."

# 3. Test
echo "[3/3] Canlı Sistem Test Ediliyor..."
HTTP_STATUS=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:80/takip || echo "ERR")
echo ">>> /takip yanıt kodu: ${HTTP_STATUS}"

echo "=========================================="
echo " TÜM İŞLEMLER BAŞARIYLA TAMAMLANDI! 🚀 "
echo " https://alpsoftservistakip.com/takip "
echo "=========================================="
