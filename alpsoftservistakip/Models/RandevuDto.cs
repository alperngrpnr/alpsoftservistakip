using System;

namespace alpsoftservistakip.Models
{
    public class RandevuItemDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string MusteriAdi { get; set; } = "";
        public string Telefon { get; set; } = "";
        public string Marka { get; set; } = "";
        public string Model { get; set; } = "";
        public string Cihaz { get; set; } = "";
        public string IslemTuru { get; set; } = "";
        public string Notlar { get; set; } = "";
        public string RandevuTarihi { get; set; } = "";
        public string RandevuTarihiRaw { get; set; } = "";
        public string RandevuSaati { get; set; } = "";
        public string Durum { get; set; } = "Bekliyor";
        public string OlusturmaTarihi { get; set; } = "";
        public int? ServisId { get; set; }

        public bool IsBekliyor => string.Equals(Durum, "Bekliyor", StringComparison.OrdinalIgnoreCase);
        public bool IsOnaylandi => string.Equals(Durum, "Onaylandı", StringComparison.OrdinalIgnoreCase);
        public bool IsServiste => string.Equals(Durum, "Servise Dönüştürüldü", StringComparison.OrdinalIgnoreCase);
        public bool IsIptal => string.Equals(Durum, "İptal Edildi", StringComparison.OrdinalIgnoreCase);

        public string DurumRenk
        {
            get
            {
                if (IsOnaylandi) return "#10B981"; // Yeşil
                if (IsBekliyor) return "#F59E0B"; // Sarı/Turuncu
                if (IsServiste) return "#3B82F6"; // Mavi
                return "#94A3B8"; // Gri/Kırmızı
            }
        }
    }

    public class RandevuAyarClientDto
    {
        public int CompanyId { get; set; } = 1;
        public string CompanyName { get; set; } = "";
        public string Address { get; set; } = "";
        public string Phone { get; set; } = "";
        public string BaslangicSaati { get; set; } = "09:00";
        public string BitisSaati { get; set; } = "19:00";
        public int RandevuAraligiDk { get; set; } = 30;
        public string CalismaGunleri { get; set; } = "1,2,3,4,5,6";
        public int AyniSaatteMaksimum { get; set; } = 1;
        public bool Aktif { get; set; } = true;
        public bool OtoOnay { get; set; } = true;
    }
}
