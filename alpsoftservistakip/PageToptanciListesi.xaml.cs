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
    public partial class PageToptanciListesi : Page
    {
        public PageToptanciListesi()
        {
            InitializeComponent();
            this.Loaded += async (s, e) => await YukleVeriAsync();
        }

        private async Task YukleVeriAsync(string aramaMetni = "")
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Class1.JwtToken);
                    string url = $"{ApiConfig.Api}/Toptanci/liste";
                    if (!string.IsNullOrWhiteSpace(aramaMetni))
                    {
                        url += $"?arama={Uri.EscapeDataString(aramaMetni)}";
                    }

                    var response = await client.GetAsync(url);
                    if (!response.IsSuccessStatusCode)
                    {
                        var err = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Veriler yüklenirken hata: {err}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var json = await response.Content.ReadAsStringAsync();
                    var list = JsonConvert.DeserializeObject<List<ToptanciListeDto>>(json);
                    
                    dgToptancilar.ItemsSource = list;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Veriler yüklenirken hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void txtArama_TextChanged(object sender, TextChangedEventArgs e)
        {
            string metin = txtArama.Text;
            await YukleVeriAsync(metin);
        }

        private async void btnTemizle_Click(object sender, RoutedEventArgs e)
        {
            txtArama.Text = "";
            await YukleVeriAsync();
        }

        private void btnGeri_Click(object sender, RoutedEventArgs e)
        {
            Window mevcutPencere = Window.GetWindow(this);
            if (mevcutPencere is Window3 window3)
            {
                window3.HideOverlay();
            }
        }

        private void btnYeniToptanci_Click(object sender, RoutedEventArgs e)
        {
            Window pencere = Window.GetWindow(this);
            if (pencere is Window3 window3)
            {
                window3.ShowOverlayPage(new PageToptanciEkle(null, () => YukleVeriAsync()));
            }
        }

        private void dgToptancilar_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgToptancilar.SelectedItem is ToptanciListeDto row)
            {
                int toptanciId = row.ID;
                Window pencere = Window.GetWindow(this);
                if (pencere is Window3 window3)
                {
                    window3.ShowOverlayPage(new PageToptanciDetay(toptanciId));
                }
            }
        }
    }
}


