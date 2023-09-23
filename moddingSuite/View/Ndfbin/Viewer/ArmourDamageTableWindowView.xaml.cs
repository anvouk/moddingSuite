using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using moddingSuite.ViewModel.Ndf;

namespace moddingSuite.View.Ndfbin.Viewer;

/// <summary>
///     Interaction logic for ArmourDamageTableWindow.xaml
/// </summary>
public partial class ArmourDamageTableWindowView : Window
{
    public ArmourDamageTableWindowView()
    {
        InitializeComponent();
    }

    private void DataGrid_LoadingRow(object w, DataGridRowEventArgs e)
    {
        ObservableCollection<string> headers = ((ArmourDamageViewModel)DataContext).RowHeaders;
        if (e.Row.GetIndex() < headers.Count) e.Row.Header = headers[e.Row.GetIndex()];
    }
}
