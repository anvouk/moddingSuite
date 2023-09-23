using System;
using moddingSuite.BL.Ndf;

namespace moddingSuite.Model.Ndfbin.Types.AllTypes;

internal class NdfLong : NdfFlatValueWrapper
{
    public NdfLong(long value)
        : base(NdfType.Long, value)
    {
    }

    public override byte[] GetBytes()
    {
        return BitConverter.GetBytes(Convert.ToInt64(Value));
    }

    public override byte[] GetNdfText()
    {
        return NdfTextWriter.NdfTextEncoding.GetBytes(Value.ToString());
    }
}
