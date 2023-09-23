using System;
using System.Collections.Generic;
using moddingSuite.Util;

namespace moddingSuite.BL.ImageService;

public class RawImage
{
    public enum Format
    {
        Format_RGB,
        Format_ARGB
    }

    public RawImage(Color32[] data, uint width, uint height)
    {
        if (data == null)
            Data = new Color32[width * height * 4]; // sizeof(Color32) == 4
        else
            Data = data;

        Width = width;
        Height = height;
    }

    public RawImage(uint width, uint height)
        : this(null, width, height)
    {
    }

    public uint Width { get; set; }

    public uint Height { get; set; }

    public Color32[] Data { get; }

    public Format ColFormat { get; set; }

    public Color32[] Scanline(uint line)
    {
        Color32[] tmp = new Color32[Width];
        Array.Copy(Data, line * Width, tmp, 0, Width);
        return tmp;
    }

    public Color32 Pixel(uint px)
    {
        return Data[px];
    }

    public Color32 Pixel(uint x, uint y)
    {
        return Pixel(y * Width + x);
    }

    public byte[] GetRawData()
    {
        List<byte> ret = new();

        foreach (Color32 col in Data)
            ret.AddRange(Utils.StructToBytes(col));

        return ret.ToArray();
    }
}
