using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AumoDekstop;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();
    }

    private void RibbonTabs_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
    {
        if (sender.SelectedItem is SelectorBarItem selected)
        {
            StatusText.Text = $"● Tab aktif: {selected.Text} | Server: https://aumo.onrender.com";
            // Nantinya di sini kita arahkan RootFrame.Navigate(typeof(...)) sesuai tab
        }
    }

    private void OnBaruClick(object sender, RoutedEventArgs e)
    {
        StatusText.Text = "● Membuat entri baru...";
    }

    private void OnSimpanClick(object sender, RoutedEventArgs e)
    {
        StatusText.Text = "● Mengirim data ke server...";
    }

    private void OnSyncClick(object sender, RoutedEventArgs e)
    {
        StatusText.Text = "● Menyinkronkan dengan https://aumo.onrender.com...";
    }
}
