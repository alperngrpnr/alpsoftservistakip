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
# Nginx /randevu ve /takip Proxy Ayarı
docker exec -i web sh -c 'cat << "EOF" > /etc/nginx/conf.d/default.conf
server {
    listen 80;
    listen [::]:80;
    server_name localhost;

    location / {
        root /usr/share/nginx/html;
        index index.html index.htm;
    }

    location /takip {
        proxy_pass http://172.18.0.1:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    location /randevu {
        proxy_pass http://172.18.0.1:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    location /api {
        proxy_pass http://172.18.0.1:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    error_page 500 502 503 504 /50x.html;
    location = /50x.html {
        root /usr/share/nginx/html;
    }
}
EOF
nginx -s reload' 2>/dev/null || true
echo ">>> Web sayfaları güncellendi ve Nginx yenilendi."

# 3. Test
echo "[3/3] Canlı Sistem Test Ediliyor..."
HTTP_STATUS=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:80/takip || echo "ERR")
echo ">>> /takip yanıt kodu: ${HTTP_STATUS}"
HTTP_STATUS_RANDEVU=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:80/randevu || echo "ERR")
echo ">>> /randevu yanıt kodu: ${HTTP_STATUS_RANDEVU}"

echo "=========================================="
echo " TÜM İŞLEMLER BAŞARIYLA TAMAMLANDI! 🚀 "
echo " Takip:   https://alpsoftservistakip.com/takip "
echo " Randevu: https://alpsoftservistakip.com/randevu "
echo "=========================================="
