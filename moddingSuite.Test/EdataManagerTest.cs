using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using moddingSuite.BL;
using moddingSuite.Model.Edata;

namespace moddingSuite.Test;

[TestClass]
public class EdataManagerTest : BaseTests
{
    [DataTestMethod]
    [DataRow("/48574/Data")]
    [DataRow("/48574/DataMap")]
    [DataRow("/48574/NDF_NotFinal")]
    [DataRow("/48574/NDF_Win")]
    [DataRow("/48574/ZZ_1")]
    [DataRow("/48574/ZZ_2")]
    [DataRow("/48574/ZZ_3a")]
    [DataRow("/48574/ZZ_3b")]
    [DataRow("/48574/ZZ_4")]
    [DataRow("/48574/ZZ_NotFinal")]
    [DataRow("/48574/ZZ_Win")]
    [DataRow("/49125/NDF_Win")]
    [DataRow("/49964/NDF_Win")]
    public void CanParseRedDragonHeader(string path)
    {
        EdataManager sut = new EdataManager($"{RedDragonGameDataPath}{path}.dat");
        sut.ParseEdataFile();
        sut.Files.Should().NotBeEmpty();
    }

    [DataTestMethod]
    [DataRow("/49125/NDF_Win")]
    [DataRow("/49964/NDF_Win")]
    [DataRow("/48574/NDF_Win")]
    public void CanReadRawData(string path)
    {
        EdataManager manager = new EdataManager($"{RedDragonGameDataPath}{path}.dat");
        manager.ParseEdataFile();

        EdataContentFile config = manager.Files.First(f => f.Path == @"pc\ndf\nonpatchable\config.ndfbin");
        byte[] bytes = manager.GetRawData(config);
        bytes.Should().NotBeEmpty();
    }

    [TestMethod]
    public void CanParseAirLandBattleHeader()
    {
        IEnumerable<string> files = Directory.EnumerateFiles(AirLandGameDataPath, "*.dat", SearchOption.AllDirectories);
        int count = 0;
        foreach (string file in files)
        {
            EdataManager sut = new EdataManager(file);
            sut.ParseEdataFile();
            sut.Header.Magic.Should().BeGreaterThan(0);
            count++;
        }

        count.Should().BeGreaterThan(0);
    }
}
