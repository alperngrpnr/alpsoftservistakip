using System.Windows.Controls;

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;

namespace alpsoftservistakip
{
    public partial class PageKayitOlustur : Page
    {
        private bool _isFormatting = false;

        public PageKayitOlustur()
        {
            InitializeComponent();
            DataContext = new ViewModels.PageKayitOlusturViewModel();
        }

        private void SayiSadece_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Sadece rakamlara izin ver
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void Telefon_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isFormatting) return;

            if (sender is TextBox textBox)
            {
                _isFormatting = true;

                // Sadece rakamları al
                string raw = new string(textBox.Text.Where(char.IsDigit).ToArray());

                if (raw.Length > 11) raw = raw.Substring(0, 11);

                // Telefon numarası 0 ile başlıyorsa formata dök
                string formatted = raw;
                if (raw.Length > 0 && raw.StartsWith("0"))
                {
                    if (raw.Length > 4) formatted = $"({raw.Substring(0, 4)}) {raw.Substring(4)}";
                    if (raw.Length > 7) formatted = $"({raw.Substring(0, 4)}) {raw.Substring(4, 3)} {raw.Substring(7)}";
                    if (raw.Length > 9) formatted = $"({raw.Substring(0, 4)}) {raw.Substring(4, 3)} {raw.Substring(7, 2)} {raw.Substring(9)}";
                }
                else if (raw.Length > 0 && !raw.StartsWith("0"))
                {
                    // 0 ile başlamıyorsa, başına 0 ekle formatlamaya yardım et
                    raw = "0" + raw;
                    if (raw.Length > 4) formatted = $"({raw.Substring(0, 4)}) {raw.Substring(4)}";
                    if (raw.Length > 7) formatted = $"({raw.Substring(0, 4)}) {raw.Substring(4, 3)} {raw.Substring(7)}";
                    if (raw.Length > 9) formatted = $"({raw.Substring(0, 4)}) {raw.Substring(4, 3)} {raw.Substring(7, 2)} {raw.Substring(9)}";
                }

                // İmleç pozisyonunu korumak içim
                int caretIndex = textBox.CaretIndex;
                int addedChars = formatted.Length - textBox.Text.Length;

                textBox.Text = formatted;

                textBox.CaretIndex = Math.Max(0, caretIndex + addedChars);
                _isFormatting = false;
            }
        }
    }
}
