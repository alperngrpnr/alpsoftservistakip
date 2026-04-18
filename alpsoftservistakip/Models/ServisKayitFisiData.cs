using System;

namespace alpsoftservistakip.Models
{
    public class ServisKayitFisiData
    {
        public int KayitID { get; set; }
        public DateTime KayitTarihi { get; set; }
        public string IsimSoyisim { get; set; }
        public string CepTelefonu { get; set; }
        public string BayiAdi { get; set; }
        public string Marka { get; set; }
        public string Model { get; set; }
        public string Ariza { get; set; }
        public string ImeiNo { get; set; }
        public string ServisDurumu { get; set; }
        public string IlgiliTeknisyen { get; set; }
        public string EkBilgiler { get; set; }
    }
}
