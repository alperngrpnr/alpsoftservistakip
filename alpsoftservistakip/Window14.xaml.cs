using System;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace alpsoftservistakip
{
    public partial class Window14 : Window
    {
        private string asilKod;
        private string kullaniciEmail;
        public bool DogrulandiMi { get; set; } = false;

        public Window14(string gelenKod, string gelenEmail)
        {
            InitializeComponent();
            asilKod = gelenKod;
            kullaniciEmail = gelenEmail;
        }

        private void btnIslem_Click(object sender, RoutedEventArgs e)
        {
            // AŞAMA 1: KOD DOĞRULAMA
            if (pnlYeniSifre.Visibility == Visibility.Collapsed)
            {
                if (txtCode.Text.Trim() == asilKod)
                {
                    // Başarılı: Kod alanını kapat, şifre alanını aç
                    txtCode.Visibility = Visibility.Collapsed;
                    pnlYeniSifre.Visibility = Visibility.Visible;
                    lblBilgi.Text = "Yeni şifrenizi giriniz:";
                    btnIslem.Content = "ŞİFREYİ GÜNCELLE";
                }
                else
                {
                    MessageBox.Show("Girdiğiniz kod hatalı!", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            // AŞAMA 2: ŞİFRE GÜNCELLEME
            else
            {
                string yeniSifre = txtYeniSifre.Password;
                string tekrarSifre = txtYeniSifreTekrar.Password;

                if (string.IsNullOrEmpty(yeniSifre) || yeniSifre.Length < 4)
                {
                    MessageBox.Show("Şifre en az 4 karakter olmalıdır!");
                    return;
                }

                if (yeniSifre != tekrarSifre)
                {
                    MessageBox.Show("Şifreler uyuşmuyor!");
                    return;
                }

                SifreyiGuncelle(yeniSifre);
            }
        }

        private void SifreyiGuncelle(string sifre)
        {
            try
            {
                string hashedSifre = HashSifre(sifre);

                using (SqlConnection conn = new SqlConnection("Server=91.247.168.204,1433;Database=alpsoftservistakip;User Id=sa;Password=Alperengurpinar4160552009;TrustServerCertificate=True;"))
                {
                    conn.Open();
                    // Users tablosundaki şifre sütun isminin 'PasswordHash' olduğundan emin ol
                    string sql = "UPDATE [Users] SET PasswordHash = @pass WHERE Email = @email";

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@pass", hashedSifre);
                    cmd.Parameters.AddWithValue("@email", kullaniciEmail);

                    int count = cmd.ExecuteNonQuery();
                    if (count > 0)
                    {
                        MessageBox.Show("Şifreniz başarıyla güncellendi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Kullanıcı bulunamadı.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata oluştu: " + ex.Message);
            }
        }

        private string HashSifre(string sifre)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(sifre));
                StringBuilder builder = new StringBuilder();
                foreach (var b in bytes) builder.Append(b.ToString("x2"));
                return builder.ToString();
            }
        }
    }
}