using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using moddingSuite.ViewModel.Base;

namespace moddingSuite.ViewModel.Edata;

public class DirectoryViewModel : FileSystemItemViewModel
{
    private ObservableCollection<FileSystemItemViewModel> _items = new();

    public DirectoryViewModel(DirectoryInfo info)
    {
        Info = info;
        OpenInFileExplorerCommand = new ActionCommand(OpenInFileExplorerExecute);
    }

    public ObservableCollection<FileSystemItemViewModel> Items
    {
        get => _items;
        set
        {
            _items = value;
            OnPropertyChanged();
        }
    }

    public DirectoryInfo Info { get; set; }

    public ICommand OpenInFileExplorerCommand { get; set; }

    public override string Name => Info.Name;

    private void OpenInFileExplorerExecute(object obj)
    {
        Process.Start(Info.FullName);
    }
}
