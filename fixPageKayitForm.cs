using System;
using System.IO;

class Program
{
    static void Main()
    {
        string path = @"C:\Users\alper\source\repos\alpsoftservistakip\alpsoftservistakip\PageKayitOlustur.xaml";
        string xaml = @"<Page x:Class=""alpsoftservistakip.PageKayitOlustur""
      xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
      xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
      xmlns:mc=""http://schemas.openxmlformats.org/markup-compatibility/2006"" 
      xmlns:d=""http://schemas.microsoft.com/expression/blend/2008"" 
      xmlns:local=""clr-namespace:alpsoftservistakip""
      mc:Ignorable=""d"" 
      d:DesignHeight=""900"" d:DesignWidth=""1200""
      Background=""#F4F7F9"">

    <Page.Resources>
        <SolidColorBrush x:Key=""PrimaryColor"" Color=""#3498DB""/>
        <SolidColorBrush x:Key=""SecondaryDark"" Color=""#2C3E50""/>
        <SolidColorBrush x:Key=""BorderColor"" Color=""#DCDDE1""/>

        <Style TargetType=""GroupBox"">
            <Setter Property=""BorderBrush"" Value=""#E1E8ED""/>
            <Setter Property=""BorderThickness"" Value=""1""/>
            <Setter Property=""Margin"" Value=""0,0,0,20""/>
            <Setter Property=""Padding"" Value=""20""/>
            <Setter Property=""Background"" Value=""White""/>
            <Setter Property=""Template"">
                <Setter.Value>
                    <ControlTemplate TargetType=""GroupBox"">
                        <Grid>
                            <Border Background=""{TemplateBinding Background}"" BorderBrush=""{TemplateBinding BorderBrush}"" 
                                    BorderThickness=""{TemplateBinding BorderThickness}"" CornerRadius=""8"" Margin=""0,15,0,0"">
                                <Border.Effect>
                                    <DropShadowEffect Color=""Black"" BlurRadius=""15"" ShadowDepth=""2"" Opacity=""0.05""/>
                                </Border.Effect>
                                <ContentPresenter Margin=""{TemplateBinding Padding}""/>
                            </Border>
                            <Border Background=""{StaticResource SecondaryDark}"" CornerRadius=""5"" HorizontalAlignment=""Left"" VerticalAlignment=""Top"" Margin=""20,0,0,0"" Padding=""15,5"">
                                <ContentPresenter ContentSource=""Header"" TextBlock.Foreground=""White"" TextBlock.FontWeight=""Bold""/>
                            </Border>
                        </Grid>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
        </Style>

        <Style TargetType=""TextBox"">
            <Setter Property=""Background"" Value=""White""/>
            <Setter Property=""BorderThickness"" Value=""1""/>
            <Setter Property=""BorderBrush"" Value=""{StaticResource BorderColor}""/>
            <Setter Property=""Padding"" Value=""10""/>
            <Setter Property=""FontSize"" Value=""14""/>
            <Setter Property=""Foreground"" Value=""#2C3E50""/>
            <Setter Property=""Height"" Value=""40""/>
            <Setter Property=""VerticalContentAlignment"" Value=""Center""/>
            <Setter Property=""Template"">
                <Setter.Value>
                    <ControlTemplate TargetType=""TextBox"">
                        <Border x:Name=""border"" Background=""{TemplateBinding Background}"" BorderBrush=""{TemplateBinding BorderBrush}"" BorderThickness=""{TemplateBinding BorderThickness}"" CornerRadius=""5"">
                            <ScrollViewer x:Name=""PART_ContentHost"" Focusable=""false"" HorizontalScrollBarVisibility=""Hidden"" VerticalScrollBarVisibility=""Hidden""/>
                        </Border>
                        <ControlTemplate.Triggers>
                            <Trigger Property=""IsFocused"" Value=""true"">
                                <Setter TargetName=""border"" Property=""BorderBrush"" Value=""{StaticResource PrimaryColor}""/>
                            </Trigger>
                        </ControlTemplate.Triggers>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
        </Style>

        <Style TargetType=""ComboBox"">
            <Setter Property=""Height"" Value=""40""/>
            <Setter Property=""FontSize"" Value=""14""/>
            <Setter Property=""VerticalContentAlignment"" Value=""Center""/>
            <Setter Property=""Padding"" Value=""10,0""/>
        </Style>

        <Style TargetType=""CheckBox"">
            <Setter Property=""Margin"" Value=""0,0,15,10""/>
            <Setter Property=""FontSize"" Value=""14""/>
            <Setter Property=""Foreground"" Value=""#2C3E50""/>
        </Style>

        <Style TargetType=""TextBlock"">
            <Setter Property=""Foreground"" Value=""#7F8C8D""/>
            <Setter Property=""FontWeight"" Value=""SemiBold""/>
            <Setter Property=""Margin"" Value=""0,0,0,5""/>
        </Style>
    </Page.Resources>

    <Grid Margin=""40"">
        <Grid.RowDefinitions>
            <RowDefinition Height=""Auto""/>
            <RowDefinition Height=""*""/>
        </Grid.RowDefinitions>

        <!-- Header & Action Buttons -->
        <Grid Grid.Row=""0"" Margin=""0,0,0,30"">
            <StackPanel Orientation=""Horizontal"">
                <Border Width=""5"" Height=""40"" Background=""#E74C3C"" Margin=""0,0,15,0""/>
                <TextBlock Text=""Servis Kaydı Oluştur / Düzenle"" FontSize=""32"" FontWeight=""ExtraBold"" Foreground=""{StaticResource SecondaryDark}"" VerticalAlignment=""Center""/>
            </StackPanel>

            <StackPanel Orientation=""Horizontal"" HorizontalAlignment=""Right"">
                <Button Content=""GERİ DÖN"" Background=""#7F8C8D"" Foreground=""White"" FontWeight=""Bold"" FontSize=""15"" Padding=""25,10"" CornerRadius=""20"" BorderThickness=""0"" Margin=""0,0,15,0"" Cursor=""Hand"" Command=""{Binding GeriDonCommand}""/>
                <Button Content=""KAYDET"" Background=""#E74C3C"" Foreground=""White"" FontWeight=""Bold"" FontSize=""15"" Padding=""30,10"" CornerRadius=""20"" BorderThickness=""0"" Cursor=""Hand"" Command=""{Binding KaydetCommand}""/>
            </StackPanel>
        </Grid>

        <!-- Form Scroll Area -->
        <ScrollViewer Grid.Row=""1"" VerticalScrollBarVisibility=""Auto"" HorizontalScrollBarVisibility=""Disabled"" Padding=""0,0,20,0"">
            <Grid>
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width=""1*""/>
                    <ColumnDefinition Width=""30""/>
                    <ColumnDefinition Width=""1*""/>
                </Grid.ColumnDefinitions>

                <!-- LEFT COLUMN -->
                <StackPanel Grid.Column=""0"">
                    <!-- Müşteri Bilgileri -->
                    <GroupBox Header=""MÜŞTERİ BİLGİLERİ"">
                        <StackPanel>
                            <TextBlock Text=""Ad Soyad *""/>
                            <TextBox x:Name=""adsoyad_skayit"" Text=""{Binding AdSoyad, UpdateSourceTrigger=PropertyChanged}"" Margin=""0,0,0,15""/>

                            <TextBlock Text=""Bayi Adı""/>
                            <TextBox x:Name=""bayiadi_skayit"" Text=""{Binding BayiAdi, UpdateSourceTrigger=PropertyChanged}"" Margin=""0,0,0,15""/>

                            <TextBlock Text=""Cep Telefonu *""/>
                            <TextBox x:Name=""ceptelefonu_skayit"" Text=""{Binding CepTelefonu, UpdateSourceTrigger=PropertyChanged}"" Margin=""0,0,0,15""/>

                            <TextBlock Text=""E-Posta""/>
                            <TextBox x:Name=""eposta_skayit"" Text=""{Binding EPosta, UpdateSourceTrigger=PropertyChanged}"" Margin=""0,0,0,15""/>

                            <TextBlock Text=""Açık Adres""/>
                            <TextBox x:Name=""adres_skayit"" Text=""{Binding Adres, UpdateSourceTrigger=PropertyChanged}"" Height=""80"" TextWrapping=""Wrap"" AcceptsReturn=""True""/>
                        </StackPanel>
                    </GroupBox>

                    <!-- Genel Durum & Garanti -->
                    <GroupBox Header=""DURUM VE GARANTİ KONTROLLERİ"">
                        <Grid>
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width=""1*""/>
                                <ColumnDefinition Width=""1*""/>
                            </Grid.ColumnDefinitions>
                            <StackPanel Grid.Column=""0"">
                                <CheckBox x:Name=""yeni"" IsChecked=""{Binding Yeni}"" Content=""Yeni""/>
                                <CheckBox x:Name=""eski"" IsChecked=""{Binding Eski}"" Content=""Eski""/>
                                <CheckBox x:Name=""tamirgörmüs"" IsChecked=""{Binding TamirGormus}"" Content=""Tamir Görmüş""/>
                                <CheckBox x:Name=""yedeklemeyapilsin"" IsChecked=""{Binding YedeklemeYapilsin}"" Content=""Yedekleme Yapılsın""/>
                            </StackPanel>
                            <StackPanel Grid.Column=""1"">
                                <CheckBox x:Name=""garantili"" IsChecked=""{Binding Garantili}"" Content=""Garantili""/>
                                <CheckBox x:Name=""garantisiz"" IsChecked=""{Binding Garantisiz}"" Content=""Garantisiz""/>
                                <CheckBox x:Name=""servisgarantili"" IsChecked=""{Binding ServisGarantili}"" Content=""Servis Garantili""/>
                            </StackPanel>
                        </Grid>
                    </GroupBox>
                </StackPanel>

                <!-- RIGHT COLUMN -->
                <StackPanel Grid.Column=""2"">
                    <!-- Cihaz Bilgileri -->
                    <GroupBox Header=""CİHAZ BİLGİLERİ &amp; DURUM"">
                        <StackPanel>
                            <TextBlock Text=""Cihaz Türü *""/>
                            <TextBox x:Name=""cihaztürü_skayit"" Text=""{Binding CihazTuru, UpdateSourceTrigger=PropertyChanged}"" Margin=""0,0,0,15""/>

                            <Grid Margin=""0,0,0,15"">
                                <Grid.ColumnDefinitions>
                                    <ColumnDefinition Width=""1*""/>
                                    <ColumnDefinition Width=""10""/>
                                    <ColumnDefinition Width=""1*""/>
                                </Grid.ColumnDefinitions>
                                <StackPanel Grid.Column=""0"">
                                    <TextBlock Text=""Marka *""/>
                                    <TextBox x:Name=""marka_skayit"" Text=""{Binding Marka, UpdateSourceTrigger=PropertyChanged}""/>
                                </StackPanel>
                                <StackPanel Grid.Column=""2"">
                                    <TextBlock Text=""Model *""/>
                                    <TextBox x:Name=""model_skayit"" Text=""{Binding Model, UpdateSourceTrigger=PropertyChanged}""/>
                                </StackPanel>
                            </Grid>

                            <TextBlock Text=""IMEI / Seri No""/>
                            <Grid Margin=""0,0,0,15"">
                                <Grid.ColumnDefinitions>
                                    <ColumnDefinition Width=""*""/>
                                    <ColumnDefinition Width=""10""/>
                                    <ColumnDefinition Width=""Auto""/>
                                </Grid.ColumnDefinitions>
                                <TextBox x:Name=""imeino_skayit"" Text=""{Binding ImeiNo, UpdateSourceTrigger=PropertyChanged}"" Grid.Column=""0""/>
                                <Button Content=""BTK SORGULA"" Grid.Column=""2"" Command=""{Binding ImeiSorgulaCommand}"" Padding=""15,0"" Background=""#34495E"" Foreground=""White"" BorderThickness=""0"" CornerRadius=""5"" Cursor=""Hand""/>
                            </Grid>
                        </StackPanel>
                    </GroupBox>

                    <!-- Servis Notları -->
                    <GroupBox Header=""ARIZA VE SERVİS BİLGİLERİ"">
                        <StackPanel>
                            <TextBlock Text=""Şikayet / Arıza Açıklaması *""/>
                            <TextBox x:Name=""sikayetiariza_skayit"" Text=""{Binding SikayetAriza, UpdateSourceTrigger=PropertyChanged}"" Height=""70"" TextWrapping=""Wrap"" AcceptsReturn=""True"" Margin=""0,0,0,15""/>

                            <TextBlock Text=""Ek Bilgiler (Parola, Aksesuar vs.)""/>
                            <TextBox x:Name=""ekbilgiler_skayit"" Text=""{Binding EkBilgiler, UpdateSourceTrigger=PropertyChanged}"" Height=""70"" TextWrapping=""Wrap"" AcceptsReturn=""True"" Margin=""0,0,0,15""/>

                            <Grid>
                                <Grid.ColumnDefinitions>
                                    <ColumnDefinition Width=""1*""/>
                                    <ColumnDefinition Width=""10""/>
                                    <ColumnDefinition Width=""1*""/>
                                </Grid.ColumnDefinitions>
                                <StackPanel Grid.Column=""0"">
                                    <TextBlock Text=""İlgili Teknisyen""/>
                                    <TextBox x:Name=""ilgiliteknisyen1"" Text=""{Binding IlgiliTeknisyen, UpdateSourceTrigger=PropertyChanged}""/>
                                </StackPanel>
                                <StackPanel Grid.Column=""2"">
                                    <TextBlock Text=""Fiyat / Ücret Bilgisi (₺)""/>
                                    <TextBox x:Name=""fiyatbilgisi_skayit1"" Text=""{Binding FiyatBilgisi, UpdateSourceTrigger=PropertyChanged}""/>
                                </StackPanel>
                            </Grid>
                        </StackPanel>
                    </GroupBox>
                </StackPanel>
            </Grid>
        </ScrollViewer>
    </Grid>
</Page>";

        File.WriteAllText(path, xaml);
    }
}
