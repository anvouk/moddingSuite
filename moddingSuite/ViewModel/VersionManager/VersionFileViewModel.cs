using System.IO;
using moddingSuite.ViewModel.Base;

namespace moddingSuite.ViewModel.VersionManager;

public class VersionFileViewModel : ViewModelBase
{
    private FileInfo _fileInfo;
    private string _path;

    public VersionFileViewModel(FileInfo i)
    {
        FileInfo = i;
        Path = i.FullName;
    }

    public string Path
    {
        get => _path;
        set
        {
            _path = value;
            OnPropertyChanged(() => Path);
        }
    }

    public FileInfo FileInfo
    {
        get => _fileInfo;
        set
        {
            _fileInfo = value;
            OnPropertyChanged(() => FileInfo);
        }
    }
}
