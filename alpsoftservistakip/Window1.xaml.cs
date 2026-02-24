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
                string subject = "Şifre Sıfırlama Doğrulama Kodu";
                string body = $"<div style='font-family:Arial;'><h2>ALPSOFT</h2><p>Kodunuz: <b>{dogrulamaKodu}</b></p></div>";

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

                    // Şirket ve Kullanıcı bilgilerini birleştirerek alıyoruz
                    string sql = @"
                        SELECT u.Id, u.CompanyId, u.FullName, u.Role, c.CompanyName 
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

                            // Eski değişkeni de destekle
                            aktifKullaniciID = Class1.AktifKullanici.ID;

                            MessageBox.Show($"Hoş geldiniz, {Class1.AktifKullanici.KullaniciAdi}!\nŞirket: {Class1.AktifKullanici.SirketAdi}",
                                "Giriş Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);

                            // 3. Geçiş Yap
                            Window3 mainWin = new Window3();
                            mainWin.Show();
                            this.Hide(); // Window3 kapandığında Shutdown olması için Hide mantıklı
                        }
                        else
                        {
                            MessageBox.Show("E-posta veya şifre hatalı!", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Bağlantı Hatası: " + ex.Message, "Sistem Hatası", MessageBoxButton.OK, MessageBoxImage.Stop);
                }
            }
        }

       
    }
}