using System;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using alpsoftservistakip.ViewModels;

namespace alpsoftservistakip
{
    public partial class Window7 : Window
    {
        public Window7ViewModel ViewModel { get; private set; }

        public Window7()
        {
            InitializeComponent();
            ViewModel = new Window7ViewModel();
            DataContext = ViewModel;

            Loaded += Window7_Loaded;
        }

        private void Window7_Loaded(object sender, RoutedEventArgs e)
        {
            // ViewModel zaten constructor'da VerileriGetir() çağırıyor
        }

        private void AltPencere_Closing(object sender, CancelEventArgs e)
        {
            
            Environment.Exit(0);
        }

        // =========================
        // DATAGRID DOUBLE CLICK
        // =========================
        private void disMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgdisservisVeriler.SelectedItem is DataRowView row)
            {
                ViewModel?.KayitDetayCommand.Execute(row);
            }
        }

        // =========================
        // WINDOW6'DAN GERİ DÖNÜŞ
        // =========================
        public void Yenile()
        {
            ViewModel?.Yenile();

            if (!this.IsVisible)
            {
                this.Show();
            }
            this.Activate();
        }

        // =========================
        // PLACEHOLDER
        // =========================
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