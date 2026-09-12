using alpsoftservistakip.Helpers;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace alpsoftservistakip
{
    public class CompanySettingsDto
    {
        public string CompanyName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string TaxOffice { get; set; }
        public string TaxNumber { get; set; }
        public string LogoUrl { get; set; }
    }

    public partial class PageAyarlar : Page
    {
        public PageAyarlar()
        {
            InitializeComponent();
            this.Loaded += PageAyarlar_Loaded;
        }

        private async void PageAyarlar_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(ApiConfig.Api + "/");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Class1.JwtToken);

                    var response = await client.GetAsync("Settings/company");

                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var settings = JsonConvert.DeserializeObject<CompanySettingsDto>(json);

                        if (settings != null)
                        {
                            txtFirmaAdi.Text = settings.CompanyName;
                            txtTelefon.Text = settings.Phone;
                            txtAdres.Text = settings.Address;
                            txtVergiDairesi.Text = settings.TaxOffice;
                            txtVergiNo.Text = settings.TaxNumber;
                            txtLogoUrl.Text = settings.LogoUrl;
                        }
                    }
                    else
                    {
                        await SessionHelper.CheckSession(response);
                    }
                }

                // SMS ayarlarını yükle
                var smsSettings = await Services.NotificationService.GetSmsSettingsAsync();
                if (smsSettings != null)
                {
                    string provider = smsSettings.SmsProvider ?? "Netgsm";
                    foreach (ComboBoxItem item in cmbSmsProvider.Items)
                    {
                        if (item.Content?.ToString()?.Equals(provider, StringComparison.OrdinalIgnoreCase) == true)
                        {
                            cmbSmsProvider.SelectedItem = item;
                            break;
                        }
                    }

                    txtSmsHeader.Text = smsSettings.SmsHeader ?? "";
                    txtSmsUser.Text = smsSettings.SmsUser ?? "";
                    txtSmsPassword.Password = smsSettings.SmsPassword ?? "";
                    txtSmsApiUrlTemplate.Text = smsSettings.SmsApiUrlTemplate ?? "";
                    chkAutoSmsOnCompleted.IsChecked = smsSettings.AutoSmsOnCompleted;
                    chkAutoSmsOnKabul.IsChecked = smsSettings.AutoSmsOnKabul;
                }

                // Google & Patron Gün Sonu ayarlarını yükle
                var pSettings = PatronRaporSettings.Load();
                txtGoogleYorumUrl.Text = pSettings.GoogleYorumUrl ?? "";
                txtGunSonuSaat.Text = !string.IsNullOrWhiteSpace(pSettings.GunSonuRaporSaati) ? pSettings.GunSonuRaporSaati : "20:00";
                txtPatronTelefon.Text = pSettings.PatronTelefon ?? "";
                chkGunSonuAktif.IsChecked = pSettings.GunSonuRaporAktif;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ayarlar yüklenirken bir hata oluştu: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnKaydet_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFirmaAdi.Text))
            {
                MessageBox.Show("Firma Adı alanı zorunludur.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var dto = new CompanySettingsDto
                {
                    CompanyName = txtFirmaAdi.Text.Trim(),
                    Phone = txtTelefon.Text.Trim(),
                    Address = txtAdres.Text.Trim(),
                    TaxOffice = txtVergiDairesi.Text.Trim(),
                    TaxNumber = txtVergiNo.Text.Trim(),
                    LogoUrl = txtLogoUrl.Text.Trim()
                };

                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(ApiConfig.Api + "/");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Class1.JwtToken);

                    string json = JsonConvert.SerializeObject(dto);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PutAsync("Settings/company", content);

                    if (response.IsSuccessStatusCode)
                    {
                        // Uygulama genelinde hafızadaki firma adını da güncelleyelim.
                        if (Class1.AktifKullanici != null)
                        {
                            Class1.AktifKullanici.SirketAdi = dto.CompanyName;
                            Class1.AktifKullanici.SirketTelefon = dto.Phone;
                            Class1.AktifKullanici.SirketAdres = dto.Address;
                            Class1.AktifKullanici.VergiDairesi = dto.TaxOffice;
                            Class1.AktifKullanici.VergiNo = dto.TaxNumber;
                            Class1.AktifKullanici.LogoUrl = dto.LogoUrl;
                        }

                        // SMS ayarlarını kaydet
                        string selProvider = (cmbSmsProvider.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Netgsm";
                        var smsDto = new Models.SmsSettingsDto
                        {
                            SmsProvider = selProvider,
                            SmsHeader = txtSmsHeader.Text.Trim(),
                            SmsUser = txtSmsUser.Text.Trim(),
                            SmsPassword = txtSmsPassword.Password,
                            SmsApiUrlTemplate = txtSmsApiUrlTemplate.Text.Trim(),
                            AutoSmsOnCompleted = chkAutoSmsOnCompleted.IsChecked == true,
                            AutoSmsOnKabul = chkAutoSmsOnKabul.IsChecked == true
                        };

                        await Services.NotificationService.SaveSmsSettingsAsync(smsDto);
                        
                        // Google & Patron ayarlarını kaydet
                        var pSettings = PatronRaporSettings.Load();
                        pSettings.GoogleYorumUrl = txtGoogleYorumUrl.Text.Trim();
                        pSettings.GunSonuRaporSaati = string.IsNullOrWhiteSpace(txtGunSonuSaat.Text) ? "20:00" : txtGunSonuSaat.Text.Trim();
                        pSettings.PatronTelefon = txtPatronTelefon.Text.Trim();
                        pSettings.GunSonuRaporAktif = chkGunSonuAktif.IsChecked == true;
                        pSettings.Save();

                        MessageBox.Show("Firma ve bildirim ayarları başarıyla kaydedildi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        if (!await SessionHelper.CheckSession(response))
                        {
                            string errMsg = await response.Content.ReadAsStringAsync();
                            MessageBox.Show("Ayarlar kaydedilemedi: " + errMsg, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bağlantı hatası: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnGeri_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null && NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        private async void btnGunSonuRaporGonder_Click(object sender, RoutedEventArgs e)
        {
            string tel = txtPatronTelefon.Text.Trim();
            if (string.IsNullOrWhiteSpace(tel))
            {
                MessageBox.Show("Lütfen önce Patron Telefon Numarasını giriniz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var (success, msg) = await Services.NotificationService.SendGunSonuRaporuAsync(tel);
            if (!success)
            {
                MessageBox.Show(msg, "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void btnTestSms_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Önce girili ayarları API'ye kaydedelim
                string selProvider = (cmbSmsProvider.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Netgsm";
                if (string.IsNullOrWhiteSpace(txtSmsUser.Text) || string.IsNullOrWhiteSpace(txtSmsPassword.Password))
                {
                    MessageBox.Show("Test SMS gönderebilmek için önce Kullanıcı Adı ve Şifre girmelisiniz.", "Eksik Bilgi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var smsDto = new Models.SmsSettingsDto
                {
                    SmsProvider = selProvider,
                    SmsHeader = txtSmsHeader.Text.Trim(),
                    SmsUser = txtSmsUser.Text.Trim(),
                    SmsPassword = txtSmsPassword.Password,
                    SmsApiUrlTemplate = txtSmsApiUrlTemplate.Text.Trim(),
                    AutoSmsOnCompleted = chkAutoSmsOnCompleted.IsChecked == true,
                    AutoSmsOnKabul = chkAutoSmsOnKabul.IsChecked == true
                };
                await Services.NotificationService.SaveSmsSettingsAsync(smsDto);

                string testPhone = txtTelefon.Text.Trim();
                if (string.IsNullOrWhiteSpace(testPhone) || testPhone.Length < 10)
                {
                    MessageBox.Show("Lütfen 'Telefon Numarası' kutusuna test SMS'inin gideceği kendi cep telefonunuzu yazın.", "Telefon Belirtin", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtTelefon.Focus();
                    return;
                }

                string testMesaj = $"AlpSoft Servis Takip: SMS entegrasyon test mesajidir. Saglayici: {selProvider}. Sistem basariyla calisiyor!";

                var (success, msg) = await Services.NotificationService.SendSmsAsync(testPhone, testMesaj);

                if (success)
                {
                    MessageBox.Show($"✅ TEST BAŞARILI!\n\n{msg}\n\nSMS '{testPhone}' numarasına başarıyla iletildi.", "SMS Testi Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"❌ SMS GÖNDERİLEMEDİ:\n\n{msg}\n\nLütfen kullanıcı bilgilerinizi ve onaylı başlığınızı kontrol edin.", "SMS Testi Başarısız", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Test gönderiminde hata: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
