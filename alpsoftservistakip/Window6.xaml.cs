using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Services = alpsoftservistakip.Services;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;

namespace alpsoftservistakip
{
    public partial class Window6 : Window
    {
        private readonly int _kayitId;
        private bool _degisiklikVar = false;
        private bool _yuklemeDevamEdiyor = true;

        // SQL Bağlantı Dizesi
        private const string connectionString = "Server=91.247.168.204,1433;Database=alpsoftservistakip;User Id=sa;Password=Alperengurpinar4160552009;TrustServerCertificate=True;";

        // PLACEHOLDER Metinleri
        private const string PlaceholderTeknisyen = "İlgili Teknisyen";
        private const string PlaceholderBayi = "Yetkili Bayi Adı*";
        private const string PlaceholderAdSoyad = "Ad Soyad";
        private const string PlaceholderTelefon = "Cep Telefonu";
        private const string PlaceholderMarka = "Marka";
        private const string PlaceholderModel = "Model";
        private const string PlaceholderAriza = "Arıza";

        // ✅ Sadece ID parametreli constructor
        public Window6(int kayitId)
        {
            InitializeComponent();
            _kayitId = kayitId;

            Title = _kayitId > 0 ? "Kayıt Detayı - ID: " + _kayitId : "Yeni Dış Servis Kaydı";

            InitializePlaceholders();
            SetButtonVisibility();

            if (_kayitId > 0)
            {
                _yuklemeDevamEdiyor = true;
                LoadRecordDetails(_kayitId);
                _yuklemeDevamEdiyor = false;
            }
            else
            {
                _yuklemeDevamEdiyor = false;
            }

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

            Window7TablosunuYenile();
        }

        private void DegisiklikEventleriniBagla()
        {
            // TextBox'lar
            ilgiliteknisyendisservis.TextChanged += TextBox_DegisiklikYapildi;
            bayiadidisservis.TextChanged += TextBox_DegisiklikYapildi;
            isimsoyisimdisservis.TextChanged += TextBox_DegisiklikYapildi;
            telefonnumarasidisservis.TextChanged += TextBox_DegisiklikYapildi;
            markadisservis.TextChanged += TextBox_DegisiklikYapildi;
            modeldisservis.TextChanged += TextBox_DegisiklikYapildi;
            arizadisservis.TextChanged += TextBox_DegisiklikYapildi;
        }

        private void TextBox_DegisiklikYapildi(object sender, TextChangedEventArgs e)
        {
            if (_yuklemeDevamEdiyor) return;

            var tb = sender as TextBox;
            if (tb.Foreground == Brushes.Black)
            {
                _degisiklikVar = true;
            }
        }

        private async void Window7TablosunuYenile()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is Window7 window7)
                {
                    if (!window7.IsVisible)
                    {
                        await Services.NavigationService.ShowWindowAsync(window7);
                    }
                    else
                    {
                        window7.Yenile();
                        window7.Activate();
                    }
                    break;
                }
            }
        }

        // =========================
        // KAYDETME İŞLEMİ (INSERT)
        // =========================
        private void Insert()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = @"INSERT INTO disserviskayitlarnew 
                           (IsimSoyisim, TelefonNumarasi, Marka, Model, Ariza, KullaniciID, İlgiliTeknisyen, YetkiliBayi, KayitTarihi) 
                           VALUES (@i, @t, @m, @mo, @a, @k, @it, @yb, @dt)";

                    SqlCommand cmd = new SqlCommand(query, con);

                    cmd.Parameters.AddWithValue("@i", isimsoyisimdisservis.Text == PlaceholderAdSoyad ? "" : isimsoyisimdisservis.Text);
                    string temizTel = new string(telefonnumarasidisservis.Text.Where(char.IsDigit).ToArray());
                    cmd.Parameters.AddWithValue("@t", temizTel);
                    cmd.Parameters.AddWithValue("@m", markadisservis.Text == PlaceholderMarka ? "" : markadisservis.Text);
                    cmd.Parameters.AddWithValue("@mo", modeldisservis.Text == PlaceholderModel ? "" : modeldisservis.Text);
                    cmd.Parameters.AddWithValue("@a", arizadisservis.Text == PlaceholderAriza ? "" : arizadisservis.Text);
                    cmd.Parameters.AddWithValue("@k", LoginWindow.aktifKullaniciID);
                    cmd.Parameters.AddWithValue("@it", ilgiliteknisyendisservis.Text == PlaceholderTeknisyen ? "" : ilgiliteknisyendisservis.Text);
                    cmd.Parameters.AddWithValue("@yb", bayiadidisservis.Text == PlaceholderBayi ? "" : bayiadidisservis.Text);
                    cmd.Parameters.AddWithValue("@dt", DateTime.Now);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("✅ Kayıt başarıyla eklendi!", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);

                        MessageBoxResult res = MessageBox.Show("Fiş yazdırmak istiyor musunuz?", "Yazdır", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (res == MessageBoxResult.Yes)
                        {
                            FisYazdir(isimsoyisimdisservis.Text, markadisservis.Text, arizadisservis.Text, telefonnumarasidisservis.Text);
                        }

                        _degisiklikVar = false;
                        Window7TablosunuYenile();
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ KAYIT HATASI: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void telefonnumarasidisservis_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (telefonnumarasidisservis.Text == PlaceholderTelefon) return;

            telefonnumarasidisservis.TextChanged -= telefonnumarasidisservis_TextChanged;

            string value = new string(telefonnumarasidisservis.Text.Where(char.IsDigit).ToArray());

            if (value.Length > 10) value = value.Substring(0, 10);

            string formatted = "";

            if (value.Length > 0)
            {
                formatted = "(" + value.Substring(0, Math.Min(value.Length, 3));
                if (value.Length > 3)
                {
                    formatted += ") " + value.Substring(3, Math.Min(value.Length - 3, 3));
                    if (value.Length > 6)
                    {
                        formatted += "-" + value.Substring(6, Math.Min(value.Length - 6, 2));
                        if (value.Length > 8)
                        {
                            formatted += "-" + value.Substring(8, Math.Min(value.Length - 8, 2));
                        }
                    }
                }
            }

            telefonnumarasidisservis.Text = formatted;
            telefonnumarasidisservis.SelectionStart = formatted.Length;

            telefonnumarasidisservis.TextChanged += telefonnumarasidisservis_TextChanged;
        }

        // =========================
        // GÜNCELLEME İŞLEMİ (UPDATE)
        // =========================
        private void Update()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = @"UPDATE disserviskayitlarnew SET 
                                   IsimSoyisim=@i, TelefonNumarasi=@t, Marka=@m, Model=@mo, 
                                   Ariza=@a, İlgiliTeknisyen=@it, YetkiliBayi=@yb 
                                   WHERE KayitID=@id";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@id", _kayitId);
                    cmd.Parameters.AddWithValue("@i", isimsoyisimdisservis.Text == PlaceholderAdSoyad ? "" : isimsoyisimdisservis.Text);

                    string temizTel = new string(telefonnumarasidisservis.Text.Where(char.IsDigit).ToArray());
                    cmd.Parameters.AddWithValue("@t", temizTel);

                    cmd.Parameters.AddWithValue("@m", markadisservis.Text == PlaceholderMarka ? "" : markadisservis.Text);
                    cmd.Parameters.AddWithValue("@mo", modeldisservis.Text == PlaceholderModel ? "" : modeldisservis.Text);
                    cmd.Parameters.AddWithValue("@a", arizadisservis.Text == PlaceholderAriza ? "" : arizadisservis.Text);
                    cmd.Parameters.AddWithValue("@it", ilgiliteknisyendisservis.Text == PlaceholderTeknisyen ? "" : ilgiliteknisyendisservis.Text);
                    cmd.Parameters.AddWithValue("@yb", bayiadidisservis.Text == PlaceholderBayi ? "" : bayiadidisservis.Text);

                    int etkilenenSatir = cmd.ExecuteNonQuery();

                    if (etkilenenSatir == 0)
                    {
                        MessageBox.Show($"⚠️ Hiçbir satır güncellenmedi!\n\nID: {_kayitId}", "Uyarı");
                        return;
                    }

                    MessageBox.Show("✅ Kayıt başarıyla güncellendi!", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);

                    _degisiklikVar = false;
                    Window7TablosunuYenile();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ GÜNCELLEME HATASI: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void disserviskaydinikaydet_Click(object sender, RoutedEventArgs e)
        {
            if (_kayitId == 0)
                Insert();
            else
                Update();
        }

        // =========================
        // YAZDIRMA METOTLARI
        // =========================
        private void FisYazdir(string madi, string cihaz, string ariza, string tel)
        {
            try
            {
                // Veritabanından güncel verileri çek
                string guncelMadi = madi;
                string guncelCihaz = cihaz;
                string guncelAriza = ariza;
                string guncelTel = tel;

                if (_kayitId > 0)
                {
                    using (SqlConnection con = new SqlConnection(connectionString))
                    {
                        con.Open();
                        SqlCommand cmd = new SqlCommand("SELECT * FROM disserviskayitlarnew WHERE KayitID=@id", con);
                        cmd.Parameters.AddWithValue("@id", _kayitId);
                        using (SqlDataReader r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                guncelMadi = r["IsimSoyisim"]?.ToString() ?? "";
                                guncelTel = r["TelefonNumarasi"]?.ToString() ?? "";

                                // Marka + Model birleştir
                                string marka = r["Marka"]?.ToString() ?? "";
                                string model = r["Model"]?.ToString() ?? "";
                                guncelCihaz = string.IsNullOrWhiteSpace(marka) ? model :
                                             string.IsNullOrWhiteSpace(model) ? marka :
                                             $"{marka} {model}";

                                guncelAriza = r["Ariza"]?.ToString() ?? "";
                            }
                        }
                    }
                }

                FlowDocument doc = olusturFlowDoc(guncelMadi, guncelCihaz, guncelAriza, guncelTel);

                MemoryStream ms = new MemoryStream();
                using (Package package = Package.Open(ms, FileMode.Create, FileAccess.ReadWrite))
                {
                    Uri uri = new Uri("pack://temp.xps");
                    PackageStore.AddPackage(uri, package);
                    using (XpsDocument xpsDoc = new XpsDocument(package, CompressionOption.NotCompressed, uri.AbsoluteUri))
                    {
                        XpsDocumentWriter writer = XpsDocument.CreateXpsDocumentWriter(xpsDoc);
                        writer.Write(((IDocumentPaginatorSource)doc).DocumentPaginator);

                        PrintPreviewWindow preview = new PrintPreviewWindow(xpsDoc.GetFixedDocumentSequence());
                        preview.ShowDialog();
                    }
                    PackageStore.RemovePackage(uri);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Yazdırma hatası: " + ex.Message);
            }
        }

        private FlowDocument olusturFlowDoc(string madi, string cihaz, string ariza, string tel)
        {
            // A4 Yatay yarısı: 297mm x 105mm = 841pt x 297pt
            FlowDocument doc = new FlowDocument
            {
                PagePadding = new Thickness(30),
                Background = Brushes.White,
                PageWidth = 841,   // A4 yatay genişlik
                PageHeight = 297,  // A4'ün yarısı yükseklik
                FontFamily = new FontFamily("Segoe UI"),
                ColumnWidth = double.PositiveInfinity, // Tek sütun
                
            };

            // Şirket adını veritabanından çek
            string sirketAdi = GetSirketAdi();

            // Tüm içeriği tek bir Grid'e koy
            Grid anaGrid = new Grid();

            // ============================================
            // BAŞLIK BÖLÜMÜ
            // ============================================

            Grid baslikGrid = new Grid { Margin = new Thickness(0, 0, 0, 10) };
            baslikGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            baslikGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Sol taraf - Şirket bilgisi
            StackPanel solPanel = new StackPanel();
            solPanel.Children.Add(new TextBlock
            {
                Text = sirketAdi,
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(37, 43, 54))
            });
            solPanel.Children.Add(new TextBlock
            {
                Text = "SERVİS TAKİP FİŞİ",
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.Gray,
                Margin = new Thickness(0, 2, 0, 0)
            });
            Grid.SetColumn(solPanel, 0);
            baslikGrid.Children.Add(solPanel);

            // Sağ taraf - Tarih ve fiş no
            StackPanel sagPanel = new StackPanel { HorizontalAlignment = HorizontalAlignment.Right };
            sagPanel.Children.Add(new TextBlock
            {
                Text = $"Fiş No: {_kayitId}",
                FontSize = 11,
                FontWeight = FontWeights.SemiBold
            });
            sagPanel.Children.Add(new TextBlock
            {
                Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm"),
                FontSize = 10,
                Foreground = Brushes.Gray,
                Margin = new Thickness(0, 2, 0, 0)
            });
            Grid.SetColumn(sagPanel, 1);
            baslikGrid.Children.Add(sagPanel);

            // Ana grid'e ekle
            Grid.SetRow(baslikGrid, 0);
            anaGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            anaGrid.Children.Add(baslikGrid);

            // Kalın ayırıcı çizgi
            Border topBorder = new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromRgb(37, 43, 54)),
                BorderThickness = new Thickness(0, 2, 0, 0),
                Margin = new Thickness(0, 0, 0, 10)
            };
            Grid.SetRow(topBorder, 1);
            anaGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            anaGrid.Children.Add(topBorder);

            // ============================================
            // ANA İÇERİK - 3 SÜTUN
            // ============================================

            Grid mainGrid = new Grid { Margin = new Thickness(0, 0, 0, 10) };
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // SOL SÜTUN - Müşteri Bilgileri
            StackPanel solSutun = new StackPanel { Margin = new Thickness(0, 0, 10, 0) };

            AddBaslik(solSutun, "MÜŞTERİ BİLGİLERİ");
            AddBilgi(solSutun, "Ad Soyad:", string.IsNullOrWhiteSpace(madi) || madi == "Ad Soyad" ? "-" : madi);
            AddBilgi(solSutun, "Telefon:", string.IsNullOrWhiteSpace(tel) || tel == "Cep Telefonu" ? "-" : tel);

            Grid.SetColumn(solSutun, 0);
            mainGrid.Children.Add(solSutun);

            // ORTA SÜTUN - Cihaz Bilgileri
            StackPanel ortaSutun = new StackPanel { Margin = new Thickness(0, 0, 10, 0) };

            AddBaslik(ortaSutun, "CİHAZ BİLGİLERİ");
            AddBilgi(ortaSutun, "Marka/Model:", string.IsNullOrWhiteSpace(cihaz) || cihaz == "Marka" ? "-" : cihaz);
            AddBilgi(ortaSutun, "Arıza:", string.IsNullOrWhiteSpace(ariza) || ariza == "Arıza" ? "-" : ariza);

            Grid.SetColumn(ortaSutun, 1);
            mainGrid.Children.Add(ortaSutun);

            // SAĞ SÜTUN - Şifre Bilgileri
            StackPanel sagSutun = new StackPanel();

            AddBaslik(sagSutun, "CİHAZ ŞİFRELERİ");

            // Desen alanı - 3x3 noktalar
            Border desenBorder = new Border
            {
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1),
                Background = Brushes.White,
                CornerRadius = new CornerRadius(3),
                Padding = new Thickness(5),
                Margin = new Thickness(0, 5, 0, 5),
                Height = 70
            };

            StackPanel desenStack = new StackPanel();
            desenStack.Children.Add(new TextBlock
            {
                Text = "Desen:",
                FontSize = 8,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.Gray,
                Margin = new Thickness(0, 0, 0, 3)
            });

            // 3x3 Desen Grid
            Grid desenGrid = CreateDesenGrid();
            desenStack.Children.Add(desenGrid);

            desenBorder.Child = desenStack;
            sagSutun.Children.Add(desenBorder);

            // Pin kodu alanı
            Border pinBorder = new Border
            {
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1),
                Background = Brushes.White,
                CornerRadius = new CornerRadius(3),
                Padding = new Thickness(8),
                Height = 40,
                Child = new StackPanel
                {
                    Children =
            {
                new TextBlock { Text = "Pin/Şifre:", FontSize = 8, FontWeight = FontWeights.SemiBold, Foreground = Brushes.Gray },
                new TextBlock { Text = "____________________", FontSize = 11, Margin = new Thickness(0, 3, 0, 0) }
            }
                }
            };
            sagSutun.Children.Add(pinBorder);

            Grid.SetColumn(sagSutun, 2);
            mainGrid.Children.Add(sagSutun);

            Grid.SetRow(mainGrid, 2);
            anaGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            anaGrid.Children.Add(mainGrid);

            // ============================================
            // İMZA BÖLÜMÜ
            // ============================================

            Border imzaAyirici = new Border
            {
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(0, 1, 0, 0),
                Margin = new Thickness(0, 5, 0, 8)
            };
            Grid.SetRow(imzaAyirici, 3);
            anaGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            anaGrid.Children.Add(imzaAyirici);

            Grid imzaGrid = new Grid();
            imzaGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            imzaGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Müşteri imzası
            StackPanel musteriStack = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
            musteriStack.Children.Add(new TextBlock
            {
                Text = "MÜŞTERİ İMZASI",
                FontSize = 8,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.Gray,
                Margin = new Thickness(0, 0, 0, 15)
            });
            musteriStack.Children.Add(new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0, 0, 0, 1),
                Width = 130
            });
            Grid.SetColumn(musteriStack, 0);
            imzaGrid.Children.Add(musteriStack);

            // Yetkili imzası
            StackPanel yetkiliStack = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
            yetkiliStack.Children.Add(new TextBlock
            {
                Text = "YETKİLİ İMZASI",
                FontSize = 8,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.Gray,
                Margin = new Thickness(0, 0, 0, 15)
            });
            yetkiliStack.Children.Add(new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0, 0, 0, 1),
                Width = 130
            });
            Grid.SetColumn(yetkiliStack, 1);
            imzaGrid.Children.Add(yetkiliStack);

            Grid.SetRow(imzaGrid, 4);
            anaGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            anaGrid.Children.Add(imzaGrid);

            // ============================================
            // ALT BİLGİ
            // ============================================

            TextBlock altBilgi = new TextBlock
            {
                Text = "Cihazınız teslim alındığında lütfen bu fişi gösteriniz. Bu belge, cihazın servisimize teslim edildiğini belgeler.",
                FontSize = 7,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 8, 0, 0),
                Foreground = Brushes.Gray,
                FontStyle = FontStyles.Italic,
                TextWrapping = TextWrapping.Wrap
            };
            Grid.SetRow(altBilgi, 5);
            anaGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            anaGrid.Children.Add(altBilgi);

            // Ana grid'i FlowDocument'e ekle
            BlockUIContainer mainContainer = new BlockUIContainer(anaGrid);
            doc.Blocks.Add(mainContainer);

            return doc;
        }

        // 3x3 Desen Grid oluştur
        private Grid CreateDesenGrid()
        {
            Grid grid = new Grid
            {
                Width = 50,
                Height = 50,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // 3 satır, 3 sütun
            for (int i = 0; i < 3; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition());
                grid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            // 9 nokta ekle
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    Ellipse nokta = new Ellipse
                    {
                        Width = 8,
                        Height = 8,
                        Fill = Brushes.Gray,
                        Stroke = Brushes.DarkGray,
                        StrokeThickness = 1,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    Grid.SetRow(nokta, row);
                    Grid.SetColumn(nokta, col);
                    grid.Children.Add(nokta);
                }
            }

            return grid;
        }

        // Yardımcı metodlar
        private void AddBaslik(StackPanel panel, string baslik)
        {
            panel.Children.Add(new TextBlock
            {
                Text = baslik,
                FontSize = 9,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(37, 43, 54)),
                Margin = new Thickness(0, 0, 0, 6),
                Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                Padding = new Thickness(5, 2, 5, 2)
            });
        }

        private void AddBilgi(StackPanel panel, string label, string deger)
        {
            StackPanel itemPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 5) };

            itemPanel.Children.Add(new TextBlock
            {
                Text = label,
                FontSize = 8,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.Gray
            });

            itemPanel.Children.Add(new TextBlock
            {
                Text = deger,
                FontSize = 9,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 1, 0, 0)
            });

            panel.Children.Add(itemPanel);
        }

        // Şirket adını veritabanından çek
        private string GetSirketAdi()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    // Varsayılan olarak ilk şirketi al (veya kullanıcı ID'sine göre belirle)
                    SqlCommand cmd = new SqlCommand(@"
                SELECT TOP 1 CompanyName FROM Companies 
                ORDER BY Id", con);

                    object result = cmd.ExecuteScalar();
                    if (result != null && !string.IsNullOrWhiteSpace(result.ToString()))
                    {
                        return result.ToString().ToUpper();
                    }
                }
            }
            catch
            {
                // Hata olursa varsayılan
            }

            return "SERVİS TAKİP";
        }

        // Yardımcı metodlar

        private void AddInfoRow(FlowDocument doc, string label, string value)
        {
            Grid grid = new Grid { Margin = new Thickness(0, 0, 0, 8) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Label
            TextBlock labelBlock = new TextBlock
            {
                Text = label,
                FontWeight = FontWeights.SemiBold,
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(52, 73, 94))
            };
            Grid.SetColumn(labelBlock, 0);
            grid.Children.Add(labelBlock);

            // Value
            TextBlock valueBlock = new TextBlock
            {
                Text = value,
                FontSize = 11,
                TextWrapping = TextWrapping.Wrap
            };
            Grid.SetColumn(valueBlock, 1);
            grid.Children.Add(valueBlock);

            BlockUIContainer container = new BlockUIContainer(grid);
            doc.Blocks.Add(container);
        }

        private TableRow CreateTableRow(string baslik, string deger)
        {
            TableRow row = new TableRow();

            TableCell cell1 = new TableCell(new Paragraph(new Run(baslik))
            {
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0)
            })
            {
                Padding = new Thickness(8),
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(0, 0, 1, 1)
            };

            TableCell cell2 = new TableCell(new Paragraph(new Run(deger))
            {
                Margin = new Thickness(0)
            })
            {
                Padding = new Thickness(8),
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(0, 0, 0, 1)
            };

            row.Cells.Add(cell1);
            row.Cells.Add(cell2);

            return row;
        }



        // =========================
        // DİĞER YARDIMCI METOTLAR
        // =========================
        private void LoadRecordDetails(int id)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("SELECT * FROM disserviskayitlarnew WHERE KayitID=@id", con);
                    cmd.Parameters.AddWithValue("@id", id);
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            SetTextBoxValue(isimsoyisimdisservis, r["IsimSoyisim"]);
                            SetTextBoxValue(telefonnumarasidisservis, r["TelefonNumarasi"]);
                            SetTextBoxValue(markadisservis, r["Marka"]);
                            SetTextBoxValue(modeldisservis, r["Model"]);
                            SetTextBoxValue(arizadisservis, r["Ariza"]);
                            SetTextBoxValue(ilgiliteknisyendisservis, r["İlgiliTeknisyen"]);
                            SetTextBoxValue(bayiadidisservis, r["YetkiliBayi"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veri yükleme hatası: " + ex.Message);
            }
        }

        private void SetTextBoxValue(TextBox tb, object value)
        {
            string val = value?.ToString() ?? "";
            if (!string.IsNullOrWhiteSpace(val))
            {
                tb.Text = val;
                tb.Foreground = Brushes.Black;
            }
        }

        private void btnFisYazdir_Click(object sender, RoutedEventArgs e)
        {
            FisYazdir(isimsoyisimdisservis.Text, markadisservis.Text, arizadisservis.Text, telefonnumarasidisservis.Text);
        }

        private void SetButtonVisibility()
        {
            if (_kayitId > 0)
            {
                Button btn = FindName("sarviskaydınıkaydet_Kopyala") as Button;
                if (btn != null) btn.Visibility = Visibility.Collapsed;
            }
        }

        private void disserviskaydigeridön_Click(object sender, RoutedEventArgs e)
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

        // PLACEHOLDER LOGICS
        private void InitializePlaceholders()
        {
            if (_kayitId == 0)
            {
                SetPlaceholder(ilgiliteknisyendisservis, PlaceholderTeknisyen);
                SetPlaceholder(bayiadidisservis, PlaceholderBayi);
                SetPlaceholder(isimsoyisimdisservis, PlaceholderAdSoyad);
                SetPlaceholder(telefonnumarasidisservis, PlaceholderTelefon);
                SetPlaceholder(markadisservis, PlaceholderMarka);
                SetPlaceholder(modeldisservis, PlaceholderModel);
                SetPlaceholder(arizadisservis, PlaceholderAriza);
            }
        }

        private void SetPlaceholder(TextBox tb, string text) { tb.Text = text; tb.Foreground = Brushes.Gray; }
        private void ClearPlaceholder(TextBox tb, string p) { if (tb.Text == p) { tb.Text = ""; tb.Foreground = Brushes.Black; } }
        private void RestorePlaceholder(TextBox tb, string p) { if (string.IsNullOrWhiteSpace(tb.Text)) { tb.Text = p; tb.Foreground = Brushes.Gray; } }

        private void ilgiliteknisyendisservis_GotFocus(object sender, RoutedEventArgs e) => ClearPlaceholder(ilgiliteknisyendisservis, PlaceholderTeknisyen);
        private void ilgiliteknisyendisservis_LostFocus(object sender, RoutedEventArgs e) => RestorePlaceholder(ilgiliteknisyendisservis, PlaceholderTeknisyen);
        private void bayiadidisservis_GotFocus(object sender, RoutedEventArgs e) => ClearPlaceholder(bayiadidisservis, PlaceholderBayi);
        private void bayiadidisservis_LostFocus(object sender, RoutedEventArgs e) => RestorePlaceholder(bayiadidisservis, PlaceholderBayi);
        private void isimsoyisimdisservis_GotFocus(object sender, RoutedEventArgs e) => ClearPlaceholder(isimsoyisimdisservis, PlaceholderAdSoyad);
        private void isimsoyisimdisservis_LostFocus(object sender, RoutedEventArgs e) => RestorePlaceholder(isimsoyisimdisservis, PlaceholderAdSoyad);
        private void telefonnumarasidisservis_GotFocus(object sender, RoutedEventArgs e) => ClearPlaceholder(telefonnumarasidisservis, PlaceholderTelefon);
        private void telefonnumarasidisservis_LostFocus(object sender, RoutedEventArgs e) => RestorePlaceholder(telefonnumarasidisservis, PlaceholderTelefon);
        private void markadisservis_GotFocus(object sender, RoutedEventArgs e) => ClearPlaceholder(markadisservis, PlaceholderMarka);
        private void markadisservis_LostFocus(object sender, RoutedEventArgs e) => RestorePlaceholder(markadisservis, PlaceholderMarka);
        private void modeldisservis_GotFocus(object sender, RoutedEventArgs e) => ClearPlaceholder(modeldisservis, PlaceholderModel);
        private void modeldisservis_LostFocus(object sender, RoutedEventArgs e) => RestorePlaceholder(modeldisservis, PlaceholderModel);
        private void arizadisservis_GotFocus(object sender, RoutedEventArgs e) => ClearPlaceholder(arizadisservis, PlaceholderAriza);
        private void arizadisservis_LostFocus(object sender, RoutedEventArgs e) => RestorePlaceholder(arizadisservis, PlaceholderAriza);
    }
}