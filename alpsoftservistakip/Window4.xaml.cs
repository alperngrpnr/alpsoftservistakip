using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace alpsoftservistakip
{
    /// <summary>
    /// Window4.xaml etkileşim mantığı
    /// </summary>
    public partial class Window4 : Window
    {
        private bool _degisiklikVar = false;

        private int _kayitId;

        public Window4()
        {
            InitializeComponent();
        }
        
        private void FormDegisti(object sender, EventArgs e)
        {
            _degisiklikVar = true;
        }


        
        private Window _oncekiPencere;

        private void AltPencere_Closing(object sender, CancelEventArgs e)
        {
            // Hangi pencere olursa olsun, çarpıya basıldığında tüm uygulamayı kapatır
            Application.Current.Shutdown();
        }

        public Window4(int id, Window oncekiPencere)
        {
            InitializeComponent();
            _kayitId = id;
            this.Title = $"Kayıt Detay Sayfası - ID: {_kayitId}";
            _oncekiPencere = oncekiPencere;

            LoadRecordDetails(_kayitId);
            DegisiklikEventleriniBagla();
            _oncekiPencere = oncekiPencere;
        }


        // Veritabanı bağlantı dizesi
        private const string ConnectionString = "Server=91.247.168.204,1433;Database=alpsoftservistakip;User Id=sa;Password=Alperengurpinar4160552009;TrustServerCertificate=True;";
        private void DegisiklikEventleriniBagla()
        {
            // 🔹 TextBox'lar
            adsoyad_skayit.TextChanged += FormDegisti;
            bayiadi_skayit.TextChanged += FormDegisti;
            ceptelefonu_skayit.TextChanged += FormDegisti;
            adres_skayit.TextChanged += FormDegisti;
            eposta_skayit.TextChanged += FormDegisti;
            cihaztürü_skayit.TextChanged += FormDegisti;
            marka_skayit.TextChanged += FormDegisti;
            model_skayit.TextChanged += FormDegisti;
            imeino_skayit.TextChanged += FormDegisti;
            ekbilgiler_skayit.TextChanged += FormDegisti;
            sikayetiariza_skayit.TextChanged += FormDegisti;
            fiyatbilgisi_skayit1.TextChanged += FormDegisti;
            ilgiliteknisyen1.TextChanged += FormDegisti;

            // 🔹 ComboBox
            servisdurumu.SelectionChanged += FormDegisti;

            // 🔹 CheckBox'lar
            yeni.Checked += FormDegisti;
            yeni.Unchecked += FormDegisti;

            eski.Checked += FormDegisti;
            eski.Unchecked += FormDegisti;

            tamirgörmüs.Checked += FormDegisti;
            tamirgörmüs.Unchecked += FormDegisti;

            garantili.Checked += FormDegisti;
            garantili.Unchecked += FormDegisti;

            garantisiz.Checked += FormDegisti;
            garantisiz.Unchecked += FormDegisti;

            servisgarantili.Checked += FormDegisti;
            servisgarantili.Unchecked += FormDegisti;

            yedeklemeyapilsin.Checked += FormDegisti;
            yedeklemeyapilsin.Unchecked += FormDegisti;
        }



        private void LoadRecordDetails(int id)
        {
            if (id <= 0) return;

            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                try
                {
                    con.Open();

                    // KayıtlıCihazlar tablosundan kaydı çeker
                    string sorgu = "SELECT * FROM kayitlicihazlar WHERE ID = @ID";
                    SqlCommand command = new SqlCommand(sorgu, con);
                    command.Parameters.AddWithValue("@ID", id);

                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        // Helper metot: Veriyi kontrol edip TextBox'ı doldurur ve rengi ayarlar
                        Action<TextBox, object, string> setTextBoxValue = (textBox, dbValue, placeholder) =>
                        {
                            string value = (dbValue != DBNull.Value) ? dbValue.ToString() : string.Empty;

                            if (string.IsNullOrEmpty(value))
                            {
                                textBox.Text = placeholder;
                                textBox.Foreground = Brushes.Gray;
                            }
                            else
                            {
                                textBox.Text = value;
                                textBox.Foreground = Brushes.Black;
                            }
                        };

                        // Helper metot: Veritabanından gelen 0/1 değerini kontrol edip CheckBox'ı ayarlar
                        Action<CheckBox, object> setCheckBoxValue = (checkBox, dbValue) =>
                        {
                            if (dbValue != DBNull.Value && Convert.ToInt32(dbValue) == 1)
                            {
                                checkBox.IsChecked = true;
                            }
                            else
                            {
                                checkBox.IsChecked = true;
                            }
                        };

                        // --- MÜŞTERİ BİLGİLERİ ---
                        setTextBoxValue(adsoyad_skayit, reader["IsimSoyisim"], "Ad Soyad*");
                        setTextBoxValue(bayiadi_skayit, reader["BayiAdi"], "Bayi Adı");
                        setTextBoxValue(ceptelefonu_skayit, reader["CepTelefonu"], "Cep Telefonu*");
                        setTextBoxValue(adres_skayit, reader["Adres"], "Adres");
                        setTextBoxValue(eposta_skayit, reader["EPosta"], "E-Posta");

                        // --- CİHAZ BİLGİLERİ ---
                        setTextBoxValue(cihaztürü_skayit, reader["CihazTuru"], "Cihaz Türü*");
                        setTextBoxValue(marka_skayit, reader["Marka"], "Marka*");
                        setTextBoxValue(model_skayit, reader["Model"], "Model*");
                        setTextBoxValue(imeino_skayit, reader["IMEI"], "IMEI No");
                        setTextBoxValue(ekbilgiler_skayit, reader["EkBilgiler"], "Ek Bilgiler");
                        setTextBoxValue(sikayetiariza_skayit, reader["Ariza"], "Şikayet/Arıza*");
                        setTextBoxValue(fiyatbilgisi_skayit1, reader["FiyatBilgisi"], "Fiyat Bilgisi");
                        servisdurumtext.Text = reader["ServisDurumu"]?.ToString();

                        if (FindName("servisdurumu") is ComboBox servisDurumuCombo)
                        {
                            servisDurumuCombo.SelectedValue = reader["ServisDurumu"]?.ToString();
                        }



                        // --- TEKNİK/SERVİS BİLGİLERİ ---
                        setTextBoxValue(ilgiliteknisyen1, reader["ServisTeknisyen"], "İlgili Teknisyen");



                        // --- CHECKBOX BİLGİLERİ ---

                        setCheckBoxValue(FindName("yeni") as CheckBox, reader["Yeni"]);
                        setCheckBoxValue(FindName("eski") as CheckBox, reader["Eski"]);
                        setCheckBoxValue(FindName("tamirgörmüs") as CheckBox, reader["TamirGormus"]);
                        setCheckBoxValue(FindName("garantili") as CheckBox, reader["Garantili"]);
                        setCheckBoxValue(FindName("garantisiz") as CheckBox, reader["Garantisiz"]);
                        setCheckBoxValue(FindName("servisgarantili") as CheckBox, reader["ServisGarantili"]);
                        setCheckBoxValue(FindName("yedeklemeyapilsin") as CheckBox, reader["YedeklemeYapilsin"]);


                        // --- TARİH BİLGİLERİ ---

                        if (FindName("servisGirisTarihi_skayit") is TextBox girisTarihiControl)
                        {
                            if (reader["ServisTarihi"] != DBNull.Value && reader["ServisTarihi"] is DateTime)
                            {
                                girisTarihiControl.Text = ((DateTime)reader["ServisTarihi"]).ToString("dd.MM.yyyy HH:mm");
                                girisTarihiControl.Foreground = Brushes.Black;
                            }
                            else
                            {
                                girisTarihiControl.Text = "Belirtilmemiş";
                                girisTarihiControl.Foreground = Brushes.Gray;
                            }
                        }

                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Kayıt detayları yüklenirken bir veritabanı hatası oluştu: " + ex.Message, "Hata");
                }
            }
        }

        // --- FOCUS/LOSTFOCUS METOTLARI ---

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

        private void btksorguladetay_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(imeino_skayit.Text))
            {
                MessageBox.Show("IMEI numarası girilmemiş.", "Uyarı",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Clipboard.SetText(imeino_skayit.Text);

            MessageBox.Show(
                "IMEI panoya kopyalandı.\nBTK sayfası açılıyor.\n\nYapıştırıp sorgulayabilirsiniz.",
                "Bilgi",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            Process.Start(new ProcessStartInfo
            {
                FileName = "https://www.turkiye.gov.tr/imei-sorgulama",
                UseShellExecute = true
            });
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

        private void Imeino_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;

            // ❌ RAKAM DEĞİLSE
            if (!Regex.IsMatch(e.Text, @"^\d+$"))
            {
                e.Handled = true;
                return;
            }

            // ❌ 15 HANEYİ GEÇİYORSA (KLAVYE ENGELİ)
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

        private void geridondetay_Click(object sender, RoutedEventArgs e)
        {
            
            _degisiklikVar = false;
            Window3 win = new Window3(); 
            win.Show();
            this.Close();
        }

        private void ceptelefonu_skayit_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text, 0);
        }

        private void ceptelefonu_skayit_TextChanged(object sender, TextChangedEventArgs e)
        {


            if (ceptelefonu_skayit.Text == "Cep Telefonu*")
                return;

            ceptelefonu_skayit.TextChanged -= ceptelefonu_skayit_TextChanged;

            string rakamlar = new string(ceptelefonu_skayit.Text.Where(char.IsDigit).ToArray());

            if (rakamlar.Length > 11)
                rakamlar = rakamlar.Substring(0, 11);

            if (rakamlar.StartsWith("0") == false && rakamlar.Length > 0)
                rakamlar = "0" + rakamlar;

            ceptelefonu_skayit.Text = TelefonFormatla(rakamlar);
            ceptelefonu_skayit.CaretIndex = ceptelefonu_skayit.Text.Length;

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

        private bool TelefonGecerliMi(string telefon)
        {
            string pattern = @"^\(0\d{3}\)\s\d{3}\s\d{2}\s\d{2}$";
            return Regex.IsMatch(telefon, pattern);
        }

        private void sarviskaydınıkaydet_Click(object sender, RoutedEventArgs e)
        {
            if (!TelefonGecerliMi(ceptelefonu_skayit.Text))
            {
                MessageBox.Show(
                    "Telefon numarası geçerli değil.\nÖrnek: (0553) 123 45 67",
                    "Hatalı Telefon",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                ceptelefonu_skayit.BorderBrush = Brushes.Red;
                return;
            }
            else
            {
                ceptelefonu_skayit.BorderBrush = Brushes.Gray;
            }

            string imei = imeino_skayit.Text;

            // ✅ Placeholder veya boşsa → kontrol yapma
            if (string.IsNullOrWhiteSpace(imei) || imei == "IMEI No")
            {
                // hiçbir şey yapma
            }
            else
            {
                if (imei.Length != 15)
                {
                    MessageBox.Show(
                        "IMEI numarası girildiyse 15 haneli olmalıdır.",
                        "Hatalı IMEI",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    imeino_skayit.BorderBrush = Brushes.Red;
                    return;
                }
            }

            string servisDurumu = servisdurumtext.Text;

            if (string.IsNullOrWhiteSpace(servisDurumu))
            {
                MessageBox.Show("Lütfen servis durumunu seçin!");
                return;
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // Eğer değişiklik varsa kullanıcıya sor
            if (_degisiklikVar)
            {
                var sonuc = MessageBox.Show("Kaydetmeden çıkmak istiyor musunuz?", "Uyarı",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (sonuc == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }

            // Kapanırken her durumda önceki pencereyi göster
            if (_oncekiPencere != null)
            {
                _oncekiPencere.Show();
            }
        }
    }
}
