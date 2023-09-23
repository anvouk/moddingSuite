﻿namespace moddingSuite.Model.Ndfbin.ChangeManager;

public class ObjectReferenceChangeEntry : ChangeEntryBase
{
    private uint _classId;
    private uint _instanceId;

    public ObjectReferenceChangeEntry(NdfPropertyValue affectedPropertyValue, uint classId, uint instanceId)
        : base(affectedPropertyValue)
    {
        ClassId = classId;
        InstanceId = instanceId;
    }

    public uint ClassId
    {
        get => _classId;
        set
        {
            _classId = value;
            OnPropertyChanged();
        }
    }

    public uint InstanceId
    {
        get => _instanceId;
        set
        {
            _instanceId = value;
            OnPropertyChanged();
        }
    }
}
