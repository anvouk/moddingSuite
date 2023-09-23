using System;
using moddingSuite.BL.Ndf;

namespace moddingSuite.Model.Ndfbin.Types.AllTypes;

public class NdfDouble : NdfFlatValueWrapper
{
    public NdfDouble(double value)
        : base(NdfType.Float64, value)
    {
    }

    public new double Value
    {
        get => (double)base.Value;
        set
        {
            base.Value = value;
            OnPropertyChanged();
        }
    }

    public override byte[] GetBytes()
    {
        return BitConverter.GetBytes(Convert.ToDouble(Value));
    }

    public override string ToString()
    {
        return string.Format("{0:0.###################################}", Value);
    }

    public override byte[] GetNdfText()
    {
        return NdfTextWriter.NdfTextEncoding.GetBytes(Value.ToString());
    }
}
