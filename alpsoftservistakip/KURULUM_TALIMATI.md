# 🔧 PARÇA ALIM SİSTEMİ - KURULUM TALİMATI

## ⚠️ ÖNEMLİ: SQL SCRIPT ÇALIŞTIRMA

Parça Alım Sistemi'ni kullanabilmek için **mutlaka** aşağıdaki SQL scriptini çalıştırmalısınız:

---

## 📋 Adım Adım Kurulum

### 1️⃣ SQL Server Management Studio (SSMS) Açın
- SQL Server Management Studio'yu açın
- Veritabanı sunucunuza bağlanın

### 2️⃣ Doğru Veritabanını Seçin
- **Object Explorer**'da uygulamanızın veritabanına sağ tıklayın
- Veya aşağıdaki komutu çalıştırın:
```sql
USE YourDatabaseName;
GO
```

### 3️⃣ Aşağıdaki SQL Scriptini Çalıştırın

```sql
-- ========================================
-- PARÇA ALIMLARI TABLOSU
-- Toptancılardan satın alınan parçaları izlemek için
-- ========================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.ParcaAlimlar') AND type in (N'U'))
BEGIN
    CREATE TABLE ParcaAlimlar (
        ID INT PRIMARY KEY IDENTITY(1,1),
        SirketID INT NOT NULL,
        IslemTarihi DATETIME DEFAULT GETDATE(),
        ParcaAdi NVARCHAR(255) NOT NULL,
        ToptanciID INT NOT NULL,
        ToptanciAdi NVARCHAR(255) NOT NULL,
        Miktar INT NOT NULL,
        BirimFiyat DECIMAL(18, 2) NOT NULL,
        [Not] NVARCHAR(MAX),
        OlusturulmaTarihi DATETIME DEFAULT GETDATE(),
        GuncellemeTarihi DATETIME DEFAULT GETDATE(),

        CONSTRAINT FK_ParcaAlimlar_Sirket FOREIGN KEY (SirketID) REFERENCES Sirketler(ID),
        CONSTRAINT FK_ParcaAlimlar_Cariler FOREIGN KEY (ToptanciID) REFERENCES Cariler(CariID),
        CONSTRAINT CHK_ParcaAlimlar_Miktar CHECK (Miktar > 0),
        CONSTRAINT CHK_ParcaAlimlar_BirimFiyat CHECK (BirimFiyat >= 0)
    );

    CREATE INDEX IX_ParcaAlimlar_SirketID ON ParcaAlimlar(SirketID);
    CREATE INDEX IX_ParcaAlimlar_ToptanciID ON ParcaAlimlar(ToptanciID);
    CREATE INDEX IX_ParcaAlimlar_IslemTarihi ON ParcaAlimlar(IslemTarihi);

    PRINT 'ParcaAlimlar tablosu başarıyla oluşturuldu.';
END
ELSE
BEGIN
    PRINT 'ParcaAlimlar tablosu zaten mevcut.';
END
GO

PRINT '============================================';
PRINT 'Parça Alım Sistemi tabloları hazır!';
PRINT '============================================';
```

---

## ✅ Kontrol Listesi

SQL scripti çalıştıktan sonra:

- ✅ **ParcaAlimlar tablosu oluşturuldu mu?**
- ✅ **Hata mesajı alındı mı?** (Alındıysa aşağıya bakın)
- ✅ Uygulamayı yeniden başlatın

---

## 🐛 Sık Sorulan Sorunlar

### Problem 1: "ParcaAlimlar tablosu zaten mevcut" mesajı
**✅ Çözüm:** Tamam! Tablo zaten kurulmuş. Uygulamayı kullanabilirsiniz.

### Problem 2: "Invalid object name 'ParcaAlimlar'"
**✅ Çözüm:** SQL scriptini çalıştırmadınız. Yukarıdaki adımları uygulayın.

### Problem 3: "Foreign key constraint failure"
**✅ Çözüm:** Cariler ve Sirketler tabloları yokmuş. Lütfen ana sistem kurulumunu tamamlayın.

### Problem 4: "Toptancılar yüklenirken hata: Invalid column name 'CariID'"
**✅ Çözüm:** Cariler tablosunun yapısı değişmiş olabilir. Sistemi yöneticiye başvurun.

---

## 🎯 Başarı Belirtileri

Kurulum başarılı ise:

✅ KASA VE PARÇA TAKİBİ sayfasında PARÇA sekmesini açtığınız zaman
✅ "Yeni Parça Alımı" butonuna tıklayabilirsiniz
✅ Toptancı açılır listesi doldurulmuştur
✅ Parça alımı kaydedebilirsiniz

---

## 📞 Destek

Hala sorun yaşıyorsanız:

1. Aşağıdaki bilgileri kontrol edin:
   - SQL Server bağlantısı sağlıklı mı?
   - Veritabanı kullanıcısı yetkili mi?
   - Sirketler tablosu var mı?
   - Cariler tablosu var mı?

2. Veritabanı yöneticisine başvurun

---

**Son Güncelleme:** 2024
**Versiyon:** 1.0
