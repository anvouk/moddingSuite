using System.Globalization;
using moddingSuite.ViewModel.Base;

namespace moddingSuite.Model.Ndfbin;

public class NdfProperty : ViewModelBase
{
    private NdfClass _class;
    private int _id;
    private string _name;

    public NdfProperty(int id)
    {
        Id = id;
    }

    public int Id
    {
        get => _id;
        set
        {
            _id = value;
            OnPropertyChanged(() => Id);
        }
    }

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged(() => Name);
        }
    }

    public NdfClass Class
    {
        get => _class;
        set
        {
            _class = value;
            OnPropertyChanged(() => Class);
        }
    }

    public override string ToString()
    {
        return Name.ToString(CultureInfo.InvariantCulture);
    }
}
