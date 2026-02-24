using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace alpsoftservistakip.ViewModels
{
    public class Window5ViewModel : BaseViewModel
    {
        private readonly string connString =
            "Server=91.247.168.204,1433;" +
            "Database=alpsoftservistakip;" +
            "User Id=sa;" +
            "Password=Alperengurpinar4160552009;" +
            "TrustServerCertificate=True;";

        private DataView _kayitlar;
        private string _aramaMetni = "Servis kaydı ara...";
        private DateTime? _secilenTarih;
        private bool _isAramaFocused;

        public DataView Kayitlar
        {
            get => _kayitlar;
            set => SetProperty(ref _kayitlar, value);
        }

        public string AramaMetni
        {
            get => _aramaMetni;
            set
            {
                if (SetProperty(ref _aramaMetni, value))
                {
                    if (value != "Servis kaydı ara...")
                    {
                        KayitlariListele(value);
                    }
                }
            }
        }

        public DateTime? SecilenTarih
        {
            get => _secilenTarih;
            set
            {
                if (SetProperty(ref _secilenTarih, value))
                {
                    if (value.HasValue)
                    {
                        TarihFiltrele(value.Value);
                    }
                    else
                    {
                        // Tarih seçimi kaldırıldığında tüm kayıtları göster
                        string aramaParametresi = (AramaMetni == "Servis kaydı ara..." || AramaMetni == "Arama")
                                                  ? ""
                                                  : AramaMetni;
                        KayitlariListele(aramaParametresi);
                    }
                }
            }
        }

        public bool IsAramaFocused
        {
            get => _isAramaFocused;
            set => SetProperty(ref _isAramaFocused, value);
        }

        public ICommand AramaGotFocusCommand { get; }
        public ICommand AramaLostFocusCommand { get; }
        public ICommand FiltreTemizleCommand { get; }
        public ICommand GeriDonCommand { get; }
        public ICommand KayitDetayCommand { get; }

        public Window5ViewModel()
        {
            AramaGotFocusCommand = new RelayCommand(_ => AramaGotFocus());
            AramaLostFocusCommand = new RelayCommand(_ => AramaLostFocus());
            FiltreTemizleCommand = new RelayCommand(_ => FiltreTemizle());
            GeriDonCommand = new RelayCommand(_ => GeriDon());
            KayitDetayCommand = new RelayCommand<object>(KayitDetay);

            KayitlariListele();
        }

        public void KayitlariListele(string aramaMetni = "")
        {
            if (Class1.AktifKullanici == null)
            {
                MessageBox.Show("Oturum bilgisi bulunamadı.");
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();

                    string sql = @"
                        SELECT * FROM kayitlicihazlar
                        WHERE CompanyId = @SirketID
                        AND (@arama = '' OR
                             IsimSoyisim LIKE '%' + @arama + '%' OR
                             BayiAdi LIKE '%' + @arama + '%' OR
                             CepTelefonu LIKE '%' + @arama + '%')
                        ORDER BY ID DESC";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@arama", aramaMetni ?? "");
                        cmd.Parameters.AddWithValue("@SirketID", Class1.AktifKullanici.SirketID);

                        DataTable dt = new DataTable();
                        new SqlDataAdapter(cmd).Fill(dt);
                        Kayitlar = dt.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("CompanyId"))
                {
                    MessageBox.Show("Veritabanı hatası: 'CompanyId' sütunu bulunamadı. Lütfen SQL tablonuza bu sütunu ekleyin.");
                }
                else
                {
                    MessageBox.Show("Veri çekme hatası: " + ex.Message);
                }
            }
        }

        private void TarihFiltrele(DateTime secilenTarih)
        {
            if (Class1.AktifKullanici == null) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    string sql = @"SELECT * FROM kayitlicihazlar 
                                   WHERE CompanyId = @SirketID 
                                   AND CAST(KayitTarihi AS DATE) = @secilenTarih
                                   ORDER BY ID DESC";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@SirketID", Class1.AktifKullanici.SirketID);
                        cmd.Parameters.AddWithValue("@secilenTarih", secilenTarih.Date);

                        DataTable dt = new DataTable();
                        new SqlDataAdapter(cmd).Fill(dt);
                        Kayitlar = dt.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Tarih filtreleme hatası: " + ex.Message);
            }
        }

        private void AramaGotFocus()
        {
            if (AramaMetni == "Servis kaydı ara...")
            {
                AramaMetni = "";
            }
        }

        private void AramaLostFocus()
        {
            if (string.IsNullOrWhiteSpace(AramaMetni))
            {
                AramaMetni = "Servis kaydı ara...";
            }
        }

        private void FiltreTemizle()
        {
            SecilenTarih = null;
            AramaMetni = "Servis kaydı ara...";
            KayitlariListele("");
        }

        private void GeriDon()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is Window3 window3)
                {
                    window3.HideOverlay();
                    break;
                }
                else if (window is Window5 window5)
                {
                    window5.ShowOverlayPage(new Page3());
                    break;
                }
            }
        }

        private void KayitDetay(object parameter)
        {
            if (parameter is DataRowView row)
            {
                int id = Convert.ToInt32(row["ID"]);
                
                // Window3'ü bul ve PageKayitOlustur'u overlay içinde aç
                Window3 mainWindow = Application.Current.Windows.OfType<Window3>().FirstOrDefault();
                if (mainWindow != null)
                {
                    PageKayitOlustur detay = new PageKayitOlustur(id);
                    mainWindow.ShowOverlayPage(detay);
                }
                else
                {
                    // Window3 bulunamazsa eski yöntemle aç
                    Window9 detay = new Window9(id);
                    detay.ShowDialog();
                }
                
                VerileriYukle();
            }
        }

        public void VerileriYukle()
        {
            string mevcutArama = (AramaMetni != "Servis kaydı ara...") ? AramaMetni : "";
            KayitlariListele(mevcutArama);
        }
    }
}

