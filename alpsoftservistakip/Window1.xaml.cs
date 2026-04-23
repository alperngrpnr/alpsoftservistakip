using System;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Services = alpsoftservistakip.Services;

namespace alpsoftservistakip
{
    public partial class LoginWindow : Window
    {
        // Global ID takibi için (Window5'te de kullanılabilir)
        public static int aktifKullaniciID = 0;

        private readonly string connString =
            "Server=91.247.168.204,1433;Database=alpsoftservistakip;User Id=sa;Password=Alperengurpinar4160552009;TrustServerCertificate=True;";

        public LoginWindow()
        {
            InitializeComponent();
            Loaded += LoginWindow_Loaded;
            this.Closing += AltPencere_Closing;
        }

        private void AltPencere_Closing(object sender, CancelEventArgs e)
        {
            // Eğer ana pencereye geçiş yapılmadıysa uygulamayı kapat
            if (Application.Current.Windows.Count <= 1)
                Application.Current.Shutdown();
        }

        

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            login();
        }

        private string HashSifre(string sifre)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(sifre));
                StringBuilder builder = new StringBuilder();
                foreach (var b in bytes)
                    builder.Append(b.ToString("x2"));
                return builder.ToString();
            }
        }

        private void LoginWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Properties.Settings.Default.HatirlananEposta))
            {
                txtUser.Text = Properties.Settings.Default.HatirlananEposta;
                chkHatirla.IsChecked = true;
                txtPass.Focus();
            }
        }

        private void SifremiUnuttum_Click(object sender, MouseButtonEventArgs e)
        {
            string email = txtUser.Text.Trim();
            if (string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Lütfen önce e-posta kutusuna adresinizi yazın.");
                return;
            }

            Random rnd = new Random();
            string kod = rnd.Next(100000, 999999).ToString();
            MailGonder(email, kod);

            Window14 verifyWin = new Window14(kod, email);
            verifyWin.ShowDialog();
        }

        private void MailGonder(string aliciEmail, string dogrulamaKodu)
        {
            try
            {
                var fromAddress = new MailAddress("alpsoft41@gmail.com", "Alpsoft Yazılım");
                var toAddress = new MailAddress(aliciEmail);
                string subject = "Şifre Sıfırlama Doğrulama Kodu - ALPSOFT";

                // Mail içerik tasarımı oldukça modern ve profesyonel bir görünüme kavuşturuldu.
                string body = $@"
                <div style='font-family: ""Segoe UI"", Arial, sans-serif; background-color: #F4F7F6; padding: 40px 10px;'>
                    <div style='max-width: 600px; margin: 0 auto; background-color: #FFFFFF; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 15px rgba(0,0,0,0.05);'>

                        <!-- Üst Banner (Logolu kısım) -->
                        <div style='background-color: #252B36; padding: 30px; text-align: center;'>
                            <h1 style='color: #FFF5C721; margin: 0; font-size: 28px; letter-spacing: 2px;'>ALPSOFT</h1>
                            <p style='color: #A0AEC0; margin: 5px 0 0 0; font-size: 14px;'>Servis Yönetim Platformu</p>
                        </div>

                        <!-- Ana İçerik -->
                        <div style='padding: 40px 30px;'>
                            <h2 style='color: #2D3748; margin-top: 0;'>Şifre Sıfırlama Talebi</h2>
                            <p style='font-size: 16px; line-height: 1.6; color: #4A5568;'>
                                Merhaba,<br><br>
                                Hesabınızın şifresini sıfırlamak için bir talepte bulundunuz. İşleminize devam etmek için aşağıdaki 6 haneli doğrulama kodunu kullanabilirsiniz:
                            </p>

                            <!-- Kod Kutusu -->
                            <div style='background-color: #F8FAFC; border: 2px dashed #CBD5E1; border-radius: 8px; padding: 25px; text-align: center; margin: 35px 0;'>
                                <span style='font-size: 36px; font-weight: bold; letter-spacing: 8px; color: #E74C3C;'>{dogrulamaKodu}</span>
                            </div>

                            <p style='font-size: 14px; color: #718096; line-height: 1.5;'>
                                Eğer bu talebi siz yapmadıysanız, lütfen bu e-postayı dikkate almayın ve hesabınızın güvende olduğundan emin olun.<br><br>
                                <strong>Önemli:</strong> Güvenliğiniz için bu doğrulama kodunu hiç kimseyle paylaşmayınız.
                            </p>
                        </div>

                        <!-- Alt Kısım (Footer) -->
                        <div style='background-color: #F1F5F9; padding: 20px; text-align: center; border-top: 1px solid #E2E8F0;'>
                            <p style='margin: 0; font-size: 12px; color: #A0AEC0;'>
                                &copy; {DateTime.Now.Year} Alpsoft Yazılım - Tüm hakları saklıdır.
                            </p>
                        </div>

                    </div>
                </div>";

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, "xuugdfgfhzmrtcji")
                };

                using (var message = new MailMessage(fromAddress, toAddress) { Subject = subject, Body = body, IsBodyHtml = true })
                {
                    smtp.Send(message);
                }
            }
            catch (Exception ex) { MessageBox.Show("Mail hatası: " + ex.Message); }
        }

        private async void KayitOl_Click(object sender, MouseButtonEventArgs e)
        {
            await Services.NavigationService.ShowWindowAsync(new WindowRegister());
            this.Close();
        }


        private void txtPass_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {

                login();
                e.Handled = true;
            }
        }

        private void btnDemo_Click(object sender, RoutedEventArgs e)
        {
            GridDemoPopup.Visibility = Visibility.Visible;
            txtDemoName.Focus();
        }

        private void btnDemoIptal_Click(object sender, RoutedEventArgs e)
        {
            GridDemoPopup.Visibility = Visibility.Collapsed;
            txtDemoName.Clear();
        }

        private void btnDemoBasla_Click(object sender, RoutedEventArgs e)
        {
            string isim = txtDemoName.Text.Trim();
            if (string.IsNullOrEmpty(isim))
            {
                MessageBox.Show("Lütfen adınızı girin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();

                    // DemoLogins tablosu yoksa oluştur
                    string createTableSql = @"
                        IF NOT EXISTS(SELECT * FROM sys.tables WHERE name = 'DemoLogins')
                        BEGIN
                            CREATE TABLE DemoLogins (
                                Id INT IDENTITY(1,1) PRIMARY KEY,
                                Name NVARCHAR(MAX),
                                LoginDate DATETIME DEFAULT GETDATE()
                            )
                        END";
                    using (SqlCommand cmdCreate = new SqlCommand(createTableSql, conn))
                    {
                        cmdCreate.ExecuteNonQuery();
                    }

                    // Demo kaydını kaydet
                    string insertSql = "INSERT INTO DemoLogins (Name, LoginDate) VALUES (@p1, GETDATE())";
                    using (SqlCommand cmdInsert = new SqlCommand(insertSql, conn))
                    {
                        cmdInsert.Parameters.AddWithValue("@p1", isim);
                        cmdInsert.ExecuteNonQuery();
                    }
                }

                // Demo kullanıcı ayarları
                Class1.AktifKullanici.ID = -1; // Gerçek bir kullanıcı değil
                Class1.AktifKullanici.SirketID = -1; // Gerçek şirket değil
                Class1.AktifKullanici.SirketAdi = "DEMO ŞİRKETİ";
                Class1.AktifKullanici.KullaniciAdi = isim;
                Class1.AktifKullanici.IsAdmin = true; // Demoda yönetim paneli vb. görebilsin
                Class1.AktifKullanici.HasStokTakibi = true; // Demoda stok takibini test edebilsin

                aktifKullaniciID = Class1.AktifKullanici.ID;

                MessageBox.Show($"Hoş geldiniz, {isim}!\nDemo sürümüne giriş yaptınız. Uygulama verileri üzerinde değişiklik yapabilirsiniz ancak diğer kullanıcılarla çakışmamak adına gerçek veri girmemenizi öneririz.",
                    "Demo Giriş Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);

                // Ana pencereyi aç
                Window3 mainWin = new Window3();
                mainWin.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Demo girişi sırasında bir hata oluştu: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void txtUser_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                login();
                e.Handled = true;
            }
        }


        private void txtUser_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void login()
        {
            string email = txtUser.Text.Trim();
            string password = txtPass.Password;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Lütfen tüm alanları doldurun.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string hashedInputPassword = HashSifre(password);

            using (SqlConnection conn = new SqlConnection(connString))
            {
                try
                {
                    conn.Open();

                    // IsLoggedIn kolonunu kontrol et ve yoksa ekle (Çok oturumlu girişi engellemek için)
                    string addColSql = "IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'IsLoggedIn' AND Object_ID = Object_ID(N'Users')) " +
                                       "BEGIN ALTER TABLE [Users] ADD IsLoggedIn BIT NOT NULL DEFAULT 0; END";
                    using (SqlCommand cmdAdd = new SqlCommand(addColSql, conn))
                    {
                        cmdAdd.ExecuteNonQuery();
                    }

                    // Modül yetkisi için Company tablosunda HasStockModule kontrolü ve ekleme
                    string addModuleColSql = "IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'HasStockModule' AND Object_ID = Object_ID(N'Companies')) " +
                                             "BEGIN ALTER TABLE [Companies] ADD HasStockModule BIT NOT NULL DEFAULT 0; END";
                    using (SqlCommand cmdModuleAdd = new SqlCommand(addModuleColSql, conn))
                    {
                        cmdModuleAdd.ExecuteNonQuery();
                    }

                    // Şirket ve Kullanıcı bilgilerini birleştirerek alıyoruz
                    string sql = @"
                        SELECT u.Id, u.CompanyId, u.FullName, u.Role, c.CompanyName, ISNULL(u.IsLoggedIn, 0) as IsLoggedIn, ISNULL(c.HasStockModule, 0) as HasStockModule
                        FROM [Users] u 
                        INNER JOIN Companies c ON u.CompanyId = c.Id 
                        WHERE u.Email = @email AND u.PasswordHash = @pass AND u.IsActive = 1";

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@email", email.ToLower());
                    cmd.Parameters.AddWithValue("@pass", hashedInputPassword);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            bool isLoggedIn = Convert.ToBoolean(reader["IsLoggedIn"]);

                            if (isLoggedIn)
                            {
                                MessageBoxResult result = MessageBox.Show("Bu hesap şu anda başka bir cihazda veya oturumda açık!\n\nGüvenlik gereği diğer oturumu kapatarak buradaki girişi zorlamak ister misiniz?", 
                                    "Oturum Zaten Açık", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                                if (result == MessageBoxResult.No)
                                {
                                    return; // Girişi engelle
                                }
                            }

                            // 1. Hatırlama Seçeneği
                            if (chkHatirla.IsChecked == true)
                            {
                                Properties.Settings.Default.HatirlananEposta = email;
                                Properties.Settings.Default.Save();
                            }
                            else
                            {
                                Properties.Settings.Default.HatirlananEposta = "";
                                Properties.Settings.Default.Save();
                            }

                            // 2. Class1 Deposunu Doldurma (KRİTİK NOKTA)
                            // Her iki nesne ismine de aynı veriyi basıyoruz ki hata çıkmasın
                            Class1.AktifKullanici.ID = Convert.ToInt32(reader["Id"]);
                            Class1.AktifKullanici.SirketID = Convert.ToInt32(reader["CompanyId"]);
                            Class1.AktifKullanici.SirketAdi = reader["CompanyName"].ToString();
                            Class1.AktifKullanici.KullaniciAdi = reader["FullName"].ToString();
                            Class1.AktifKullanici.IsAdmin = reader["Role"].ToString() == "Admin";
                            Class1.AktifKullanici.HasStokTakibi = Convert.ToBoolean(reader["HasStockModule"]);

                            // Eski değişkeni de destekle
                            aktifKullaniciID = Class1.AktifKullanici.ID;

                            MessageBox.Show($"Hoş geldiniz, {Class1.AktifKullanici.KullaniciAdi}!\nŞirket: {Class1.AktifKullanici.SirketAdi}",
                                "Giriş Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);

                        }
                        else
                        {
                            MessageBox.Show("E-posta veya şifre hatalı!", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    } // reader.Close() for next update query

                    // Oturumu Açıldı Olarak İşaretle
                    using (SqlCommand updCmd = new SqlCommand("UPDATE [Users] SET IsLoggedIn = 1 WHERE Id = @id", conn))
                    {
                        updCmd.Parameters.AddWithValue("@id", Class1.AktifKullanici.ID);
                        updCmd.ExecuteNonQuery();
                    }

                    // 3. Geçiş Yap
                    Window3 mainWin = new Window3();
                    mainWin.Show();
                    this.Hide(); // Window3 kapandığında Shutdown olması için Hide mantıklı
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Bağlantı Hatası: " + ex.Message, "Sistem Hatası", MessageBoxButton.OK, MessageBoxImage.Stop);
                }
            }
        }

       
    }
}
