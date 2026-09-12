using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using alpsoftservistakip.ViewModels;

namespace alpsoftservistakip
{
    public partial class PageDisServis : Page
    {
        public PageDisServisViewModel ViewModel { get; private set; }

        public PageDisServis()
        {
            InitializeComponent();
            ViewModel = new PageDisServisViewModel();
            DataContext = ViewModel;
        }

        private void disMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgdisservisVeriler.SelectedItem != null)
            {
                var selectedItem = dgdisservisVeriler.SelectedItem;
                Newtonsoft.Json.Linq.JObject row = new Newtonsoft.Json.Linq.JObject();

                if (selectedItem is System.Data.DataRowView dataRowView)
                {
                    foreach (System.Data.DataColumn col in dataRowView.Row.Table.Columns)
                    {
                        var val = dataRowView[col.ColumnName];
                        if (val != null && val != DBNull.Value)
                            row[col.ColumnName] = Newtonsoft.Json.Linq.JToken.FromObject(val);
                        else
                            row[col.ColumnName] = Newtonsoft.Json.Linq.JValue.CreateNull();
                    }
                }
                else
                {
                    row = Newtonsoft.Json.Linq.JObject.FromObject(selectedItem);
                }

                int id = GetServisKayitId(row);

                Window3 mainWindow = Application.Current.Windows.OfType<Window3>().FirstOrDefault();
                if (mainWindow != null)
                {
                    PageKayitOlustur detay = new PageKayitOlustur();
                    var vm = detay.DataContext as ViewModels.PageKayitOlusturViewModel;
                    if (vm != null)
                    {
                        if (id > 0)
                        {
                            vm.CarregarKayit(id);
                        }
                        else
                        {
                            vm.DisServisSatirindanYukle(row);
                        }
                    }
                    mainWindow.ShowOverlayPage(detay);
                }
            }
        }

        private int GetServisKayitId(Newtonsoft.Json.Linq.JObject row)
        {
            string[] oncelikliKolonlar = { "ServisKayitID", "ServisKayitId", "CihazKayitID", "CihazKayitId", "KaynakKayitID", "KaynakKayitId" };

            foreach (var kolon in oncelikliKolonlar)
            {
                var token = row.GetValue(kolon, System.StringComparison.OrdinalIgnoreCase);
                if (token != null && token.Type != Newtonsoft.Json.Linq.JTokenType.Null)
                {
                    if (int.TryParse(token.ToString(), out int id) && id > 0)
                    {
                        return id;
                    }
                }
            }

            return 0;
        }

        private void aramadisservis1_GotFocus(object sender, RoutedEventArgs e)
        {
            ViewModel?.AramaGotFocusCommand.Execute(null);
        }

        private void aramadisservis1_LostFocus(object sender, RoutedEventArgs e)
        {
            ViewModel?.AramaLostFocusCommand.Execute(null);
        }
    }
}



