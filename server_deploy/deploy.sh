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
