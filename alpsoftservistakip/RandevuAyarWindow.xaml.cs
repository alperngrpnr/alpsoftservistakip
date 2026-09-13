using alpsoftservistakip.Helpers;
using alpsoftservistakip.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace alpsoftservistakip
{
    public partial class RandevuAyarWindow : Window
    {
        public RandevuAyarWindow()
        {
            InitializeComponent();
            this.Loaded += async (s, e) => await AyarlariYukleAsync();
        }

        private void btnKapat_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async Task AyarlariYukleAsync()
        {
            try
            {
                int companyId = Class1.AktifKullanici?.SirketID ?? 1;
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(ApiConfig.BaseUrl + "/");
                    client.Timeout = TimeSpan.FromSeconds(6);

                    var res = await client.GetAsync($"api/randevu/ayarlar?companyId={companyId}");
                    if (res.IsSuccessStatusCode)
                    {
                        string json = await res.Content.ReadAsStringAsync();
                        var ayar = JsonConvert.DeserializeObject<RandevuAyarClientDto>(json);
                        if (ayar != null)
                        {
                            chkAktif.IsChecked = ayar.Aktif;

                            // Günler
                            var gunler = (ayar.CalismaGunleri ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                            chkPzt.IsChecked = gunler.Contains("1");
                            chkSal.IsChecked = gunler.Contains("2");
                            chkCar.IsChecked = gunler.Contains("3");
                            chkPer.IsChecked = gunler.Contains("4");
                            chkCum.IsChecked = gunler.Contains("5");
                            chkCmt.IsChecked = gunler.Contains("6");
                            chkPaz.IsChecked = gunler.Contains("7");

                            // Saatler
                            SelectComboItem(cmbBaslangic, ayar.BaslangicSaati);
                            SelectComboItem(cmbBitis, ayar.BitisSaati);

                            // Aralık
                            SelectComboItem(cmbAralik, $"{ayar.RandevuAraligiDk} Dakika");

                            // Kapasite
                            SelectComboItem(cmbKapasite, $"{ayar.AyniSaatteMaksimum} Kişi");
                        }
                    }
                }
            }
            catch { }
        }

        private void SelectComboItem(ComboBox cmb, string val)
        {
            if (string.IsNullOrWhiteSpace(val)) return;
            foreach (ComboBoxItem item in cmb.Items)
            {
                if (item.Content?.ToString()?.Trim() == val.Trim())
                {
                    cmb.SelectedItem = item;
                    break;
                }
            }
        }

        private async void btnKaydet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnKaydet.IsEnabled = false;
                btnKaydet.Content = "Kaydediliyor...";

                var gunList = new List<string>();
                if (chkPzt.IsChecked == true) gunList.Add("1");
                if (chkSal.IsChecked == true) gunList.Add("2");
                if (chkCar.IsChecked == true) gunList.Add("3");
                if (chkPer.IsChecked == true) gunList.Add("4");
                if (chkCum.IsChecked == true) gunList.Add("5");
                if (chkCmt.IsChecked == true) gunList.Add("6");
                if (chkPaz.IsChecked == true) gunList.Add("7");

                string baslangic = (cmbBaslangic.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "09:00";
                string bitis = (cmbBitis.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "19:00";

                string aralikText = (cmbAralik.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "30";
                int aralik = 30;
                int.TryParse(new string(aralikText.Where(char.IsDigit).ToArray()), out aralik);
                if (aralik <= 0) aralik = 30;

                string kapText = (cmbKapasite.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "1";
                int kapasite = 1;
                int.TryParse(new string(kapText.Where(char.IsDigit).ToArray()), out kapasite);
                if (kapasite <= 0) kapasite = 1;

                var payload = new RandevuAyarClientDto
                {
                    CompanyId = Class1.AktifKullanici?.SirketID ?? 1,
                    BaslangicSaati = baslangic,
                    BitisSaati = bitis,
                    RandevuAraligiDk = aralik,
                    CalismaGunleri = string.Join(",", gunList),
                    AyniSaatteMaksimum = kapasite,
                    Aktif = chkAktif.IsChecked == true,
                    OtoOnay = true
                };

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(ApiConfig.BaseUrl + "/");
                    client.Timeout = TimeSpan.FromSeconds(6);

                    var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                    var res = await client.PostAsync("api/randevu/ayarlar-kaydet", content);

                    if (res.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Randevu ve çalışma saatleri ayarları başarıyla kaydedildi!", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                        this.DialogResult = true;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Ayarlar kaydedilirken sunucu hatası oluştu.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message, "Bağlantı Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                btnKaydet.IsEnabled = true;
                btnKaydet.Content = "💾 Ayarları Kaydet";
            }
        }
    }
}
