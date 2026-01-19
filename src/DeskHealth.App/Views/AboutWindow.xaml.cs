using System.Windows;

namespace DeskHealth.App.Views;

/// <summary>
/// 关于窗口
/// </summary>
public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
    }

    private void OnOkClicked(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
