﻿using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using moddingSuite.Model.Ndfbin;
using moddingSuite.Model.Ndfbin.Types.AllTypes;
using moddingSuite.ViewModel.Base;

namespace moddingSuite.ViewModel.Ndf;

internal class ArmourDamageViewModel : ObjectWrapperViewModel<NdfObject>
{
    public string Name;

    public ArmourDamageViewModel(NdfObject obj, ViewModelBase parentVm)
        : base(obj, parentVm)
    {
        Name = obj.Class.Name;
        switch (obj.Class.Name)
        {
            case "TGameplayArmeArmureContainer":
                readTGameplayArmeArmureContainer(obj);
                break;
            case "TGameplayDamageResistanceContainer":
                readTGameplayDamageResistanceContainer(obj);
                break;
            default:
                throw new Exception(string.Format("Cannot read object {0} as ArmourDamageViewModel", obj.Class.Name));
        }
    }

    public DataTable TableData { get; } = new();
    public ObservableCollection<string> RowHeaders { get; } = new();

    private void readTGameplayArmeArmureContainer(NdfObject obj)
    {
        NdfCollection maps = obj.PropertyValues[0].Value as NdfCollection;
        for (int i = 0; i < 42; i++) TableData.Columns.Add(i.ToString());
        for (int i = 0; i < 70; i++)
        {
            CollectionItemValueHolder map =
                maps.FirstOrDefault(x => ((NdfMap)x.Value).Key.Value.ToString().Equals(i.ToString()));
            if (map != null)
            {
                NdfCollection collection = ((MapValueHolder)((NdfMap)map.Value).Value).Value as NdfCollection;
                DataRow row = TableData.NewRow();
                TableData.Rows.Add(row);
                for (int j = 0; j < collection.Count; j++) row[j] = collection[j].Value;
            }
        }

        TableData.AcceptChanges();
    }

    private void readTGameplayDamageResistanceContainer(NdfObject obj)
    {
        NdfCollection armourFamilies = obj.PropertyValues[1].Value as NdfCollection;
        NdfCollection damageFamilies = obj.PropertyValues[0].Value as NdfCollection;
        NdfCollection values = obj.PropertyValues[2].Value as NdfCollection;
        TableData.Clear();
        TableData.TableName = "table";
        foreach (CollectionItemValueHolder armourFamily in armourFamilies)
            TableData.Columns.Add(armourFamily.Value.ToString());
        int k = 0;
        for (int i = 0; i < damageFamilies.Count; i++)
        {
            RowHeaders.Add(damageFamilies[i].Value.ToString());
            DataRow row = TableData.NewRow();
            TableData.Rows.Add(row);
            ObservableCollection<NdfPropertyValue> damageFamily = new();
            for (int j = 0; j < armourFamilies.Count; j++) row[j] = values[k++].Value;
        }

        TableData.AcceptChanges();
    }
    /*public ObservableCollection<ObservableCollection<NdfPropertyValue>> TableData { get; } =
        new ObservableCollection<ObservableCollection<NdfPropertyValue>>();*/
}
