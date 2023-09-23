using System.Globalization;

namespace moddingSuite.Model.Edata;

/// <summary>
///     The reversed struct from Hex Workshop - much love to Hob_gadling for his hard work and help.
///     The chunks after offset and fileSize are because of long (int64)
///     struct dictFileEntry {
///     DWORD groupId;
///     DWORD fileEntrySize;
///     DWORD offset;
///     DWORD chunk2;
///     DWORD fileSize;
///     DWORD chunk4;
///     blob checksum[16];
///     zstring name;
///     };
/// </summary>
public class EdataContentFile : EdataEntity
{
    private byte[] _checkSum = new byte[16];
    private EdataDir _directory;
    private EdataFileType _fileType = EdataFileType.Unknown;
    private uint _id;
    private long _offset;
    private string _path;
    private long _size;

    public EdataContentFile(string name, EdataDir dir = null)
        : base(name)
    {
        Directory = dir;
        if (dir != null)
            dir.Files.Add(this);
    }

    public EdataContentFile()
        : base(string.Empty)
    {
    }

    public string Path
    {
        get => _path;
        set
        {
            _path = value;
            OnPropertyChanged();
        }
    }

    public long Offset
    {
        get => _offset;
        set
        {
            _offset = value;
            OnPropertyChanged();
        }
    }

    public long Size
    {
        get => _size;
        set
        {
            _size = value;
            OnPropertyChanged();
        }
    }

    public byte[] Checksum
    {
        get => _checkSum;
        set
        {
            _checkSum = value;
            OnPropertyChanged();
        }
    }

    public uint Id
    {
        get => _id;
        set
        {
            _id = value;
            OnPropertyChanged("Name");
        }
    }

    public EdataFileType FileType
    {
        get => _fileType;
        set
        {
            _fileType = value;
            OnPropertyChanged();
        }
    }

    public EdataDir Directory
    {
        get => _directory;
        set
        {
            _directory = value;
            OnPropertyChanged();
        }
    }

    public override string ToString()
    {
        return Path.ToString(CultureInfo.CurrentCulture);
    }
}
