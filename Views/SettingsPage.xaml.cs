using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DroidTidal.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        this.InitializeComponent();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        ApiUrlBox.Text = App.SettingsService.ApiBaseUrl;

        var quality = App.SettingsService.AudioQuality;
        for (int i = 0; i < QualityCombo.Items.Count; i++)
        {
            if (QualityCombo.Items[i] is ComboBoxItem item && item.Tag as string == quality)
            {
                QualityCombo.SelectedIndex = i;
                break;
            }
        }
    }

    private void ApiUrl_TextChanged(object sender, TextChangedEventArgs e)
    {
        var url = ApiUrlBox.Text.Trim();
        if (!string.IsNullOrEmpty(url))
            App.SettingsService.ApiBaseUrl = url;
    }

    private void Quality_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (QualityCombo.SelectedItem is ComboBoxItem item && item.Tag is string quality)
            App.SettingsService.AudioQuality = quality;
    }
}
