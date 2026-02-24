using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using alpsoftservistakip.ViewModels;

namespace alpsoftservistakip
{
    public partial class Page5 : Page
    {
        public Window5ViewModel ViewModel { get; private set; }

        public Page5()
        {
            InitializeComponent();
            ViewModel = new Window5ViewModel();
            DataContext = ViewModel;
            
            if (dgVeriler != null)
            {
                dgVeriler.MouseDoubleClick += dgVeriler_MouseDoubleClick;
            }
            
            if (dtpServisTarihi != null)
            {
                dtpServisTarihi.SelectedDateChanged += DtpServisTarihi_SelectedDateChanged;
            }
        }
        
        private void DtpServisTarihi_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpServisTarihi != null && ViewModel != null)
            {
                ViewModel.SecilenTarih = dtpServisTarihi.SelectedDate;
            }
        }

        private void dgVeriler_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ViewModel?.KayitDetayCommand.Execute(dgVeriler.SelectedItem);
        }

        private void geridön_Click(object sender, RoutedEventArgs e)
        {
            // Window3'e geri dön
            foreach (Window window in Application.Current.Windows)
            {
                if (window is Window3 window3)
                {
                    window3.HideOverlay();
                    break;
                }
            }
        }

        private void arama_GotFocus(object sender, RoutedEventArgs e)
        {
            ViewModel?.AramaGotFocusCommand.Execute(null);
        }

        private void arama_LostFocus(object sender, RoutedEventArgs e)
        {
            ViewModel?.AramaLostFocusCommand.Execute(null);
        }
    }
}

