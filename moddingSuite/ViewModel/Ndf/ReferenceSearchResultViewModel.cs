﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using moddingSuite.Model.Ndfbin;
using moddingSuite.View.DialogProvider;
using moddingSuite.ViewModel.Base;

namespace moddingSuite.ViewModel.Ndf;

public class ReferenceSearchResultViewModel : ViewModelBase
{
    public ReferenceSearchResultViewModel(List<NdfPropertyValue> results, NdfEditorMainViewModel editor)
    {
        Results = new ObservableCollection<NdfPropertyValue>(results);

        Editor = editor;

        DetailsCommand = new ActionCommand(DetailsExecute);
    }

    public ObservableCollection<NdfPropertyValue> Results { get; set; }

    public ICommand DetailsCommand { get; set; }

    public NdfEditorMainViewModel Editor { get; set; }

    private void DetailsExecute(object obj)
    {
        NdfPropertyValue propVal = obj as NdfPropertyValue;

        if (propVal == null)
            return;

        NdfClassViewModel vm = new(propVal.Instance.Class, this);

        NdfObjectViewModel inst = vm.Instances.SingleOrDefault(x => x.Id == propVal.Instance.Id);

        if (inst == null)
            return;

        vm.InstancesCollectionView.MoveCurrentTo(inst);

        DialogProvider.ProvideView(vm, Editor);
    }
}
