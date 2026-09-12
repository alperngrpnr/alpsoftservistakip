using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Services = alpsoftservistakip.Services;
using System.Windows.Input;
using System.Windows.Media;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.ComponentModel;
using alpsoftservistakip.Helpers;
using alpsoftservistakip.Models;

namespace alpsoftservistakip
{
    public partial class Window9 : Window
    {
        private bool _degisiklikVar = false;
        private int _kayitId;
        private bool _yuklemeDevamEdiyor = true; // ✅ EKLEME

        // Veritabani baglantisi kaldirildi - API kullaniliyor

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

        private async void LoadRecordDetails(int id)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Class1.JwtToken);
                    var response = await client.GetAsync($"{ApiConfig.Api}/KayitliCihazlar/{id}");
                    if (!response.IsSuccessStatusCode) return;

                    var json = await response.Content.ReadAsStringAsync();
                    var cihaz = Newtonsoft.Json.JsonConvert.DeserializeObject<KayitliCihazDto>(json);
                    if (cihaz == null) return;

                    SetText(adsoyad_skayit, cihaz.IsimSoyisim, "Ad Soyad*");
                    SetText(bayiadi_skayit, cihaz.BayiAdi, "Bayi Adı");
                    SetText(ceptelefonu_skayit, cihaz.CepTelefonu, "Cep Telefonu*");
                    SetText(adres_skayit, cihaz.Adres, "Adres");
                    SetText(eposta_skayit, cihaz.EPosta, "E-Posta");
                    SetText(cihaztürü_skayit, cihaz.CihazTuru, "Cihaz Türü*");
                    SetText(marka_skayit, cihaz.Marka, "Marka*");
                    SetText(model_skayit, cihaz.Model, "Model*");
                    SetText(imeino_skayit, cihaz.IMEI, "IMEI No");
                    SetText(ekbilgiler_skayit, cihaz.EkBilgiler, "Ek Bilgiler");
                    SetText(sikayetiariza_skayit, cihaz.Ariza, "Şikayet/Arıza*");
                    SetText(fiyatbilgisi_skayit1, cihaz.FiyatBilgisi, "Fiyat Bilgisi");
                    SetText(ilgiliteknisyen1, cihaz.ServisTeknisyen, "İlgili Teknisyen");

                    servisdurumtext.Text = cihaz.ServisDurumu;

                    yeni.IsChecked = cihaz.Yeni;
                    eski.IsChecked = cihaz.Eski;
                    tamirgörmüs.IsChecked = cihaz.TamirGormus;
                    garantili.IsChecked = cihaz.Garantili;
                    garantisiz.IsChecked = cihaz.Garantisiz;
                    servisgarantili.IsChecked = cihaz.ServisGarantili;
                    yedeklemeyapilsin.IsChecked = cihaz.YedeklemeYapilsin;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
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

        private async void Window5TablosunuYenile()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is Window5 window5)
                {
                    if (!window5.IsVisible)
                    {
                        await Services.NavigationService.ShowWindowAsync(window5);
                    }
                    else
                    {
                        window5.VerileriYukle();
                        window5.Activate();
                    }
                    break;
                }
            }
        }

        // --- KAYIT METODU ---

        private async void sarviskaydınıkaydet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Class1.JwtToken);

                    string Clean(string text, string placeholder)
                        => (text == placeholder || string.IsNullOrWhiteSpace(text)) ? null : text;

                    var dto = new CreateKayitliCihazDto
                    {
                        BayiAdi = Clean(bayiadi_skayit.Text, "Bayi Adı"),
                        IsimSoyisim = Clean(adsoyad_skayit.Text, "Ad Soyad*"),
                        CepTelefonu = Clean(ceptelefonu_skayit.Text, "Cep Telefonu*"),
                        Adres = Clean(adres_skayit.Text, "Adres"),
                        EPosta = Clean(eposta_skayit.Text, "E-Posta"),
                        CihazTuru = Clean(cihaztürü_skayit.Text, "Cihaz Türü*"),
                        Marka = Clean(marka_skayit.Text, "Marka*"),
                        Model = Clean(model_skayit.Text, "Model*"),
                        IMEI = Clean(imeino_skayit.Text, "IMEI No"),
                        EkBilgiler = Clean(ekbilgiler_skayit.Text, "Ek Bilgiler"),
                        ServisDurumu = servisdurumtext.Text,
                        ServisTeknisyen = Clean(ilgiliteknisyen1.Text, "İlgili Teknisyen"),
                        Ariza = Clean(sikayetiariza_skayit.Text, "Şikayet/Arıza*"),
                        FiyatBilgisi = Clean(fiyatbilgisi_skayit1.Text, "Fiyat Bilgisi")
                    };

                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(dto);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync($"{ApiConfig.Api}/KayitliCihazlar", content);

                    if (response.IsSuccessStatusCode)
                        MessageBox.Show("✅ Kayıt başarıyla eklendi");
                    else
                    {
                        var err = await response.Content.ReadAsStringAsync();
                        MessageBox.Show("HATA: " + err);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("HATA: " + ex.Message);
            }
        }


        // --- GERİ DÖN BUTONU ---

        private async void sarviskaydınıkaydet_Kopyala_Click(object sender, RoutedEventArgs e)
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
            await Services.NavigationService.ShowWindowAsync(new Window3());
            // keep current window state (don't open a new top-level window)
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
