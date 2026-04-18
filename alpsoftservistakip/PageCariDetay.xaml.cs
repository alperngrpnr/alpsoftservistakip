using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
                using (SqlConnection con = Veritabani.BaglantiAl())
                {
                    await con.OpenAsync();
                    string query = "SELECT AdSoyadUnvan, GuncelBakiye FROM Cariler WHERE CariID = @id AND SirketID = @sirketId";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@id", _cariId);
                        cmd.Parameters.AddWithValue("@sirketId", Class1.AktifKullanici.SirketID);

                        using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                        {
                            if (dr.Read())
                            {
                                string unvan = dr["AdSoyadUnvan"]?.ToString() ?? "";
                                decimal bakiye = dr["GuncelBakiye"] != DBNull.Value ? Convert.ToDecimal(dr["GuncelBakiye"]) : 0m;

                                txtMusteriAdi.Text = unvan + " - Cari Hesabı";
                                txtGuncelBakiye.Text = bakiye.ToString("N2", new System.Globalization.CultureInfo("tr-TR")) + " ₺";

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
                using (SqlConnection con = Veritabani.BaglantiAl())
                {
                    await con.OpenAsync();
                    string query = @"SELECT CAST(0 as bit) as Secili, IslemTarihi, IslemTipi, Tutar, Aciklama 
                                     FROM CariHareketler 
                                     WHERE CariID = @id AND SirketID = @sirketId";

                    if (baslangic.HasValue)
                        query += " AND IslemTarihi >= @baslangic";
                    if (bitis.HasValue)
                        query += " AND IslemTarihi <= @bitis";

                    query += " ORDER BY IslemTarihi DESC";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@id", _cariId);
                        cmd.Parameters.AddWithValue("@sirketId", Class1.AktifKullanici.SirketID);

                        if (baslangic.HasValue)
                            cmd.Parameters.AddWithValue("@baslangic", baslangic.Value.Date);

                        if (bitis.HasValue)
                            // Bitiş gününün tamamını kapsamak için o günün son saatine ayarlıyoruz
                            cmd.Parameters.AddWithValue("@bitis", bitis.Value.Date.AddDays(1).AddSeconds(-1));

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        await Task.Run(() => da.Fill(dt));
                        dgHareketler.ItemsSource = dt.DefaultView;
                    }
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
            string temizTutar = txtIslemTutar.Text.Replace("₺", "").Replace(" ", "").Trim();
            temizTutar = temizTutar.Replace(".", "").Replace(",", "."); // Bilgisayar dilinden bağımsız mutlak düzeltme

            if (!decimal.TryParse(temizTutar, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal tutar) || tutar <= 0)
            {
                MessageBox.Show("Lütfen geçerli bir tutar girin (örn: 1500.50 veya 1500,50)", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string aciklama = txtIslemAciklama.Text.Trim();

            try
            {
                using (SqlConnection con = Veritabani.BaglantiAl())
                {
                    await con.OpenAsync();

                    // Bakiye Güncellemesi: hareketlerden yeniden hesapla (idempotent, çift ekleme yapmaz)
                    string query = @"INSERT INTO CariHareketler (CariID, SirketID, IslemTarihi, IslemTipi, Tutar, Aciklama) 
                                     VALUES (@id, @sirketId, @tarih, @tip, @tutar, @aciklama);

                                     UPDATE Cariler
                                     SET GuncelBakiye = ISNULL((
                                         SELECT SUM(
                                             CASE
                                                 WHEN IslemTipi LIKE N'Borçlandırma%' OR IslemTipi LIKE N'Borc%' THEN Tutar
                                                 WHEN IslemTipi LIKE N'Tahsilat%' THEN -Tutar
                                                 ELSE 0
                                             END)
                                         FROM CariHareketler
                                         WHERE CariID = @id AND SirketID = @sirketId
                                     ), 0)
                                     WHERE CariID = @id AND SirketID = @sirketId;";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@id", _cariId);
                        cmd.Parameters.AddWithValue("@sirketId", Class1.AktifKullanici.SirketID);
                        cmd.Parameters.AddWithValue("@tarih", DateTime.Now);
                        cmd.Parameters.AddWithValue("@tip", _sayfaModu);
                        cmd.Parameters.AddWithValue("@tutar", tutar);
                        cmd.Parameters.AddWithValue("@aciklama", string.IsNullOrEmpty(aciklama) ? (object)DBNull.Value : aciklama);

                        await cmd.ExecuteNonQueryAsync();
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
                using (SqlConnection con = Veritabani.BaglantiAl())
                {
                    await con.OpenAsync();

                    // İlk olarak cariye ait tüm hareketleri (dökümü) sil
                    string deleteHareketlerQuery = "DELETE FROM CariHareketler WHERE CariID = @id AND SirketID = @sirketId";
                    using (SqlCommand cmd = new SqlCommand(deleteHareketlerQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@id", _cariId);
                        cmd.Parameters.AddWithValue("@sirketId", Class1.AktifKullanici.SirketID);
                        await cmd.ExecuteNonQueryAsync();
                    }

                    // Sonra carinin kendisini sil
                    string deleteCariQuery = "DELETE FROM Cariler WHERE CariID = @id AND SirketID = @sirketId";
                    using (SqlCommand cmd = new SqlCommand(deleteCariQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@id", _cariId);
                        cmd.Parameters.AddWithValue("@sirketId", Class1.AktifKullanici.SirketID);
                        await cmd.ExecuteNonQueryAsync();
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
                using (SqlConnection con = Veritabani.BaglantiAl())
                {
                    await con.OpenAsync();

                    // Mevcut bakiyeyi al
                    decimal mevcutBakiye = 0;
                    using (SqlCommand cmdGet = new SqlCommand("SELECT ISNULL(GuncelBakiye, 0) FROM Cariler WHERE CariID = @id AND SirketID = @sirketId", con))
                    {
                        cmdGet.Parameters.AddWithValue("@id", _cariId);
                        cmdGet.Parameters.AddWithValue("@sirketId", Class1.AktifKullanici.SirketID);
                        object val = await cmdGet.ExecuteScalarAsync();
                        if (val != null) mevcutBakiye = Convert.ToDecimal(val);
                    }

                    if (mevcutBakiye == 0)
                    {
                        MessageBox.Show("Müşterinin bakiyesi zaten sıfır.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    // Hareket ekle
                    string islemTipi = mevcutBakiye > 0 ? "Tahsilat (Sıfırlama)" : "Borçlandırma (Sıfırlama)";
                    decimal tutar = Math.Abs(mevcutBakiye);

                    string insertQuery = @"INSERT INTO CariHareketler (CariID, SirketID, IslemTarihi, IslemTipi, Tutar, Aciklama) 
                                           VALUES (@id, @sirketId, @tarih, @tip, @tutar, 'Hesap tamamen sıfırlandı. Eski Bakiye: ' + @eskiStr);

                                           UPDATE Cariler SET GuncelBakiye = 0 WHERE CariID = @id AND SirketID = @sirketId;";

                    using (SqlCommand cmdIns = new SqlCommand(insertQuery, con))
                    {
                        cmdIns.Parameters.AddWithValue("@id", _cariId);
                        cmdIns.Parameters.AddWithValue("@sirketId", Class1.AktifKullanici.SirketID);
                        cmdIns.Parameters.AddWithValue("@tarih", DateTime.Now);
                        cmdIns.Parameters.AddWithValue("@tip", islemTipi);
                        cmdIns.Parameters.AddWithValue("@tutar", tutar);
                        cmdIns.Parameters.AddWithValue("@eskiStr", mevcutBakiye.ToString("N2") + " ₺");

                        await cmdIns.ExecuteNonQueryAsync();
                    }
                }

                MessageBox.Show("Hesap başarıyla sıfırlandı.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                await CariBilgileriniDoldur();
                await CariHareketleriniGetir();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Sıfırlama işlemi sırasında hata oluştu: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnFisYazdir_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog printDialog = new PrintDialog();

            if (printDialog.ShowDialog() == true)
            {
                // Tablodaki seçili satırları filtreliyoruz
                DataView dv = (DataView)dgHareketler.ItemsSource;
                DataTable dtOrjinal = dv.Table;

                // Seçili olan satırları bul
                DataRow[] seciliSatirlar = dtOrjinal.Select("Secili = True");

                // Eğer seçili olan varsa sadece onları yazdıracağımız bir tablo oluştur, yoksa tümünü al
                DataTable printTable = dtOrjinal.Clone();
                if (seciliSatirlar.Length > 0)
                {
                    foreach (DataRow row in seciliSatirlar)
                    {
                        printTable.ImportRow(row);
                    }
                }
                else
                {
                    printTable = dtOrjinal.Copy();
                }

                // DataGrid veri kaynağını geçici olarak seçili veriler ile değiştiriyoruz
                dgHareketler.ItemsSource = printTable.DefaultView;

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
                titlePanel.Children.Add(new TextBlock { Text = "Resmi Hesap Dökümü" + (seciliSatirlar.Length > 0 ? " (Seçili Kayıtlar Oluşturuldu)" : ""), FontSize = 14, Foreground = Brushes.Gray, Margin = new Thickness(0, 5, 0, 0) });
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
                    dgHareketler.ItemsSource = dv;
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


