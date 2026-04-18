-- ========================================
-- ========================================
-- SIRKETLER TABLOSU KONTROLÜ
-- ========================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.Sirketler') AND type in (N'U'))
BEGIN
    PRINT '⚠️ UYARI: Sirketler tablosu bulunamadı. Toptancı tabloları FK kısıtlaması olmadan oluşturulacak.';
END
GO

-- ========================================
-- TOPTANCI (SUPPLIER) TABLOSU
-- ========================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.Toptancilar') AND type in (N'U'))
BEGIN
    CREATE TABLE Toptancilar (
        ID INT PRIMARY KEY IDENTITY(1,1),
        SirketID INT NOT NULL,
        FirmaAdi NVARCHAR(255) NOT NULL,
        Telefon NVARCHAR(20),
        IBAN NVARCHAR(50),
        Aciklama NVARCHAR(MAX),
        OlusturulmaTarihi DATETIME DEFAULT GETDATE(),
        GuncellemeTarihi DATETIME DEFAULT GETDATE(),

        CONSTRAINT UQ_Toptancilar_FirmaAdi UNIQUE(SirketID, FirmaAdi)
    );

    CREATE INDEX IX_Toptancilar_SirketID ON Toptancilar(SirketID);
    CREATE INDEX IX_Toptancilar_FirmaAdi ON Toptancilar(FirmaAdi);

    PRINT '✅ Toptancilar tablosu başarıyla oluşturuldu.';
END
ELSE
BEGIN
    PRINT '✅ Toptancilar tablosu zaten mevcut.';
END
GO

-- FK CONSTRAINT EKLEME (Sirketler tablosu varsa)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.Sirketler') AND type in (N'U'))
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Toptancilar_Sirket')
    BEGIN
        ALTER TABLE Toptancilar
        ADD CONSTRAINT FK_Toptancilar_Sirket 
        FOREIGN KEY (SirketID) REFERENCES Sirketler(ID);
        PRINT '✅ FK_Toptancilar_Sirket constraint eklendi.';
    END
END
GO

-- ========================================
-- TOPTANCIDAN ALINAN PARÇALAR TABLOSU
-- ========================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.ToptanciParcaAlimlar') AND type in (N'U'))
BEGIN
    CREATE TABLE ToptanciParcaAlimlar (
        ID INT PRIMARY KEY IDENTITY(1,1),
        SirketID INT NOT NULL,
        ToptanciID INT NOT NULL,
        KayitID INT,
        ParcaAdi NVARCHAR(255) NOT NULL,
        Adet INT NOT NULL,
        BirimFiyati DECIMAL(18, 2) NOT NULL,
        ToplamTutar DECIMAL(18, 2) NOT NULL,
        OdenenTutar DECIMAL(18, 2) DEFAULT 0,
        BorcluTutar DECIMAL(18, 2) DEFAULT 0,
        Aciklama NVARCHAR(MAX),
        IslemTarihi DATETIME DEFAULT GETDATE(),
        OlusturulmaTarihi DATETIME DEFAULT GETDATE(),
        GuncellemeTarihi DATETIME DEFAULT GETDATE(),

        CONSTRAINT FK_ToptanciParcaAlimlar_Toptanci FOREIGN KEY (ToptanciID) REFERENCES Toptancilar(ID),
        CONSTRAINT CHK_ToptanciParcaAlimlar_Adet CHECK (Adet > 0),
        CONSTRAINT CHK_ToptanciParcaAlimlar_BirimFiyati CHECK (BirimFiyati >= 0),
        CONSTRAINT CHK_ToptanciParcaAlimlar_ToplamTutar CHECK (ToplamTutar >= 0)
    );

    CREATE INDEX IX_ToptanciParcaAlimlar_SirketID ON ToptanciParcaAlimlar(SirketID);
    CREATE INDEX IX_ToptanciParcaAlimlar_ToptanciID ON ToptanciParcaAlimlar(ToptanciID);
    CREATE INDEX IX_ToptanciParcaAlimlar_KayitID ON ToptanciParcaAlimlar(KayitID);
    CREATE INDEX IX_ToptanciParcaAlimlar_IslemTarihi ON ToptanciParcaAlimlar(IslemTarihi);

    PRINT '✅ ToptanciParcaAlimlar tablosu başarıyla oluşturuldu.';
END
ELSE
BEGIN
    PRINT '✅ ToptanciParcaAlimlar tablosu zaten mevcut.';
END
GO

-- FK CONSTRAINTS EKLEME (Sirketler ve Kayitlar tabloları varsa)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.Sirketler') AND type in (N'U'))
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ToptanciParcaAlimlar_Sirket')
    BEGIN
        ALTER TABLE ToptanciParcaAlimlar
        ADD CONSTRAINT FK_ToptanciParcaAlimlar_Sirket 
        FOREIGN KEY (SirketID) REFERENCES Sirketler(ID);
        PRINT '✅ FK_ToptanciParcaAlimlar_Sirket constraint eklendi.';
    END
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.Kayitlar') AND type in (N'U'))
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ToptanciParcaAlimlar_Kayit')
    BEGIN
        ALTER TABLE ToptanciParcaAlimlar
        ADD CONSTRAINT FK_ToptanciParcaAlimlar_Kayit 
        FOREIGN KEY (KayitID) REFERENCES Kayitlar(ID);
        PRINT '✅ FK_ToptanciParcaAlimlar_Kayit constraint eklendi.';
    END
END
GO

-- ========================================
-- KONTROL MESAJLARI
-- ========================================
PRINT '';
PRINT '============================================';
PRINT 'TOPTANCI YÖNETİMİ SİSTEMİ HAZIR!';
PRINT '============================================';

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.Toptancilar') AND type in (N'U'))
    PRINT '✅ Toptancilar tablosu - OK'
ELSE
    PRINT '❌ Toptancilar tablosu - HATA!'

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.ToptanciParcaAlimlar') AND type in (N'U'))
    PRINT '✅ ToptanciParcaAlimlar tablosu - OK'
ELSE
    PRINT '❌ ToptanciParcaAlimlar tablosu - HATA!'

PRINT '';
