using alpsoftservistakip.Helpers;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace alpsoftservistakip
{
    public partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            InitializeComponent();
        }

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

        private async void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            string adSoyad = txtFullName.Text.Trim();
            string eposta = txtEmail.Text.Trim();
            string sifre = isPassword1Visible ? txtPasswordVisible.Text : txtPassword.Password;
            string sifreTekrar = isPassword2Visible ? txtPasswordRepeatVisible.Text : txtPasswordRepeat.Password;
            string sirketAdi = txtCompanyName.Text.Trim();
            string subeAdi = string.IsNullOrWhiteSpace(txtBranchName.Text) ? "Merkez Şube" : txtBranchName.Text.Trim();
            string aktivasyon = txtActivationCode.Text.Trim();

            // 1. Zorunlu Alan Kontrolü
            if (string.IsNullOrEmpty(adSoyad) || string.IsNullOrEmpty(eposta) || string.IsNullOrEmpty(sifre) || string.IsNullOrEmpty(sirketAdi))
            {
                MessageBox.Show("Lütfen zorunlu alanları eksiksiz doldurun!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Şifre Eşleşme Kontrolü
            if (sifre != sifreTekrar)
            {
                MessageBox.Show("Şifreler uyuşmuyor!", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                // 3. API Bağlantısını Kuruyoruz
                var client = new HttpClient
                {
                    BaseAddress = new Uri(ApiConfig.Api),
                    Timeout = TimeSpan.FromSeconds(30)
                };

                // API'nin beklediği Register modeline (DTO) göre verileri paketliyoruz
                var registerData = new
                {
                    FullName = adSoyad,
                    Email = eposta,
                    PasswordHash = HashSifre(sifre),
                    CompanyName = sirketAdi,
                    BranchName = subeAdi,
                    Address = txtAddress.Text.Trim(),
                    ActivationCode = aktivasyon
                };

                string json = JsonConvert.SerializeObject(registerData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // API üzerindeki Auth/register endpoint'ine istek atıyoruz
                var response = await client.PostAsync($"{ApiConfig.Api}/Auth/register", content);

                if (response.IsSuccessStatusCode)
                {
                    Class1.AktifKullanici.SirketAdi = sirketAdi;
                    Class1.AktifKullanici.AdSoyad = adSoyad;
                    Class1.AktifKullanici.Email = eposta;
                    Properties.Settings.Default.HatirlananEposta = eposta;
                    Properties.Settings.Default.Save();

                    MessageBox.Show("Şirket kaydı başarıyla oluşturuldu! Giriş ekranına yönlendiriliyorsunuz.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);

                    // 4. KAYIT BAŞARILI: Kullanıcıyı akıcı bir şekilde giriş sayfasına geri gönderiyoruz
                    NavigationService?.Navigate(new LoginPage());
                }
                else
                {
                    string errorResult = await response.Content.ReadAsStringAsync();
                    string errorMessage = $"Kayıt başarısız - Status: {(int)response.StatusCode}\n\nDetay: {errorResult}";
                    MessageBox.Show(errorMessage, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"API Bağlantı Hatası: {ex.Message}", "Sistem Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool isPassword1Visible = false;
        private bool isPassword2Visible = false;

        private void btnTogglePassword1_Click(object sender, RoutedEventArgs e)
        {
            isPassword1Visible = !isPassword1Visible;
            if (isPassword1Visible)
            {
                txtPasswordVisible.Text = txtPassword.Password;
                txtPasswordVisible.Visibility = Visibility.Visible;
                txtPassword.Visibility = Visibility.Collapsed;
                iconPassword1.Text = "\uE18C";
            }
            else
            {
                txtPassword.Password = txtPasswordVisible.Text;
                txtPassword.Visibility = Visibility.Visible;
                txtPasswordVisible.Visibility = Visibility.Collapsed;
                iconPassword1.Text = "\uE18B";
            }
        }

        private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!isPassword1Visible) txtPasswordVisible.Text = txtPassword.Password;
        }

        private void txtPasswordVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isPassword1Visible) txtPassword.Password = txtPasswordVisible.Text;
        }

        private void btnTogglePassword2_Click(object sender, RoutedEventArgs e)
        {
            isPassword2Visible = !isPassword2Visible;
            if (isPassword2Visible)
            {
                txtPasswordRepeatVisible.Text = txtPasswordRepeat.Password;
                txtPasswordRepeatVisible.Visibility = Visibility.Visible;
                txtPasswordRepeat.Visibility = Visibility.Collapsed;
                iconPassword2.Text = "\uE18C";
            }
            else
            {
                txtPasswordRepeat.Password = txtPasswordRepeatVisible.Text;
                txtPasswordRepeat.Visibility = Visibility.Visible;
                txtPasswordRepeatVisible.Visibility = Visibility.Collapsed;
                iconPassword2.Text = "\uE18B";
            }
        }

        private void txtPasswordRepeat_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!isPassword2Visible) txtPasswordRepeatVisible.Text = txtPasswordRepeat.Password;
        }

        private void txtPasswordRepeatVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isPassword2Visible) txtPasswordRepeat.Password = txtPasswordRepeatVisible.Text;
        }

        // Giriş Yap Sayfasına Geri Dönüş
        private void BtnGitGirisYap_Click(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new LoginPage());
        }
    }
}
