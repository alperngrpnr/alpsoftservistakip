using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using alpsoftservistakip;
using alpsoftservistakip.Helpers;
using alpsoftservistakip.Models;
using alpsoftservistakip.Services;

namespace alpsoftservistakip.ViewModels
{
    public class PageKayitOlusturViewModel : BaseViewModel
    {
        private bool _degisiklikVar = false;
        private int _kayitId;
        private bool _yuklemeDevamEdiyor = false;
        private bool _disServisSatirKaynagi = false;
        private int _disServisAdayKayitId = 0;

        public bool DegisiklikVar
        {
            get => _degisiklikVar;
            set => SetProperty(ref _degisiklikVar, value);
        }

        public bool YuklemeDevamEdiyor
        {
            get => _yuklemeDevamEdiyor;
            set => SetProperty(ref _yuklemeDevamEdiyor, value);
        }

        private bool _isFromDisServis = false;
        public bool IsFromDisServis
        {
            get => _isFromDisServis;
            set
            {
                if (SetProperty(ref _isFromDisServis, value))
                {
                    OnPropertyChanged(nameof(DisServisGonderVisibility));
                    OnPropertyChanged(nameof(FisiYazdirVisibility));
                }
            }
        }

        public Visibility DisServisGonderVisibility => IsFromDisServis ? Visibility.Collapsed : Visibility.Visible;
        public Visibility FisiYazdirVisibility => IsFromDisServis ? Visibility.Collapsed : Visibility.Visible;
        public Visibility KayitSilVisibility => _kayitId > 0 && !IsFromDisServis ? Visibility.Visible : Visibility.Collapsed;

        // Properties
        private string _bayiAdi;
        private string _adSoyad;
        private string _cepTelefonu;
        private string _adres;
        private string _eposta;
        private string _cihazTuru;
        private string _marka;
        private string _model;
        private string _imeiNo;
        private string _ekBilgiler;
        private string _sikayetAriza;
        private string _fiyatBilgisi;
        private string _odemeTuru = "Nakit";
        private string _ilgiliTeknisyen;
        private string _servisDurumu;
        private bool _yeni;
        private bool _eski;
        private bool _tamirGormus;
        private bool _garantili;
        private bool _garantisiz;
        private bool _servisGarantili;
        private bool _yedeklemeYapilsin;
        private System.Collections.ObjectModel.ObservableCollection<string> _bayiListesi;
        private System.Collections.ObjectModel.ObservableCollection<string> _tekniksijenListesi;

        // ── Toptancı & Parça ────────────────────────────────────────────────
        private System.Collections.ObjectModel.ObservableCollection<Models.ToptanciListeDto> _toptanciListesi;
        private System.Collections.ObjectModel.ObservableCollection<Models.ServisParcaItemDto> _parcaListesi = new System.Collections.ObjectModel.ObservableCollection<Models.ServisParcaItemDto>();
        private Models.ToptanciListeDto _seciliToptanci;
        private string _parcaAdi = "";
        private int _parcaAdet = 1;
        private decimal _parcaBirimFiyati = 0;

        private DateTime _kayitTarihi = DateTime.Today;
        private string _kayitSaati = DateTime.Now.ToString("HH:mm");

        public DateTime KayitTarihi
        {
            get => _kayitTarihi;
            set
            {
                if (_kayitTarihi != value)
                {
                    _kayitTarihi = value;
                    OnPropertyChanged(nameof(KayitTarihi));
                    OnPropertyChanged(nameof(KayitTarihiFormatli));
                    if (!YuklemeDevamEdiyor) DegisiklikVar = true;
                }
            }
        }

        public string KayitSaati
        {
            get => _kayitSaati;
            set
            {
                if (_kayitSaati != value)
                {
                    _kayitSaati = value;
                    OnPropertyChanged(nameof(KayitSaati));
                    OnPropertyChanged(nameof(KayitTarihiFormatli));
                    if (!YuklemeDevamEdiyor) DegisiklikVar = true;
                }
            }
        }

        public string KayitBaslik => _kayitId > 0 ? $"Servis #{_kayitId}" : "Yeni Servis Kaydı";
        public string KayitTarihiFormatli => $"{KayitTarihi:dd.MM.yyyy} {KayitSaati}";
        public Visibility KayitNoBadgeVisibility => _kayitId > 0 ? Visibility.Visible : Visibility.Collapsed;

        public string BayiAdi { get => _bayiAdi; set => SetProperty(ref _bayiAdi, value); }
        public string AdSoyad { get => _adSoyad; set { SetProperty(ref _adSoyad, value); if (!YuklemeDevamEdiyor) DegisiklikVar = true; } }
        public string CepTelefonu { get => _cepTelefonu; set { SetProperty(ref _cepTelefonu, value); if (!YuklemeDevamEdiyor) DegisiklikVar = true; } }
        public string Adres { get => _adres; set { SetProperty(ref _adres, value); if (!YuklemeDevamEdiyor) DegisiklikVar = true; } }
        public string EPosta { get => _eposta; set { SetProperty(ref _eposta, value); if (!YuklemeDevamEdiyor) DegisiklikVar = true; } }
        public string CihazTuru { get => _cihazTuru; set { SetProperty(ref _cihazTuru, value); if (!YuklemeDevamEdiyor) DegisiklikVar = true; } }
        public string Marka { get => _marka; set { SetProperty(ref _marka, value); if (!YuklemeDevamEdiyor) DegisiklikVar = true; } }
        public string Model { get => _model; set { SetProperty(ref _model, value); if (!YuklemeDevamEdiyor) DegisiklikVar = true; } }
        public string ImeiNo { get => _imeiNo; set { SetProperty(ref _imeiNo, value); if (!YuklemeDevamEdiyor) DegisiklikVar = true; } }
        public string EkBilgiler { get => _ekBilgiler; set { SetProperty(ref _ekBilgiler, value); if (!YuklemeDevamEdiyor) DegisiklikVar = true; } }
        public string SikayetAriza { get => _sikayetAriza; set { SetProperty(ref _sikayetAriza, value); if (!YuklemeDevamEdiyor) DegisiklikVar = true; } }
        public string FiyatBilgisi
        {
            get => _fiyatBilgisi;
            set
            {
                if (SetProperty(ref _fiyatBilgisi, value))
                {
                    OnPropertyChanged(nameof(NetKarOzet));
                    OnPropertyChanged(nameof(NetKarGorunurluk));
                    if (!YuklemeDevamEdiyor) DegisiklikVar = true;
                }
            }
        }
        public string OdemeTuru { get => _odemeTuru; set { SetProperty(ref _odemeTuru, value); if (!YuklemeDevamEdiyor) DegisiklikVar = true; } }
        public System.Collections.ObjectModel.ObservableCollection<string> OdemeTuruListesi { get; } = new System.Collections.ObjectModel.ObservableCollection<string> { "Nakit", "Kredi Kartı", "Havale/EFT", "Veresiye" };
        public string IlgiliTeknisyen { get => _ilgiliTeknisyen; set { SetProperty(ref _ilgiliTeknisyen, value); if (!YuklemeDevamEdiyor) DegisiklikVar = true; } }
        public string ServisDurumu 
        { 
            get => _servisDurumu; 
            set 
            { 
                SetProperty(ref _servisDurumu, value); 
                if (!YuklemeDevamEdiyor) DegisiklikVar = true; 
            } 
        }
        public bool Yeni { get => _yeni; set { SetProperty(ref _yeni, value); if (!YuklemeDevamEdiyor) DegisiklikVar = true; } }
        public bool Eski { get => _eski; set { SetProperty(ref _eski, value); if (!YuklemeDevamEdiyor) DegisiklikVar = true; } }
        public bool TamirGormus { get => _tamirGormus; set { SetProperty(ref _tamirGormus, value); if (!YuklemeDevamEdiyor) DegisiklikVar = true; } }
        public bool Garantili { get => _garantili; set { SetProperty(ref _garantili, value); if (!YuklemeDevamEdiyor) DegisiklikVar = true; } }
        public bool Garantisiz { get => _garantisiz; set { SetProperty(ref _garantisiz, value); if (!YuklemeDevamEdiyor) DegisiklikVar = true; } }
        public bool ServisGarantili { get => _servisGarantili; set { SetProperty(ref _servisGarantili, value); if (!YuklemeDevamEdiyor) DegisiklikVar = true; } }
        public bool YedeklemeYapilsin { get => _yedeklemeYapilsin; set { SetProperty(ref _yedeklemeYapilsin, value); if (!YuklemeDevamEdiyor) DegisiklikVar = true; } }

        public System.Collections.ObjectModel.ObservableCollection<string> BayiListesi
        {
            get => _bayiListesi;
            set => SetProperty(ref _bayiListesi, value);
        }

        public System.Collections.ObjectModel.ObservableCollection<string> TekniksijenListesi
        {
            get => _tekniksijenListesi;
            set => SetProperty(ref _tekniksijenListesi, value);
        }

        // ── Toptancı & Parça ────────────────────────────────────────────────
        public System.Collections.ObjectModel.ObservableCollection<Models.ToptanciListeDto> ToptanciListesi
        {
            get => _toptanciListesi;
            set => SetProperty(ref _toptanciListesi, value);
        }

        public Models.ToptanciListeDto SeciliToptanci
        {
            get => _seciliToptanci;
            set
            {
                if (SetProperty(ref _seciliToptanci, value))
                {
                    OnPropertyChanged(nameof(NetKarOzet));
                    OnPropertyChanged(nameof(NetKarGorunurluk));
                    if (!YuklemeDevamEdiyor) DegisiklikVar = true;
                }
            }
        }

        public System.Collections.ObjectModel.ObservableCollection<Models.ServisParcaItemDto> ParcaListesi
        {
            get => _parcaListesi;
            set => SetProperty(ref _parcaListesi, value);
        }

        public string ParcaAdi
        {
            get => _parcaAdi;
            set
            {
                if (SetProperty(ref _parcaAdi, value))
                {
                    OnPropertyChanged(nameof(ToplamParcaMaliyeti));
                    OnPropertyChanged(nameof(NetKarOzet));
                    OnPropertyChanged(nameof(NetKarGorunurluk));
                    if (!YuklemeDevamEdiyor) DegisiklikVar = true;
                }
            }
        }

        public int ParcaAdet
        {
            get => _parcaAdet;
            set
            {
                if (SetProperty(ref _parcaAdet, value))
                {
                    OnPropertyChanged(nameof(ToplamParcaMaliyeti));
                    OnPropertyChanged(nameof(NetKarOzet));
                    if (!YuklemeDevamEdiyor) DegisiklikVar = true;
                }
            }
        }

        public decimal ParcaBirimFiyati
        {
            get => _parcaBirimFiyati;
            set
            {
                if (SetProperty(ref _parcaBirimFiyati, value))
                {
                    OnPropertyChanged(nameof(ToplamParcaMaliyeti));
                    OnPropertyChanged(nameof(NetKarOzet));
                    if (!YuklemeDevamEdiyor) DegisiklikVar = true;
                }
            }
        }

        /// <summary>Toplam parça maliyeti (USD)</summary>
        public decimal ToplamParcaMaliyeti
        {
            get
            {
                decimal total = _parcaListesi?.Sum(p => p.ToplamTutar) ?? 0;
                if ((_parcaListesi == null || _parcaListesi.Count == 0) && ParcaAdet > 0 && ParcaBirimFiyati > 0)
                {
                    total += ParcaAdet * ParcaBirimFiyati;
                }
                return total;
            }
        }

        /// <summary>Net kâr görsel özet — parça USD cinsinden gösterilir</summary>
        public string NetKarOzet
        {
            get
            {
                decimal servisUcreti = 0;
                if (!string.IsNullOrWhiteSpace(FiyatBilgisi))
                    decimal.TryParse(System.Text.RegularExpressions.Regex.Replace(FiyatBilgisi, @"[^\d,\.]", "").Replace(',', '.'),
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out servisUcreti);

                decimal toplamUsd = ToplamParcaMaliyeti;
                int count = _parcaListesi?.Count ?? 0;
                if (count == 0 && toplamUsd > 0) count = 1;

                if (toplamUsd > 0)
                {
                    decimal usdKur = DovizKuruHelper.VarsayilanUsdKur;
                    decimal maliyetTl = Math.Round(toplamUsd * usdKur, 2);
                    decimal netKar = servisUcreti - maliyetTl;
                    return $"{count} Parça: ${toplamUsd:N2} (~{maliyetTl:N2} ₺) | Net Kâr: {(netKar >= 0 ? "+" : "")}{netKar:N2} ₺";
                }
                else if (servisUcreti > 0)
                {
                    return $"Servis Ücreti: {servisUcreti:N2} ₺";
                }
                return "";
            }
        }

        public Visibility NetKarGorunurluk => !string.IsNullOrWhiteSpace(NetKarOzet) ? Visibility.Visible : Visibility.Collapsed;

        private System.Collections.ObjectModel.ObservableCollection<BranchDto> _subeler = new System.Collections.ObjectModel.ObservableCollection<BranchDto>();
        public System.Collections.ObjectModel.ObservableCollection<BranchDto> Subeler
        {
            get => _subeler;
            set => SetProperty(ref _subeler, value);
        }

        private int? _seciliSubeId;
        public int? SeciliSubeId
        {
            get => _seciliSubeId;
            set
            {
                if (SetProperty(ref _seciliSubeId, value))
                {
                    if (!YuklemeDevamEdiyor) DegisiklikVar = true;
                    OnPropertyChanged(nameof(SeciliSubeAdi));
                }
            }
        }

        public string SeciliSubeAdi
        {
            get
            {
                if (SeciliSubeId.HasValue && SeciliSubeId.Value > 0)
                {
                    var b = Subeler?.FirstOrDefault(x => x.BranchId == SeciliSubeId.Value);
                    if (b != null) return b.BranchName;
                }
                return "Şube Seçiniz";
            }
        }

        public async Task YukleSubelerAsync()
        {
            try
            {
                var list = await BranchService.GetBranchesAsync();
                Subeler = new System.Collections.ObjectModel.ObservableCollection<BranchDto>(list);
                if ((!SeciliSubeId.HasValue || SeciliSubeId.Value <= 0) && Subeler.Count > 0)
                {
                    if (Class1.AktifSubeId > 0 && Subeler.Any(x => x.BranchId == Class1.AktifSubeId))
                    {
                        SeciliSubeId = Class1.AktifSubeId;
                    }
                    else
                    {
                        var merkez = Subeler.FirstOrDefault(x => x.IsMerkez) ?? Subeler.First();
                        SeciliSubeId = merkez.BranchId;
                    }
                }
            }
            catch { }
        }

        public ICommand ParcaEkleCommand { get; }
        public ICommand ParcaSilCommand { get; }

        public ICommand KaydetCommand { get; }
        public ICommand KayitSilCommand { get; }
        public ICommand GeriDonCommand { get; }
        public ICommand ImeiSorgulaCommand { get; }
        public ICommand ServisDurumuChangedCommand { get; }
        public ICommand TelefonFormatlaCommand { get; }
        public ICommand FisiYazdirCommand { get; }
        public ICommand EtiketiYazdirCommand { get; }
        public ICommand MusteriyeBildirCommand { get; }
        public ICommand KopyalaCommand { get; }
        public ICommand DisServisGonderCommand { get; }
        public ICommand DisServiseGonderCommand => DisServisGonderCommand;

        public PageKayitOlusturViewModel(int kayitId = 0)
        {
            _kayitId = kayitId;
            KaydetCommand = new RelayCommand(_ => Kaydet());
            KayitSilCommand = new RelayCommand(_ => KayitSil());
            GeriDonCommand = new RelayCommand(_ => GeriDon());
            ImeiSorgulaCommand = new RelayCommand(_ => ImeiSorgula());
            ServisDurumuChangedCommand = new RelayCommand<string>(ServisDurumuChanged);
            TelefonFormatlaCommand = new RelayCommand<string>(TelefonFormatla);
            FisiYazdirCommand = new RelayCommand(_ => FisiYazdir());
            EtiketiYazdirCommand = new RelayCommand(_ => EtiketiYazdir());
            MusteriyeBildirCommand = new RelayCommand(_ => MusteriyeBildir());
            KopyalaCommand = new RelayCommand(_ => Kopyala());
            DisServisGonderCommand = new RelayCommand(_ => DisServisGonder());
            ParcaEkleCommand = new RelayCommand(_ => ParcaEkle());
            ParcaSilCommand = new RelayCommand<Models.ServisParcaItemDto>(ParcaSil);

            // Listeleri yükle
            BayiListesi = new System.Collections.ObjectModel.ObservableCollection<string>();
            TekniksijenListesi = new System.Collections.ObjectModel.ObservableCollection<string>();
            ToptanciListesi = new System.Collections.ObjectModel.ObservableCollection<Models.ToptanciListeDto>();

            _ = YukleBayiListesiAsync();
            _ = YukluTekniksijenListesiAsync();
            _ = YukleToptanciListesiAsync();
            _ = YukleSubelerAsync();

            if (kayitId > 0)
            {
                CarregarKayit(kayitId);
            }
        }

        public async void CarregarKayit(int id)
        {
            _kayitId = id;
            OnPropertyChanged(nameof(KayitSilVisibility));
            OnPropertyChanged(nameof(KayitBaslik));
            OnPropertyChanged(nameof(KayitNoBadgeVisibility));
            _disServisSatirKaynagi = false;
            _disServisAdayKayitId = 0;
            YuklemeDevamEdiyor = true;
            await VerileriYukleAsync(id);
            YuklemeDevamEdiyor = false;
            DegisiklikVar = false;
        }

        public void DisServisSatirindanYukle(DataRowView row)
        {
            if (row == null)
                return;

            YuklemeDevamEdiyor = true;

            AdSoyad = GetRowString(row, "IsimSoyisim", "MusteriAdSoyad", "MusteriAdi");
            BayiAdi = GetRowString(row, "BayiAdi", "YetkiliBayi", "DisServisYeri");
            CepTelefonu = GetRowString(row, "CepTelefonu", "Telefon", "TelefonNo", "Gsm");
            EPosta = GetRowString(row, "EPosta", "Email", "Mail");
            CihazTuru = GetRowString(row, "CihazTuru", "UrunTuru", "Tur");
            Marka = GetRowString(row, "Marka");
            Model = GetRowString(row, "Model");
            ImeiNo = GetRowString(row, "IMEI", "ImeiNo", "SeriNo");
            EkBilgiler = GetRowString(row, "EkBilgiler", "Notlar", "Aciklama");
            SikayetAriza = GetRowString(row, "Ariza", "ArizaDetayi", "Sikayet");
            ServisDurumu = GetRowString(row, "ServisDurumu", "Durum");
            IlgiliTeknisyen = GetRowString(row, "ServisTeknisyen", "Teknisyen", "SorumluTeknisyen");

            _disServisSatirKaynagi = true;
            IsFromDisServis = true;
            _disServisAdayKayitId = GetRowInt(row, "ServisKayitID", "ServisKayitId", "CihazKayitID", "CihazKayitId", "KaynakKayitID", "KaynakKayitId", "KayitID", "KayitId", "ID");

            int bagliKayitId = GetRowInt(row, "ServisKayitID", "ServisKayitId", "CihazKayitID", "CihazKayitId", "KaynakKayitID", "KaynakKayitId");
            _kayitId = bagliKayitId > 0 ? bagliKayitId : 0;
            OnPropertyChanged(nameof(KayitSilVisibility));

            DegisiklikVar = false;
            YuklemeDevamEdiyor = false;
        }

        // JObject overload: PageDisServis.xaml.cs'den gelen API verisi için
        public void DisServisSatirindanYukle(Newtonsoft.Json.Linq.JObject row)
        {
            if (row == null)
                return;

            string GetStr(params string[] keys)
            {
                foreach (var k in keys)
                {
                    var t = row.GetValue(k, System.StringComparison.OrdinalIgnoreCase);
                    if (t != null && t.Type != Newtonsoft.Json.Linq.JTokenType.Null)
                        return t.ToString();
                }
                return "";
            }
            int GetInt(params string[] keys)
            {
                foreach (var k in keys)
                {
                    var t = row.GetValue(k, System.StringComparison.OrdinalIgnoreCase);
                    if (t != null && t.Type != Newtonsoft.Json.Linq.JTokenType.Null)
                        if (int.TryParse(t.ToString(), out int v)) return v;
                }
                return 0;
            }

            YuklemeDevamEdiyor = true;

            AdSoyad = GetStr("IsimSoyisim", "MusteriAdSoyad", "MusteriAdi");
            BayiAdi = GetStr("BayiAdi", "YetkiliBayi", "DisServisYeri");
            CepTelefonu = GetStr("CepTelefonu", "Telefon", "TelefonNo", "Gsm");
            EPosta = GetStr("EPosta", "Email", "Mail");
            CihazTuru = GetStr("CihazTuru", "UrunTuru", "Tur");
            Marka = GetStr("Marka");
            Model = GetStr("Model");
            ImeiNo = GetStr("IMEI", "ImeiNo", "SeriNo");
            EkBilgiler = GetStr("EkBilgiler", "Notlar", "Aciklama");
            SikayetAriza = GetStr("Ariza", "ArizaDetayi", "Sikayet");
            ServisDurumu = GetStr("ServisDurumu", "Durum");
            IlgiliTeknisyen = GetStr("ServisTeknisyen", "Teknisyen", "SorumluTeknisyen");

            _disServisSatirKaynagi = true;
            IsFromDisServis = true;
            _disServisAdayKayitId = GetInt("ServisKayitID", "ServisKayitId", "CihazKayitID", "CihazKayitId", "KaynakKayitID", "KaynakKayitId", "KayitID", "KayitId", "ID");

            int bagliKayitId = GetInt("ServisKayitID", "ServisKayitId", "CihazKayitID", "CihazKayitId", "KaynakKayitID", "KaynakKayitId");
            _kayitId = bagliKayitId > 0 ? bagliKayitId : 0;
            OnPropertyChanged(nameof(KayitSilVisibility));

            DegisiklikVar = false;
            YuklemeDevamEdiyor = false;
        }

        private async Task VerileriYukleAsync(int id)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Class1.JwtToken);
                    var response = await client.GetAsync($"{ApiConfig.Api}/KayitliCihazlar/{id}");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var data = JsonConvert.DeserializeObject<KayitliCihazDto>(json);
                        if (data != null)
                        {
                            AdSoyad = data.IsimSoyisim ?? "";
                            BayiAdi = data.BayiAdi ?? "";
                            CepTelefonu = data.CepTelefonu ?? "";
                            Adres = data.Adres ?? "";
                            EPosta = data.EPosta ?? "";
                            CihazTuru = data.CihazTuru ?? "";
                            Marka = data.Marka ?? "";
                            Model = data.Model ?? "";
                            ImeiNo = data.IMEI ?? "";
                            EkBilgiler = data.EkBilgiler ?? "";
                            SikayetAriza = data.Ariza ?? "";
                            FiyatBilgisi = data.FiyatBilgisi ?? "";
                            OdemeTuru = string.IsNullOrWhiteSpace(data.OdemeTuru) ? "Nakit" : data.OdemeTuru;
                            IlgiliTeknisyen = data.ServisTeknisyen ?? "";
                            ServisDurumu = data.ServisDurumu ?? "";

                            Yeni = data.Yeni;
                            Eski = data.Eski;
                            TamirGormus = data.TamirGormus;
                            Garantili = data.Garantili;
                            Garantisiz = data.Garantisiz;
                            ServisGarantili = data.ServisGarantili;
                            YedeklemeYapilsin = data.YedeklemeYapilsin;

                            if (data.BranchId.HasValue && data.BranchId.Value > 0)
                            {
                                SeciliSubeId = data.BranchId.Value;
                            }
                            else if (Class1.AktifSubeId > 0)
                            {
                                SeciliSubeId = Class1.AktifSubeId;
                            }

                            if (data.KayitTarihi != default(DateTime) && data.KayitTarihi.Year > 2000)
                            {
                                KayitTarihi = data.KayitTarihi.Date;
                                KayitSaati = data.KayitTarihi.ToString("HH:mm");
                            }

                            // Toptancı ve parça maliyet bilgilerini yükle
                            if (ToptanciListesi == null || ToptanciListesi.Count == 0)
                            {
                                await YukleToptanciListesiAsync();
                            }

                            ParcaListesi.Clear();
                            if (data.Parcalar != null && data.Parcalar.Count > 0)
                            {
                                foreach (var p in data.Parcalar)
                                {
                                    ParcaListesi.Add(new Models.ServisParcaItemDto
                                    {
                                        ID = p.ID,
                                        ToptanciID = p.ToptanciID,
                                        ToptanciAdi = p.ToptanciAdi ?? ToptanciListesi.FirstOrDefault(t => t.ID == p.ToptanciID)?.FirmaAdi ?? "Toptancı",
                                        ParcaAdi = p.ParcaAdi,
                                        Adet = p.Adet,
                                        BirimFiyati = p.BirimFiyati
                                    });
                                }
                            }
                            else if (data.ToptanciID.HasValue && data.ToptanciID.Value > 0 && !string.IsNullOrWhiteSpace(data.ParcaAdi))
                            {
                                ParcaListesi.Add(new Models.ServisParcaItemDto
                                {
                                    ToptanciID = data.ToptanciID.Value,
                                    ToptanciAdi = ToptanciListesi.FirstOrDefault(t => t.ID == data.ToptanciID.Value)?.FirmaAdi ?? "Toptancı",
                                    ParcaAdi = data.ParcaAdi,
                                    Adet = data.ParcaAdet.GetValueOrDefault(1),
                                    BirimFiyati = data.ParcaBirimFiyati.GetValueOrDefault(0)
                                });
                            }

                            SeciliToptanci = null;
                            ParcaAdi = "";
                            ParcaAdet = 1;
                            ParcaBirimFiyati = 0;

                            OnPropertyChanged(nameof(SeciliToptanci));
                            OnPropertyChanged(nameof(ParcaAdi));
                            OnPropertyChanged(nameof(ParcaAdet));
                            OnPropertyChanged(nameof(ParcaBirimFiyati));
                            OnPropertyChanged(nameof(ToplamParcaMaliyeti));
                            OnPropertyChanged(nameof(NetKarOzet));
                            OnPropertyChanged(nameof(NetKarGorunurluk));
                        }
                    }
                    else
                    {
                        var err = await response.Content.ReadAsStringAsync();
                        MessageBox.Show("Veri yükleme hatası: " + err);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veri yükleme hatası: " + ex.Message);
            }
        }

        private async Task YukleBayiListesiAsync()
        {
            try
            {
                BayiListesi.Clear();
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Class1.JwtToken);
                    var response = await client.GetAsync($"{ApiConfig.Api}/Cariler/bayiler");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var liste = JsonConvert.DeserializeObject<List<string>>(json);
                        if (liste != null)
                        {
                            foreach (var item in liste)
                            {
                                if (!string.IsNullOrWhiteSpace(item))
                                    BayiListesi.Add(item);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bayi listesi yükleme hatası: " + ex.Message);
            }
        }

        private async Task YukluTekniksijenListesiAsync()
        {
            try
            {
                TekniksijenListesi.Clear();
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Class1.JwtToken);
                    var response = await client.GetAsync($"{ApiConfig.Api}/Users/teknisyenler");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var liste = JsonConvert.DeserializeObject<List<string>>(json);
                        if (liste != null)
                        {
                            foreach (var item in liste)
                            {
                                if (!string.IsNullOrWhiteSpace(item))
                                    TekniksijenListesi.Add(item);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Teknisyen listesi yükleme hatası: " + ex.Message);
            }
        }

        private async Task YukleToptanciListesiAsync()
        {
            try
            {
                ToptanciListesi.Clear();
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Class1.JwtToken);
                    var response = await client.GetAsync($"{ApiConfig.Api}/Toptanci/liste");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var liste = JsonConvert.DeserializeObject<List<Models.ToptanciListeDto>>(json);
                        if (liste != null)
                        {
                            foreach (var item in liste)
                                ToptanciListesi.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Toptancı listesi yükleme hatası: " + ex.Message);
            }
        }

        private async void KayitSil()
        {
            if (_kayitId <= 0) return;

            var result = MessageBox.Show($"Seçili servis kaydını (Kayıt No: {_kayitId}) silmek istediğinize emin misiniz?",
                "Kayıt Sil", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Class1.JwtToken);
                        var response = await client.DeleteAsync($"{ApiConfig.Api}/kayitlicihazlar/{_kayitId}");

                        if (response.IsSuccessStatusCode)
                        {
                            MessageBox.Show("Kayıt başarıyla silindi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                            GeriDon(); // Silindikten sonra detay penceresini kapat
                        }
                        else
                        {
                            string err = await response.Content.ReadAsStringAsync();
                            MessageBox.Show("Silme işlemi başarısız: " + err, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Bağlantı hatası: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void Kaydet()
        {
            try
            {
                DateTime sonKayitTarihi = KayitTarihi.Date;
                if (TimeSpan.TryParse(KayitSaati, out TimeSpan ts))
                {
                    sonKayitTarihi = sonKayitTarihi.Add(ts);
                }
                else
                {
                    sonKayitTarihi = sonKayitTarihi.Add(DateTime.Now.TimeOfDay);
                }

                var dto = new CreateKayitliCihazDto
                {
                    KayitTarihi = sonKayitTarihi,
                    BayiAdi = (BayiAdi == "Bayi Adı" || string.IsNullOrWhiteSpace(BayiAdi)) ? null : BayiAdi,
                    IsimSoyisim = (AdSoyad == "Ad Soyad*" || string.IsNullOrWhiteSpace(AdSoyad)) ? "" : AdSoyad,
                    CepTelefonu = (CepTelefonu == "Cep Telefonu*" || string.IsNullOrWhiteSpace(CepTelefonu)) ? "" : CepTelefonu,
                    Adres = (Adres == "Adres" || string.IsNullOrWhiteSpace(Adres)) ? null : Adres,
                    EPosta = (EPosta == "E-Posta" || string.IsNullOrWhiteSpace(EPosta)) ? null : EPosta,
                    ServisTeknisyen = (IlgiliTeknisyen == "İlgili Teknisyen" || string.IsNullOrWhiteSpace(IlgiliTeknisyen)) ? null : IlgiliTeknisyen,
                    ServisDurumu = string.IsNullOrWhiteSpace(ServisDurumu) ? null : ServisDurumu,
                    // "Teslim" geçiyorsa kasaya bugünün tarihiyle yazılır
                    TeslimTarihi = (!string.IsNullOrWhiteSpace(ServisDurumu) &&
                                    ServisDurumu.IndexOf("Teslim", StringComparison.OrdinalIgnoreCase) >= 0)
                                   ? DateTime.Now
                                   : (DateTime?)null,
                    CihazTuru = (CihazTuru == "Cihaz Türü*" || string.IsNullOrWhiteSpace(CihazTuru)) ? "" : CihazTuru,
                    Marka = (Marka == "Marka*" || string.IsNullOrWhiteSpace(Marka)) ? "" : Marka,
                    Model = (Model == "Model*" || string.IsNullOrWhiteSpace(Model)) ? "" : Model,
                    IMEI = (ImeiNo == "IMEI No" || string.IsNullOrWhiteSpace(ImeiNo)) ? null : ImeiNo,
                    EkBilgiler = (EkBilgiler == "Ek Bilgiler" || string.IsNullOrWhiteSpace(EkBilgiler)) ? null : EkBilgiler,
                    Ariza = (SikayetAriza == "Şikayet/Arıza*" || string.IsNullOrWhiteSpace(SikayetAriza)) ? "" : SikayetAriza,
                    FiyatBilgisi = (FiyatBilgisi == "Fiyat Bilgisi" || string.IsNullOrWhiteSpace(FiyatBilgisi)) ? null : FiyatBilgisi,
                    OdemeTuru = string.IsNullOrWhiteSpace(OdemeTuru) ? "Nakit" : OdemeTuru,
                    Yeni = Yeni,
                    Eski = Eski,
                    TamirGormus = TamirGormus,
                    Garantili = Garantili,
                    Garantisiz = Garantisiz,
                    ServisGarantili = ServisGarantili,
                    YedeklemeYapilsin = YedeklemeYapilsin,
                    // Toptancı & Parça
                    ToptanciID = SeciliToptanci != null ? SeciliToptanci.ID : 0,
                    ParcaAdi = string.IsNullOrWhiteSpace(ParcaAdi) ? "" : ParcaAdi,
                    ParcaAdet = ParcaAdet,
                    ParcaBirimFiyati = ParcaBirimFiyati,
                    BranchId = (SeciliSubeId.HasValue && SeciliSubeId.Value > 0)
                        ? SeciliSubeId.Value
                        : (Class1.AktifSubeId > 0 ? Class1.AktifSubeId : (Subeler?.FirstOrDefault(x => x.IsMerkez)?.BranchId ?? Subeler?.FirstOrDefault()?.BranchId))
                };

                // Eğer kullanıcı alanları doldurup "+ Ekle" butonuna basmadıysa otomatik ekle
                if (SeciliToptanci != null && !string.IsNullOrWhiteSpace(ParcaAdi) && ParcaAdet > 0 && ParcaBirimFiyati > 0)
                {
                    ParcaListesi.Add(new Models.ServisParcaItemDto
                    {
                        ToptanciID = SeciliToptanci.ID,
                        ToptanciAdi = SeciliToptanci.FirmaAdi,
                        ParcaAdi = ParcaAdi.Trim(),
                        Adet = ParcaAdet,
                        BirimFiyati = ParcaBirimFiyati
                    });
                    ParcaAdi = "";
                    ParcaAdet = 1;
                    ParcaBirimFiyati = 0;
                }

                // Çoklu parça listesini DTO'ya ekle
                dto.Parcalar = ParcaListesi.ToList();
                if (dto.Parcalar.Count > 0)
                {
                    var first = dto.Parcalar[0];
                    dto.ToptanciID = first.ToptanciID;
                    dto.ParcaAdi = string.Join(", ", dto.Parcalar.Select(x => x.ParcaAdi));
                    dto.ParcaAdet = dto.Parcalar.Sum(x => x.Adet);
                    dto.ParcaBirimFiyati = first.BirimFiyati;
                }

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Class1.JwtToken);
                    var content = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");

                    if (_kayitId > 0)
                    {
                        var response = await client.PutAsync($"{ApiConfig.Api}/KayitliCihazlar/{_kayitId}", content);
                        if (!response.IsSuccessStatusCode)
                        {
                            var err = await response.Content.ReadAsStringAsync();
                            MessageBox.Show($"Güncelleme başarısız: {err}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        MessageBox.Show("✅ Kayıt başarıyla güncellendi!", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        var response = await client.PostAsync($"{ApiConfig.Api}/KayitliCihazlar", content);
                        if (!response.IsSuccessStatusCode)
                        {
                            var err = await response.Content.ReadAsStringAsync();
                            MessageBox.Show($"Kayıt oluşturulamadı: {err}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        var resJson = await response.Content.ReadAsStringAsync();
                        var resObj = JsonConvert.DeserializeObject<dynamic>(resJson);
                        if (resObj?.ID != null)
                        {
                            _kayitId = (int)resObj.ID;
                        }
                        MessageBox.Show("✅ Yeni kayıt başarıyla eklendi!", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }

                // ── MÜŞTERİ BİLDİRİMİ (WHATSAPP / SMS) ──
                if (_kayitId > 0 && !string.IsNullOrWhiteSpace(dto.CepTelefonu))
                {
                    if (dto.ServisDurumu == "Tamamlandı" || dto.ServisDurumu == "Teslim Edildi" ||
                        dto.ServisDurumu == "Fiyat Onayı Bekliyor" || dto.ServisDurumu == "Fiyat Teklifi Verildi")
                    {
                        var bildirimSonuc = MessageBox.Show(
                            $"Cihaz servis durumu '{dto.ServisDurumu}' olarak kaydedildi.\n\nMüşteriye WhatsApp veya SMS ile durum bildirimi göndermek ister misiniz?",
                            "Müşteri Bilgilendirmesi",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                        if (bildirimSonuc == MessageBoxResult.Yes)
                        {
                            MusteriyeBildir();
                        }
                    }
                }

                // ── VERESİYE DEFTERİ ENTEGRASYONU ──
                if (_kayitId > 0)
                {
                    string srvNo = $"SRV-{_kayitId:D6}";
                    if (string.Equals(dto.OdemeTuru, "Veresiye", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            decimal servisUcreti = 0;
                            if (!string.IsNullOrWhiteSpace(dto.FiyatBilgisi))
                            {
                                var clean = System.Text.RegularExpressions.Regex.Replace(dto.FiyatBilgisi, @"[^\d,\.]", "").Replace(",", ".");
                                decimal.TryParse(clean, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out servisUcreti);
                            }

                            if (servisUcreti > 0 && !string.IsNullOrWhiteSpace(dto.IsimSoyisim))
                            {
                                int custId = await VeresiyeService.GetOrCreateCustomerByNamePhoneAsync(dto.IsimSoyisim, dto.CepTelefonu);
                                if (custId > 0)
                                {
                                    string aciklama = $"Servis Borcu: {srvNo} ({dto.Marka} {dto.Model} - {dto.Ariza})";
                                    await VeresiyeService.AddOrUpdateVeresiyeBorcByRefAsync(custId, servisUcreti, aciklama, srvNo);
                                }
                            }
                        }
                        catch (Exception vEx)
                        {
                            Console.WriteLine("Veresiye defteri servis borç kaydetme hatası: " + vEx.Message);
                        }
                    }
                    else
                    {
                        // Eğer önceden veresiye olup şimdi başka bir ödeme türüne alındıysa borç defterinden kaldır
                        try
                        {
                            await VeresiyeService.RemoveVeresiyeBorcByRefAsync(srvNo);
                        }
                        catch { }
                    }
                }

                DegisiklikVar = false;
                _disServisSatirKaynagi = false;
                _disServisAdayKayitId = 0;

                foreach (Window window in Application.Current.Windows)
                {
                    if (window is Window3 window3)
                    {
                        window3.GoBack();
                        break;
                    }
                    else if (window is Window5 window5)
                    {
                        window5.VerileriYukle();
                        window5.HideOverlay();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ HATA:\n\n{ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ParcaEkle()
        {
            if (SeciliToptanci == null)
            {
                MessageBox.Show("Lütfen bir toptancı seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(ParcaAdi))
            {
                MessageBox.Show("Lütfen parça adı girin (Örn: Ekran, Batarya).", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ParcaAdet <= 0)
            {
                MessageBox.Show("Lütfen geçerli bir adet girin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ParcaBirimFiyati <= 0)
            {
                MessageBox.Show("Lütfen parçanın dolar birim fiyatını ($) girin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ParcaListesi.Add(new Models.ServisParcaItemDto
            {
                ToptanciID = SeciliToptanci.ID,
                ToptanciAdi = SeciliToptanci.FirmaAdi,
                ParcaAdi = ParcaAdi.Trim(),
                Adet = ParcaAdet,
                BirimFiyati = ParcaBirimFiyati
            });

            ParcaAdi = "";
            ParcaAdet = 1;
            ParcaBirimFiyati = 0;

            OnPropertyChanged(nameof(ToplamParcaMaliyeti));
            OnPropertyChanged(nameof(NetKarOzet));
            OnPropertyChanged(nameof(NetKarGorunurluk));
            if (!YuklemeDevamEdiyor) DegisiklikVar = true;
        }

        private void ParcaSil(Models.ServisParcaItemDto item)
        {
            if (item != null && ParcaListesi.Contains(item))
            {
                ParcaListesi.Remove(item);
                OnPropertyChanged(nameof(ToplamParcaMaliyeti));
                OnPropertyChanged(nameof(NetKarOzet));
                OnPropertyChanged(nameof(NetKarGorunurluk));
                if (!YuklemeDevamEdiyor) DegisiklikVar = true;
            }
        }

        private void GeriDon()
        {
            if (DegisiklikVar)
            {
                var sonuc = MessageBox.Show(
                    "Kaydedilmemiş değişiklikler var. Geri dönmek istiyor musunuz?",
                    "Uyarı",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (sonuc == MessageBoxResult.No)
                    return;
            }

            DegisiklikVar = false;

            foreach (Window window in Application.Current.Windows)
            {
                if (window is Window3 window3)
                {
                    window3.GoBack();
                    break;
                }
                else if (window is Window5 window5)
                {
                    window5.HideOverlay();
                    break;
                }
            }
        }

        private void ImeiSorgula()
        {
            if (ImeiNo.Length < 15 || ImeiNo == "IMEI No")
            {
                MessageBox.Show("Geçerli bir IMEI girin!");
                return;
            }
            System.Windows.Clipboard.SetText(ImeiNo);
            Process.Start(new ProcessStartInfo { FileName = "https://www.turkiye.gov.tr/imei-sorgulama", UseShellExecute = true });
        }

        private void ServisDurumuChanged(string durum)
        {
            ServisDurumu = durum;
        }

        private void TelefonFormatla(string telefon)
        {
            if (telefon == "Cep Telefonu*") return;

            string raw = new string(telefon.Where(char.IsDigit).ToArray());

            if (raw.Length > 11) raw = raw.Substring(0, 11);
            if (!raw.StartsWith("0") && raw.Length > 0) raw = "0" + raw;

            string formatted = raw;
            if (raw.Length > 4) formatted = $"({raw.Substring(0, 4)}) {raw.Substring(4)}";
            if (raw.Length > 7) formatted = $"({raw.Substring(0, 4)}) {raw.Substring(4, 3)} {raw.Substring(7)}";
            if (raw.Length > 9) formatted = $"({raw.Substring(0, 4)}) {raw.Substring(4, 3)} {raw.Substring(7, 2)} {raw.Substring(9)}";

            CepTelefonu = formatted;
        }

        private void FisiYazdir()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(AdSoyad) || string.IsNullOrWhiteSpace(Marka) || string.IsNullOrWhiteSpace(Model))
                {
                    MessageBox.Show("Müşteri adı, marka ve model bilgileri zorunludur!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                DateTime fisiKayitTarihi = KayitTarihi.Date;
                if (TimeSpan.TryParse(KayitSaati, out TimeSpan ts))
                {
                    fisiKayitTarihi = fisiKayitTarihi.Add(ts);
                }
                else
                {
                    fisiKayitTarihi = fisiKayitTarihi.Add(DateTime.Now.TimeOfDay);
                }

                var fisiData = new alpsoftservistakip.Models.ServisKayitFisiData
                {
                    KayitID = _kayitId,
                    KayitTarihi = fisiKayitTarihi,
                    IsimSoyisim = AdSoyad,
                    CepTelefonu = CepTelefonu,
                    BayiAdi = BayiAdi,
                    Marka = Marka,
                    Model = Model,
                    Ariza = SikayetAriza,
                    ImeiNo = ImeiNo,
                    ServisDurumu = ServisDurumu,
                    IlgiliTeknisyen = IlgiliTeknisyen,
                    EkBilgiler = EkBilgiler,
                    FiyatBilgisi = FiyatBilgisi
                };

                var fisi = new alpsoftservistakip.Controls.ServisKayitFisi()
                {
                    DataContext = fisiData
                };

                Services.WpfPrintService.ShowPrintPreview(fisi, "SERVIS_KAYIT_FISI", "SERVIS KAYIT FİŞİ ÖNİZLEMESİ");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fiş yazdırma hatası: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EtiketiYazdir()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(AdSoyad) || string.IsNullOrWhiteSpace(Marka) || string.IsNullOrWhiteSpace(Model))
                {
                    MessageBox.Show("Müşteri adı, marka ve model bilgileri zorunludur!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                DateTime fisiKayitTarihi = KayitTarihi.Date;
                if (TimeSpan.TryParse(KayitSaati, out TimeSpan ts))
                {
                    fisiKayitTarihi = fisiKayitTarihi.Add(ts);
                }
                else
                {
                    fisiKayitTarihi = fisiKayitTarihi.Add(DateTime.Now.TimeOfDay);
                }

                string srvNo = _kayitId > 0 ? $"SRV-{_kayitId:D6}" : "SRV-YENİ";

                var model = new Models.CihazEtiketiModel
                {
                    ServisNo = srvNo,
                    MusteriAdi = AdSoyad,
                    Telefon = CepTelefonu,
                    Marka = Marka,
                    Model = Model,
                    Ariza = SikayetAriza,
                    CihazSifresi = EkBilgiler,
                    KayitTarihi = fisiKayitTarihi,
                    FirmaAdi = Class1.AktifKullanici?.SirketAdi ?? "AlpSoft Teknik Servis"
                };

                var win = new WindowCihazEtiketYazdir(model)
                {
                    Owner = Application.Current.MainWindow
                };
                win.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Etiket yazdırma hatası: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MusteriyeBildir()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(CepTelefonu))
                {
                    MessageBox.Show("Müşteriye bildirim gönderebilmek için bir cep telefonu numarası girilmelidir.", "Telefon Eksik", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var ctx = new Services.NotificationContext
                {
                    MusteriAdi = AdSoyad ?? "",
                    Telefon = CepTelefonu ?? "",
                    Cihaz = $"{Marka} {Model}".Trim(),
                    ServisNo = _kayitId > 0 ? $"SRV-{_kayitId:D6}" : "",
                    Ariza = SikayetAriza ?? "",
                    Tutar = FiyatBilgisi ?? "",
                    Durum = ServisDurumu ?? ""
                };

                Services.NotificationTemplateType initialTemplate = Services.NotificationTemplateType.Tamamlandi;
                if (ServisDurumu == "Kabul Edildi") initialTemplate = Services.NotificationTemplateType.Kabul;
                else if (ServisDurumu == "İşlemde" || ServisDurumu == "Fiyat Onayı Bekliyor" || ServisDurumu == "Fiyat Teklifi Verildi") initialTemplate = Services.NotificationTemplateType.FiyatOnay;
                else if (ServisDurumu == "Teslim Edildi") initialTemplate = Services.NotificationTemplateType.TeslimEdildi;

                var wnd = new MusteriBildirimWindow(ctx, initialTemplate);
                wnd.Owner = Application.Current.MainWindow;
                wnd.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bildirim penceresi açılırken hata: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Kopyala()
        {
            try
            {
                string metin = $"AD: {AdSoyad}\n" +
                               $"TELEFON: {CepTelefonu}\n" +
                               $"BAYİ: {BayiAdi}\n" +
                               $"CİHAZ: {Marka} {Model}\n" +
                               $"ARIZA: {SikayetAriza}\n" +
                               $"IMEI: {ImeiNo}";

                System.Windows.Clipboard.SetText(metin);
                MessageBox.Show("Bilgiler panonuza kopyalandı!", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kopyalama hatası: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DisServisGonder()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(AdSoyad) || string.IsNullOrWhiteSpace(Marka) || 
                    string.IsNullOrWhiteSpace(Model) || string.IsNullOrWhiteSpace(SikayetAriza))
                {
                    MessageBox.Show("Lütfen zorunlu alanları doldurunuz:\n- Ad Soyadı\n- Marka\n- Model\n- Arıza Detayı", 
                        "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_kayitId <= 0)
                {
                    MessageBox.Show("Lütfen önce cihaz kaydını 'KAYDET' butonu ile kaydediniz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                GoruntuDısServisGonderDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Dış servis gönderme hatası: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GoruntuDısServisGonderDialog()
        {
            var pencere = new Window
            {
                Title = "Dış Servis'e Gönder",
                Width = 400,
                Height = 350,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize,
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(18, 18, 18))
            };

            var stackPanel = new StackPanel { Margin = new Thickness(20) };

            var label = new TextBlock
            {
                Text = "Dış Servis Bilgileri",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = System.Windows.Media.Brushes.White,
                Margin = new Thickness(0, 0, 0, 20)
            };
            stackPanel.Children.Add(label);

            stackPanel.Children.Add(new TextBlock { Text = "Dış Servis Adı / Yeri:", Foreground = System.Windows.Media.Brushes.LightGray, Margin = new Thickness(0,0,0,5) });
            var txtYer = new TextBox { Height = 35, Margin = new Thickness(0,0,0,15), Background=new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(45,45,45)), Foreground=System.Windows.Media.Brushes.White };
            stackPanel.Children.Add(txtYer);

            stackPanel.Children.Add(new TextBlock { Text = "İlgili Kişi:", Foreground = System.Windows.Media.Brushes.LightGray, Margin = new Thickness(0,0,0,5) });
            var txtKisi = new TextBox { Height = 35, Margin = new Thickness(0,0,0,25), Background=new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(45,45,45)), Foreground=System.Windows.Media.Brushes.White  };
            stackPanel.Children.Add(txtKisi);

            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };

            var btnIptal = new Button { Content = "İptal", Width = 90, Height = 35, Background = System.Windows.Media.Brushes.Gray, Foreground = System.Windows.Media.Brushes.White, Margin = new Thickness(0,0,10,0) };
            btnIptal.Click += (s, e) => pencere.Close();

            var btnGonder = new Button { Content = "Gönder", Width = 90, Height = 35, Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(52, 152, 219)), Foreground = System.Windows.Media.Brushes.White };
            btnGonder.Click += (s, e) => 
            {
                if (string.IsNullOrWhiteSpace(txtYer.Text))
                {
                    MessageBox.Show("Lütfen Dış Servis Adı alanını doldurun.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                TaşıDısServisKayıtlarına(txtYer.Text, txtKisi.Text);
                pencere.Close();
            };

            buttonPanel.Children.Add(btnIptal);
            buttonPanel.Children.Add(btnGonder);
            stackPanel.Children.Add(buttonPanel);

            pencere.Content = stackPanel;
            pencere.ShowDialog();
        }

        private async void TaşıDısServisKayıtlarına(string disServisYeri, string ilgiliKisi)
        {
            try
            {
                if (_kayitId <= 0)
                    return;

                var dto = new DisServisTasimaDto
                {
                    KayitId = _kayitId,
                    DisServisYeri = disServisYeri,
                    IlgiliKisi = ilgiliKisi
                };

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Class1.JwtToken);
                    var content = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
                    var response = await client.PostAsync($"{ApiConfig.Api}/DisServis/tasima", content);
                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Kayıt başarıyla Dış Servis Kayıtlarına aktarıldı.", "Başarılı", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        var err = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Dış servis aktarma hatası: {err}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Dış servis aktarma hatası: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetRowString(DataRowView row, params string[] kolonlar)
        {
            foreach (var kolon in kolonlar)
            {
                if (row.DataView.Table.Columns.Contains(kolon) && row[kolon] != DBNull.Value)
                {
                    return row[kolon]?.ToString() ?? "";
                }
            }

            return "";
        }

        private int GetRowInt(DataRowView row, params string[] kolonlar)
        {
            foreach (var kolon in kolonlar)
            {
                if (row.DataView.Table.Columns.Contains(kolon) && row[kolon] != DBNull.Value)
                {
                    if (int.TryParse(row[kolon].ToString(), out int id) && id > 0)
                    {
                        return id;
                    }
                }
            }

            return 0;
        }
    }
}

