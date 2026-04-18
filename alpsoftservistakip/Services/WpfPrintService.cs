using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.IO;
using System.Xml;

namespace alpsoftservistakip.Services
{
    public static class WpfPrintService
    {
        public static void ShowPrintPreview(FrameworkElement printContent, string documentName, string previewTitle = "FİŞ ÖNİZLEMESİ")
        {
            if (printContent == null) return;

            var fixedDocument = CreateFixedDocument(printContent);

            Window previewWindow = new Window
            {
                Title = previewTitle,
                Width = 900,
                Height = 800,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Background = new SolidColorBrush(Color.FromRgb(240, 244, 248)),
                ShowInTaskbar = false
            };

            Grid mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            Button btnYazdir = new Button
            {
                Content = "🖨 YAZICIYI SEÇ VE ÇIKART",
                Height = 60,
                Margin = new Thickness(20),
                Background = new SolidColorBrush(Color.FromRgb(46, 204, 113)),
                Foreground = Brushes.White,
                FontSize = 22,
                FontWeight = FontWeights.ExtraBold,
                Cursor = System.Windows.Input.Cursors.Hand
            };
            Grid.SetRow(btnYazdir, 0);
            mainGrid.Children.Add(btnYazdir);

            DocumentViewer documentViewer = new DocumentViewer
            {
                Document = fixedDocument,
                Margin = new Thickness(20)
            };

            Grid.SetRow(documentViewer, 1);
            mainGrid.Children.Add(documentViewer);

            previewWindow.Content = mainGrid;

            btnYazdir.Click += (s, ev) =>
            {
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    try
                    {
                        fixedDocument.DocumentPaginator.PageSize = new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);
                        printDialog.PrintDocument(fixedDocument.DocumentPaginator, documentName);
                        MessageBox.Show("Yazdırma işlemi başarıyla gönderildi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                        previewWindow.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Yazdırma sırasında hata oluştu: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            };

            previewWindow.ShowDialog();
        }

        private static FixedDocument CreateFixedDocument(FrameworkElement source)
        {
            const double pageWidth = 793.0;   
            const double pageHeight = 1122.0; 

            FrameworkElement content = CloneElement(source) ?? source;

            content.Measure(new Size(pageWidth, double.PositiveInfinity));
            content.Arrange(new Rect(new Point(0, 0), new Size(pageWidth, content.DesiredSize.Height)));
            content.UpdateLayout();

            FixedPage fixedPage = new FixedPage
            {
                Width = pageWidth,
                Height = Math.Max(pageHeight, content.DesiredSize.Height + 40)
            };
            fixedPage.Children.Add(content);

            PageContent pageContent = new PageContent();
            ((IAddChild)pageContent).AddChild(fixedPage);

            FixedDocument doc = new FixedDocument();
            doc.Pages.Add(pageContent);
            return doc;
        }

        private static FrameworkElement CloneElement(FrameworkElement source)
        {
            try
            {
                string xaml = XamlWriter.Save(source);
                using (var stringReader = new StringReader(xaml))
                using (var xmlReader = XmlReader.Create(stringReader))
                {
                    return XamlReader.Load(xmlReader) as FrameworkElement;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}


