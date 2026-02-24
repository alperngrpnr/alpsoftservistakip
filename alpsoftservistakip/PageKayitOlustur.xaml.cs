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
using alpsoftservistakip.ViewModels;

namespace alpsoftservistakip
{
    public partial class PageKayitOlustur : Page
    {
        public PageKayitOlusturViewModel ViewModel { get; private set; }

        public PageKayitOlustur() : this(0)
        {
        }

        public PageKayitOlustur(int kayitId)
        {
            InitializeComponent();
            ViewModel = new PageKayitOlusturViewModel(kayitId);
            DataContext = ViewModel;
        }


        // Aşağıdaki metotlar Window9.xaml.cs'den birebir alınmıştır (focus, telefon, IMEI, vs.)

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



