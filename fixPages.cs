using System;
using System.IO;

class Program
{
    static void Main()
    {
        string p5 = @"C:\Users\alper\source\repos\alpsoftservistakip\alpsoftservistakip\Page5.xaml";
        string pDis = @"C:\Users\alper\source\repos\alpsoftservistakip\alpsoftservistakip\PageDisServis.xaml";

        string cariStyles = @"
        <Style TargetType=""DataGrid"">
            <Setter Property=""Background"" Value=""White""/>
            <Setter Property=""BorderThickness"" Value=""0""/>
            <Setter Property=""RowHeight"" Value=""55""/>
            <Setter Property=""FontFamily"" Value=""Segoe UI""/>
            <Setter Property=""FontSize"" Value=""14""/>
            <Setter Property=""HeadersVisibility"" Value=""Column""/>
            <Setter Property=""GridLinesVisibility"" Value=""Horizontal""/>
            <Setter Property=""HorizontalGridLinesBrush"" Value=""#EDF2F7""/>
            <Setter Property=""CanUserAddRows"" Value=""False""/>
        </Style>

        <Style TargetType=""DataGridColumnHeader"">
            <Setter Property=""Background"" Value=""White""/>
            <Setter Property=""Foreground"" Value=""#718096""/>
            <Setter Property=""FontFamily"" Value=""Segoe UI""/>
            <Setter Property=""FontWeight"" Value=""SemiBold""/>
            <Setter Property=""FontSize"" Value=""14""/>
            <Setter Property=""Padding"" Value=""15,10""/>
            <Setter Property=""BorderThickness"" Value=""0,0,0,2""/>
            <Setter Property=""BorderBrush"" Value=""#E2E8F0""/>
            <Setter Property=""HorizontalContentAlignment"" Value=""Center""/>
        </Style>

        <Style TargetType=""DataGridRow"">
            <Setter Property=""Background"" Value=""White""/>
            <Style.Triggers>
                <Trigger Property=""IsSelected"" Value=""True"">
                    <Setter Property=""Background"" Value=""#E1F0FA""/>
                    <Setter Property=""Foreground"" Value=""#2C3E50""/>
                </Trigger>
                <Trigger Property=""IsMouseOver"" Value=""True"">
                    <Setter Property=""Background"" Value=""#F8FAFC""/>
                </Trigger>
            </Style.Triggers>
        </Style>

        <Style TargetType=""DataGridCell"">
            <Setter Property=""BorderThickness"" Value=""0""/>
            <Setter Property=""TextBlock.TextAlignment"" Value=""Center""/>
            <Setter Property=""Template"">
                <Setter.Value>
                    <ControlTemplate TargetType=""DataGridCell"">
                        <Border Background=""{TemplateBinding Background}"">
                            <ContentPresenter VerticalAlignment=""Center"" HorizontalAlignment=""Center"" Margin=""5""/>
                        </Border>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
            <Style.Triggers>
                <Trigger Property=""IsSelected"" Value=""True"">
                    <Setter Property=""Background"" Value=""Transparent""/>
                </Trigger>
            </Style.Triggers>
        </Style>
";
        if (File.Exists(p5))
        {
            string t5 = File.ReadAllText(p5);
            int startRes5 = t5.IndexOf(@"<Style TargetType=""{x:Type DataGrid}"" x:Key=""ModernDataGridStyle"">");
            int endRes5 = t5.IndexOf(@"</Page.Resources>");
            if(startRes5 != -1 && endRes5 != -1) {
                t5 = t5.Substring(0, startRes5) + cariStyles + "\r\n    " + t5.Substring(endRes5);
                t5 = t5.Replace(@"Style=""{StaticResource ModernDataGridStyle}""", "");
            }
            File.WriteAllText(p5, t5);
        }

        if (File.Exists(pDis))
        {
            string td = File.ReadAllText(pDis);
            int startResD = td.IndexOf(@"<Style TargetType=""{x:Type DataGridColumnHeader}"">");
            int endResD = td.IndexOf(@"<Style x:Key=""ModernButtonStyle""");
            if(startResD != -1 && endResD != -1) {
                td = td.Substring(0, startResD) + cariStyles + "\r\n        " + td.Substring(endResD);
                td = td.Replace(@"Style=""{StaticResource ModernDataGridStyle}""", "");
                td = td.Replace(@"Style=""{StaticResource DisServisDataGridStyle}""", "");
            }
            File.WriteAllText(pDis, td);
        }
        Console.WriteLine("Done");
    }
}
