﻿using System.Drawing;
using Rhino.Geometry;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Special;

namespace Robots.Grasshopper;

public class LoadRobotSystem : GH_Component
{
    GH_ValueList? _valueList = null;
    IGH_Param? _parameter = null;

    public LoadRobotSystem() : base("Load robot system", "Load robot", "Loads a robot system either from the library or from a custom file", "Robots", "Components") { }
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override Guid ComponentGuid => new("{7722D7E3-98DE-49B5-9B1D-E0D1B938B4A7}");
    protected override Bitmap Icon => Properties.Resources.iconRobot;

    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddTextParameter("Name", "N", "Name of the Robot system", GH_ParamAccess.item);
        pManager.AddPlaneParameter("Base", "P", "Base plane", GH_ParamAccess.item, Plane.WorldXY);
        _parameter = pManager[0];
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddParameter(new RobotSystemParameter(), "Robot system", "R", "Robot system", GH_ParamAccess.item);
    }

    protected override void BeforeSolveInstance()
    {
        if (_parameter is null)
            throw new ArgumentNullException(nameof(_parameter));

        if (_valueList is not null)
            return;

        var inputValueList = _parameter.Sources.FirstOrDefault(s => s is GH_ValueList) as GH_ValueList;
        _valueList = inputValueList ?? new GH_ValueList();

        if (inputValueList is null)
        {
            _valueList.CreateAttributes();
            _valueList.Attributes.Pivot = new PointF(Attributes.Pivot.X - 180, Attributes.Pivot.Y - 31);
            AddRobotsToValueList(_valueList);
            Instances.ActiveCanvas.Document.AddObject(_valueList, false);
            _parameter.AddSource(_valueList);
            _parameter.CollectData();
        }
        else
        {
            AddRobotsToValueList(_valueList);
        }
    }

    void AddRobotsToValueList(GH_ValueList valueList)
    {
        var selected = valueList.FirstSelectedItem;
        var robotSystems = RobotSystem.ListRobotSystems();

        if (robotSystems.SequenceEqual(valueList.ListItems.Select(i => i.Name)))
            return;

        valueList.ListItems.Clear();

        foreach (string robotSystemName in robotSystems)
            valueList.ListItems.Add(new GH_ValueListItem(robotSystemName, $"\"{robotSystemName}\""));

        if (selected is null)
            return;

        var selectedIndex = valueList.ListItems.FindIndex(s => s.Name == selected.Name);

        if (selectedIndex != -1)
            valueList.SelectItem(selectedIndex);
    }

    protected override void SolveInstance(IGH_DataAccess DA)
    {
        string? name = null;
        GH_Plane? basePlane = null;

        if (!DA.GetData(0, ref name) || name is null) { return; }
        if (!DA.GetData(1, ref basePlane) || basePlane is null) { return; }

        var robotSystem = RobotSystem.Load(name, basePlane.Value);
        DA.SetData(0, new GH_RobotSystem(robotSystem));
    }
}
