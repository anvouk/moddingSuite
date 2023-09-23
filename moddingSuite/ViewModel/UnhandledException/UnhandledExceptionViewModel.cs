using System;
using System.Text;
using System.Windows.Input;
using moddingSuite.ViewModel.Base;

namespace moddingSuite.ViewModel.UnhandledException;

public class UnhandledExceptionViewModel : ViewModelBase
{
    private string _errorText;
    private string _title;

    public UnhandledExceptionViewModel(Exception exception)
    {
        Title = "An unhandled exception occured";

        SendErrorCommand = new ActionCommand(SendErrorExecute);

        StringBuilder sb = new StringBuilder();

        Exception excep = exception;

        while (excep != null)
        {
            sb.Append(exception);
            excep = excep.InnerException;
        }

        ErrorText = sb.ToString();
    }

    public string Title
    {
        get => _title;
        set
        {
            _title = value;
            OnPropertyChanged();
        }
    }

    public string ErrorText
    {
        get => _errorText;
        set
        {
            _errorText = value;
            OnPropertyChanged();
        }
    }

    public ICommand SendErrorCommand { get; set; }

    private void SendErrorExecute(object obj)
    {
        throw new NotImplementedException();
    }
}
