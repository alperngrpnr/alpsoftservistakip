# 👨‍💼 TOPTANCI YÖNETİMİ SİSTEMİ - KURULUM VE KULLANIMA

## 📋 Sistem Özeti

Yeni oluşturulan Toptancı Yönetimi sistemi, servis firmanızın tedarikçilerini ve onlardan satın aldığı parçaları takip etmek için profesyonel bir çözüm sunar.

---

## 🗄️ VERİTABANI TABLOLARI

### 1. **Toptancilar** Tablosu
```sql
- ID: Benzersiz tanımlayıcı
- SirketID: Şirket referansı
- FirmaAdi: Toptancı işletme adı (UNIQUE)
- Telefon: İletişim numarası
- IBAN: Banka hesap numarası
- Aciklama: Ek notlar
- OlusturulmaTarihi: Oluşturulma tarihi
- GuncellemeTarihi: Son güncelleme tarihi
```

### 2. **ToptanciParcaAlimlar** Tablosu
```sql
- ID: Benzersiz tanımlayıcı
- SirketID: Şirket referansı
- ToptanciID: Toptancı referansı
- KayitID: Servis kaydı referansı (opsiyonel)
- ParcaAdi: Parça adı
- Adet: Satın alınan miktar
- BirimFiyati: Birim fiyat
- ToplamTutar: Adet × BirimFiyat
- OdenenTutar: Ödenen tutarı
- BorcluTutar: Kalan borç (ToplamTutar - OdenenTutar)
- Aciklama: Ek notlar
- IslemTarihi: İşlem tarihi
- OlusturulmaTarihi: Oluşturulma tarihi
- GuncellemeTarihi: Son güncelleme tarihi
```

---

## 🎨 ARAYÜZ BILEŞENLERI

### 1. **PageToptanciListesi** (Toptancı Listesi)
- **Özellikler:**
  - Tüm toptancıları tabloda gösterir
  - Telefon ve IBAN bilgilerini gösterir
  - Arama ve filtreleme (isterseniz eklenebilir)

- **Butonlar:**
  - ➕ **YENİ TOPTANCI EKLE**: Yeni toptancı eklemeYENİ sayfasını açar
  - **DETAY**: Toptancıdan alınan tüm parçaları ve toplam borcu gösterir
  - **DÜZENLE**: Toptancı bilgilerini düzenler
  - **SİL**: Toptancıyı siler (sadece parça olmayan toptancılar silinebilir - constraint var)

### 2. **PageToptanciEkle** (Yeni/Düzenle Modalı)
- **Giriş Alanları:**
  - ✅ Toptancı Adı (Zorunlu)
  - 📞 Telefon Numarası (Opsiyonel)
  - 🏦 IBAN Bilgisi (Opsiyonel)
  - 📝 Açıklama (Opsiyonel)

- **Davranış:**
  - Yeni eklerken: "YENİ TOPTANCI EKLE" başlığı
  - Düzenlerken: "TOPTANCI DÜZENLE" başlığı ve verileri yükler
  - Kaydetme sonrası otomatik olarak listeyi günceller

### 3. **PageToptanciDetay** (Toptancı Detayları)
- **Gösterilen Bilgiler:**
  - Toptancı Adı, Telefon, IBAN
  - **TOPLAM BORÇ**: Toptancıya olan toplam borç
  - **TOPLAM ALMIŞIZ**: Toptancıdan aldığımız toplam tutar

- **Tablo:**
  - Toptancıdan alınan tüm parçalar
  - Sütunlar: Parça Adı, Adet, Birim Fiyatı, Toplam Tutar, Ödenen Tutar, Borçlu Tutar, Tarih

- **Özet Panel:**
  - Toplam Adet
  - Toplam Harcanan
  - Kalan Borç

---

## 🔗 İNTEGRASYON (Servis Kaydıyla)

### Servis Kaydından Parça Ekleme (İLERİ SÜRÜMÜ)

**Aşaması:**
1. Servis kaydında parça ekleme kısmında, parça kaydettikten sonra
2. Toptancı seçme dropdown'u gösterilecek
3. Toptancı ve fiyat seçildikten sonra
4. `ToptanciParcaAlimlar` tablosuna otomatik kayıt yapılacak
5. Servis kaydının ID'si `KayitID` alanına işlenecek

**SQL Sorgusu:**
```sql
INSERT INTO ToptanciParcaAlimlar 
  (SirketID, ToptanciID, KayitID, ParcaAdi, Adet, BirimFiyati, ToplamTutar, IslemTarihi)
VALUES 
  (@sirketId, @toptanciId, @kayitId, @parcaAdi, @adet, @birimFiyati, @toplam, GETDATE())
```

---

## 📊 BORCLU TUTAR HESAPLAMASI

**Otomatik Formül:**
```
BorcluTutar = ToplamTutar - OdenenTutar
```

**Güncelleme:**
- Ödeme kaydı yapıldığında, `OdenenTutar` artacak
- Borclu tutar otomatik olarak düşecek
- Toplam borç sorgusu: `SELECT SUM(BorcluTutar) FROM ToptanciParcaAlimlar WHERE ToptanciID = ?`

---

## 🚀 KURULUM ADIMLARI

### 1. SQL Tabloları Oluşturma
```bash
# SQL Server'da çalıştırın
KURULUM_TALIMATI.md dosyasında verilen SQL scriptini çalıştırın:
SQL_Scripts/Toptancilar_CreateTable.sql
```

### 2. C# Projede Derleme
```bash
dotnet build
```

### 3. Uygulamada Kullanım
- Ana menüden "👨‍💼 TOPTANCI YÖN." butonuna tıklayın
- Yeni toptancı ekleyin
- Parça alımlarını takip edin

---

## 📈 RAPORLAMA ÖZELLİKLERİ

**Gelecek Sürüm İçin Hazırlanan Alanlar:**
- ✅ Toptancı bazında toplam borç raporu
- ✅ Ödeme geçmişi
- ✅ Toptancı performans analizi (en çok satın aldığımız toptancı, vs.)
- ✅ Vade geçmiş alacak uyarıları

---

## 🛠️ TEKNIK ÖZELLİKLER

- **Framework:** .NET Framework 4.7.2
- **Veritabanı:** SQL Server
- **UI:** WPF (XAML)
- **Tasarım Deseni:** MVVM-ready (ilgili dosyalar var)
- **Güvenlik:** SirketID-based veri izolasyonu

---

## ⚠️ ÖNEMLİ NOTLAR

1. **Silme Kısıtlama:**
   - FK constraint nedeniyle, üzerinde parça kaydı olan toptancı silinemez
   - Önce tüm parça kayıtlarını silmelisiniz

2. **UNIQUE Constraint:**
   - Aynı şirket içinde aynı isimli toptancı olamaz

3. **Veri Güvenliği:**
   - Tüm sorgular SirketID kontrolü yaparak diğer şirketlerin verilerini korur

4. **Para Formatı:**
   - ₺ sembolü ve 2 ondalak basamak otomatik formatlanır
   - Veritabanında DECIMAL(18,2) tipidir

---

## 📞 DESTEK

Sorunlar için aşağıdaki bilgileri kontrol edin:
- ❌ "Toptancilar tablosu bulunamadı" hatası → SQL scriptini çalıştırdığınızdan emin olun
- ❌ UNIQUE constraint hatası → Aynı isimli toptancı var mı kontrol edin
- ❌ FK constraint hatası → Parça kayıtlarını silip tekrar deneyin

---

**Hazırlanma Tarihi:** 2024  
**Sürüm:** 1.0.0 (Beta)  
**Durum:** ✅ Üretim Hazır
