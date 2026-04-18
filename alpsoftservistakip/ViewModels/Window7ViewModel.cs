using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using alpsoftservistakip;

namespace alpsoftservistakip.ViewModels
{
    public class Window7ViewModel : BaseViewModel
    {
        private const string ConnectionString = "Server=91.247.168.204,1433;Database=alpsoftservistakip;User Id=sa;Password=Alperengurpinar4160552009;TrustServerCertificate=True;";

        private DataView _veriler;
        public DataView Veriler
        {
            get => _veriler;
            set => SetProperty(ref _veriler, value);
        }

        private string _aramaMetni = "İsim veya Model Ara...";
        public string AramaMetni
        {
            get => _aramaMetni;
            set
            {
                if (SetProperty(ref _aramaMetni, value))
                {
                    if (value != "İsim veya Model Ara..." && !string.IsNullOrWhiteSpace(value))
                    {
                        VerileriGetir(value);
                    }
                    else if (string.IsNullOrWhiteSpace(value) || value == "İsim veya Model Ara...")
                    {
                        VerileriGetir("");
                    }
                }
            }
        }

        private DateTime? _secilenTarih;
        public DateTime? SecilenTarih
        {
            get => _secilenTarih;
            set
            {
                if (SetProperty(ref _secilenTarih, value))
                {
                    if (value.HasValue)
                    {
                        VerileriGetirTarihFiltresi(value.Value);
                    }
                    else
                    {
                        VerileriGetir("");
                    }
                }
            }
        }

        public ICommand AramaGotFocusCommand { get; }
        public ICommand AramaLostFocusCommand { get; }
        public ICommand FiltreTemizleCommand { get; }
        public ICommand GeriDonCommand { get; }
        public ICommand KayitDetayCommand { get; }

        public Window7ViewModel()
        {
            AramaGotFocusCommand = new RelayCommand(_ => AramaGotFocus());
            AramaLostFocusCommand = new RelayCommand(_ => AramaLostFocus());
            FiltreTemizleCommand = new RelayCommand(_ => FiltreTemizle());
            GeriDonCommand = new RelayCommand(_ => GeriDon());
            KayitDetayCommand = new RelayCommand<object>(KayitDetay);

            VerileriGetir();
        }

        public void VerileriGetir(string arama = "")
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    string sql = @"SELECT * FROM disserviskayitlarnew 
                                   WHERE KullaniciID = @KullaniciID
                                   AND (@arama = '' 
                                   OR IsimSoyisim LIKE '%' + @arama + '%' 
                                   OR Marka LIKE '%' + @arama + '%'
                                   OR YetkiliBayi LIKE '%' + @arama + '%')
                                   ORDER BY KayitID DESC";

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@KullaniciID", LoginWindow.aktifKullaniciID);
                    cmd.Parameters.AddWithValue("@arama", arama);

                    DataTable dt = new DataTable();
                    new SqlDataAdapter(cmd).Fill(dt);

                    Veriler = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Beklenmeyen hata: " + ex.Message);
            }
        }

        private void VerileriGetirTarihFiltresi(DateTime secilenTarih)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    string sql = @"SELECT * FROM disserviskayitlarnew 
                                   WHERE KullaniciID = @KullaniciID 
                                   AND CAST(KayitTarihi AS DATE) = @secilenTarih
                                   ORDER BY KayitID DESC";

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@KullaniciID", LoginWindow.aktifKullaniciID);
                    cmd.Parameters.AddWithValue("@secilenTarih", secilenTarih.Date);

                    DataTable dt = new DataTable();
                    new SqlDataAdapter(cmd).Fill(dt);

                    Veriler = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Tarih filtreleme hatası: " + ex.Message);
            }
        }

        private void AramaGotFocus()
        {
            if (AramaMetni == "İsim veya Model Ara...")
            {
                AramaMetni = "";
            }
        }

        private void AramaLostFocus()
        {
            if (string.IsNullOrWhiteSpace(AramaMetni))
            {
                AramaMetni = "İsim veya Model Ara...";
            }
        }

        private void FiltreTemizle()
        {
            SecilenTarih = null;
            AramaMetni = "İsim veya Model Ara...";
            VerileriGetir("");
        }

        private void GeriDon()
        {
            Window3 mainWindow = Application.Current.Windows.OfType<Window3>().FirstOrDefault();
            if (mainWindow != null)
            {
                mainWindow.Show();
                mainWindow.Activate();
            }
            else
            {
                Window3 yeniWindow = new Window3();
                yeniWindow.Show();
            }
        }

        private void KayitDetay(object parameter)
        {
            if (parameter is DataRowView row)
            {
                int id = Convert.ToInt32(row["KayitID"]);

                Window3 mainWindow = Application.Current.Windows.OfType<Window3>().FirstOrDefault();
                if (mainWindow != null)
                {
                    PageKayitOlustur detay = new PageKayitOlustur();
                    var vm = detay.DataContext as ViewModels.PageKayitOlusturViewModel;
                    if (vm != null)
                    {
                        vm.CarregarKayit(id);
                    }
                    mainWindow.ShowOverlayPage(detay);
                }
                else
                {
                    Window6 detay = new Window6(id);
                    detay.ShowDialog();
                }

                string aramaParametresi = (AramaMetni == "İsim veya Model Ara..." || AramaMetni == "Arama")
                                          ? ""
                                          : AramaMetni;

                VerileriGetir(aramaParametresi);
            }
        }

        public void Yenile()
        {
            string arama = AramaMetni == "Arama" || AramaMetni == "İsim veya Model Ara..." ? "" : AramaMetni;
            VerileriGetir(arama);
        }
    }
}

