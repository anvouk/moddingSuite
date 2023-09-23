﻿using System.Linq;
using System.Windows.Forms;
using moddingSuite.Model.Ndfbin;
using moddingSuite.Model.Ndfbin.Types.AllTypes;

namespace moddingSuite.ZoneEditor.ScenarioItems;

public abstract class ScenarioItem
{
    protected string Name = "default";
    protected PaintEventHandler paintEvent;
    public Control propertypanel;
    private bool selected;

    public ScenarioItem()
    {
        paintEvent = paint;
    }

    public override string ToString()
    {
        return Name;
    }

    public abstract void attachTo(Control c);
    public abstract void detachFrom(Control c);
    public abstract void setSelected(bool selected);
    public abstract void buildNdf(NdfBinary data, ref int i);
    protected abstract void paint(object sen, PaintEventArgs e);

    protected static NdfObject createNdfObject(NdfBinary data, string str)
    {
        NdfClass classView = data.Classes.Single(x => x.Name.Equals(str));
        NdfObject inst = classView.Manager.CreateInstanceOf(classView, false);
        classView.Instances.Add(inst);
        //classView.Instances.Add(new NdfObjectViewModel(inst, data));
        return inst;
    }

    protected static NdfPropertyValue getProperty(NdfObject obj, string str)
    {
        return obj.PropertyValues.Single(x => x.Property.Name.Equals(str));
    }

    protected static NdfFileNameString getAutoName(NdfBinary data, int i)
    {
        string nameStr = string.Format("P0_AutoName_{0}", i);
        return getString(data, nameStr);
    }

    protected static NdfFileNameString getString(NdfBinary data, string nameStr)
    {
        NdfStringReference nameRef = data.Strings.Single(x => x.Value.Equals(nameStr));
        return new NdfFileNameString(nameRef);
    }
}
