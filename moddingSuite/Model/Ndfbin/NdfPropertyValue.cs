using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using moddingSuite.Model.Ndfbin.ChangeManager;
using moddingSuite.Model.Ndfbin.Types;
using moddingSuite.Model.Ndfbin.Types.AllTypes;
using moddingSuite.Util;
using moddingSuite.View.DialogProvider;
using moddingSuite.ViewModel.Base;
using moddingSuite.ViewModel.Ndf;

namespace moddingSuite.Model.Ndfbin;

public class NdfPropertyValue : ViewModelBase, IValueHolder, IEditableObject
{
    private bool _dirty;
    private NdfObject _instance;


    private byte[] _oldVal;
    private NdfProperty _property;
    private NdfValueWrapper _value;

    public NdfPropertyValue(NdfObject instance)
    {
        _instance = instance;

        DetailsCommand = new ActionCommand(DetailsCommandExecute);
    }

    public NdfType Type
    {
        get
        {
            if (Value == null)
                return NdfType.Unset;

            return Value.Type;
        }
    }

    public NdfProperty Property
    {
        get => _property;
        set
        {
            _property = value;
            OnPropertyChanged();
        }
    }

    public NdfObject Instance
    {
        get => _instance;
        set
        {
            _instance = value;
            OnPropertyChanged();
        }
    }

    public ICommand DetailsCommand { get; set; }

    public void BeginEdit()
    {
        if (_dirty)
            return;

        _oldVal = Value.GetBytes();

        _dirty = true;
    }

    public void CancelEdit()
    {
        _dirty = false;
    }

    public void EndEdit()
    {
        if (!_dirty)
            return;

        byte[] newVal = Value.GetBytes();

        if (newVal != null && _oldVal != null && Utils.ByteArrayCompare(newVal, _oldVal))
            return;

        ChangeEntryBase change = null;

        switch (Value.Type)
        {
            case NdfType.Map:
                NdfMap map = Value as NdfMap;
                change = new MapChangeEntry(this, map.Key, map.Value as MapValueHolder);

                break;

            case NdfType.ObjectReference:
                NdfObjectReference refe = Value as NdfObjectReference;
                change = new ObjectReferenceChangeEntry(this, refe.Class.Id, refe.InstanceId);

                break;

            default:
                change = new FlatChangeEntry(this, Value);

                break;
        }

        Manager.ChangeManager.AddChange(change);

        _dirty = false;
    }

    public void DetailsCommandExecute(object obj)
    {
        IEnumerable<DataGridCellInfo> item = obj as IEnumerable<DataGridCellInfo>;

        if (item == null)
            return;

        IValueHolder prop = item.First().Item as IValueHolder;

        FollowDetails(prop);
    }

    private void FollowDetails(IValueHolder prop)
    {
        if (prop == null || prop.Value == null)
            return;

        switch (prop.Value.Type)
        {
            case NdfType.MapList:
            case NdfType.List:
                FollowList(prop);
                break;
            case NdfType.ObjectReference:
                FollowObjectReference(prop);
                break;
            case NdfType.Map:
                NdfMap map = prop.Value as NdfMap;

                if (map != null)
                {
                    FollowDetails(map.Key);
                    FollowDetails(map.Value as IValueHolder);
                }

                break;
            default:
                return;
        }
    }

    private void FollowObjectReference(IValueHolder prop)
    {
        NdfObjectReference refe = prop.Value as NdfObjectReference;

        if (refe == null)
            return;

        NdfClassViewModel vm = new NdfClassViewModel(refe.Class, null);

        NdfObjectViewModel inst = vm.Instances.SingleOrDefault(x => x.Id == refe.InstanceId);

        if (inst == null)
            return;

        vm.InstancesCollectionView.MoveCurrentTo(inst);

        DialogProvider.ProvideView(vm);
    }

    private void FollowList(IValueHolder prop)
    {
        NdfCollection refe = prop.Value as NdfCollection;

        if (refe == null)
            return;


        ListEditorViewModel editor = new ListEditorViewModel(refe, Manager);
        DialogProvider.ProvideView(editor);
    }

    #region IValueHolder Members

    public NdfValueWrapper Value
    {
        get => _value;
        set
        {
            _value = value;
            _value.ParentProperty = this;
            OnPropertyChanged();
        }
    }

    public NdfBinary Manager => Property.Class.Manager;

    #endregion
}
