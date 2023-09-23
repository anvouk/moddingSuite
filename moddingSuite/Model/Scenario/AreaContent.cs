using System.Collections.Generic;
using moddingSuite.Model.Common;

namespace moddingSuite.Model.Scenario;

public class AreaContent
{
    public List<AreaClipped> ClippedAreas { get; set; } = new();

    public AreaClipped BorderTriangle { get; set; } = new();

    public AreaClipped BorderVertex { get; set; } = new();

    public List<AreaVertex> Vertices { get; set; } = new();

    public List<MeshTriangularFace> Triangles { get; set; } = new();
}
