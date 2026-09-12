using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows;
using System.Windows.Input;
using Newtonsoft.Json;
using alpsoftservistakip.Helpers;
using alpsoftservistakip.Models;

namespace alpsoftservistakip.ViewModels
{
    public class Window5ViewModel : BaseViewModel
    {
        private IEnumerable _kayitlar;
        private string _aramaMetni = "Servis kaydı ara...";
        private DateTime? _secilenTarih;
        private bool _isAramaFocused;

        public IEnumerable Kayitlar
        {
            get => _kayitlar;
            set => SetProperty(ref _kayitlar, value);
        }

        private System.Threading.CancellationTokenSource _aramaCts;

        public string AramaMetni
        {
            get => _aramaMetni;
            set
            {
                string duzeltilmis = value;
                if (duzeltilmis != null && duzeltilmis != "Servis kaydı ara..." && duzeltilmis.Contains("*"))
                {
                    duzeltilmis = duzeltilmis.Replace('*', '-');
                }

                if (SetProperty(ref _aramaMetni, duzeltilmis))
                {
                    if (duzeltilmis != "Servis kaydı ara...")
                    {
                        _aramaCts?.Cancel();
                        _aramaCts = new System.Threading.CancellationTokenSource();
                        var token = _aramaCts.Token;

                        System.Threading.Tasks.Task.Delay(120, token).ContinueWith(t =>
                        {
                            if (!t.IsCanceled)
                            {
                                App.Current?.Dispatcher?.Invoke(() => KayitlariListele(duzeltilmis));
                            }
                        }, System.Threading.Tasks.TaskScheduler.Default);
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
        public ICommand FisiYazdirCommand { get; }
        public ICommand EtiketiYazdirCommand { get; }
        public ICommand MusteriyeBildirCommand { get; }

        public Window5ViewModel()
        {
            AramaGotFocusCommand = new RelayCommand(_ => AramaGotFocus());
            AramaLostFocusCommand = new RelayCommand(_ => AramaLostFocus());
            FiltreTemizleCommand = new RelayCommand(_ => FiltreTemizle());
            GeriDonCommand = new RelayCommand(_ => GeriDon());
            KayitDetayCommand = new RelayCommand<object>(KayitDetay);
            FisiYazdirCommand = new RelayCommand<object>(FisiYazdir);
            EtiketiYazdirCommand = new RelayCommand<object>(EtiketiYazdir);
            MusteriyeBildirCommand = new RelayCommand<object>(MusteriyeBildir);

            KayitlariListele();
        }

        public async void KayitlariListele(string aramaMetni = "")
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Class1.JwtToken);
                    string branchParam = Class1.AktifSubeId > 0 ? $"&branchId={Class1.AktifSubeId}" : "";
                    string url = $"{ApiConfig.Api}/KayitliCihazlar?arama={Uri.EscapeDataString(aramaMetni ?? "")}{branchParam}";
                    var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        Kayitlar = JsonConvert.DeserializeObject<List<KayitliCihazDto>>(json);
                    }
                    else
                    {
                        var err = await response.Content.ReadAsStringAsync();
                        MessageBox.Show("Veri çekme hatası: " + err);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veri çekme hatası: " + ex.Message);
            }
        }

        private async void TarihFiltrele(DateTime secilenTarih)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Class1.JwtToken);
                    string branchParam = Class1.AktifSubeId > 0 ? $"&branchId={Class1.AktifSubeId}" : "";
                    string url = $"{ApiConfig.Api}/KayitliCihazlar/tarih-filtrele?tarih={secilenTarih:yyyy-MM-dd}{branchParam}";
                    var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        Kayitlar = JsonConvert.DeserializeObject<List<KayitliCihazDto>>(json);
                    }
                    else
                    {
                        var err = await response.Content.ReadAsStringAsync();
                        MessageBox.Show("Tarih filtreleme hatası: " + err);
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
            int id = 0;
            if (parameter is KayitliCihazDto dto)
            {
                id = dto.ID;
            }
            else if (parameter is DataRowView row)
            {
                id = Convert.ToInt32(row["ID"]);
            }

            if (id <= 0) return;

            // Window3'ü bul ve PageKayitOlustur'u overlay içinde aç
            Window3 mainWindow = Application.Current.Windows.OfType<Window3>().FirstOrDefault();
            if (mainWindow != null)
            {
                PageKayitOlustur detay = new PageKayitOlustur();
                // ViewModel'e ID'yi geç
                var viewModel = detay.DataContext as ViewModels.PageKayitOlusturViewModel;
                if (viewModel != null)
                {
                    // ID'yi ViewModel'e ayarla - onu reload yap
                    viewModel.CarregarKayit(id);
                }
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

        public void VerileriYukle()
        {
            string mevcutArama = (AramaMetni != "Servis kaydı ara...") ? AramaMetni : "";
            KayitlariListele(mevcutArama);
        }

        private void FisiYazdir(object parameter)
        {
            try
            {
                Models.ServisKayitFisiData fisiData = null;

                if (parameter is KayitliCihazDto dto)
                {
                    fisiData = new Models.ServisKayitFisiData
                    {
                        KayitID = dto.ID,
                        KayitTarihi = dto.KayitTarihi,
                        IsimSoyisim = dto.IsimSoyisim ?? "",
                        CepTelefonu = dto.CepTelefonu ?? "",
                        BayiAdi = dto.BayiAdi ?? "",
                        Marka = dto.Marka ?? "",
                        Model = dto.Model ?? "",
                        Ariza = dto.Ariza ?? "",
                        ImeiNo = dto.IMEI ?? "",
                        ServisDurumu = dto.ServisDurumu ?? "",
                        IlgiliTeknisyen = dto.ServisTeknisyen ?? "",
                        EkBilgiler = dto.EkBilgiler ?? ""
                    };
                }
                else if (parameter is DataRowView row)
                {
                    fisiData = new Models.ServisKayitFisiData
                    {
                        KayitID = Convert.ToInt32(row["ID"]),
                        KayitTarihi = Convert.ToDateTime(row["KayitTarihi"]),
                        IsimSoyisim = row["IsimSoyisim"]?.ToString() ?? "",
                        CepTelefonu = row["CepTelefonu"]?.ToString() ?? "",
                        BayiAdi = row["BayiAdi"]?.ToString() ?? "",
                        Marka = row["Marka"]?.ToString() ?? "",
                        Model = row["Model"]?.ToString() ?? "",
                        Ariza = row["Ariza"]?.ToString() ?? "",
                        ImeiNo = row["IMEI"]?.ToString() ?? "",
                        ServisDurumu = row["ServisDurumu"]?.ToString() ?? "",
                        IlgiliTeknisyen = row["ServisTeknisyen"]?.ToString() ?? "",
                        EkBilgiler = row["EkBilgiler"]?.ToString() ?? ""
                    };
                }

                if (fisiData != null)
                {
                    var fisi = new alpsoftservistakip.Controls.ServisKayitFisi()
                    {
                        DataContext = fisiData
                    };

                    Services.WpfPrintService.ShowPrintPreview(fisi, "SERVIS_KAYIT_FISI", "SERVIS KAYIT FİŞİ ÖNİZLEMESİ");
                }
                else
                {
                    MessageBox.Show("Lütfen bir kayıt seçin!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fiş yazdırma hatası: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EtiketiYazdir(object parameter)
        {
            try
            {
                if (parameter is KayitliCihazDto dto)
                {
                    var win = new WindowCihazEtiketYazdir(dto)
                    {
                        Owner = Application.Current.MainWindow
                    };
                    win.ShowDialog();
                }
                else if (parameter is DataRowView row)
                {
                    var win = new WindowCihazEtiketYazdir(row)
                    {
                        Owner = Application.Current.MainWindow
                    };
                    win.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Lütfen etiketini yazdırmak istediğiniz servisi seçin!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Etiket yazdırma hatası: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MusteriyeBildir(object parameter)
        {
            try
            {
                string musteriAdi = "";
                string telefon = "";
                string marka = "";
                string model = "";
                string ariza = "";
                string durum = "";
                string fiyat = "";
                int id = 0;

                if (parameter is KayitliCihazDto dto)
                {
                    id = dto.ID;
                    musteriAdi = dto.IsimSoyisim ?? "";
                    telefon = dto.CepTelefonu ?? "";
                    marka = dto.Marka ?? "";
                    model = dto.Model ?? "";
                    ariza = dto.Ariza ?? "";
                    durum = dto.ServisDurumu ?? "";
                    fiyat = dto.FiyatBilgisi ?? "";
                }
                else if (parameter is DataRowView row)
                {
                    id = Convert.ToInt32(row["ID"]);
                    musteriAdi = row["IsimSoyisim"]?.ToString() ?? "";
                    telefon = row["CepTelefonu"]?.ToString() ?? "";
                    marka = row["Marka"]?.ToString() ?? "";
                    model = row["Model"]?.ToString() ?? "";
                    ariza = row["Ariza"]?.ToString() ?? "";
                    durum = row["ServisDurumu"]?.ToString() ?? "";
                    fiyat = row["FiyatBilgisi"]?.ToString() ?? "";
                }

                if (id <= 0)
                {
                    MessageBox.Show("Lütfen önce tablodan bildirim göndermek istediğiniz servis kaydını seçin.", "Kayıt Seçilmedi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(telefon))
                {
                    MessageBox.Show("Seçilen kayıtta geçerli bir cep telefonu bulunamadı.", "Telefon Eksik", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var ctx = new Services.NotificationContext
                {
                    MusteriAdi = musteriAdi,
                    Telefon = telefon,
                    Cihaz = $"{marka} {model}".Trim(),
                    ServisNo = $"SRV-{id:D6}",
                    Ariza = ariza,
                    Tutar = fiyat,
                    Durum = durum
                };

                Services.NotificationTemplateType template = Services.NotificationTemplateType.Tamamlandi;
                if (durum == "Kabul Edildi") template = Services.NotificationTemplateType.Kabul;
                else if (durum == "İşlemde" || durum == "Fiyat Onayı Bekliyor" || durum == "Fiyat Teklifi Verildi") template = Services.NotificationTemplateType.FiyatOnay;
                else if (durum == "Teslim Edildi") template = Services.NotificationTemplateType.TeslimEdildi;

                var wnd = new MusteriBildirimWindow(ctx, template);
                wnd.Owner = Application.Current.MainWindow;
                wnd.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bildirim penceresi açılırken hata: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

