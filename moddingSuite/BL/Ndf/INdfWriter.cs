using System.IO;
using moddingSuite.Model.Ndfbin;

namespace moddingSuite.BL.Ndf;

internal interface INdfWriter
{
    void Write(Stream outStrea, NdfBinary ndf, bool compressed);
}
