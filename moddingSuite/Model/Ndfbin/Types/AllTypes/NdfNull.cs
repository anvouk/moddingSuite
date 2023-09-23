using System;

namespace moddingSuite.Model.Ndfbin.Types.AllTypes
{
    public class NdfNull : NdfValueWrapper
    {
        public NdfNull()
            : base(NdfType.Unset)
        {
        }

        public override string ToString()
        {
            return "<null>";
        }

        public override byte[] GetBytes()
        {
            // TODO: find a way to prevent editing null fields
            return new byte[] {};
        }

        public override byte[] GetNdfText()
        {
            throw new NotImplementedException();
        }
    }
}
