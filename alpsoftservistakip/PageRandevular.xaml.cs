using alpsoftservistakip.Helpers;
using alpsoftservistakip.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace alpsoftservistakip
{
    public partial class PageRandevular : Page
    {
        private List<RandevuItemDto> _allRandevular = new List<RandevuItemDto>();
        private bool _isLoaded = false;

        public PageRandevular()
        {
            InitializeComponent();
            _isLoaded = true;
            this.Loaded += async (s, e) => await RandevulariYukleAsync();
        }

        private async Task RandevulariYukleAsync()
        {
            try
            {
                int companyId = Class1.AktifKullanici?.SirketID ?? 1;
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(ApiConfig.BaseUrl + "/");
                    client.Timeout = TimeSpan.FromSeconds(6);

                    string url = $"api/randevu/liste?companyId={companyId}";
                    var res = await client.GetAsync(url);
                    if (res.IsSuccessStatusCode)
                    {
                        string json = await res.Content.ReadAsStringAsync();
                        _allRandevular = JsonConvert.DeserializeObject<List<RandevuItemDto>>(json) ?? new List<RandevuItemDto>();
                        FiltreleVeGoster();
                    }
                }
            }
            catch (Exception ex)
            {
                // Sessiz hata
            }
        }

        private void FiltreleVeGoster()
        {
            if (!_isLoaded || dgRandevular == null) return;

            string todayStr = DateTime.Today.ToString("dd.MM.yyyy");

            if (_allRandevular == null) _allRandevular = new List<RandevuItemDto>();

            // İstatistikleri güncelle
            if (txtBugunSayisi != null)
                txtBugunSayisi.Text = _allRandevular.Count(r => r != null && r.RandevuTarihi == todayStr).ToString();
            if (txtBekleyenSayisi != null)
                txtBekleyenSayisi.Text = _allRandevular.Count(r => r != null && r.IsBekliyor).ToString();
            if (txtOnaylananSayisi != null)
                txtOnaylananSayisi.Text = _allRandevular.Count(r => r != null && r.IsOnaylandi).ToString();

            IEnumerable<RandevuItemDto> list = _allRandevular;

            if (rbBugun?.IsChecked == true)
            {
                list = list.Where(r => r != null && r.RandevuTarihi == todayStr);
            }
            else if (dpTarihSec?.SelectedDate != null)
            {
                string seciliTarih = dpTarihSec.SelectedDate.Value.ToString("dd.MM.yyyy");
                list = list.Where(r => r != null && r.RandevuTarihi == seciliTarih);
            }

            dgRandevular.ItemsSource = list
                .Where(r => r != null)
                .OrderBy(r => r.RandevuTarihiRaw ?? "")
                .ThenBy(r => r.RandevuSaati ?? "")
                .ToList();
        }

        private void Filtre_Changed(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded) return;
            if (dpTarihSec != null) dpTarihSec.SelectedDate = null;
            FiltreleVeGoster();
        }

        private void dpTarihSec_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isLoaded) return;
            if (dpTarihSec?.SelectedDate != null)
            {
                if (rbBugun != null) rbBugun.IsChecked = false;
                if (rbTumu != null) rbTumu.IsChecked = false;
                FiltreleVeGoster();
            }
        }

        private async void btnYenile_Click(object sender, RoutedEventArgs e)
        {
            await RandevulariYukleAsync();
        }

        private void btnAyarlar_Click(object sender, RoutedEventArgs e)
        {
            var win = new RandevuAyarWindow();
            if (win.ShowDialog() == true)
            {
                // Ayarlar kaydedildiğinde
            }
        }

        private void btnLinkKopyala_Click(object sender, RoutedEventArgs e)
        {
            string link = $"{ApiConfig.WebTakipUrl}/randevu";
            try
            {
                Clipboard.SetText(link);
                MessageBox.Show($"Müşteri Randevu Linki Kopyalandı:\n\n{link}\n\nBu linki müşterilerinize WhatsApp veya Instagram'dan göndererek dükkanınıza özel randevu almalarını sağlayabilirsiniz.", "Web Randevu Linki", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch
            {
                MessageBox.Show($"Randevu Linkiniz: {link}", "Web Randevu Linki", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void btnServiseCevir_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is RandevuItemDto randevu)
            {
                var ask = MessageBox.Show(
                    $"{randevu.MusteriAdi} isimli müşterinin randevusu ({randevu.Cihaz} - {randevu.IslemTuru}) servis kaydına aktarılsın mı?",
                    "Servis Kaydına Dönüştür",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (ask == MessageBoxResult.Yes)
                {
                    // 1. Randevu Durumunu Güncelle
                    await DurumGuncelleAsync(randevu.Id, "Servise Dönüştürüldü");
                    randevu.Durum = "Servise Dönüştürüldü";
                    FiltreleVeGoster();

                    // 2. PageKayitOlustur'u önceden doldurulmuş bilgilerle aç
                    var page = new PageKayitOlustur(randevu.MusteriAdi, randevu.Telefon, randevu.Marka, randevu.Model, randevu.IslemTuru, randevu.Notlar);

                    // Window3 OverlayHost üzerinden aç
                    foreach (Window window in Application.Current.Windows)
                    {
                        if (window is Window3 win3)
                        {
                            win3.ShowOverlayPage(page);
                            break;
                        }
                    }
                }
            }
        }

        private void btnWhatsApp_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is RandevuItemDto randevu)
            {
                string cleanPhone = new string((randevu.Telefon ?? "").Where(char.IsDigit).ToArray());
                if (cleanPhone.StartsWith("0")) cleanPhone = "90" + cleanPhone.Substring(1);
                else if (!cleanPhone.StartsWith("90")) cleanPhone = "90" + cleanPhone;

                string sirketAdi = Class1.AktifKullanici?.SirketAdi ?? "AlpSoft Teknik Servis";
                string mesaj = $"Merhaba Sn. {randevu.MusteriAdi}, {randevu.RandevuTarihi} saat {randevu.RandevuSaati} tarihli {randevu.Cihaz} cihazınız için servis randevunuz oluşturulmuştur. Belirtilen saatte servisimize bekleriz. - {sirketAdi}";

                string url = $"https://wa.me/{cleanPhone}?text={Uri.EscapeDataString(mesaj)}";
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
                catch { }
            }
        }

        private async void btnIptal_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is RandevuItemDto randevu)
            {
                var ask = MessageBox.Show($"{randevu.MusteriAdi} randevusunu iptal etmek istediğinize emin misiniz?", "Randevu İptali", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (ask == MessageBoxResult.Yes)
                {
                    await DurumGuncelleAsync(randevu.Id, "İptal Edildi");
                    randevu.Durum = "İptal Edildi";
                    FiltreleVeGoster();
                }
            }
        }

        private async Task DurumGuncelleAsync(int id, string yeniDurum)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(ApiConfig.BaseUrl + "/");
                    client.Timeout = TimeSpan.FromSeconds(5);

                    var payload = new { id = id, durum = yeniDurum };
                    var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                    await client.PostAsync("api/randevu/durum-guncelle", content);
                }
            }
            catch { }
        }
    }
}
