using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace alpsoftservistakip.ViewModels
{
    public class Page3ViewModel : BaseViewModel
    {
        private readonly string connectionString = "Server=91.247.168.204,1433;" +
            "Database=alpsoftservistakip;" +
            "User Id=sa;" +
            "Password=Alperengurpinar4160552009;" +
            "TrustServerCertificate=True;";

        private DataView _isciler;
        private string _yeniAd;
        private string _yeniEmail;
        private string _yeniSifre;
        private bool _isAdminPanelVisible;
        private bool _isAdminPanelButtonVisible = true;

        public DataView Isciler
        {
            get => _isciler;
            set => SetProperty(ref _isciler, value);
        }

        public string YeniAd
        {
            get => _yeniAd;
            set => SetProperty(ref _yeniAd, value);
        }

        public string YeniEmail
        {
            get => _yeniEmail;
            set => SetProperty(ref _yeniEmail, value);
        }

        public string YeniSifre
        {
            get => _yeniSifre;
            set => SetProperty(ref _yeniSifre, value);
        }

        public bool IsAdminPanelVisible
        {
            get => _isAdminPanelVisible;
            set => SetProperty(ref _isAdminPanelVisible, value);
        }

        public bool IsAdminPanelButtonVisible
        {
            get => _isAdminPanelButtonVisible;
            set => SetProperty(ref _isAdminPanelButtonVisible, value);
        }

        public ICommand IsciKaydetCommand { get; }
        public ICommand IsciSilCommand { get; }
        public ICommand AdminPanelAcCommand { get; }
        public ICommand AdminPanelKapatCommand { get; }
        public ICommand KayitlarCommand { get; }
        public ICommand KayitOlusturCommand { get; }
        public ICommand DisServisKayitlariCommand { get; }

        public Page3ViewModel()
        {
            IsciKaydetCommand = new RelayCommand(_ => IsciKaydet());
            IsciSilCommand = new RelayCommand<string>(IsciSil);
            AdminPanelAcCommand = new RelayCommand(_ => AdminPanelAc());
            AdminPanelKapatCommand = new RelayCommand(_ => AdminPanelKapat());
            KayitlarCommand = new RelayCommand(_ => Kayitlar());
            KayitOlusturCommand = new RelayCommand(_ => KayitOlustur());
            DisServisKayitlariCommand = new RelayCommand(_ => DisServisKayitlari());

            if (Class1.AktifKullanici != null && Class1.AktifKullanici.IsAdmin == false)
            {
                IsAdminPanelButtonVisible = false;
            }

            TabloyuGuncelle();
        }

        private void TabloyuGuncelle()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "SELECT FullName, Email, Role FROM Users WHERE CompanyId = @sirketId AND IsActive = 1";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@sirketId", Class1.AktifKullanici.SirketID);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    Isciler = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veriler yüklenirken hata oluştu: " + ex.Message);
            }
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

        private void IsciKaydet()
        {
            if (string.IsNullOrEmpty(YeniAd) ||
                string.IsNullOrEmpty(YeniEmail) ||
                string.IsNullOrEmpty(YeniSifre))
            {
                MessageBox.Show("Lütfen tüm alanları (Ad, Email, Şifre) doldurun!");
                return;
            }

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string girilenSifre = YeniSifre.Trim();
                    string hashliSifre = HashSifre(girilenSifre);

                    string query = "INSERT INTO Users (CompanyId, FullName, Email, PasswordHash, Role, IsActive, LicenseId) " +
                                   "VALUES (3, @p1, @p2, @p3, 'Worker', 1, 3)";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@p1", YeniAd.Trim());
                    cmd.Parameters.AddWithValue("@p2", YeniEmail.Trim().ToLower());
                    cmd.Parameters.AddWithValue("@p3", hashliSifre);

                    cmd.ExecuteNonQuery();

                    MessageBox.Show($"İşçi başarıyla oluşturuldu!\nEmail: {YeniEmail}\nŞifre: {girilenSifre}");

                    YeniAd = "";
                    YeniEmail = "";
                    YeniSifre = "";
                    TabloyuGuncelle();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kayıt sırasında hata oluştu: " + ex.Message);
            }
        }

        private void IsciSil(string email)
        {
            if (string.IsNullOrEmpty(email)) return;

            MessageBoxResult result = MessageBox.Show($"{email} adresli işçiyi silmek istediğinize emin misiniz?", "Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection con = new SqlConnection(connectionString))
                    {
                        con.Open();
                        string query = "DELETE FROM Users WHERE Email = @p1 AND Role = 'Worker'";
                        SqlCommand cmd = new SqlCommand(query, con);
                        cmd.Parameters.AddWithValue("@p1", email);
                        cmd.ExecuteNonQuery();

                        MessageBox.Show("İşçi başarıyla silindi.");
                        TabloyuGuncelle();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Hata: " + ex.Message);
                }
            }
        }

        private void AdminPanelAc()
        {
            IsAdminPanelVisible = true;
            TabloyuGuncelle();
        }

        private void AdminPanelKapat()
        {
            IsAdminPanelVisible = false;
        }

        private void Kayitlar()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is Window5 window5)
                {
                    window5.HideOverlay();
                    window5.VerileriYukle();
                    break;
                }
            }
        }

        private async void KayitOlustur()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is Window5 window5)
                {
                    // Overlay zaten açıksa önce kapat, sonra yeni sayfayı aç
                    window5.HideOverlay();
                    // Animasyon tamamlansın diye kısa bir bekleme (daha hızlı)
                    await System.Threading.Tasks.Task.Delay(120);
                    window5.ShowOverlayPage(new PageKayitOlustur());
                    break;
                }
            }
        }

        private async void DisServisKayitlari()
        {
            // If a Window5 host exists, hide its overlay first
            foreach (Window window in Application.Current.Windows)
            {
                if (window is Window5 window5)
                {
                    window5.HideOverlay();
                    // wait briefly so the hide animation can complete
                    await System.Threading.Tasks.Task.Delay(120);
                    break;
                }
            }

            // Open Window6 for Dış Servis Kayıtları (new record -> id = 0)
            var w = new alpsoftservistakip.Window6(0);
            w.Show();
        }
    }
}

