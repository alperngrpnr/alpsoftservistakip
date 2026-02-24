using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using alpsoftservistakip.ViewModels;

namespace alpsoftservistakip
{
    public partial class Page3 : Page
    {
        public Page3ViewModel ViewModel { get; private set; }

        public Page3()
        {
            InitializeComponent();
            ViewModel = new Page3ViewModel();
            DataContext = ViewModel;
        }

        private void IsciSil_Click(object sender, RoutedEventArgs e)
        {
            var email = (sender as Button)?.Tag?.ToString();
            if (!string.IsNullOrEmpty(email))
            {
                ViewModel?.IsciSilCommand.Execute(email);
            }
        }
    }
}