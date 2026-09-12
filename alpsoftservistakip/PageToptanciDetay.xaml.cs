using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using alpsoftservistakip.Helpers;
using alpsoftservistakip.Models;
using Newtonsoft.Json;

namespace alpsoftservistakip
{
    public partial class PageToptanciDetay : Page
    {
        private int _toptanciId;

        public PageToptanciDetay(int toptanciId)
        {
            InitializeComponent();
            _toptanciId = toptanciId;
            this.Loaded += async (s, e) => await YukleVeriAsync();
        }

        private async Task YukleVeriAsync()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Class1.JwtToken);
                    var response = await client.GetAsync($"{ApiConfig.Api}/Toptanci/{_toptanciId}/detay");
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        var err = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Veriler yüklenirken hata: {err}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var json = await response.Content.ReadAsStringAsync();
                    var dto = JsonConvert.DeserializeObject<ToptanciDetayDto>(json);
                    
                    txtToptanciAdi.Text = dto.FirmaAdi ?? "";
                    txtTelefon.Text = dto.Telefon ?? "-";
                    txtIBAN.Text = dto.IBAN ?? "-";
                    
                    dgParcaAlimlar.ItemsSource = dto.ParcaAlimlari;
                    
                    txtToplamAdet.Text = $"{dto.ToplamAdet} ADET";
                    txtToplamTutar.Text = $"$ {dto.ToplamHarcanan:N2}";
                    txtToplamBorc.Text = $"$ {dto.ToplamBorc:N2}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Veriler yüklenirken hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnKapat_Click(object sender, RoutedEventArgs e)
        {
            Window pencere = Window.GetWindow(this);
            if (pencere is Window3 window3)
            {
                window3.ShowOverlayPage(new PageToptanciListesi());
            }
        }
    }
}


