using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls; // Buton, TextBox ve DataGrid için gereklidir.
using System.Windows.Media.Animation;
using System.Threading.Tasks;
using Services = alpsoftservistakip.Services;

namespace alpsoftservistakip
{
    public partial class Window3 : Window
    {
        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
            else
                this.WindowState = WindowState.Maximized;

            UpdateMaximizeIcon();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // Tetikler Window_Closing eventini
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
            {
                if (e.ClickCount == 2)
                {
                    btnMaximize_Click(sender, new RoutedEventArgs());
                    return;
                }

                this.DragMove();
            }
        }

        // GÖRSELDEKİ VERİTABANI ADINA GÖRE GÜNCELLENDİ
        string connectionString = "Server=91.247.168.204,1433;" +
            "Database=alpsoftservistakip;" +
            "User Id=sa;" +
            "Password=Alperengurpinar4160552009;" +
            "TrustServerCertificate=True;";


        public Window3()
        {
            InitializeComponent();
            UpdateMaximizeIcon();

            // YETKİ KONTROLÜ: Giriş yapan kişi Admin değilse butonu gizle
            if (Class1.AktifKullanici.IsAdmin == false)
            {
                btnAdminPanelAc.Visibility = Visibility.Collapsed;

                // Opsiyonel: İşçinin lisans durumunu görmesini de istemeyebilirsin
                // txtLisansDurum.Visibility = Visibility.Collapsed; 
            }

            try { TabloyuGuncelle(); } catch { }
        }

        private void Window3_StateChanged(object sender, EventArgs e)
        {
            UpdateMaximizeIcon();
        }

        private void UpdateMaximizeIcon()
        {
            if (txtMaximizeIcon == null)
                return;

            txtMaximizeIcon.Text = this.WindowState == WindowState.Maximized ? "\uE923" : "\uE922";
        }

        private void TabloyuGuncelle()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    // Sadece giriş yapan kullanıcının şirketine ait işçileri getir
                    string query = "SELECT FullName, Email, Role FROM Users WHERE CompanyId = @sirketId AND IsActive = 1";

                    SqlCommand cmd = new SqlCommand(query, con);
                    // LoginWindow'da atadığımız Şirket ID'sini buraya parametre olarak gönderiyoruz
                    cmd.Parameters.AddWithValue("@sirketId", Class1.AktifKullanici.SirketID);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgIsciler.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veriler yüklenirken hata oluştu: " + ex.Message);
            }
        }
        // Önce metodun aynısını buraya ekle (LoginWindow ile aynı olmalı)
        private string HashSifre(string sifre)
        {
            using (System.Security.Cryptography.SHA256 sha = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(sifre));
                System.Text.StringBuilder builder = new System.Text.StringBuilder();
                foreach (var b in bytes)
                    builder.Append(b.ToString("x2"));
                return builder.ToString();
            }
        }

        private void IsciKaydet_Click(object sender, RoutedEventArgs e)
        {
            // Boş alan kontrolü
            if (string.IsNullOrEmpty(txtYeniAd.Text) ||
                string.IsNullOrEmpty(txtYeniEmail.Text) ||
                string.IsNullOrEmpty(txtYeniSifre.Text))
            {
                MessageBox.Show("Lütfen tüm alanları (Ad, Email, Şifre) doldurun!");
                return;
            }

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    // Artık şifreyi kutudan alıyoruz ve hashliyoruz
                    string girilenSifre = txtYeniSifre.Text.Trim();
                    string hashliSifre = HashSifre(girilenSifre);

                    string query = "INSERT INTO Users (CompanyId, FullName, Email, PasswordHash, Role, IsActive, LicenseId) " +
                                   "VALUES (3, @p1, @p2, @p3, 'Worker', 1, 3)";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@p1", txtYeniAd.Text.Trim());
                    cmd.Parameters.AddWithValue("@p2", txtYeniEmail.Text.Trim().ToLower());
                    cmd.Parameters.AddWithValue("@p3", hashliSifre);

                    cmd.ExecuteNonQuery();

                    MessageBox.Show($"İşçi başarıyla oluşturuldu!\nEmail: {txtYeniEmail.Text}\nŞifre: {girilenSifre}");

                    // Temizlik
                    txtYeniAd.Clear();
                    txtYeniEmail.Clear();
                    txtYeniSifre.Clear();
                    TabloyuGuncelle();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kayıt sırasında hata oluştu: " + ex.Message);
            }
        }

        private void IsciSil_Click(object sender, RoutedEventArgs e)
        {
            // Butonun Tag özelliğine bağladığımız Email bilgisini alıyoruz
            var email = (sender as Button).Tag.ToString();

            MessageBoxResult result = MessageBox.Show($"{email} adresli işçiyi silmek istediğinize emin misiniz?", "Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection con = new SqlConnection(connectionString))
                    {
                        con.Open();
                        // Veritabanından tamamen silmek yerine IsActive = 0 yapabilirsin veya direkt DELETE kullanabilirsin
                        string query = "DELETE FROM Users WHERE Email = @p1 AND Role = 'Worker'";
                        SqlCommand cmd = new SqlCommand(query, con);
                        cmd.Parameters.AddWithValue("@p1", email);
                        cmd.ExecuteNonQuery();

                        MessageBox.Show("İşçi başarıyla silindi.");
                        TabloyuGuncelle(); // Listeyi yenile
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Hata: " + ex.Message);
                }
            }
        }
        private void btnAdminPanelAc_Click(object sender, RoutedEventArgs e)
        {
            AdminPanelTabakasi.Visibility = Visibility.Visible;
            TabloyuGuncelle();
        }

        private void AdminPanelKapat_Click(object sender, RoutedEventArgs e) => AdminPanelTabakasi.Visibility = Visibility.Collapsed;

        // Overlay navigation methods - Window5 içeriğini Page olarak göster
        private void kayitlar_Click(object sender, RoutedEventArgs e) 
        { 
            // Direkt sayfayı göster - overlay zaten açıksa yeni sayfaya geçer
            ShowOverlayPage(new Page5()); 
        }
        
        private void kayitolustur_Click(object sender, RoutedEventArgs e) 
        { 
            ShowOverlayPage(new PageKayitOlustur()); 
        }

        private void disserviskayitlari_Click(object sender, RoutedEventArgs e)
        {
            PageKayitOlustur page = new PageKayitOlustur();
            ShowOverlayPage(page);
        }

        private void caritakip_Click(object sender, RoutedEventArgs e)
        {
            ShowOverlayPage(new PageCariListesi());
        }

        private void btnToptanciYonetimi_Click(object sender, RoutedEventArgs e)
        {
            ShowOverlayPage(new PageToptanciListesi());
        }

        private void disserviskayitlari_Copy_Click(object sender, RoutedEventArgs e)
        {
            // Dış servis kayıtları sayfasını overlay içinde aç
            PageDisServis page = new PageDisServis();
            ShowOverlayPage(page);
        }

        // Element-level fade helpers for smooth in-window page transitions
        private Task FadeOutElementAsync(UIElement el, int ms = 200)
        {
            var tcs = new TaskCompletionSource<bool>();
            var da = new DoubleAnimation(0, TimeSpan.FromMilliseconds(ms));
            da.FillBehavior = FillBehavior.Stop;
            da.Completed += (s, e) =>
            {
                el.Dispatcher.Invoke(() => el.Opacity = 0);
                tcs.SetResult(true);
            };
            el.Dispatcher.Invoke(() => el.BeginAnimation(UIElement.OpacityProperty, da));
            return tcs.Task;
        }

        private Task FadeInElementAsync(UIElement el, int ms = 200)
        {
            var tcs = new TaskCompletionSource<bool>();
            var da = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(ms));
            da.FillBehavior = FillBehavior.Stop;
            da.Completed += (s, e) =>
            {
                el.Dispatcher.Invoke(() => el.Opacity = 1);
                tcs.SetResult(true);
            };
            el.Dispatcher.Invoke(() =>
            {
                el.Opacity = 0;
                el.BeginAnimation(UIElement.OpacityProperty, da);
            });
            return tcs.Task;
        }

        private Task FadeToElementAsync(UIElement el, double to, int ms = 200)
        {
            var tcs = new TaskCompletionSource<bool>();
            var da = new DoubleAnimation(to, TimeSpan.FromMilliseconds(ms));
            da.FillBehavior = FillBehavior.Stop;
            da.Completed += (s, e) =>
            {
                el.Dispatcher.Invoke(() => el.Opacity = to);
                tcs.SetResult(true);
            };
            el.Dispatcher.Invoke(() => el.BeginAnimation(UIElement.OpacityProperty, da));
            return tcs.Task;
        }

        public void ShowOverlayPage(System.Windows.Controls.Page page)
        {
            try
            {
                // Page5 için backdrop gösterme (blur sorunu için)
                if (page is Page5)
                {
                    OverlayBackdrop.Visibility = Visibility.Collapsed;
                    OverlayBackdrop.Opacity = 0;
                }
                else
                {
                    OverlayBackdrop.Visibility = Visibility.Visible;
                    OverlayBackdrop.Opacity = 0.6;
                }

                OverlayFrame.Visibility = Visibility.Visible;
                OverlayFrame.Opacity = 1;
                
                OverlayFrame.Navigate(page);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Geçiş sırasında hata: " + ex.Message);
            }
        }

        public void HideOverlay()
        {
            try
            {
                // Animasyon yok, direkt kapat
                OverlayFrame.Visibility = Visibility.Collapsed;
                OverlayFrame.Content = null;
                OverlayFrame.Opacity = 1;

                OverlayBackdrop.Visibility = Visibility.Collapsed;
                OverlayBackdrop.Opacity = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Geçiş sırasında hata: " + ex.Message);
                OverlayFrame.Visibility = Visibility.Collapsed;
                OverlayFrame.Content = null;
                OverlayFrame.Opacity = 1;
            }
        }
        private void AltPencere_Closing(object sender, CancelEventArgs e) { this.Close(); }

        private void YetkiKontrolü()
        {
            // Eğer giriş yapan kişi Admin değilse Admin Paneli butonunu gizle
            if (!Class1.AktifKullanici.IsAdmin)
            {
                btnAdminPanelAc.Visibility = Visibility.Collapsed;
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                if (Class1.AktifKullanici.ID > 0)
                {
                    using (SqlConnection con = new SqlConnection(connectionString))
                    {
                        con.Open();
                        using (SqlCommand cmd = new SqlCommand("UPDATE [Users] SET IsLoggedIn = 0 WHERE Id = @id", con))
                        {
                            cmd.Parameters.AddWithValue("@id", Class1.AktifKullanici.ID);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch { }

            Application.Current.Shutdown();
        }
    }
}
