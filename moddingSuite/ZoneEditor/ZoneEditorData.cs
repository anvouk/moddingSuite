using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Media.Media3D;
using moddingSuite.Geometry;
using moddingSuite.Model.Ndfbin;
using moddingSuite.Model.Ndfbin.Types.AllTypes;
using moddingSuite.Model.Scenario;
using moddingSuite.ZoneEditor;
using moddingSuite.ZoneEditor.ScenarioItems;

namespace ZoneEditor;

public class ZoneEditorData
{
    private readonly NdfBinary data;
    private readonly Editor editor;

    private readonly ScenarioFile scenarioFile;

    //List<Outline> zoneOutlines = new List<Outline>();
    private readonly List<ScenarioItem> scenarioItems = new();
    public ScenarioItem selectedItem;
    private int spawnNumber = 1;
    private int startPosNumber = 1;
    private int zoneNumber;
    private readonly List<Zone> zones = new();

    public ZoneEditorData(ScenarioFile sf, string path)
    {
        scenarioFile = sf;
        editor = new Editor(this, path);

        data = sf.NdfBinary;
        foreach (Area area in sf.ZoneData.AreaManagers[1])
        {
            //var nodes=Geometry.getOutline(area.Content);
            //var zone = new Outline(nodes);
            //zoneOutlines.Add(zone);
            zoneNumber++;
            Zone zone = new(editor, area);
            scenarioItems.Add(zone);
            zones.Add(zone);
            editor.addScenarioItem(zone);
            Console.WriteLine("name:");
            Console.WriteLine(area.Name);
            Console.WriteLine("en name");
            /*Console.WriteLine("zone\n");
            foreach (var c in area.Content.ClippedAreas)
            {
                Console.Write("vertices=[");
        var scen = area.Content;
        foreach (var v in scen.Vertices.GetRange(c.StartVertex,c.VertexCount))
        {
            Console.WriteLine("{0:G},{1:G},{2:G};", (int)v.X, (int)v.Y, (int)v.Center);
        }
        Console.WriteLine("]");

        Console.Write("tri=[");
        foreach (var v in scen.Triangles.GetRange(c.StartTriangle,c.TriangleCount))
        {
            Console.WriteLine("{0},{1},{2};", (int)v.Point1, (int)v.Point2, (int)v.Point3);
        }
        Console.WriteLine("]");
            }*/
        }

        doZoneProperties();
        Application.EnableVisualStyles();
        Application.Run(editor);
        //Application.SetCompatibleTextRenderingDefault(false);
    }

    public EventHandler AddZone => addZone;

    public EventHandler AddLandSpawn => addLandSpawn;

    public EventHandler AddAirSpawn => addAirSpawn;

    public EventHandler AddSeaSpawn => addSeaSpawn;

    public EventHandler AddCV => addCV;

    public EventHandler AddFOB => addFOB;

    public ScenarioItem setSelectedItem(string s)
    {
        if (s == null) return null;
        if (selectedItem != null) selectedItem.setSelected(false);
        selectedItem = scenarioItems.Find(x => x.ToString().Equals(s));
        selectedItem.setSelected(true);
        return selectedItem;
    }

    public void Save()
    {
        Console.WriteLine("saving");
        //Zones
        scenarioFile.ZoneData.AreaManagers[1].Clear();
        int i = 0;
        foreach (Zone zone in zones)
        {
            Area area = zone.getArea();
            area.Id = i++;
            scenarioFile.ZoneData.AreaManagers[1].Add(area);
        }

        //delete old Markups
        purgeData();
        //Markups
        int j = 1;
        scenarioItems.ForEach(x => x.buildNdf(data, ref j));
        //data.Classes.First().Object.Manager.CreateInstanceOf
    }

    private void purgeData()
    {
        string[] toBePurged =
        {
            "TGameDesignItem",
            "TGameDesignAddOn_CommandPoints",
            "TGameDesignAddOn_StartingPoint",
            "TGameDesignAddOn_ReinforcementLocation",
            "TGameDesignAddOn_MaritimeCorridor",
            "TGameDesignAddOn_AerialCorridor",
            "TGameDesignAddOn_StartingCommandUnit",
            "TGameDesignAddOn_StartingFOB"
        };
        foreach (string str in toBePurged)
        {
            if (!data.Classes.Any(x => x.Name.Equals(str))) continue;
            NdfClass viewModel = data.Classes.Single(x => x.Name.Equals(str));
            //foreach (var inst in viewModel.Instances)
            while (viewModel.Instances.Count > 0)
            {
                NdfObject inst = viewModel.Instances.Last();
                if (inst == null)
                    return;

                viewModel.Manager.DeleteInstance(inst);

                viewModel.Instances.Remove(inst);
            }
        }

        NdfCollection list = data.Classes.First().Instances.First().PropertyValues.First().Value as NdfCollection;
        list.Clear();
    }

    private void doZoneProperties()
    {
        NdfCollection list = data.Classes.First().Instances.First().PropertyValues.First().Value as NdfCollection;
        foreach (CollectionItemValueHolder item in list)
        {
            NdfObjectReference reference = item.Value as NdfObjectReference;
            if (reference.Instance == null) continue;
            NdfObject designItem = reference.Instance;
            NdfVector position =
                designItem.PropertyValues.First(x => x.Property.Name.Equals("Position")).Value as NdfVector;
            NdfSingle rotation =
                designItem.PropertyValues.First(x => x.Property.Name.Equals("Rotation")).Value as NdfSingle;
            NdfObjectReference addonReference =
                designItem.PropertyValues.First(x => x.Property.Name.Equals("AddOn")).Value as NdfObjectReference;

            NdfPropertyValue scale = designItem.PropertyValues.FirstOrDefault(x => x.Property.Name.Equals("Scale"));

            float s = 1;

            if (scale != null)
            {
                NdfVector sc = scale.Value as NdfVector;
                if (sc != null)
                    s = (float)((Point3D)sc.Value).X;
            }

            NdfObject addon = addonReference.Instance;

            Point3D q = (Point3D)position.Value;
            AreaVertex p = new();
            p.X = (float)q.X;
            p.Y = (float)q.Y;
            Zone zone;

            zone = zones.FirstOrDefault(x =>
                Geometry.isInside(p, x.getRawOutline())
            );


            if (addon.Class.Name.Equals("TGameDesignAddOn_CommandPoints") && zone != null)
            {
                NdfInt32 pos = addon.PropertyValues.First(x => x.Property.Name.Equals("Points")).Value as NdfInt32;
                if (pos == null)
                    zone.value = 0;
                else
                    zone.value = (int)pos.Value;
            }


            if (addon.Class.Name.Equals("TGameDesignAddOn_StartingPoint") && zone != null)
            {
                NdfInt32 pos = addon.PropertyValues.First(x => x.Property.Name.Equals("AllianceNum")).Value as NdfInt32;
                if (pos == null)
                    zone.possession = 0;
                else
                    zone.possession = (Possession)pos.Value;
            }

            if (addon.Class.Name.Equals("TGameDesignAddOn_ReinforcementLocation") && zone != null)
            {
                Spawn spawn = new Spawn(Geometry.convertPoint(q), rotation.Value, s, spawnNumber++, SpawnType.Land);
                editor.addScenarioItem(spawn);
                scenarioItems.Add(spawn);
            }

            if (addon.Class.Name.Equals("TGameDesignAddOn_MaritimeCorridor") && zone != null)
            {
                Spawn spawn = new Spawn(Geometry.convertPoint(q), rotation.Value, s, spawnNumber++, SpawnType.Sea);
                editor.addScenarioItem(spawn);
                scenarioItems.Add(spawn);
            }

            if (addon.Class.Name.Equals("TGameDesignAddOn_AerialCorridor") && zone != null)
            {
                Spawn spawn = new Spawn(Geometry.convertPoint(q), rotation.Value, s, spawnNumber++, SpawnType.Air);
                editor.addScenarioItem(spawn);
                scenarioItems.Add(spawn);
            }

            if (addon.Class.Name.Equals("TGameDesignAddOn_StartingCommandUnit") && zone != null)
            {
                NdfPropertyValue prop = addon.PropertyValues.First(x => x.Property.Name.Equals("AllocationPriority"));
                int prio = 0;
                if (!(prop.Value is NdfNull)) prio = (int)((NdfInt32)prop.Value).Value;
                Icon startPos = new(Geometry.convertPoint(q), startPosNumber++, IconType.CV, prio);
                editor.addScenarioItem(startPos);
                scenarioItems.Add(startPos);
            }

            if (addon.Class.Name.Equals("TGameDesignAddOn_StartingFOB") && zone != null)
            {
                NdfPropertyValue prop = addon.PropertyValues.First(x => x.Property.Name.Equals("AllocationPriority"));
                int prio = 0;
                if (!(prop.Value is NdfNull)) prio = (int)((NdfInt32)prop.Value).Value;
                Icon startPos = new(Geometry.convertPoint(q), startPosNumber++, IconType.FOB, prio);
                editor.addScenarioItem(startPos);
                scenarioItems.Add(startPos);
            }


            //Console.WriteLine(rotation);
        }
    }

    private void addZone(object obj, EventArgs e)
    {
        Zone zone = new(editor, editor.LeftClickPoint, zoneNumber++);
        scenarioItems.Add(zone);
        zones.Add(zone);
        editor.addScenarioItem(zone, true);
    }

    private void addLandSpawn(object obj, EventArgs e)
    {
        Spawn spawn = new(PanAndZoom.fromLocalToGlobal(editor.LeftClickPoint), spawnNumber++, SpawnType.Land);
        scenarioItems.Add(spawn);
        editor.addScenarioItem(spawn, true);
        //Console.WriteLine("add land spawn");
    }

    private void addAirSpawn(object obj, EventArgs e)
    {
        Spawn spawn = new(PanAndZoom.fromLocalToGlobal(editor.LeftClickPoint), spawnNumber++, SpawnType.Air);
        scenarioItems.Add(spawn);
        editor.addScenarioItem(spawn, true);

        // Console.WriteLine("add air spawn");
    }

    private void addSeaSpawn(object obj, EventArgs e)
    {
        Spawn spawn = new(PanAndZoom.fromLocalToGlobal(editor.LeftClickPoint), spawnNumber++, SpawnType.Sea);
        scenarioItems.Add(spawn);
        editor.addScenarioItem(spawn, true);
    }

    private void addCV(object obj, EventArgs e)
    {
        Icon icon = new(PanAndZoom.fromLocalToGlobal(editor.LeftClickPoint), startPosNumber++, IconType.CV);
        scenarioItems.Add(icon);
        editor.addScenarioItem(icon, true);
    }

    private void addFOB(object obj, EventArgs e)
    {
        Icon icon = new(PanAndZoom.fromLocalToGlobal(editor.LeftClickPoint), startPosNumber++, IconType.CV);
        scenarioItems.Add(icon);
        editor.addScenarioItem(icon, true);
    }

    public void deleteItem(object o, EventArgs e)
    {
        scenarioItems.Remove(selectedItem);
        if (selectedItem is Zone) zones.Remove((Zone)selectedItem);
        editor.deleteItem(selectedItem);
    }
}
