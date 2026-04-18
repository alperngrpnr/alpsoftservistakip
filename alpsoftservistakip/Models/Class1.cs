using System;

namespace alpsoftservistakip.Models
{
    public class ServisModel
    {
        public int ID { get; set; }
        // SQL'de sütun adýn neyse (GirisTarihi ise) burayý ona göre eþitleyeceðiz
        public DateTime ServisTarihi { get; set; }
        public string BayiAdi { get; set; }
        public string IsimSoyisim { get; set; }
        public string CepTelefonu { get; set; }
        public string CihazTuru { get; set; }
        public string Marka { get; set; }
        public string Model { get; set; }
        public string Ariza { get; set; }
        public string ServisDurumu { get; set; }
    }
}

