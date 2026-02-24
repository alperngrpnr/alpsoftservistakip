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
            if (dgdisservisVeriler.SelectedItem is DataRowView row)
            {
                ViewModel?.KayitDetayCommand.Execute(row);
            }
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



