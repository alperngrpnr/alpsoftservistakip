using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.ComponentModel;

namespace alpsoftservistakip
{
    public partial class Window9 : Window
    {
        private bool _degisiklikVar = false;
        private int _kayitId;
        private bool _yuklemeDevamEdiyor = true; // ✅ EKLEME

        private const string ConnectionString = "Server=91.247.168.204,1433;Database=alpsoftservistakip;User Id=sa;Password=Alperengurpinar4160552009;TrustServerCertificate=True;";

        public Window9()
        {
            InitializeComponent();
            _kayitId = 0;
            this.Title = "Yeni Kayıt Ekleme Sayfası";

            _yuklemeDevamEdiyor = false; // ✅ Yükleme bitti
            DegisiklikEventleriniBagla();
        }
        private void AltPencere_Closing(object sender, CancelEventArgs e)
        {
            // Hangi pencere olursa olsun, çarpıya basıldığında tüm uygulamayı kapatır
            Application.Current.Shutdown();
        }


        public Window9(int id)
        {
            InitializeComponent();
            _kayitId = id;
            this.Title = $"Kayıt Detay Sayfası - ID: {_kayitId}";

            _yuklemeDevamEdiyor = true; // ✅ Yükleme başladı
            LoadRecordDetails(_kayitId);
            _yuklemeDevamEdiyor = false; // ✅ Yükleme bitti

            DegisiklikEventleriniBagla();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (_degisiklikVar)
            {
                var sonuc = MessageBox.Show(
                    "Kaydedilmemiş değişiklikler var. Çıkmak istiyor musunuz?",
                    "Uyarı",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (sonuc == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }

            Window5TablosunuYenile();
        }

        private void DegisiklikEventleriniBagla()
        {
            // TextBox'lar
            adsoyad_skayit.TextChanged += TextBox_DegisiklikYapildi;
            bayiadi_skayit.TextChanged += TextBox_DegisiklikYapildi;
            ceptelefonu_skayit.TextChanged += TextBox_DegisiklikYapildi;
            adres_skayit.TextChanged += TextBox_DegisiklikYapildi;
            eposta_skayit.TextChanged += TextBox_DegisiklikYapildi;
            cihaztürü_skayit.TextChanged += TextBox_DegisiklikYapildi;
            marka_skayit.TextChanged += TextBox_DegisiklikYapildi;
            model_skayit.TextChanged += TextBox_DegisiklikYapildi;
            imeino_skayit.TextChanged += TextBox_DegisiklikYapildi;
            ekbilgiler_skayit.TextChanged += TextBox_DegisiklikYapildi;
            sikayetiariza_skayit.TextChanged += TextBox_DegisiklikYapildi;
            fiyatbilgisi_skayit1.TextChanged += TextBox_DegisiklikYapildi;
            ilgiliteknisyen1.TextChanged += TextBox_DegisiklikYapildi;

            // ComboBox
            servisdurumu.SelectionChanged += (s, e) =>
            {
                if (!_yuklemeDevamEdiyor) _degisiklikVar = true;
            };

            // CheckBox'lar
            yeni.Checked += CheckBox_DegisiklikYapildi;
            yeni.Unchecked += CheckBox_DegisiklikYapildi;
            eski.Checked += CheckBox_DegisiklikYapildi;
            eski.Unchecked += CheckBox_DegisiklikYapildi;
            tamirgörmüs.Checked += CheckBox_DegisiklikYapildi;
            tamirgörmüs.Unchecked += CheckBox_DegisiklikYapildi;
            garantili.Checked += CheckBox_DegisiklikYapildi;
            garantili.Unchecked += CheckBox_DegisiklikYapildi;
            garantisiz.Checked += CheckBox_DegisiklikYapildi;
            garantisiz.Unchecked += CheckBox_DegisiklikYapildi;
            servisgarantili.Checked += CheckBox_DegisiklikYapildi;
            servisgarantili.Unchecked += CheckBox_DegisiklikYapildi;
            yedeklemeyapilsin.Checked += CheckBox_DegisiklikYapildi;
            yedeklemeyapilsin.Unchecked += CheckBox_DegisiklikYapildi;
        }

        private void TextBox_DegisiklikYapildi(object sender, TextChangedEventArgs e)
        {
            if (_yuklemeDevamEdiyor) return; // ✅ Yükleme sırasında değişiklik sayma

            var tb = sender as TextBox;
            if (tb.Foreground == Brushes.Black)
            {
                _degisiklikVar = true;
            }
        }

        private void CheckBox_DegisiklikYapildi(object sender, RoutedEventArgs e)
        {
            if (_yuklemeDevamEdiyor) return; // ✅ Yükleme sırasında değişiklik sayma
            _degisiklikVar = true;
        }

        // --- YARDIMCI METOTLAR ---

        private object GetSqlValue(TextBox textBox, string placeholder)
        {
            return (textBox.Text == placeholder || string.IsNullOrWhiteSpace(textBox.Text))
                ? (object)DBNull.Value
                : (object)textBox.Text;
        }

        // --- DETAY YÜKLEME METODU ---

        private void LoadRecordDetails(int id)
        {
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                try
                {
                    con.Open();
                    string sorgu = "SELECT * FROM kayitlicihazlar WHERE ID = @ID";
                    SqlCommand cmd = new SqlCommand(sorgu, con);
                    cmd.Parameters.AddWithValue("@ID", id);

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        SetText(adsoyad_skayit, reader["IsimSoyisim"], "Ad Soyad*");
                        SetText(bayiadi_skayit, reader["BayiAdi"], "Bayi Adı");
                        SetText(ceptelefonu_skayit, reader["CepTelefonu"], "Cep Telefonu*");
                        SetText(adres_skayit, reader["Adres"], "Adres");
                        SetText(eposta_skayit, reader["EPosta"], "E-Posta");
                        SetText(cihaztürü_skayit, reader["CihazTuru"], "Cihaz Türü*");
                        SetText(marka_skayit, reader["Marka"], "Marka*");
                        SetText(model_skayit, reader["Model"], "Model*");
                        SetText(imeino_skayit, reader["IMEI"], "IMEI No");
                        SetText(ekbilgiler_skayit, reader["EkBilgiler"], "Ek Bilgiler");
                        SetText(sikayetiariza_skayit, reader["Ariza"], "Şikayet/Arıza*");
                        SetText(fiyatbilgisi_skayit1, reader["FiyatBilgisi"], "Fiyat Bilgisi");
                        SetText(ilgiliteknisyen1, reader["ServisTeknisyen"], "İlgili Teknisyen");

                        servisdurumtext.Text = reader["ServisDurumu"]?.ToString();

                        yeni.IsChecked = reader["Yeni"] != DBNull.Value && (bool)reader["Yeni"];
                        eski.IsChecked = reader["Eski"] != DBNull.Value && (bool)reader["Eski"];
                        tamirgörmüs.IsChecked = reader["TamirGormus"] != DBNull.Value && (bool)reader["TamirGormus"];
                        garantili.IsChecked = reader["Garantili"] != DBNull.Value && (bool)reader["Garantili"];
                        garantisiz.IsChecked = reader["Garantisiz"] != DBNull.Value && (bool)reader["Garantisiz"];
                        servisgarantili.IsChecked = reader["ServisGarantili"] != DBNull.Value && (bool)reader["ServisGarantili"];
                        yedeklemeyapilsin.IsChecked = reader["YedeklemeYapilsin"] != DBNull.Value && (bool)reader["YedeklemeYapilsin"];
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Hata: " + ex.Message);
                }
            }
        }

        private void SetText(TextBox tb, object value, string placeholder)
        {
            string val = value?.ToString();
            if (string.IsNullOrEmpty(val))
            {
                tb.Text = placeholder;
                tb.Foreground = Brushes.Gray;
            }
            else
            {
                tb.Text = val;
                tb.Foreground = Brushes.Black;
            }
        }

        // --- FOCUS/LOSTFOCUS METOTLARI (aynı kalacak) ---

        private void adsoyad_skayit_GotFocus(object sender, RoutedEventArgs e)
        {
            if (adsoyad_skayit.Text == "Ad Soyad*")
            {
                adsoyad_skayit.Text = "";
                adsoyad_skayit.Foreground = Brushes.Black;
            }
        }
        private void adsoyad_skayit_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(adsoyad_skayit.Text))
            {
                adsoyad_skayit.Text = "Ad Soyad*";
                adsoyad_skayit.Foreground = Brushes.Gray;
            }
        }

        private void bayiadi_skayit_GotFocus(object sender, RoutedEventArgs e)
        {
            if (bayiadi_skayit.Text == "Bayi Adı")
            {
                bayiadi_skayit.Text = "";
                bayiadi_skayit.Foreground = Brushes.Black;
            }
        }
        private void bayiadi_skayit_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(bayiadi_skayit.Text))
            {
                bayiadi_skayit.Text = "Bayi Adı";
                bayiadi_skayit.Foreground = Brushes.Gray;
            }
        }

        private void ceptelefonu_skayit_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ceptelefonu_skayit.Text == "Cep Telefonu*")
            {
                ceptelefonu_skayit.Text = "";
                ceptelefonu_skayit.Foreground = Brushes.Black;
            }
        }
        private void ceptelefonu_skayit_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ceptelefonu_skayit.Text))
            {
                ceptelefonu_skayit.Text = "Cep Telefonu*";
                ceptelefonu_skayit.Foreground = Brushes.Gray;
            }
        }

        private void adres_skayit_GotFocus(object sender, RoutedEventArgs e)
        {
            if (adres_skayit.Text == "Adres")
            {
                adres_skayit.Text = "";
                adres_skayit.Foreground = Brushes.Black;
            }
        }
        private void adres_skayit_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(adres_skayit.Text))
            {
                adres_skayit.Text = "Adres";
                adres_skayit.Foreground = Brushes.Gray;
            }
        }

        private void eposta_skayit_GotFocus(object sender, RoutedEventArgs e)
        {
            if (eposta_skayit.Text == "E-Posta")
            {
                eposta_skayit.Text = "";
                eposta_skayit.Foreground = Brushes.Black;
            }
        }
        private void eposta_skayit_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(eposta_skayit.Text))
            {
                eposta_skayit.Text = "E-Posta";
                eposta_skayit.Foreground = Brushes.Gray;
            }
        }

        private void cihaztürü_skayit_GotFocus(object sender, RoutedEventArgs e)
        {
            if (cihaztürü_skayit.Text == "Cihaz Türü*")
            {
                cihaztürü_skayit.Text = "";
                cihaztürü_skayit.Foreground = Brushes.Black;
            }
        }
        private void cihaztürü_skayit_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(cihaztürü_skayit.Text))
            {
                cihaztürü_skayit.Text = "Cihaz Türü*";
                cihaztürü_skayit.Foreground = Brushes.Gray;
            }
        }

        private void marka_skayit_GotFocus(object sender, RoutedEventArgs e)
        {
            if (marka_skayit.Text == "Marka*")
            {
                marka_skayit.Text = "";
                marka_skayit.Foreground = Brushes.Black;
            }
        }
        private void marka_skayit_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(marka_skayit.Text))
            {
                marka_skayit.Text = "Marka*";
                marka_skayit.Foreground = Brushes.Gray;
            }
        }

        private void model_skayit_GotFocus(object sender, RoutedEventArgs e)
        {
            if (model_skayit.Text == "Model*")
            {
                model_skayit.Text = "";
                model_skayit.Foreground = Brushes.Black;
            }
        }
        private void model_skayit_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(model_skayit.Text))
            {
                model_skayit.Text = "Model*";
                model_skayit.Foreground = Brushes.Gray;
            }
        }

        private void imeino_skayit_GotFocus(object sender, RoutedEventArgs e)
        {
            if (imeino_skayit.Text == "IMEI No")
            {
                imeino_skayit.Text = "";
                imeino_skayit.Foreground = Brushes.Black;
            }
        }
        private void imeino_skayit_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(imeino_skayit.Text))
            {
                imeino_skayit.Text = "IMEI No";
                imeino_skayit.Foreground = Brushes.Gray;
            }
        }

        private void ekbilgiler_skayit_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ekbilgiler_skayit.Text == "Ek Bilgiler")
            {
                ekbilgiler_skayit.Text = "";
                ekbilgiler_skayit.Foreground = Brushes.Black;
            }
        }
        private void ekbilgiler_skayit_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ekbilgiler_skayit.Text))
            {
                ekbilgiler_skayit.Text = "Ek Bilgiler";
                ekbilgiler_skayit.Foreground = Brushes.Gray;
            }
        }

        private void ilgiliteknisyen1_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ilgiliteknisyen1.Text == "İlgili Teknisyen")
            {
                ilgiliteknisyen1.Text = "";
                ilgiliteknisyen1.Foreground = Brushes.Black;
            }
        }
        private void ilgiliteknisyen1_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ilgiliteknisyen1.Text))
            {
                ilgiliteknisyen1.Text = "İlgili Teknisyen";
                ilgiliteknisyen1.Foreground = Brushes.Gray;
            }
        }

        private void sikayetiariza_skayit_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sikayetiariza_skayit.Text == "Şikayet/Arıza*")
            {
                sikayetiariza_skayit.Text = "";
                sikayetiariza_skayit.Foreground = Brushes.Black;
            }
        }
        private void sikayetiariza_skayit_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(sikayetiariza_skayit.Text))
            {
                sikayetiariza_skayit.Text = "Şikayet/Arıza*";
                sikayetiariza_skayit.Foreground = Brushes.Gray;
            }
        }

        private void fiyatbilgisi_skayit1_GotFocus(object sender, RoutedEventArgs e)
        {
            if (fiyatbilgisi_skayit1.Text == "Fiyat Bilgisi")
            {
                fiyatbilgisi_skayit1.Text = "";
                fiyatbilgisi_skayit1.Foreground = Brushes.Black;
            }
        }
        private void fiyatbilgisi_skayit1_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(fiyatbilgisi_skayit1.Text))
            {
                fiyatbilgisi_skayit1.Text = "Fiyat Bilgisi";
                fiyatbilgisi_skayit1.Foreground = Brushes.Gray;
            }
        }

        private void Window5TablosunuYenile()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is Window5 window5)
                {
                    if (!window5.IsVisible)
                    {
                        window5.Show();
                    }
                    window5.VerileriYukle();
                    window5.Activate();
                    break;
                }
            }
        }

        // --- KAYIT METODU ---

        private void sarviskaydınıkaydet_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(adsoyad_skayit.Text) ||
                adsoyad_skayit.Text == "Ad Soyad*" ||
                string.IsNullOrWhiteSpace(ceptelefonu_skayit.Text) ||
                ceptelefonu_skayit.Text == "Cep Telefonu*")
            {
                MessageBox.Show("Lütfen yıldızlı alanları doldurun!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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

                    cmd.Parameters.AddWithValue("@BayiAdi", GetSqlValue(bayiadi_skayit, "Bayi Adı"));
                    cmd.Parameters.AddWithValue("@IsimSoyisim", GetSqlValue(adsoyad_skayit, "Ad Soyad*"));
                    cmd.Parameters.AddWithValue("@CepTelefonu", ceptelefonu_skayit.Text);
                    cmd.Parameters.AddWithValue("@Adres", GetSqlValue(adres_skayit, "Adres"));
                    cmd.Parameters.AddWithValue("@EPosta", GetSqlValue(eposta_skayit, "E-Posta"));
                    cmd.Parameters.AddWithValue("@ServisTeknisyen", GetSqlValue(ilgiliteknisyen1, "İlgili Teknisyen"));
                    cmd.Parameters.AddWithValue("@ServisDurumu", string.IsNullOrWhiteSpace(servisdurumtext.Text) ? (object)DBNull.Value : servisdurumtext.Text);
                    cmd.Parameters.AddWithValue("@CihazTuru", GetSqlValue(cihaztürü_skayit, "Cihaz Türü*"));
                    cmd.Parameters.AddWithValue("@Marka", GetSqlValue(marka_skayit, "Marka*"));
                    cmd.Parameters.AddWithValue("@Model", GetSqlValue(model_skayit, "Model*"));
                    cmd.Parameters.AddWithValue("@IMEI", GetSqlValue(imeino_skayit, "IMEI No"));
                    cmd.Parameters.AddWithValue("@EkBilgiler", GetSqlValue(ekbilgiler_skayit, "Ek Bilgiler"));
                    cmd.Parameters.AddWithValue("@Ariza", GetSqlValue(sikayetiariza_skayit, "Şikayet/Arıza*"));
                    cmd.Parameters.AddWithValue("@FiyatBilgisi", GetSqlValue(fiyatbilgisi_skayit1, "Fiyat Bilgisi"));
                    cmd.Parameters.AddWithValue("@Yeni", yeni.IsChecked ?? false);
                    cmd.Parameters.AddWithValue("@Eski", eski.IsChecked ?? false);
                    cmd.Parameters.AddWithValue("@TamirGormus", tamirgörmüs.IsChecked ?? false);
                    cmd.Parameters.AddWithValue("@Garantili", garantili.IsChecked ?? false);
                    cmd.Parameters.AddWithValue("@Garantisiz", garantisiz.IsChecked ?? false);
                    cmd.Parameters.AddWithValue("@ServisGarantili", servisgarantili.IsChecked ?? false);
                    cmd.Parameters.AddWithValue("@YedeklemeYapilsin", yedeklemeyapilsin.IsChecked ?? false);

                    if (_kayitId == 0)
                    {
                        cmd.Parameters.AddWithValue("@KullaniciID", LoginWindow.aktifKullaniciID);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@ID", _kayitId);
                    }

                    int etkilenenSatir = cmd.ExecuteNonQuery();

                    if (etkilenenSatir == 0)
                    {
                        MessageBox.Show($"⚠️ Hiçbir satır güncellenmedi!\n\nID: {_kayitId}", "Uyarı");
                        return;
                    }

                    string mesaj = _kayitId > 0 ? "✅ Kayıt başarıyla güncellendi!" : "✅ Yeni kayıt başarıyla eklendi!";
                    MessageBox.Show(mesaj, "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);

                    _degisiklikVar = false; // ✅ Kaydettikten sonra sıfırla
                    Window5TablosunuYenile();
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"❌ HATA:\n\n{ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // --- GERİ DÖN BUTONU ---

        private void sarviskaydınıkaydet_Kopyala_Click(object sender, RoutedEventArgs e)
        {
            if (_degisiklikVar)
            {
                var sonuc = MessageBox.Show(
                    "Kaydedilmemiş değişiklikler var. Geri dönmek istiyor musunuz?",
                    "Uyarı",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (sonuc == MessageBoxResult.No)
                    return;
            }

            _degisiklikVar = false;
            this.Close();
        }

        // --- DİĞER METOTLAR (TELEFON, IMEI vs.) ---

        private bool TelefonGecerliMi(string telefon)
        {
            string pattern = @"^\(0\d{3}\)\s\d{3}\s\d{2}\s\d{2}$";
            return Regex.IsMatch(telefon, pattern);
        }

        private void ceptelefonu_skayit_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text, 0);
        }

        private void ceptelefonu_skayit_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ceptelefonu_skayit.Text == "Cep Telefonu*") return;

            ceptelefonu_skayit.TextChanged -= ceptelefonu_skayit_TextChanged;
            string raw = new string(ceptelefonu_skayit.Text.Where(char.IsDigit).ToArray());

            if (raw.Length > 11) raw = raw.Substring(0, 11);
            if (!raw.StartsWith("0") && raw.Length > 0) raw = "0" + raw;

            string formatted = raw;
            if (raw.Length > 4) formatted = $"({raw.Substring(0, 4)}) {raw.Substring(4)}";
            if (raw.Length > 7) formatted = $"({raw.Substring(0, 4)}) {raw.Substring(4, 3)} {raw.Substring(7)}";
            if (raw.Length > 9) formatted = $"({raw.Substring(0, 4)}) {raw.Substring(4, 3)} {raw.Substring(7, 2)} {raw.Substring(9)}";

            ceptelefonu_skayit.Text = formatted;
            ceptelefonu_skayit.SelectionStart = formatted.Length;
            ceptelefonu_skayit.TextChanged += ceptelefonu_skayit_TextChanged;
        }

        private string TelefonFormatla(string input)
        {
            if (input.Length <= 4)
                return input;

            if (input.Length <= 7)
                return $"({input.Substring(0, 4)}) {input.Substring(4)}";

            if (input.Length <= 9)
                return $"({input.Substring(0, 4)}) {input.Substring(4, 3)} {input.Substring(7)}";

            return $"({input.Substring(0, 4)}) {input.Substring(4, 3)} {input.Substring(7, 2)} {input.Substring(9)}";
        }

        private void servisdurumu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (servisdurumu.SelectedItem is ComboBoxItem item)
            {
                servisdurumtext.Text = item.Content.ToString();
            }
        }

        private void imeino_skayit_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text, 0);
        }

        private void imeino_skayit_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (imeino_skayit.Text.Length > 15)
            {
                imeino_skayit.Text = imeino_skayit.Text.Substring(0, 15);
                imeino_skayit.CaretIndex = 15;
            }
        }

        private void btksorgula_Click(object sender, RoutedEventArgs e)
        {
            if (imeino_skayit.Text.Length < 15 || imeino_skayit.Text == "IMEI No")
            {
                MessageBox.Show("Geçerli bir IMEI girin!");
                return;
            }
            Clipboard.SetText(imeino_skayit.Text);
            Process.Start(new ProcessStartInfo { FileName = "https://www.turkiye.gov.tr/imei-sorgulama", UseShellExecute = true });
        }

        private void Imeino_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;

            if (!Regex.IsMatch(e.Text, @"^\d+$"))
            {
                e.Handled = true;
                return;
            }

            if (tb.Text.Length >= 15)
            {
                e.Handled = true;
                return;
            }
        }

        private void Imeino_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                e.Handled = true;
            }
        }

        private void Imeino_Loaded(object sender, RoutedEventArgs e)
        {
            DataObject.AddPastingHandler(imeino_skayit, OnImeinoPaste);
        }

        private void OnImeinoPaste(object sender, DataObjectPastingEventArgs e)
        {
            if (!e.DataObject.GetDataPresent(DataFormats.Text))
            {
                e.CancelCommand();
                return;
            }

            string pasteText = e.DataObject.GetData(DataFormats.Text).ToString();

            if (!Regex.IsMatch(pasteText, @"^\d+$"))
            {
                e.CancelCommand();
                return;
            }

            TextBox tb = sender as TextBox;

            if ((tb.Text.Length + pasteText.Length) > 15)
            {
                e.CancelCommand();
            }
        }

        private void Imeino_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = sender as TextBox;

            if (tb.Text.Length > 15)
            {
                tb.Text = tb.Text.Substring(0, 15);
                tb.CaretIndex = tb.Text.Length;
            }
        }
    }
}