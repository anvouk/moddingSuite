using System.Globalization;
using moddingSuite.ViewModel.Base;

namespace moddingSuite.Model.Edata;

public abstract class EdataEntity : ViewModelBase
{
    private int _fileEntrySize;
    private string _name;

    public EdataEntity(string name)
    {
        Name = name;
    }


    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged();
        }
    }

    public int FileEntrySize
    {
        get => _fileEntrySize;
        set
        {
            _fileEntrySize = value;
            OnPropertyChanged();
        }
    }

    public override string ToString()
    {
        return Name.ToString(CultureInfo.CurrentCulture);
    }
}
