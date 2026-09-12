using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using alpsoftservistakip.Helpers;
using alpsoftservistakip.Models;
using Newtonsoft.Json;

namespace alpsoftservistakip
{
    public partial class PageCariDetay : Page
    {
        private int _cariId;
        private string _sayfaModu = ""; // Borçlandırma veya Tahsilat belirlemek için

        public PageCariDetay(int cariId)
        {
            InitializeComponent();
            _cariId = cariId;
            this.Loaded += PageCariDetay_Loaded;
        }

        private async void PageCariDetay_Loaded(object sender, RoutedEventArgs e)
        {
            await CariBilgileriniDoldur();
            await CariHareketleriniGetir();
        }

        private async Task CariBilgileriniDoldur()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Class1.JwtToken);
                    var response = await client.GetAsync($"{ApiConfig.Api}/Cariler/{_cariId}");
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        var err = await response.Content.ReadAsStringAsync();
                        MessageBox.Show("Cari bilgileri yüklenemedi: " + err, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var json = await response.Content.ReadAsStringAsync();
                    var dto = JsonConvert.DeserializeObject<CariDetayDto>(json);
                    
                    string unvan = dto.AdSoyadUnvan ?? "";
                    decimal bakiye = dto.GuncelBakiye;

                    txtMusteriAdi.Text = unvan + " - Cari Hesabı";
                    txtGuncelBakiye.Text = "$" + bakiye.ToString("N2", new System.Globalization.CultureInfo("tr-TR"));

                    if (bakiye > 0)
                    {
                        txtGuncelBakiye.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E74C3C")); // Kırmızı
                        txtDurum.Text = "(Müşteri Borçlu)";
                    }
                    else if (bakiye < 0)
                    {
                        txtGuncelBakiye.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2ECC71")); // Yeşil
                        txtDurum.Text = "(Müşteri Alacaklı / Size Fazla Ödeme Yaptı)";
                    }
                    else
                    {
                        txtGuncelBakiye.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#718096")); // Gri
                        txtDurum.Text = "(Hesap Sıfırlandı)";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cari bilgileri yüklenemedi: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task CariHareketleriniGetir(DateTime? baslangic = null, DateTime? bitis = null)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Class1.JwtToken);
                    string url = $"{ApiConfig.Api}/Cariler/{_cariId}/hareketler?";
                    if (baslangic.HasValue)
                        url += $"baslangic={baslangic.Value:yyyy-MM-dd}&";
                    if (bitis.HasValue)
                        url += $"bitis={bitis.Value:yyyy-MM-dd}&";
                        
                    var response = await client.GetAsync(url);
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        var err = await response.Content.ReadAsStringAsync();
                        MessageBox.Show("Hareketler yüklenemedi: " + err, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var json = await response.Content.ReadAsStringAsync();
                    var list = JsonConvert.DeserializeObject<List<CariHareketDto>>(json);
                    
                    dgHareketler.ItemsSource = list;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hareketler yüklenemedi: " + ex.Message);
            }
        }

        private async void TarihFiltresi_Changed(object sender, SelectionChangedEventArgs e)
        {
            await CariHareketleriniGetir(dpBaslangic.SelectedDate, dpBitis.SelectedDate);
        }

        private async void btnFiltreTemizle_Click(object sender, RoutedEventArgs e)
        {
            dpBaslangic.SelectedDate = null;
            dpBitis.SelectedDate = null;
            await CariHareketleriniGetir();
        }

        private void btnBorclandir_Click(object sender, RoutedEventArgs e)
        {
            _sayfaModu = "Borçlandırma";
            txtPopupBaslik.Text = "Hesaba Borç Ekle (Satış/Hizmet Fişi)";
            txtPopupBaslik.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E74C3C"));
            PanelYeniIslem.Visibility = Visibility.Visible;
            txtIslemTutar.Clear();
            txtIslemAciklama.Clear();
        }

        private void btnTahsilat_Click(object sender, RoutedEventArgs e)
        {
            _sayfaModu = "Tahsilat";
            txtPopupBaslik.Text = "Hesaba Tahsilat Ekle (Ödeme / Nakit Girişi)";
            txtPopupBaslik.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2ECC71"));
            PanelYeniIslem.Visibility = Visibility.Visible;
            txtIslemTutar.Clear();
            txtIslemAciklama.Clear();
        }

        private void btnPopupIptal_Click(object sender, RoutedEventArgs e)
        {
            PanelYeniIslem.Visibility = Visibility.Collapsed;
        }

        private async void btnPopupKaydet_Click(object sender, RoutedEventArgs e)
        {
            string temizTutar = txtIslemTutar.Text.Replace("$", "").Replace(" ", "").Trim();
            temizTutar = temizTutar.Replace(".", "").Replace(",", "."); // Bilgisayar dilinden bağımsız mutlak düzeltme

            if (!decimal.TryParse(temizTutar, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal tutar) || tutar <= 0)
            {
                MessageBox.Show("Lütfen geçerli bir tutar girin (örn: 1500.50 veya 1500,50)", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string aciklama = txtIslemAciklama.Text.Trim();

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Class1.JwtToken);
                    var dto = new CariHareketEkleDto 
                    {
                        CariID = _cariId,
                        IslemTipi = _sayfaModu,
                        Tutar = tutar,
                        Aciklama = aciklama
                    };
                    
                    var content = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
                    var response = await client.PostAsync($"{ApiConfig.Api}/Cariler/hareket", content);
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        var err = await response.Content.ReadAsStringAsync();
                        MessageBox.Show("İşlem kaydedilirken hata oluştu: " + err, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                PanelYeniIslem.Visibility = Visibility.Collapsed;
                MessageBox.Show("İşlem başarıyla eklendi ve Bakiye güncellendi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);

                // Sayfayı yenile (Bakiyeyi ve Listeyi)
                await CariBilgileriniDoldur();
                await CariHareketleriniGetir();
            }
            catch (Exception ex)
            {
                MessageBox.Show("İşlem kaydedilirken hata oluştu: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnGeri_Click(object sender, RoutedEventArgs e)
        {
            Window mevcutPencere = Window.GetWindow(this);
            if (mevcutPencere is Window3 window3)
            {
                // Listedeki güncel bakiyeleri görebilmek için tekrar CariListesi sayfasını yüklüyoruz.
                window3.ShowOverlayPage(new PageCariListesi());
            }
        }

        private async void btnCariSil_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult onay = MessageBox.Show("Bu müşteriyi (Cariyi) ve tüm işlem geçmişini kalıcı olarak silmek istediğinize emin misiniz? Bu işlem geri alınamaz!", "Cariyi Sil", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (onay != MessageBoxResult.Yes) return;

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Class1.JwtToken);
                    var response = await client.DeleteAsync($"{ApiConfig.Api}/Cariler/{_cariId}");
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        var err = await response.Content.ReadAsStringAsync();
                        MessageBox.Show("Silme işlemi başarısız: " + err, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                MessageBox.Show("Müşteri ve tüm hesap hareketleri başarıyla silindi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                // Silme işlemi bitince listeye geri dön
                btnGeri_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Silme işlemi sırasında hata oluştu: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnSifirla_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult onay = MessageBox.Show("Müşterinin güncel bakiyesini tamamen sıfırlamak (Temizlemek) istediğinize emin misiniz? Bu işlem geri alınamaz.", "Onay", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (onay != MessageBoxResult.Yes) return;

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Class1.JwtToken);
                    var content = new StringContent("", Encoding.UTF8, "application/json");
                    var response = await client.PostAsync($"{ApiConfig.Api}/Cariler/{_cariId}/sifirla", content);
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        var err = await response.Content.ReadAsStringAsync();
                        MessageBox.Show("Sıfırlama işlemi başarısız: " + err, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                MessageBox.Show("Müşteri bakiyesi başarıyla sıfırlandı.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                
                await CariBilgileriniDoldur();
                await CariHareketleriniGetir();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Sıfırlama sırasında hata oluştu: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnFisYazdir_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog printDialog = new PrintDialog();

            if (printDialog.ShowDialog() == true)
            {
                var dtOrjinal = dgHareketler.ItemsSource as System.Collections.IEnumerable;
                if (dtOrjinal == null) return;

                var seciliSatirlar = new System.Collections.Generic.List<object>();
                var orjinalListe = new System.Collections.Generic.List<object>();

                foreach (var item in dtOrjinal)
                {
                    orjinalListe.Add(item);
                    var seciliProp = item.GetType().GetProperty("Secili");
                    if (seciliProp != null && seciliProp.GetValue(item) is bool secili && secili)
                    {
                        seciliSatirlar.Add(item);
                    }
                }

                var printTable = seciliSatirlar.Count > 0 ? seciliSatirlar : orjinalListe;
                bool varMiSecili = seciliSatirlar.Count > 0;

                // DataGrid veri kaynağını geçici olarak seçili veriler ile değiştiriyoruz
                dgHareketler.ItemsSource = printTable;

                // Daha Premium Bir Ekstre ve PDF Çıktısı (A4 Yapısı)
                StackPanel printPanel = new StackPanel();
                printPanel.Margin = new Thickness(50);
                printPanel.Background = Brushes.White;

                // ÜST BÖLÜM: Başlık ve Logo Konteyneri
                Grid headerGrid = new Grid();
                headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                headerGrid.Margin = new Thickness(0, 0, 0, 20);

                StackPanel titlePanel = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
                titlePanel.Children.Add(new TextBlock { Text = "CARİ HESAP EKSTRESİ", FontSize = 32, FontWeight = FontWeights.Black, Foreground = new SolidColorBrush(Color.FromRgb(44, 62, 80)) });
                titlePanel.Children.Add(new TextBlock { Text = "Resmi Hesap Dökümü" + (varMiSecili ? " (Seçili Kayıtlar Oluşturuldu)" : ""), FontSize = 14, Foreground = Brushes.Gray, Margin = new Thickness(0, 5, 0, 0) });
                Grid.SetColumn(titlePanel, 0);
                headerGrid.Children.Add(titlePanel);

                // Firma Logonuzu Fiştte Gösterelim
                Image logo = new Image { Width = 100, Height = 100, HorizontalAlignment = HorizontalAlignment.Right };
                try { logo.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Alpsoftlogotransparan.png")); } catch { }
                Grid.SetColumn(logo, 1);
                headerGrid.Children.Add(logo);

                printPanel.Children.Add(headerGrid);

                // Ayraç Çizgisi
                printPanel.Children.Add(new Border { Height = 2, Background = new SolidColorBrush(Color.FromRgb(237, 242, 247)), Margin = new Thickness(0, 0, 0, 25) });

                // MÜŞTERİ VE BAKİYE BİLGİSİ (Yan Yana)
                Grid infoGrid = new Grid();
                infoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                infoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                infoGrid.Margin = new Thickness(0, 0, 0, 30);

                StackPanel leftInfo = new StackPanel();
                leftInfo.Children.Add(new TextBlock { Text = "MÜŞTERİ / FİRMA UNVANI", FontSize = 12, FontWeight = FontWeights.Bold, Foreground = Brushes.Gray, Margin = new Thickness(0, 0, 0, 5) });
                leftInfo.Children.Add(new TextBlock { Text = txtMusteriAdi.Text.Replace(" - Cari Hesabı", ""), FontSize = 18, FontWeight = FontWeights.Bold, Foreground = Brushes.Black, TextWrapping = TextWrapping.Wrap });

                if (dpBaslangic.SelectedDate.HasValue || dpBitis.SelectedDate.HasValue)
                {
                    string baslangic = dpBaslangic.SelectedDate.HasValue ? dpBaslangic.SelectedDate.Value.ToString("dd.MM.yyyy") : "Tüm Geçmiş";
                    string bitis = dpBitis.SelectedDate.HasValue ? dpBitis.SelectedDate.Value.ToString("dd.MM.yyyy") : "Bugün";
                    leftInfo.Children.Add(new TextBlock { Text = $"Filtrelenen Dönem: {baslangic} - {bitis}", FontSize = 13, Foreground = new SolidColorBrush(Color.FromRgb(52, 152, 219)), FontWeight=FontWeights.SemiBold, Margin = new Thickness(0, 8, 0, 0) });
                }
                Grid.SetColumn(leftInfo, 0);
                infoGrid.Children.Add(leftInfo);

                StackPanel rightInfo = new StackPanel { HorizontalAlignment = HorizontalAlignment.Right };
                rightInfo.Children.Add(new TextBlock { Text = "GÜNCEL BAKİYE", FontSize = 12, FontWeight = FontWeights.Bold, Foreground = Brushes.Gray, Margin = new Thickness(0, 0, 0, 5), HorizontalAlignment = HorizontalAlignment.Right });
                rightInfo.Children.Add(new TextBlock { Text = txtGuncelBakiye.Text, FontSize = 26, FontWeight = FontWeights.Black, Foreground = Brushes.Black, HorizontalAlignment = HorizontalAlignment.Right });
                rightInfo.Children.Add(new TextBlock { Text = txtDurum.Text, FontSize = 13, FontWeight=FontWeights.SemiBold, Foreground = Brushes.DimGray, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 2, 0, 0) });
                Grid.SetColumn(rightInfo, 1);
                infoGrid.Children.Add(rightInfo);

                printPanel.Children.Add(infoGrid);

                // HAREKETLER TABLOSU BAŞLIĞI
                printPanel.Children.Add(new TextBlock { Text = "HESAP HAREKETLERİ DÖKÜMÜ", FontSize = 14, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(44, 62, 80)), Margin = new Thickness(0, 0, 0, 10) });

                // DataGrid'i Kağıda uyarlamak için mevcut hiyerarşisinden ödünç alıyoruz
                var parentGrid = (System.Windows.Controls.Grid)dgHareketler.Parent;
                parentGrid.Children.Remove(dgHareketler); // Geçici çıkart

                var prevBackground = dgHareketler.Background;
                var prevBorder = dgHareketler.BorderBrush;
                var prevThick = dgHareketler.BorderThickness;
                var prevRowBg = dgHareketler.RowBackground;
                var prevAltRowBg = dgHareketler.AlternatingRowBackground;
                var prevGridLines = dgHareketler.GridLinesVisibility;

                // "Seç" kolonunu yazdırırken veya önizlemede gizle
                var secKolonu = dgHareketler.Columns[0];
                var prevSecVisibility = secKolonu.Visibility;
                secKolonu.Visibility = Visibility.Collapsed;

                // Print Formatına Özel Grid Tasarımı
                dgHareketler.Background = Brushes.White;
                dgHareketler.BorderThickness = new Thickness(1);
                dgHareketler.BorderBrush = Brushes.LightGray;
                dgHareketler.RowBackground = Brushes.White;
                dgHareketler.AlternatingRowBackground = new SolidColorBrush(Color.FromRgb(248, 250, 252));
                dgHareketler.GridLinesVisibility = DataGridGridLinesVisibility.Horizontal;
                dgHareketler.HorizontalGridLinesBrush = Brushes.LightGray;

                // Kağıda Sığdır
                dgHareketler.Width = printDialog.PrintableAreaWidth - 100;
                dgHareketler.HorizontalAlignment = HorizontalAlignment.Left;

                printPanel.Children.Add(dgHareketler);

                // FOOTER BİLGİSİ (En alt sayfa sonu)
                TextBlock footer = new TextBlock
                {
                    Text = $"Bu ekstre {DateTime.Now:dd.MM.yyyy HH:mm} tarihinde AlpSoft Servis Takip Yazılımı üzerinden otomatik oluşturulmuştur.",
                    FontSize = 11,
                    FontStyle = FontStyles.Italic,
                    Foreground = Brushes.Gray,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 40, 0, 0)
                };
                printPanel.Children.Add(footer);

                // Sayfayı Çıkart'ın İçinde Düzenle
                printPanel.Measure(new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight));
                printPanel.Arrange(new Rect(new Point(0, 0), printPanel.DesiredSize));

                try
                {
                    printDialog.PrintVisual(printPanel, "Premium Cari Fiş Ekstresi");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Yazdırma/PDF oluşturma sırasında hata oluştu: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    // Tabloyu ekranda eski yerine ve eski şıklığına geri taşı
                    printPanel.Children.Remove(dgHareketler);

                    // Tekrar orjinal veri kaynağını bağla
                    dgHareketler.ItemsSource = dtOrjinal;
                    secKolonu.Visibility = prevSecVisibility;

                    dgHareketler.Width = double.NaN; // Genişliği otomatik (Auto) yap
                    dgHareketler.Background = prevBackground;
                    dgHareketler.BorderBrush = prevBorder;
                    dgHareketler.BorderThickness = prevThick;
                    dgHareketler.RowBackground = prevRowBg;
                    dgHareketler.AlternatingRowBackground = prevAltRowBg;
                    dgHareketler.GridLinesVisibility = prevGridLines;

                    parentGrid.Children.Add(dgHareketler); // Yeni yerine geri ekle
                }
            }
        }
    }
}


