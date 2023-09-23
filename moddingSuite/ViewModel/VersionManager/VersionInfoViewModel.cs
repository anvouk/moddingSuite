using System;
using System.Collections.ObjectModel;
using System.IO;
using moddingSuite.ViewModel.Base;

namespace moddingSuite.ViewModel.VersionManager;

public class VersionInfoViewModel : ViewModelBase
{
    private string _directory;
    private DirectoryInfo _directoryInfo;
    private VersionManagerViewModel _manager;
    private int _version;

    public VersionInfoViewModel(string path, VersionManagerViewModel manager)
        : this(new DirectoryInfo(path), manager)
    {
    }

    public VersionInfoViewModel(DirectoryInfo directoryInfo, VersionManagerViewModel manager)
    {
        Manager = manager;

        DirectoryInfo = directoryInfo;
        DirectoryPath = directoryInfo.FullName;
        Version = Convert.ToInt32(directoryInfo.Name);

        foreach (FileInfo file in directoryInfo.EnumerateFiles())
            VersionFiles.Add(new VersionFileViewModel(file));
    }

    public string DirectoryPath
    {
        get => _directory;
        set
        {
            _directory = value;
            OnPropertyChanged(() => DirectoryPath);
        }
    }

    public int Version
    {
        get => _version;
        set
        {
            _version = value;
            OnPropertyChanged(() => Version);
        }
    }

    public DirectoryInfo DirectoryInfo
    {
        get => _directoryInfo;
        set
        {
            _directoryInfo = value;
            OnPropertyChanged(() => DirectoryInfo);
        }
    }

    public ObservableCollection<VersionFileViewModel> VersionFiles { get; } = new();

    public VersionManagerViewModel Manager
    {
        get => _manager;
        set
        {
            _manager = value;
            OnPropertyChanged(() => Manager);
        }
    }
}
