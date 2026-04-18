# 🔧 TOPTANCI YÖNETİMİ - KURULUM TALIMATI

## 📝 İÇİNDEKİLER
1. [Veritabanı Kurulumu](#-veritabanı-kurulumu)
2. [Uygulamada Aktivasyon](#-uygulamada-aktivasyon)
3. [Kullanım Kılavuzu](#-kullanım-kılavuzu)
4. [İleri Özellikler](#-ileri-özellikler)

---

## 🗄️ Veritabanı Kurulumu

### Adım 1: SQL Script'i Çalıştırma (YENİ!)

**⚠️ ÖNEMLİ:** Bu script artık Sirketler tablosunun olup olmadığını otomatik olarak kontrol ediyor!

1. **SQL Server Management Studio (SSMS)** açın
2. **Bağlı olduğunuz veritabanını seçin**
3. **Yeni Sorgu** penceresini açın (Ctrl+N)
4. Aşağıdaki dosyayı açın:
   ```
   SQL_Scripts/Toptancilar_CreateTable.sql
   ```
5. **F5** tuşu ile sorguyu çalıştırın

**Ne Olur?**
- ✅ Sirketler tablosu varsa → FK constraints otomatik eklenir
- ✅ Sirketler tablosu yoksa → Tablolar FK'siz oluşturulur (uyarı alırsınız)
- ✅ Kayitlar tablosu varsa → FK_ToptanciParcaAlimlar_Kayit constraint eklenir
- ✅ Kayitlar tablosu yoksa → Opsiyonel olarak atlanır

### Adım 2: Tablo Doğrulaması

Tablolar oluşturuldu mu kontrol etmek için:

```sql
-- Toptancilar tablosunu kontrol et
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Toptancilar';

-- ToptanciParcaAlimlar tablosunu kontrol et
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ToptanciParcaAlimlar';

-- Constraints'i kontrol et
SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
WHERE TABLE_NAME IN ('Toptancilar', 'ToptanciParcaAlimlar')
```

Her sorgu da sonuç döndürmelidir. ✅

### Adım 3: Varolan FK Constraints'i Güncelleme (İLERİ)

Eğer daha sonra Sirketler veya Kayitlar tabloları eklenirse:

```sql
-- Sirketler tablosu vardır, ancak Toptancilar'da FK yok ise:
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.Sirketler'))
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Toptancilar_Sirket')
    BEGIN
        ALTER TABLE Toptancilar
        ADD CONSTRAINT FK_Toptancilar_Sirket 
        FOREIGN KEY (SirketID) REFERENCES Sirketler(ID);
    END
END
GO
```

---

## 🎯 Uygulamada Aktivasyon

### Adım 1: Uygulamayı Güncelleyin
- Projeyi **Rebuild** edin (Ctrl+Alt+F7)
- Çözüm çalışıyor olmalı

### Adım 2: Ana Menüdeki Düğmeyi Kontrol Edin
- Uygulamayı başlatın
- Ana ekranda **"👨‍💼 TOPTANCI YÖN."** düğmesini görün

---

## 📖 Kullanım Kılavuzu

### ✅ Yeni Toptancı Ekleme

1. **Ana menüden** "👨‍💼 TOPTANCI YÖN." butonuna tıklayın
2. **"➕ YENİ TOPTANCI EKLE"** butonuna tıklayın
3. Modal pencerede bilgileri girin:
   - 📝 **Toptancı Adı** *(Zorunlu)*
   - 📞 **Telefon Numarası** (Opsiyonel)
   - 🏦 **IBAN Bilgisi** (Opsiyonel)
   - 📋 **Açıklama** (Opsiyonel)
4. **KAYDET** butonuna tıklayın

> ⚠️ **Not:** Aynı isimli toptancı olamaz. Eğer eklerken hata alırsanız, isim kontrolü yapın.

---

### 🔍 Toptancı Detaylarını Görme

1. Toptancı listesinde **"DETAY"** butonuna tıklayın
2. Açılan sayfada görün:
   - ✅ Toptancı adı, telefon ve IBAN
   - 💰 **TOPLAM BORÇ**: Bu toptancıya borçlu olduğunuz tutarı
   - ✅ **TOPLAM ALMIŞIZ**: Bu toptancıdan aldığımız toplam tutarı
   - 📊 Tüm parça alımlarının tablosu

**Tablo Sütunları:**
- Parça Adı
- Adet (Kaç tane aldık)
- Birim Fiyatı
- Toplam Tutar (Adet × Birim Fiyat)
- Ödenen Tutar
- Borçlu Tutar (Toplam - Ödenen)
- İşlem Tarihi

---

### ✏️ Toptancı Düzenle

1. Toptancı listesinde **"DÜZENLE"** butonuna tıklayın
2. Bilgileri güncelleyin
3. **GÜNCELLE** butonuna tıklayın

> 📌 Toptancı adını değiştirirken başka bir adla çakışmadığından emin olun.

---

### 🗑️ Toptancı Sil

1. Toptancı listesinde **"SİL"** butonuna tıklayın
2. Onay mesajında **Evet**'i seçin

> ⚠️ **ÖNEMLİ:** Eğer bu toptancıdan alınan parça kaydı varsa, silme işlemi başarısız olur. Önce parça kayıtlarını silmelisiniz.

---

## 🚀 İleri Özellikler

### Servis Kaydından Otomatik Parça Taşıma

**Gelecek Sürüm:** Servis kaydı oluştururken parça eklerseniz, bu parçalar otomatik olarak toptancı sistemine kaydedilecektir.

**Şu anda:** Özel API hazır (ToptanciHelper.cs)

```csharp
// Örnek Kod (İleri kullanım):
bool basarili = await ToptanciHelper.ToptanciParcasiEkleAsync(
    sirketId: Class1.AktifKullanici.SirketID,
    toptanciId: selectedToptanciId,
    kayitId: currentServiceRecordId,
    parcaAdi: "Kompresör",
    adet: 2,
    birimFiyati: 150.00m,
    aciklama: "Servis kaydından eklendi"
);
```

### Ödeme Kaydı Yapma

```csharp
// Toptancıya ödeme yap
bool basarili = await ToptanciHelper.OdemeKaydeAsync(
    parcaAlimId: recordId,
    odenecekTutar: 300.00m,
    sirketId: Class1.AktifKullanici.SirketID
);
```

### Toplam Borç Hesaplama

```csharp
// Herhangi bir toptancıya olan toplam borç
decimal toplamBorc = await ToptanciHelper.ToplamBorcHesaplaAsync(
    sirketId: Class1.AktifKullanici.SirketID,
    toptanciId: selectedToptanciId
);
```

---

## 🐛 Sorun Giderme

### ❌ "Toptancilar tablosu bulunamadı" Hatası
**Çözüm:** SQL scriptini çalıştırdığınızdan emin olun (Adım 1)

### ❌ "Foreign Key Constraint" Hatası
**Çözüm:** Toptancı üzerinde parça kaydı var. Önce parçaları silin.

### ❌ "UNIQUE Constraint" Hatası
**Çözüm:** Aynı isimli toptancı zaten var. İsmi değiştirin.

### ❌ "Bağlantı Hatası"
**Çözüm:** SQL Server bağlantı ayarlarınızı kontrol edin (App.config)

---

## 📞 İletişim

Kurulum veya kullanımla ilgili sorularınız için:
- 📧 **E-posta:** [kurucunun e-postası]
- 📱 **Telefon:** [Destek numarası]

---

**Son Güncelleme:** 2024  
**Sürüm:** 1.0.0 (Beta - Üretim Hazır)  
**Yazı Yazarı:** Alpsoft Servis Takip Sistemi

---

## ✅ KONTROL LİSTESİ

- [ ] SQL tablolarını oluşturdum
- [ ] Uygulama başarıyla derlendi
- [ ] Ana menüde "TOPTANCI YÖN." düğmesi görünüyor
- [ ] Yeni toptancı ekleyebildim
- [ ] Detay sayfasını açabiliyorum
- [ ] Toptancı bilgilerini düzenleyebildim
- [ ] Toptancı silebiliyorum (varsa)

✨ **Tüm adımlar tamamlandıysa, sistem kullanıma hazır!**
