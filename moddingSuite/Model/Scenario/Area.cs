using System.Windows.Media.Media3D;

namespace moddingSuite.Model.Scenario;

public class Area
{
    public string Name { get; set; }

    public int Id { get; set; }

    public Point3D AttachmentPoint { get; set; }

    public AreaContent Content { get; set; }
}
