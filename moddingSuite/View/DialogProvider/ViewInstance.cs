using System;
using System.Windows;
using moddingSuite.ViewModel.Base;

namespace moddingSuite.View.DialogProvider;

public class ViewInstance
{
    public ViewInstance(Window view, ViewModelBase vm)
    {
        if (view == null)
            throw new ArgumentException("view");

        if (vm == null)
            throw new ArgumentException("vm");

        View = view;
        ViewModel = vm;
    }

    public Window View { get; protected set; }
    public ViewModelBase ViewModel { get; protected set; }
}
