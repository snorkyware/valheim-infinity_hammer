using System;
using System.Collections.Generic;
using System.Linq;
using ServerDevcommands;
using UnityEngine;

namespace InfinityHammer;
public class HammerZoopCommand
{
  private static void Command(string direction, string action, string reverse)
  {
    AutoComplete.Register($"hammer_zoop_{direction}", (int index) =>
    {
      if (index == 0) return ParameterInfo.Create($"Meters towards the {direction} direction (<color=yellow>number</color> or <color=yellow>number*auto</color> for automatic step size).");
      return ParameterInfo.None;
    });
    Helper.Command($"hammer_zoop_{direction}", $"[value=auto] - Zoops towards the {direction} direction.", (args) =>
    {
      HammerHelper.CheatCheck();
      var ghost = HammerHelper.GetPlacementGhost();
      var value = "auto";
      if (args.Args.Length > 1)
        value = args[1];
      var zoop = Parse.Direction(args.Args, 2) < 0 ? reverse : action;
      if (Selection.Get() is not ObjectSelection selected)
        return;
      try
      {

        if (zoop == "left")
          selected.ZoopLeft(value);
        else if (zoop == "right")
          selected.ZoopRight(value);
        else if (zoop == "up")
          selected.ZoopUp(value);
        else if (zoop == "down")
          selected.ZoopDown(value);
        else if (zoop == "forward")
          selected.ZoopForward(value);
        else if (zoop == "backward")
          selected.ZoopBackward(value);
      }
      catch (InvalidOperationException e)
      {
        selected.ZoopReset();
        throw e;
      }
    });
  }
  public HammerZoopCommand()
  {
    Command("left", "left", "right");
    Command("right", "right", "left");
    Command("down", "down", "up");
    Command("up", "up", "down");
    Command("backward", "backward", "forward");
    Command("forward", "forward", "backward");
  }
}
public partial class ObjectSelection : BaseSelection
{

  private int ZoopsX = 0;
  private int ZoopsY = 0;
  private int ZoopsZ = 0;
  private Vector3 ZoopOffset = new();
  private readonly Dictionary<Vector3Int, GameObject> Zoops = [];
  public void ZoopReset()
  {
    if (Objects.Count > 1)
      ToSingle();
    ZoopsX = 0;
    ZoopsY = 0;
    ZoopsZ = 0;
    ZoopOffset = new();
    Zoops.Clear();
  }
  private void RemoveChildSub(Vector3Int index)
  {
    RemoveObject(Zoops[index]);
    Zoops.Remove(index);
  }
  private void RemoveChildX()
  {
    for (var y = 0; Math.Abs(y) <= Math.Abs(ZoopsY); y += Sign(ZoopsY))
      for (var z = 0; Math.Abs(z) <= Math.Abs(ZoopsZ); z += Sign(ZoopsZ))
        RemoveChildSub(new(ZoopsX, y, z));
  }
  private void RemoveChildY()
  {
    for (var x = 0; Math.Abs(x) <= Math.Abs(ZoopsX); x += Sign(ZoopsX))
      for (var z = 0; Math.Abs(z) <= Math.Abs(ZoopsZ); z += Sign(ZoopsZ))
        RemoveChildSub(new(x, ZoopsY, z));
  }
  private void RemoveChildZ()
  {
    for (var x = 0; Math.Abs(x) <= Math.Abs(ZoopsX); x += Sign(ZoopsX))
      for (var y = 0; Math.Abs(y) <= Math.Abs(ZoopsY); y += Sign(ZoopsY))
        RemoveChildSub(new(x, y, ZoopsZ));
  }
  private void AddChildSub(Vector3Int index)
  {
    var pos = GetOffset(index);
    var baseObj = Zoops.Count == 0 ? SelectedObject : Zoops.First().Value;
    Zoops[index] = AddObject(baseObj, pos);
  }
  private void AddChildX(string offset)
  {
    UpdateOffsetX(offset);
    ZNetView.m_forceDisableInit = true;
    for (var y = 0; Math.Abs(y) <= Math.Abs(ZoopsY); y += Sign(ZoopsY))
      for (var z = 0; Math.Abs(z) <= Math.Abs(ZoopsZ); z += Sign(ZoopsZ))
        AddChildSub(new(ZoopsX, y, z));
    ZNetView.m_forceDisableInit = false;
  }
  private void AddChildY(string offset)
  {
    UpdateOffsetY(offset);
    ZNetView.m_forceDisableInit = true;
    for (var x = 0; Math.Abs(x) <= Math.Abs(ZoopsX); x += Sign(ZoopsX))
      for (var z = 0; Math.Abs(z) <= Math.Abs(ZoopsZ); z += Sign(ZoopsZ))
        AddChildSub(new(x, ZoopsY, z));
    ZNetView.m_forceDisableInit = false;
  }

  private void AddChildZ(string offset)
  {
    UpdateOffsetZ(offset);
    ZNetView.m_forceDisableInit = true;
    for (var x = 0; Math.Abs(x) <= Math.Abs(ZoopsX); x += Sign(ZoopsX))
      for (var y = 0; Math.Abs(y) <= Math.Abs(ZoopsY); y += Sign(ZoopsY))
        AddChildSub(new(x, y, ZoopsZ));
    ZNetView.m_forceDisableInit = false;
  }
  private Vector3 GetOffset(Vector3Int index)
  {
    var offset = ZoopOffset;
    offset.x *= index.x;
    offset.y *= index.y;
    offset.z *= index.z;
    return offset;
  }
  private void UpdateOffsetX(string offset)
  {
    var baseObj = SelectedObject.transform.GetChild(0).gameObject;
    var size = HammerHelper.ParseSize(baseObj, offset);
    ZoopOffset.x = size.x * SelectedObject.transform.localScale.z;
  }
  private void UpdateOffsetY(string offset)
  {
    var baseObj = SelectedObject.transform.GetChild(0).gameObject;
    var size = HammerHelper.ParseSize(baseObj, offset);
    ZoopOffset.y = size.y * SelectedObject.transform.localScale.y;
  }
  private void UpdateOffsetZ(string offset)
  {
    var baseObj = SelectedObject.transform.GetChild(0).gameObject;
    var size = HammerHelper.ParseSize(baseObj, offset);
    ZoopOffset.z = size.z * SelectedObject.transform.localScale.z;
  }
  private void ZoopPostprocess()
  {
    CountObjects();
    var scale = Scaling.Build.Value;
    Helper.GetPlayer().SetupPlacementGhost();
    Scaling.Build.SetScale(scale);
  }
  public void ZoopRight(string offset)
  {
    if (Objects.Count > 1 && ZoopsX == 0 && ZoopsY == 0 && ZoopsZ == 0) return;
    if (ZoopsX < 0)
    {
      RemoveChildX();
      ZoopsX += 1;
    }
    else
    {
      ZoopsX += 1;
      AddChildX(offset);
    }
    ZoopPostprocess();
  }
  public void ZoopLeft(string offset)
  {
    if (Objects.Count > 1 && ZoopsX == 0 && ZoopsY == 0 && ZoopsZ == 0) return;
    if (ZoopsX > 0)
    {
      RemoveChildX();
      ZoopsX -= 1;
    }
    else
    {
      ZoopsX -= 1;
      AddChildX(offset);
    }
    ZoopPostprocess();
  }
  public void ZoopUp(string offset)
  {
    if (Objects.Count > 1 && ZoopsX == 0 && ZoopsY == 0 && ZoopsZ == 0) return;
    if (ZoopsY < 0)
    {
      RemoveChildY();
      ZoopsY += 1;
    }
    else
    {
      ZoopsY += 1;
      AddChildY(offset);
    }
    ZoopPostprocess();
  }
  public void ZoopDown(string offset)
  {
    if (Objects.Count > 1 && ZoopsX == 0 && ZoopsY == 0 && ZoopsZ == 0) return;
    if (ZoopsY > 0)
    {
      RemoveChildY();
      ZoopsY -= 1;
    }
    else
    {
      ZoopsY -= 1;
      AddChildY(offset);
    }
    ZoopPostprocess();
  }
  public void ZoopForward(string offset)
  {
    if (Objects.Count > 1 && ZoopsX == 0 && ZoopsY == 0 && ZoopsZ == 0) return;
    if (ZoopsZ < 0)
    {
      RemoveChildZ();
      ZoopsZ += 1;
    }
    else
    {
      ZoopsZ += 1;
      AddChildZ(offset);
    }
    ZoopPostprocess();
  }
  public void ZoopBackward(string offset)
  {
    if (Objects.Count > 1 && ZoopsX == 0 && ZoopsY == 0 && ZoopsZ == 0) return;
    if (ZoopsZ > 0)
    {
      RemoveChildZ();
      ZoopsZ -= 1;
    }
    else
    {
      ZoopsZ -= 1;
      AddChildZ(offset);
    }
    ZoopPostprocess();
  }

  private int Sign(int value) => value >= 0 ? 1 : -1;
}
