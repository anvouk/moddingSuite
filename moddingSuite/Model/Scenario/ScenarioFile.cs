using System.Collections.Generic;
using moddingSuite.Model.Ndfbin;
using moddingSuite.ViewModel.Base;

namespace moddingSuite.Model.Scenario;

public class ScenarioFile : ViewModelBase
{
    private byte[] _checksum;
    private NdfBinary _ndfBinary;
    private int _version;
    private AreaFile _zoneData;
    public long lastPartStartByte;

    public byte[] Checksum
    {
        get => _checksum;
        set
        {
            _checksum = value;
            OnPropertyChanged();
        }
    }

    public NdfBinary NdfBinary
    {
        get => _ndfBinary;
        set
        {
            _ndfBinary = value;
            OnPropertyChanged("Checksum");
        }
    }

    public int Version
    {
        get => _version;
        set
        {
            _version = value;
            OnPropertyChanged();
        }
    }

    public List<byte[]> ContentFiles { get; set; } = new();

    public AreaFile ZoneData
    {
        get => _zoneData;
        set
        {
            _zoneData = value;
            OnPropertyChanged();
        }
    }
}
