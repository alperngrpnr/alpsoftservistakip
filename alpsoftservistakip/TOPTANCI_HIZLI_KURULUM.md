# ⚡ TOPTANCI SİSTEMİ - HIZLI KURULUM (5 DAKİKA)

## 🚀 YENİ VERSİYON HATA DÜZELMESİ

**Sorun:** SQL script'te Sirketler tablosu olmadığında FK constraint hatası  
**Çözüm:** ✅ **FK constraints artık otomatik kontrol ediliyor!**

---

## 📋 3 ADIMDA BAŞLAYINIZ

### 1️⃣ SQL SCRIPT'İ ÇALIŞTIR (SSMS)
```
📂 Dosya: SQL_Scripts/Toptancilar_CreateTable.sql
🖱️ Nerede: SQL Server Management Studio
```

- SSMS açın → Yeni Sorgu (Ctrl+N) → Script'i yapıştır → F5

**✅ İşlem Tamamlandı:**
- Tablosu oluşturulur (Sirketler olup olmadığına bakılmaksızın)
- Uyarı mesajları gösterilir (bilgilendirme amaçlı)
- FK constraints gerekirse sonra eklenir

### 2️⃣ UYGULAMAYI BAŞLAT
```bash
dotnet run
```

### 3️⃣ TOPTANCI YÖNETİMİNE GİT
```
Ana Menü → 👨‍💼 TOPTANCI YÖN. → ➕ YENİ TOPTANCI EKLE
```

---

## 🔍 VERİ TABANINDA NE OLUŞTU?

### Toptancilar Tablosu
```sql
┌─────────────────────────────┐
│ Toptancilar                 │
├─────────────────────────────┤
│ ✅ ID (Primary Key)         │
│ ✅ SirketID (Index)         │
│ ✅ FirmaAdi (UNIQUE Index)  │
│ ✅ Telefon, IBAN            │
│ ⏳ FK (Sirketler varsa)     │
└─────────────────────────────┘
```

### ToptanciParcaAlimlar Tablosu
```sql
┌────────────────────────────────┐
│ ToptanciParcaAlimlar           │
├────────────────────────────────┤
│ ✅ ID (Primary Key)            │
│ ✅ SirketID (Index)            │
│ ✅ ToptanciID (FK → Toptancilar)
│ ✅ KayitID (Index, OPT)        │
│ ✅ Parça bilgileri             │
│ ✅ Adet, Fiyat, Borç           │
│ ⏳ FK_Sirket (Sirketler varsa) │
│ ⏳ FK_Kayit (Kayitlar varsa)   │
└────────────────────────────────┘
```

---

## ✨ YENI ÖZELLİKLER

### 🔧 Otomatik FK Yönetimi
```sql
IF EXISTS (Sirketler)
  ADD CONSTRAINT FK_Toptancilar_Sirket
ELSE
  PRINT '⚠️ Sirketler tablosu bulunamadı'
```

### 🛡️ Kontrol Mekanizmaları
- ✅ Table existence check
- ✅ Constraint duplicate prevention  
- ✅ Error messaging
- ✅ Graceful fallback

---

## 🎯 KONTROL LİSTESİ

- [ ] SQL script'i çalıştırdım
- [ ] Başarı mesajını gördüm
- [ ] Uygulamayı başlattım
- [ ] "TOPTANCI YÖN." butonunu gördüm
- [ ] Yeni toptancı ekledim

✅ Tamamlandıysa sistem hazır!

---

## 🐛 SORUN YAŞIYORSANIZ?

### ❌ "Foreign key references invalid table" Hatası
**Çözüm:** ✅ Artık olmamalı! Ancak alıyorsanız:
1. Script'i en son versiyonu kullandığınızdan emin olun
2. Eski script'i sil, yenisini çalıştır

### ❌ "Toptancilar tablosu - HATA!" Mesajı
**Çözüm:** 
1. SSMS'de tabloyu kontrol et:
   ```sql
   SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES 
   WHERE TABLE_NAME = 'Toptancilar'
   ```
2. Tablo yoksa script hata almıştır

### ❌ Uygulamada "Toptancı Yönetimi" açılmıyor
**Çözüm:**
1. Derle: `dotnet build`
2. Başlat: `dotnet run`

---

## 📞 DOSYA KONUMLARI

| Dosya | Yol |
|-------|-----|
| SQL Script | `SQL_Scripts/Toptancilar_CreateTable.sql` |
| Kurulum Rehberi | `TOPTANCI_KURULUM_TALIMATI.md` |
| Teknik Dokümantasyon | `TOPTANCI_YONETIMI_DOKUMENTASYON.md` |

---

**Sürüm:** 1.0.1 (FK Hata Düzeltmesi)  
**Durum:** ✅ HAZIR

🎉 **Hep açı, hiç kapalı!**
