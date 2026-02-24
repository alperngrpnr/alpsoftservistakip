using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using System.Threading.Tasks;
using alpsoftservistakip;

namespace alpsoftservistakip.ViewModels
{
    public class PageKayitOlusturViewModel : BaseViewModel
    {
        private const string ConnectionString = "Server=91.247.168.204,1433;Database=alpsoftservistakip;User Id=sa;Password=Alperengurpinar4160552009;TrustServerCertificate=True;";

        private bool _degisiklikVar = false;
        private int _kayitId;
        private bool _yuklemeDevamEdiyor = false;

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

        public ICommand KaydetCommand { get; }
        public ICommand GeriDonCommand { get; }
        public ICommand ImeiSorgulaCommand { get; }
        public ICommand ServisDurumuChangedCommand { get; }
        public ICommand TelefonFormatlaCommand { get; }

        public PageKayitOlusturViewModel(int kayitId = 0)
        {
            _kayitId = kayitId;
            KaydetCommand = new RelayCommand(_ => Kaydet());
            GeriDonCommand = new RelayCommand(_ => GeriDon());
            ImeiSorgulaCommand = new RelayCommand(_ => ImeiSorgula());
            ServisDurumuChangedCommand = new RelayCommand<string>(ServisDurumuChanged);
            TelefonFormatlaCommand = new RelayCommand<string>(TelefonFormatla);
            
            if (kayitId > 0)
            {
                YuklemeDevamEdiyor = true;
                VerileriYukle(kayitId);
                YuklemeDevamEdiyor = false;
            }
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

        private async void Kaydet()
        {
            

            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                try
                {
                    con.Open();

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
                            (KullaniciID, BayiAdi, IsimSoyisim, CepTelefonu, Adres, EPosta, 
                            ServisTeknisyen, ServisDurumu, CihazTuru, Marka, Model, IMEI, 
                            EkBilgiler, Ariza, FiyatBilgisi, Yeni, Eski, TamirGormus, 
                            Garantili, Garantisiz, ServisGarantili, YedeklemeYapilsin) 
                            VALUES 
                            (@KullaniciID, @BayiAdi, @IsimSoyisim, @CepTelefonu, @Adres, @EPosta, 
                            @ServisTeknisyen, @ServisDurumu, @CihazTuru, @Marka, @Model, @IMEI, 
                            @EkBilgiler, @Ariza, @FiyatBilgisi, @Yeni, @Eski, @TamirGormus, 
                            @Garantili, @Garantisiz, @ServisGarantili, @YedeklemeYapilsin)";
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
                    }

                    int etkilenenSatir = cmd.ExecuteNonQuery();

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

        private object GetSqlValue(string value, string placeholder)
        {
            return (value == placeholder || string.IsNullOrWhiteSpace(value))
                ? (object)DBNull.Value
                : (object)value;
        }
    }
}

