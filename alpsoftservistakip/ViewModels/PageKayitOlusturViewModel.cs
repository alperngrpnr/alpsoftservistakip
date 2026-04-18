using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Diagnostics;
using System.Threading.Tasks;
using alpsoftservistakip;
using alpsoftservistakip.Services;

namespace alpsoftservistakip.ViewModels
{
    public class PageKayitOlusturViewModel : BaseViewModel
    {
        private const string ConnectionString = "Server=91.247.168.204,1433;Database=alpsoftservistakip;User Id=sa;Password=Alperengurpinar4160552009;TrustServerCertificate=True;";

        private bool _degisiklikVar = false;
        private int _kayitId;
        private bool _yuklemeDevamEdiyor = false;
        private bool _disServisSatirKaynagi = false;
        private int _disServisAdayKayitId = 0;

        public bool DegisiklikVar
        {
            get => _degisiklikVar;
            set => SetProperty(ref _degisiklikVar, value);
        }

        public bool YuklemeDevamEdiyor
        {
            get => _yuklemeDevamEdiyor;
            set => SetProperty(ref _yuklemeDevamEdiyor, value);
        }

        // Properties
        private string _bayiAdi;
        private string _adSoyad;
        private string _cepTelefonu;
        private string _adres;
        private string _eposta;
        private string _cihazTuru;
        private string _marka;
        private string _model;
        private string _imeiNo;
        private string _ekBilgiler;
        private string _sikayetAriza;
        private string _fiyatBilgisi;
        private string _ilgiliTeknisyen;
        private string _servisDurumu;
        private bool _yeni;
        private bool _eski;
        private bool _tamirGormus;
        private bool _garantili;
        private bool _garantisiz;
        private bool _servisGarantili;
        private bool _yedeklemeYapilsin;
        private System.Collections.ObjectModel.ObservableCollection<string> _bayiListesi;
        private System.Collections.ObjectModel.ObservableCollection<string> _tekniksijenListesi;

        public string BayiAdi { get => _bayiAdi; set => SetProperty(ref _bayiAdi, value); }
        public string AdSoyad { get => _adSoyad; set { SetProperty(ref _adSoyad, value); if (!YuklemeDevamEdiyor) DegisiklikVar = true; } }
        public string CepTelefonu { get => _cepTelefonu; set { SetProperty(ref _cepTelefonu, value); if (!YuklemeDevamEdiyor) DegisiklikVar = true; } }
        public string Adres { get => _adres; set { SetProperty(ref _adres, value); if (!YuklemeDevamEdiyor) DegisiklikVar = true; } }
        public string EPosta { get => _eposta; set { SetProperty(ref _eposta, value); if (!YuklemeDevamEdiyor) DegisiklikVar = true; } }
        public string CihazTuru { get => _cihazTuru; set { SetProperty(ref _cihazTuru, value); if (!YuklemeDevamEdiyor) DegisiklikVar = true; } }
        public string Marka { get => _marka; set { SetProperty(ref _marka, value); if (!YuklemeDevamEdiyor) DegisiklikVar = true; } }
        public string Model { get => _model; set { SetProperty(ref _model, value); if (!YuklemeDevamEdiyor) DegisiklikVar = true; } }
        public string ImeiNo { get => _imeiNo; set { SetProperty(ref _imeiNo, value); if (!YuklemeDevamEdiyor) DegisiklikVar = true; } }
        public string EkBilgiler { get => _ekBilgiler; set { SetProperty(ref _ekBilgiler, value); if (!YuklemeDevamEdiyor) DegisiklikVar = true; } }
        public string SikayetAriza { get => _sikayetAriza; set { SetProperty(ref _sikayetAriza, value); if (!YuklemeDevamEdiyor) DegisiklikVar = true; } }
        public string FiyatBilgisi { get => _fiyatBilgisi; set { SetProperty(ref _fiyatBilgisi, value); if (!YuklemeDevamEdiyor) DegisiklikVar = true; } }
        public string IlgiliTeknisyen { get => _ilgiliTeknisyen; set { SetProperty(ref _ilgiliTeknisyen, value); if (!YuklemeDevamEdiyor) DegisiklikVar = true; } }
        public string ServisDurumu { get => _servisDurumu; set { SetProperty(ref _servisDurumu, value); if (!YuklemeDevamEdiyor) DegisiklikVar = true; } }
        public bool Yeni { get => _yeni; set { SetProperty(ref _yeni, value); if (!YuklemeDevamEdiyor) DegisiklikVar = true; } }
        public bool Eski { get => _eski; set { SetProperty(ref _eski, value); if (!YuklemeDevamEdiyor) DegisiklikVar = true; } }
        public bool TamirGormus { get => _tamirGormus; set { SetProperty(ref _tamirGormus, value); if (!YuklemeDevamEdiyor) DegisiklikVar = true; } }
        public bool Garantili { get => _garantili; set { SetProperty(ref _garantili, value); if (!YuklemeDevamEdiyor) DegisiklikVar = true; } }
        public bool Garantisiz { get => _garantisiz; set { SetProperty(ref _garantisiz, value); if (!YuklemeDevamEdiyor) DegisiklikVar = true; } }
        public bool ServisGarantili { get => _servisGarantili; set { SetProperty(ref _servisGarantili, value); if (!YuklemeDevamEdiyor) DegisiklikVar = true; } }
        public bool YedeklemeYapilsin { get => _yedeklemeYapilsin; set { SetProperty(ref _yedeklemeYapilsin, value); if (!YuklemeDevamEdiyor) DegisiklikVar = true; } }

        public System.Collections.ObjectModel.ObservableCollection<string> BayiListesi
        {
            get => _bayiListesi;
            set => SetProperty(ref _bayiListesi, value);
        }

        public System.Collections.ObjectModel.ObservableCollection<string> TekniksijenListesi
        {
            get => _tekniksijenListesi;
            set => SetProperty(ref _tekniksijenListesi, value);
        }

        public ICommand KaydetCommand { get; }
        public ICommand GeriDonCommand { get; }
        public ICommand ImeiSorgulaCommand { get; }
        public ICommand ServisDurumuChangedCommand { get; }
        public ICommand TelefonFormatlaCommand { get; }
        public ICommand FisiYazdirCommand { get; }
        public ICommand KopyalaCommand { get; }
        public ICommand DisServisGonderCommand { get; }
        public ICommand DisServiseGonderCommand => DisServisGonderCommand;

        public PageKayitOlusturViewModel(int kayitId = 0)
        {
            _kayitId = kayitId;
            KaydetCommand = new RelayCommand(_ => Kaydet());
            GeriDonCommand = new RelayCommand(_ => GeriDon());
            ImeiSorgulaCommand = new RelayCommand(_ => ImeiSorgula());
            ServisDurumuChangedCommand = new RelayCommand<string>(ServisDurumuChanged);
            TelefonFormatlaCommand = new RelayCommand<string>(TelefonFormatla);
            FisiYazdirCommand = new RelayCommand(_ => FisiYazdir());
            KopyalaCommand = new RelayCommand(_ => Kopyala());
            DisServisGonderCommand = new RelayCommand(_ => DisServisGonder());

            // Listeleri yükle
            BayiListesi = new System.Collections.ObjectModel.ObservableCollection<string>();
            TekniksijenListesi = new System.Collections.ObjectModel.ObservableCollection<string>();

            YukleBayiListesi();
            YukluTekniksijenListesi();

            if (kayitId > 0)
            {
                YuklemeDevamEdiyor = true;
                VerileriYukle(kayitId);
                YuklemeDevamEdiyor = false;
            }
        }

        public void CarregarKayit(int id)
        {
            _kayitId = id;
            _disServisSatirKaynagi = false;
            _disServisAdayKayitId = 0;
            YuklemeDevamEdiyor = true;
            VerileriYukle(id);
            YuklemeDevamEdiyor = false;
        }

        public void DisServisSatirindanYukle(DataRowView row)
        {
            if (row == null)
                return;

            YuklemeDevamEdiyor = true;

            AdSoyad = GetRowString(row, "IsimSoyisim", "MusteriAdSoyad", "MusteriAdi");
            BayiAdi = GetRowString(row, "BayiAdi", "YetkiliBayi", "DisServisYeri");
            CepTelefonu = GetRowString(row, "CepTelefonu", "Telefon", "TelefonNo", "Gsm");
            EPosta = GetRowString(row, "EPosta", "Email", "Mail");
            CihazTuru = GetRowString(row, "CihazTuru", "UrunTuru", "Tur");
            Marka = GetRowString(row, "Marka");
            Model = GetRowString(row, "Model");
            ImeiNo = GetRowString(row, "IMEI", "ImeiNo", "SeriNo");
            EkBilgiler = GetRowString(row, "EkBilgiler", "Notlar", "Aciklama");
            SikayetAriza = GetRowString(row, "Ariza", "ArizaDetayi", "Sikayet");
            ServisDurumu = GetRowString(row, "ServisDurumu", "Durum");
            IlgiliTeknisyen = GetRowString(row, "ServisTeknisyen", "Teknisyen", "SorumluTeknisyen");

            _disServisSatirKaynagi = true;
            _disServisAdayKayitId = GetRowInt(row, "ServisKayitID", "ServisKayitId", "CihazKayitID", "CihazKayitId", "KaynakKayitID", "KaynakKayitId", "KayitID", "KayitId", "ID");

            int bagliKayitId = GetRowInt(row, "ServisKayitID", "ServisKayitId", "CihazKayitID", "CihazKayitId", "KaynakKayitID", "KaynakKayitId");
            _kayitId = bagliKayitId > 0 ? bagliKayitId : 0;

            DegisiklikVar = false;
            YuklemeDevamEdiyor = false;
        }

        private void VerileriYukle(int id)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();
                    string sorgu = "SELECT * FROM kayitlicihazlar WHERE ID = @ID";
                    SqlCommand cmd = new SqlCommand(sorgu, con);
                    cmd.Parameters.AddWithValue("@ID", id);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            AdSoyad = reader["IsimSoyisim"]?.ToString() ?? "";
                            BayiAdi = reader["BayiAdi"]?.ToString() ?? "";
                            CepTelefonu = reader["CepTelefonu"]?.ToString() ?? "";
                            Adres = reader["Adres"]?.ToString() ?? "";
                            EPosta = reader["EPosta"]?.ToString() ?? "";
                            CihazTuru = reader["CihazTuru"]?.ToString() ?? "";
                            Marka = reader["Marka"]?.ToString() ?? "";
                            Model = reader["Model"]?.ToString() ?? "";
                            ImeiNo = reader["IMEI"]?.ToString() ?? "";
                            EkBilgiler = reader["EkBilgiler"]?.ToString() ?? "";
                            SikayetAriza = reader["Ariza"]?.ToString() ?? "";
                            FiyatBilgisi = reader["FiyatBilgisi"]?.ToString() ?? "";
                            IlgiliTeknisyen = reader["ServisTeknisyen"]?.ToString() ?? "";
                            ServisDurumu = reader["ServisDurumu"]?.ToString() ?? "";

                            Yeni = reader["Yeni"] != DBNull.Value && (bool)reader["Yeni"];
                            Eski = reader["Eski"] != DBNull.Value && (bool)reader["Eski"];
                            TamirGormus = reader["TamirGormus"] != DBNull.Value && (bool)reader["TamirGormus"];
                            Garantili = reader["Garantili"] != DBNull.Value && (bool)reader["Garantili"];
                            Garantisiz = reader["Garantisiz"] != DBNull.Value && (bool)reader["Garantisiz"];
                            ServisGarantili = reader["ServisGarantili"] != DBNull.Value && (bool)reader["ServisGarantili"];
                            YedeklemeYapilsin = reader["YedeklemeYapilsin"] != DBNull.Value && (bool)reader["YedeklemeYapilsin"];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veri yükleme hatası: " + ex.Message);
            }
        }

        private void YukleBayiListesi()
        {
            try
            {
                BayiListesi.Clear();
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();
                    string sorgu = "SELECT DISTINCT AdSoyadUnvan FROM Cariler WHERE SirketID = @SirketID ORDER BY AdSoyadUnvan";
                    SqlCommand cmd = new SqlCommand(sorgu, con);
                    cmd.Parameters.AddWithValue("@SirketID", Class1.AktifKullanici.SirketID);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string bayiAdi = reader["AdSoyadUnvan"]?.ToString() ?? "";
                            if (!string.IsNullOrWhiteSpace(bayiAdi))
                            {
                                BayiListesi.Add(bayiAdi);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bayi listesi yükleme hatası: " + ex.Message);
            }
        }

        private void YukluTekniksijenListesi()
        {
            try
            {
                TekniksijenListesi.Clear();
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();
                    string sorgu = "SELECT DISTINCT FullName FROM Users WHERE CompanyId = @SirketID ORDER BY FullName";
                    SqlCommand cmd = new SqlCommand(sorgu, con);
                    cmd.Parameters.AddWithValue("@SirketID", Class1.AktifKullanici.SirketID);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string teknisyen = reader["FullName"]?.ToString() ?? "";
                            if (!string.IsNullOrWhiteSpace(teknisyen))
                            {
                                TekniksijenListesi.Add(teknisyen);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Teknisyen listesi yükleme hatası: " + ex.Message);
            }
        }

        private async void Kaydet()
        {
            

            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                try
                {
                    con.Open();

                    if (_kayitId <= 0 && _disServisSatirKaynagi)
                    {
                        int bulunanKayitId = DisServisKaydindanKayitIdCoz(con);
                        if (bulunanKayitId > 0)
                        {
                            _kayitId = bulunanKayitId;
                        }
                    }

                    string query;
                    if (_kayitId > 0)
                    {
                        query = @"UPDATE kayitlicihazlar SET 
                            BayiAdi=@BayiAdi, IsimSoyisim=@IsimSoyisim, CepTelefonu=@CepTelefonu, Adres=@Adres, 
                            EPosta=@EPosta, ServisTeknisyen=@ServisTeknisyen, ServisDurumu=@ServisDurumu, 
                            CihazTuru=@CihazTuru, Marka=@Marka, Model=@Model, IMEI=@IMEI, 
                            EkBilgiler=@EkBilgiler, Ariza=@Ariza, FiyatBilgisi=@FiyatBilgisi,
                            Yeni=@Yeni, Eski=@Eski, TamirGormus=@TamirGormus, Garantili=@Garantili,
                            Garantisiz=@Garantisiz, ServisGarantili=@ServisGarantili, YedeklemeYapilsin=@YedeklemeYapilsin
                            WHERE ID=@ID";
                    }
                    else
                    {
                        query = @"INSERT INTO kayitlicihazlar 
                            (KullaniciID, CompanyId, BayiAdi, IsimSoyisim, CepTelefonu, Adres, EPosta, 
                            ServisTeknisyen, ServisDurumu, CihazTuru, Marka, Model, IMEI, 
                            EkBilgiler, Ariza, FiyatBilgisi, Yeni, Eski, TamirGormus, 
                            Garantili, Garantisiz, ServisGarantili, YedeklemeYapilsin) 
                            VALUES 
                            (@KullaniciID, @CompanyId, @BayiAdi, @IsimSoyisim, @CepTelefonu, @Adres, @EPosta, 
                            @ServisTeknisyen, @ServisDurumu, @CihazTuru, @Marka, @Model, @IMEI, 
                            @EkBilgiler, @Ariza, @FiyatBilgisi, @Yeni, @Eski, @TamirGormus, 
                            @Garantili, @Garantisiz, @ServisGarantili, @YedeklemeYapilsin);
                            SELECT CAST(SCOPE_IDENTITY() AS INT);";
                    }

                    SqlCommand cmd = new SqlCommand(query, con);

                    cmd.Parameters.AddWithValue("@BayiAdi", GetSqlValue(BayiAdi, "Bayi Adı"));
                    cmd.Parameters.AddWithValue("@IsimSoyisim", GetSqlValue(AdSoyad, "Ad Soyad*"));
                    cmd.Parameters.AddWithValue("@CepTelefonu", CepTelefonu);
                    cmd.Parameters.AddWithValue("@Adres", GetSqlValue(Adres, "Adres"));
                    cmd.Parameters.AddWithValue("@EPosta", GetSqlValue(EPosta, "E-Posta"));
                    cmd.Parameters.AddWithValue("@ServisTeknisyen", GetSqlValue(IlgiliTeknisyen, "İlgili Teknisyen"));
                    cmd.Parameters.AddWithValue("@ServisDurumu", string.IsNullOrWhiteSpace(ServisDurumu) ? (object)DBNull.Value : ServisDurumu);
                    cmd.Parameters.AddWithValue("@CihazTuru", GetSqlValue(CihazTuru, "Cihaz Türü*"));
                    cmd.Parameters.AddWithValue("@Marka", GetSqlValue(Marka, "Marka*"));
                    cmd.Parameters.AddWithValue("@Model", GetSqlValue(Model, "Model*"));
                    cmd.Parameters.AddWithValue("@IMEI", GetSqlValue(ImeiNo, "IMEI No"));
                    cmd.Parameters.AddWithValue("@EkBilgiler", GetSqlValue(EkBilgiler, "Ek Bilgiler"));
                    cmd.Parameters.AddWithValue("@Ariza", GetSqlValue(SikayetAriza, "Şikayet/Arıza*"));
                    cmd.Parameters.AddWithValue("@FiyatBilgisi", GetSqlValue(FiyatBilgisi, "Fiyat Bilgisi"));
                    cmd.Parameters.AddWithValue("@Yeni", Yeni);
                    cmd.Parameters.AddWithValue("@Eski", Eski);
                    cmd.Parameters.AddWithValue("@TamirGormus", TamirGormus);
                    cmd.Parameters.AddWithValue("@Garantili", Garantili);
                    cmd.Parameters.AddWithValue("@Garantisiz", Garantisiz);
                    cmd.Parameters.AddWithValue("@ServisGarantili", ServisGarantili);
                    cmd.Parameters.AddWithValue("@YedeklemeYapilsin", YedeklemeYapilsin);

                    if (_kayitId > 0)
                    {
                        cmd.Parameters.AddWithValue("@ID", _kayitId);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@KullaniciID", LoginWindow.aktifKullaniciID);
                        cmd.Parameters.AddWithValue("@CompanyId", Class1.AktifKullanici.SirketID);
                    }

                    int etkilenenSatir;

                    if (_kayitId > 0)
                    {
                        etkilenenSatir = cmd.ExecuteNonQuery();
                    }
                    else
                    {
                        object yeniIdObj = cmd.ExecuteScalar();
                        if (yeniIdObj != null && yeniIdObj != DBNull.Value)
                        {
                            _kayitId = Convert.ToInt32(yeniIdObj);
                            etkilenenSatir = 1;
                        }
                        else
                        {
                            etkilenenSatir = 0;
                        }
                    }

                    if (etkilenenSatir == 0)
                    {
                        MessageBox.Show("⚠️ Hiçbir satır güncellenmedi!", "Uyarı");
                        return;
                    }

                    if (_kayitId > 0)
                    {
                        MessageBox.Show("✅ Kayıt başarıyla güncellendi!", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("✅ Yeni kayıt başarıyla eklendi!", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    DegisiklikVar = false;
                    _disServisSatirKaynagi = false;
                    _disServisAdayKayitId = 0;

                    foreach (Window window in Application.Current.Windows)
                    {
                        if (window is Window3 window3)
                        {
                            // Window3'te Page5'i yenile
                            window3.HideOverlay();
                            await System.Threading.Tasks.Task.Delay(120);
                            window3.ShowOverlayPage(new Page5());
                            break;
                        }
                        else if (window is Window5 window5)
                        {
                            window5.VerileriYukle();
                            window5.HideOverlay();
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"❌ HATA:\n\n{ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void GeriDon()
        {
            if (DegisiklikVar)
            {
                var sonuc = MessageBox.Show(
                    "Kaydedilmemiş değişiklikler var. Geri dönmek istiyor musunuz?",
                    "Uyarı",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (sonuc == MessageBoxResult.No)
                    return;
            }

            DegisiklikVar = false;

            foreach (Window window in Application.Current.Windows)
            {
                if (window is Window3 window3)
                {
                    window3.HideOverlay();
                    break;
                }
                else if (window is Window5 window5)
                {
                    window5.HideOverlay();
                    break;
                }
            }
        }

        private void ImeiSorgula()
        {
            if (ImeiNo.Length < 15 || ImeiNo == "IMEI No")
            {
                MessageBox.Show("Geçerli bir IMEI girin!");
                return;
            }
            System.Windows.Clipboard.SetText(ImeiNo);
            Process.Start(new ProcessStartInfo { FileName = "https://www.turkiye.gov.tr/imei-sorgulama", UseShellExecute = true });
        }

        private void ServisDurumuChanged(string durum)
        {
            ServisDurumu = durum;
        }

        private void TelefonFormatla(string telefon)
        {
            if (telefon == "Cep Telefonu*") return;

            string raw = new string(telefon.Where(char.IsDigit).ToArray());

            if (raw.Length > 11) raw = raw.Substring(0, 11);
            if (!raw.StartsWith("0") && raw.Length > 0) raw = "0" + raw;

            string formatted = raw;
            if (raw.Length > 4) formatted = $"({raw.Substring(0, 4)}) {raw.Substring(4)}";
            if (raw.Length > 7) formatted = $"({raw.Substring(0, 4)}) {raw.Substring(4, 3)} {raw.Substring(7)}";
            if (raw.Length > 9) formatted = $"({raw.Substring(0, 4)}) {raw.Substring(4, 3)} {raw.Substring(7, 2)} {raw.Substring(9)}";

            CepTelefonu = formatted;
        }

        private void FisiYazdir()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(AdSoyad) || string.IsNullOrWhiteSpace(Marka) || string.IsNullOrWhiteSpace(Model))
                {
                    MessageBox.Show("Müşteri adı, marka ve model bilgileri zorunludur!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var fisiData = new alpsoftservistakip.Models.ServisKayitFisiData
                {
                    KayitID = _kayitId,
                    KayitTarihi = System.DateTime.Now,
                    IsimSoyisim = AdSoyad,
                    CepTelefonu = CepTelefonu,
                    BayiAdi = BayiAdi,
                    Marka = Marka,
                    Model = Model,
                    Ariza = SikayetAriza,
                    ImeiNo = ImeiNo,
                    ServisDurumu = ServisDurumu,
                    IlgiliTeknisyen = IlgiliTeknisyen,
                    EkBilgiler = EkBilgiler
                };

                var fisi = new alpsoftservistakip.Controls.ServisKayitFisi()
                {
                    DataContext = fisiData
                };

                Services.WpfPrintService.ShowPrintPreview(fisi, "SERVIS_KAYIT_FISI", "SERVIS KAYIT FİŞİ ÖNİZLEMESİ");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fiş yazdırma hatası: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Kopyala()
        {
            try
            {
                string metin = $"AD: {AdSoyad}\n" +
                               $"TELEFON: {CepTelefonu}\n" +
                               $"BAYİ: {BayiAdi}\n" +
                               $"CİHAZ: {Marka} {Model}\n" +
                               $"ARIZA: {SikayetAriza}\n" +
                               $"IMEI: {ImeiNo}";

                System.Windows.Clipboard.SetText(metin);
                MessageBox.Show("Bilgiler panonuza kopyalandı!", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kopyalama hatası: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DisServisGonder()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(AdSoyad) || string.IsNullOrWhiteSpace(Marka) || 
                    string.IsNullOrWhiteSpace(Model) || string.IsNullOrWhiteSpace(SikayetAriza))
                {
                    MessageBox.Show("Lütfen zorunlu alanları doldurunuz:\n- Ad Soyadı\n- Marka\n- Model\n- Arıza Detayı", 
                        "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_kayitId <= 0)
                {
                    MessageBox.Show("Lütfen önce cihaz kaydını 'KAYDET' butonu ile kaydediniz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                GoruntuDısServisGonderDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Dış servis gönderme hatası: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GoruntuDısServisGonderDialog()
        {
            var pencere = new Window
            {
                Title = "Dış Servis'e Gönder",
                Width = 400,
                Height = 350,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize,
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(18, 18, 18))
            };

            var stackPanel = new StackPanel { Margin = new Thickness(20) };

            var label = new TextBlock
            {
                Text = "Dış Servis Bilgileri",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = System.Windows.Media.Brushes.White,
                Margin = new Thickness(0, 0, 0, 20)
            };
            stackPanel.Children.Add(label);

            stackPanel.Children.Add(new TextBlock { Text = "Dış Servis Adı / Yeri:", Foreground = System.Windows.Media.Brushes.LightGray, Margin = new Thickness(0,0,0,5) });
            var txtYer = new TextBox { Height = 35, Margin = new Thickness(0,0,0,15), Background=new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(45,45,45)), Foreground=System.Windows.Media.Brushes.White };
            stackPanel.Children.Add(txtYer);

            stackPanel.Children.Add(new TextBlock { Text = "İlgili Kişi:", Foreground = System.Windows.Media.Brushes.LightGray, Margin = new Thickness(0,0,0,5) });
            var txtKisi = new TextBox { Height = 35, Margin = new Thickness(0,0,0,25), Background=new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(45,45,45)), Foreground=System.Windows.Media.Brushes.White  };
            stackPanel.Children.Add(txtKisi);

            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };

            var btnIptal = new Button { Content = "İptal", Width = 90, Height = 35, Background = System.Windows.Media.Brushes.Gray, Foreground = System.Windows.Media.Brushes.White, Margin = new Thickness(0,0,10,0) };
            btnIptal.Click += (s, e) => pencere.Close();

            var btnGonder = new Button { Content = "Gönder", Width = 90, Height = 35, Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(52, 152, 219)), Foreground = System.Windows.Media.Brushes.White };
            btnGonder.Click += (s, e) => 
            {
                if (string.IsNullOrWhiteSpace(txtYer.Text))
                {
                    MessageBox.Show("Lütfen Dış Servis Adı alanını doldurun.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                TaşıDısServisKayıtlarına(txtYer.Text, txtKisi.Text);
                pencere.Close();
            };

            buttonPanel.Children.Add(btnIptal);
            buttonPanel.Children.Add(btnGonder);
            stackPanel.Children.Add(buttonPanel);

            pencere.Content = stackPanel;
            pencere.ShowDialog();
        }

        private void TaşıDısServisKayıtlarına(string disServisYeri, string ilgiliKisi)
        {
            try
            {
                if (_kayitId <= 0)
                    return;

                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    var kolonlar = GetDisServisKolonBilgileri(con);
                    var eklenecekKolonlar = new List<string>();
                    var eklenecekParametreler = new List<string>();
                    var ekCmd = new SqlCommand();
                    ekCmd.Connection = con;

                    void AlanEkle(string kolon, string parametre, object deger)
                    {
                        if (string.IsNullOrWhiteSpace(kolon)) return;
                        eklenecekKolonlar.Add($"[{kolon}]");
                        eklenecekParametreler.Add(parametre);
                        ekCmd.Parameters.AddWithValue(parametre, deger ?? (object)DBNull.Value);
                    }

                    AlanEkle(KolonBul(kolonlar, "KullaniciID", "KullaniciId", "UserId"), "@KullaniciID", LoginWindow.aktifKullaniciID);
                    AlanEkle(KolonBul(kolonlar, "KayitTarihi", "Tarih", "CreatedDate"), "@KayitTarihi", DateTime.Now);
                    AlanEkle(KolonBul(kolonlar, "KayitID", "KayitId", "ServisKayitID", "ServisKayitId"), "@KayitID", _kayitId);
                    AlanEkle(KolonBul(kolonlar, "IsimSoyisim", "MusteriAdi", "MusteriAdSoyad"), "@IsimSoyisim", AdSoyad ?? "");
                    AlanEkle(KolonBul(kolonlar, "CepTelefonu", "Telefon", "TelefonNo", "Gsm"), "@CepTelefonu", CepTelefonu ?? "");
                    AlanEkle(KolonBul(kolonlar, "YetkiliBayi", "BayiAdi", "DisServisYeri"), "@YetkiliBayi", disServisYeri ?? "");
                    AlanEkle(KolonBul(kolonlar, "IrtibatKisi", "IlgiliKisi", "YetkiliKisi"), "@IrtibatKisi", ilgiliKisi ?? "");
                    AlanEkle(KolonBul(kolonlar, "Marka"), "@Marka", Marka ?? "");
                    AlanEkle(KolonBul(kolonlar, "Model"), "@Model", Model ?? "");
                    AlanEkle(KolonBul(kolonlar, "IMEI", "ImeiNo", "SeriNo"), "@IMEI", ImeiNo ?? "");
                    AlanEkle(KolonBul(kolonlar, "Ariza", "ArizaDetayi", "Sikayet"), "@Ariza", SikayetAriza ?? "");
                    AlanEkle(KolonBul(kolonlar, "EkBilgiler", "Notlar", "Aciklama"), "@EkBilgiler", EkBilgiler ?? "");
                    AlanEkle(KolonBul(kolonlar, "ServisDurumu", "Durum"), "@ServisDurumu", ServisDurumu ?? "Dış Servis'te");
                    AlanEkle(KolonBul(kolonlar, "ServisTeknisyen", "Teknisyen", "SorumluTeknisyen"), "@ServisTeknisyen", IlgiliTeknisyen ?? "");

                    if (eklenecekKolonlar.Count == 0)
                    {
                        MessageBox.Show("Dış servis tablosunda eşleşen kolon bulunamadı.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    ekCmd.CommandText = $"INSERT INTO disserviskayitlarnew ({string.Join(", ", eklenecekKolonlar)}) VALUES ({string.Join(", ", eklenecekParametreler)})";
                    ekCmd.ExecuteNonQuery();

                    MessageBox.Show("Kayıt başarıyla Dış Servis Kayıtlarına aktarıldı.", "Başarılı", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Dış servis aktarma hatası: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Dictionary<string, bool> GetDisServisKolonBilgileri(SqlConnection con)
        {
            var kolonlar = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

            string sorgu = @"SELECT c.COLUMN_NAME,
                                    COLUMNPROPERTY(OBJECT_ID(c.TABLE_SCHEMA + '.' + c.TABLE_NAME), c.COLUMN_NAME, 'IsIdentity') AS IsIdentity
                             FROM INFORMATION_SCHEMA.COLUMNS c
                             WHERE c.TABLE_NAME = 'disserviskayitlarnew'";
            using (SqlCommand cmd = new SqlCommand(sorgu, con))
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    string kolonAdi = reader["COLUMN_NAME"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(kolonAdi))
                    {
                        bool isIdentity = reader["IsIdentity"] != DBNull.Value && Convert.ToInt32(reader["IsIdentity"]) == 1;
                        kolonlar[kolonAdi] = isIdentity;
                    }
                }
            }

            return kolonlar;
        }

        private string KolonBul(Dictionary<string, bool> mevcutKolonlar, params string[] adaylar)
        {
            foreach (var aday in adaylar)
            {
                if (mevcutKolonlar.TryGetValue(aday, out bool isIdentity) && !isIdentity)
                {
                    return aday;
                }
            }

            return null;
        }

        private int DisServisKaydindanKayitIdCoz(SqlConnection con)
        {
            string adSoyad = Temizle(AdSoyad);
            string marka = Temizle(Marka);
            string model = Temizle(Model);
            string imei = Temizle(ImeiNo);
            string cep = Temizle(CepTelefonu);

            if (_disServisAdayKayitId > 0)
            {
                string idSorgu = @"SELECT TOP 1 ID FROM kayitlicihazlar
                                   WHERE CompanyId = @CompanyId
                                   AND ID = @ID
                                   AND (@IsimSoyisim = '' OR IsimSoyisim = @IsimSoyisim)
                                   AND (@Marka = '' OR Marka = @Marka)
                                   AND (@Model = '' OR Model = @Model)
                                   AND (@IMEI = '' OR IMEI = @IMEI)";

                using (SqlCommand cmd = new SqlCommand(idSorgu, con))
                {
                    cmd.Parameters.AddWithValue("@CompanyId", Class1.AktifKullanici.SirketID);
                    cmd.Parameters.AddWithValue("@ID", _disServisAdayKayitId);
                    cmd.Parameters.AddWithValue("@IsimSoyisim", adSoyad);
                    cmd.Parameters.AddWithValue("@Marka", marka);
                    cmd.Parameters.AddWithValue("@Model", model);
                    cmd.Parameters.AddWithValue("@IMEI", imei);

                    object sonuc = cmd.ExecuteScalar();
                    if (sonuc != null && sonuc != DBNull.Value)
                    {
                        return Convert.ToInt32(sonuc);
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(adSoyad) || string.IsNullOrWhiteSpace(marka) || string.IsNullOrWhiteSpace(model))
            {
                return 0;
            }

            string eslesmeSorgu = @"SELECT TOP 1 ID FROM kayitlicihazlar
                                    WHERE CompanyId = @CompanyId
                                    AND IsimSoyisim = @IsimSoyisim
                                    AND Marka = @Marka
                                    AND Model = @Model
                                    AND (@IMEI = '' OR IMEI = @IMEI)
                                    AND (@CepTelefonu = '' OR CepTelefonu = @CepTelefonu)
                                    ORDER BY ID DESC";

            using (SqlCommand cmd = new SqlCommand(eslesmeSorgu, con))
            {
                cmd.Parameters.AddWithValue("@CompanyId", Class1.AktifKullanici.SirketID);
                cmd.Parameters.AddWithValue("@IsimSoyisim", adSoyad);
                cmd.Parameters.AddWithValue("@Marka", marka);
                cmd.Parameters.AddWithValue("@Model", model);
                cmd.Parameters.AddWithValue("@IMEI", imei);
                cmd.Parameters.AddWithValue("@CepTelefonu", cep);

                object sonuc = cmd.ExecuteScalar();
                if (sonuc != null && sonuc != DBNull.Value)
                {
                    return Convert.ToInt32(sonuc);
                }
            }

            return 0;
        }

        private string Temizle(string text)
        {
            return string.IsNullOrWhiteSpace(text) ? "" : text.Trim();
        }

        private object GetSqlValue(string value, string placeholder)
        {
            return (value == placeholder || string.IsNullOrWhiteSpace(value))
                ? (object)DBNull.Value
                : (object)value;
        }

        private string GetRowString(DataRowView row, params string[] kolonlar)
        {
            foreach (var kolon in kolonlar)
            {
                if (row.DataView.Table.Columns.Contains(kolon) && row[kolon] != DBNull.Value)
                {
                    return row[kolon]?.ToString() ?? "";
                }
            }

            return "";
        }

        private int GetRowInt(DataRowView row, params string[] kolonlar)
        {
            foreach (var kolon in kolonlar)
            {
                if (row.DataView.Table.Columns.Contains(kolon) && row[kolon] != DBNull.Value)
                {
                    if (int.TryParse(row[kolon].ToString(), out int id) && id > 0)
                    {
                        return id;
                    }
                }
            }

            return 0;
        }
    }
}

