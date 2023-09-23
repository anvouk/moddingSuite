using System;
using moddingSuite.Util;

namespace moddingSuite.Model.Ndfbin.Types.AllTypes;

public class NdfHash : NdfFlatValueWrapper
{
    public NdfHash(byte[] value)
        : base(NdfType.Hash, value)
    {
    }

    public new byte[] Value
    {
        get => (byte[])base.Value;
        set
        {
            base.Value = value;
            OnPropertyChanged(() => Value);
        }
    }

    public override byte[] GetBytes()
    {
        return Value;
    }

    public override byte[] GetNdfText()
    {
        throw new NotImplementedException();
    }

    public override string ToString()
    {
        return Utils.ByteArrayToBigEndianHexByteString(Value);
    }
}
