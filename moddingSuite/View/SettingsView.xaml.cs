using System.Windows;
using System.Windows.Forms;
using moddingSuite.Model.Settings;

namespace moddingSuite.View;

/// <summary>
///     Interaktionslogik für SettingsView.xaml
/// </summary>
public partial class SettingsView : Window
{
    public SettingsView()
    {
        InitializeComponent();
    }

    private void SaveButtonClick(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void CanceButtonClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void WorkSpaceBrowserButtonClick(object sender, RoutedEventArgs e)
    {
        Settings settings = DataContext as Settings;

        if (settings == null)
            return;

        FolderBrowserDialog folderDlg = new()
        {
            SelectedPath = settings.SavePath,
            //RootFolder = Environment.SpecialFolder.MyComputer,
            ShowNewFolderButton = true
        };

        if (folderDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            settings.SavePath = folderDlg.SelectedPath;
    }

    private void GameSpaceButtonClick(object sender, RoutedEventArgs e)
    {
        Settings settings = DataContext as Settings;

        if (settings == null)
            return;

        FolderBrowserDialog folderDlg = new()
        {
            SelectedPath = settings.WargamePath,
            //RootFolder = Environment.SpecialFolder.MyComputer,
            ShowNewFolderButton = true
        };

        if (folderDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            settings.WargamePath = folderDlg.SelectedPath;
    }
}
