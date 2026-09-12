using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using alpsoftservistakip.Helpers;
using alpsoftservistakip.Models;
using Newtonsoft.Json;

namespace alpsoftservistakip
{
    public partial class PageCariEkle : Page
    {
        public PageCariEkle()
        {
            InitializeComponent();
        }

        private async void btnKaydet_Click(object sender, RoutedEventArgs e)
        {
            string unvan = txtUnvan.Text.Trim();
            string telefon = txtTelefon.Text.Trim();
            string email = txtEmail.Text.Trim();
            string vergiDairesi = txtVergiDairesi.Text.Trim();
            string vergiNo = txtVergiNo.Text.Trim();
            string adres = txtAdres.Text.Trim();

            if (string.IsNullOrWhiteSpace(unvan))
            {
                MessageBox.Show("Firma ünvanı / Müşteri adı boş bırakılamaz!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Class1.JwtToken);
                    var dto = new CariEkleDto 
                    {
                        AdSoyadUnvan = unvan,
                        Telefon = telefon,
                        Email = email,
                        Adres = adres,
                        VergiDairesi = vergiDairesi,
                        VergiNo = vergiNo
                    };
                    
                    var content = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
                    var response = await client.PostAsync($"{ApiConfig.Api}/Cariler", content);
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        var err = await response.Content.ReadAsStringAsync();
                        MessageBox.Show("Cari eklenirken hata oluştu: " + err, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                MessageBox.Show("Cari kart başarıyla kaydedildi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                GeriDon();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cari eklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnIptal_Click(object sender, RoutedEventArgs e)
        {
            GeriDon();
        }

        private void txtTelefon_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Sadece sayılara izin ver
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, "^[0-9]+$");
        }

        private void GeriDon()
        {
            Window mevcutPencere = Window.GetWindow(this);
            if (mevcutPencere is Window3 window3)
            {
                // Mevcut overlay sayfasını CariListesi olarak güncelliyoruz
                window3.ShowOverlayPage(new PageCariListesi());
            }
        }
    }
}


