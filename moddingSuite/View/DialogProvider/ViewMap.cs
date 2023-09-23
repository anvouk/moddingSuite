using System;
using System.Windows;
using moddingSuite.ViewModel.Base;

namespace moddingSuite.View.DialogProvider;

public class ViewMap<TView, TViewModel> : IViewMap
    where TView : Window
    where TViewModel : ViewModelBase
{
    public ViewMap()
    {
        ViewType = typeof(TView);
        ViewModelType = typeof(TViewModel);
    }

    public Type ViewType { get; protected set; }
    public Type ViewModelType { get; protected set; }
}
