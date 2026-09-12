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
    public partial class PageToptanciEkle : Page
    {
        private int? _toptanciId = null;
        private Func<Task> _yenilemeFonksiyonu;

        public PageToptanciEkle(int? toptanciId = null, Func<Task> yenilemeFonksiyonu = null)
        {
            InitializeComponent();
            _toptanciId = toptanciId;
            _yenilemeFonksiyonu = yenilemeFonksiyonu;

            if (_toptanciId.HasValue)
            {
                txtUstBaslik.Text = "Toptancı Düzenle";
                btnKaydet.Content = "GÜNCELLE";
                _ = YukleVeriAsync();
            }
        }

        private async Task YukleVeriAsync()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Class1.JwtToken);
                    var response = await client.GetAsync($"{ApiConfig.Api}/Toptanci/{_toptanciId.Value}/detay");
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        var err = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Veriler yüklenirken hata: {err}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var json = await response.Content.ReadAsStringAsync();
                    var dto = JsonConvert.DeserializeObject<ToptanciDetayDto>(json);
                    
                    txtFirmaAdi.Text = dto.FirmaAdi ?? "";
                    txtTelefon.Text = dto.Telefon ?? "";
                    txtIBAN.Text = dto.IBAN ?? "";
                    txtAciklama.Text = dto.Aciklama ?? "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Veriler yüklenirken hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnKaydet_Click(object sender, RoutedEventArgs e)
        {
            string firmaAdi = txtFirmaAdi.Text.Trim();
            string telefon = txtTelefon.Text.Trim();
            string iban = txtIBAN.Text.Trim();
            string aciklama = txtAciklama.Text.Trim();

            if (string.IsNullOrWhiteSpace(firmaAdi))
            {
                MessageBox.Show("Toptancı adı boş bırakılamaz!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Class1.JwtToken);
                    var dto = new ToptanciKaydetDto 
                    {
                        ID = _toptanciId ?? 0,
                        FirmaAdi = firmaAdi,
                        Telefon = telefon,
                        IBAN = iban,
                        Aciklama = aciklama
                    };
                    
                    var content = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
                    var response = await client.PostAsync($"{ApiConfig.Api}/Toptanci/kaydet", content);
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        var err = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Kaydetme sırasında hata: {err}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                MessageBox.Show("Toptancı bilgileri başarıyla kaydedildi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                
                if (_yenilemeFonksiyonu != null)
                {
                    await _yenilemeFonksiyonu();
                }

                GeriDon();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kayıt sırasında hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnIptal_Click(object sender, RoutedEventArgs e)
        {
            GeriDon();
        }

        private void GeriDon()
        {
            Window pencere = Window.GetWindow(this);
            if (pencere is Window3 window3)
            {
                window3.ShowOverlayPage(new PageToptanciListesi());
            }
        }
    }
}


