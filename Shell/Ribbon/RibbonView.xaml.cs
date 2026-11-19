namespace AumoDekstop.Shell.Ribbon;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

/// <summary>Ribbon utama: baris tab + panel tombol per tab.</summary>
public sealed partial class RibbonView : UserControl
{
    private readonly bool _isReady;

    /// <summary>Dipicu saat tombol ribbon diklik; nilainya id dari RibbonCommand.</summary>
    public event EventHandler<string>? CommandInvoked;

    /// <summary>Dipicu setelah tab berganti dan panel tombol sudah disesuaikan.</summary>
    public event EventHandler<RibbonTabChangedEventArgs>? TabChanged;

    public RibbonView()
    {
        InitializeComponent();
        _isReady = true;
    }

    /// <summary>Memilih tab Beranda dan menampilkan panel tombolnya.</summary>
    public void SelectHome()
    {
        RibbonTabs.SelectedItem = HomeTab;
        ShowPanel("home");
    }

    private void ShowPanel(string? tag)
    {
        // RibbonCoa sengaja tidak diatur di sini: perilaku asli dipertahankan.
        RibbonHome.Visibility = tag == "home" ? Visibility.Visible : Visibility.Collapsed;
        RibbonPeriods.Visibility = tag == "periods" ? Visibility.Visible : Visibility.Collapsed;
        RibbonData.Visibility = tag == "data" ? Visibility.Visible : Visibility.Collapsed;
        RibbonReport.Visibility = tag == "report" ? Visibility.Visible : Visibility.Collapsed;
        RibbonSettings.Visibility = tag == "settings" ? Visibility.Visible : Visibility.Collapsed;
    }

    private void RibbonTabs_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
    {
        // Event ini dapat terpicu saat InitializeComponent, sebelum panel terisi
        if (!_isReady) return;
        if (sender.SelectedItem is not SelectorBarItem selected) return;

        string? tag = selected.Tag?.ToString();
        ShowPanel(tag);
        TabChanged?.Invoke(this, new RibbonTabChangedEventArgs(tag, selected.Text));
    }

    private void OnLogoutClick(object sender, RoutedEventArgs e)
    {
        CommandInvoked?.Invoke(this, RibbonCommand.Logout);
    }

    private void OnPanelCommand(object? sender, string command)
    {
        CommandInvoked?.Invoke(this, command);
    }
}
