using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using alpsoftservistakip.Helpers;
using alpsoftservistakip.Models;
using Newtonsoft.Json;

namespace alpsoftservistakip
{
    public partial class PageCariListesi : Page
    {
        public PageCariListesi()
        {
            InitializeComponent();
            this.Loaded += PageCariListesi_Loaded;
        }

        private async void PageCariListesi_Loaded(object sender, RoutedEventArgs e)
        {
            await CarileriGetirAsync();
        }

        private async Task CarileriGetirAsync(string aramaMetni = "")
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Class1.JwtToken);
                    string url = $"{ApiConfig.Api}/Cariler";
                    if (!string.IsNullOrWhiteSpace(aramaMetni))
                    {
                        url += $"?arama={Uri.EscapeDataString(aramaMetni)}";
                    }

                    var response = await client.GetAsync(url);
                    if (!response.IsSuccessStatusCode)
                    {
                        var err = await response.Content.ReadAsStringAsync();
                        MessageBox.Show("Cariler yüklenirken hata oluştu: " + err, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var json = await response.Content.ReadAsStringAsync();
                    var list = JsonConvert.DeserializeObject<List<CariListeDto>>(json);
                    
                    dgCariler.ItemsSource = list;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cariler yüklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void txtArama_TextChanged(object sender, TextChangedEventArgs e)
        {
            string metin = txtArama.Text;
            await CarileriGetirAsync(metin);
        }

        private async void btnTemizle_Click(object sender, RoutedEventArgs e)
        {
            txtArama.Text = "";
            await CarileriGetirAsync();
        }

        private void btnGeri_Click(object sender, RoutedEventArgs e)
        {
            Window mevcutPencere = Window.GetWindow(this);
            if (mevcutPencere is Window3 window3)
            {
                window3.HideOverlay();
            }
        }

        private void btnYeniCari_Click(object sender, RoutedEventArgs e)
        {
            Window mevcutPencere = Window.GetWindow(this);
            if (mevcutPencere is Window3 window3)
            {
                window3.ShowOverlayPage(new PageCariEkle());
            }
        }

        private void dgCariler_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgCariler.SelectedItem is CariListeDto row)
            {
                int cariId = row.CariID;
                string adSoyad = row.AdSoyadUnvan;

                Window mevcutPencere = Window.GetWindow(this);
                if (mevcutPencere is Window3 window3)
                {
                    window3.ShowOverlayPage(new PageCariDetay(cariId));
                }
            }
        }
    }
}

