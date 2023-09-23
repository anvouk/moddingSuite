using System.Collections.Generic;
using moddingSuite.BL.DDS;
using moddingSuite.ViewModel.Base;

namespace moddingSuite.Model.Textures;

public class TgvFile : ViewModelBase
{
    private PixelFormats _format;
    private uint _height;
    private uint _imageHeight;
    private uint _imageWidth;
    private bool _isCompressed;

    private ushort _mipMapCount;

    private List<uint> _offsets = new();
    private string _pixelFormatStr;
    private List<uint> _sizes = new();
    private byte[] _sourceChecksum;
    private uint _version;
    private uint _width;

    public uint Version
    {
        get => _version;
        set
        {
            _version = value;
            OnPropertyChanged();
        }
    }

    public bool IsCompressed
    {
        get => _isCompressed;
        set
        {
            _isCompressed = value;
            OnPropertyChanged();
        }
    }

    public uint Width
    {
        get => _width;
        set
        {
            _width = value;
            OnPropertyChanged();
        }
    }

    public uint Height
    {
        get => _height;
        set
        {
            _height = value;
            OnPropertyChanged();
        }
    }

    public uint ImageWidth
    {
        get => _imageWidth;
        set
        {
            _imageWidth = value;
            OnPropertyChanged();
        }
    }

    public uint ImageHeight
    {
        get => _imageHeight;
        set
        {
            _imageHeight = value;
            OnPropertyChanged();
        }
    }

    public ushort MipMapCount
    {
        get => _mipMapCount;
        set
        {
            _mipMapCount = value;
            OnPropertyChanged();
        }
    }

    public PixelFormats Format
    {
        get => _format;
        set
        {
            _format = value;
            OnPropertyChanged();
        }
    }

    public byte[] SourceChecksum
    {
        get => _sourceChecksum;
        set
        {
            _sourceChecksum = value;
            OnPropertyChanged();
        }
    }

    public List<uint> Offsets
    {
        get => _offsets;
        set
        {
            _offsets = value;
            OnPropertyChanged();
        }
    }

    public List<uint> Sizes
    {
        get => _sizes;
        set
        {
            _sizes = value;
            OnPropertyChanged();
        }
    }

    public string PixelFormatStr
    {
        get => _pixelFormatStr;
        set
        {
            _pixelFormatStr = value;
            OnPropertyChanged();
        }
    }

    public List<TgvMipMap> MipMaps { get; } = new();
}
