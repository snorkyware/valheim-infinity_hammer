using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace InfinityHammer
{
  public class BlueprintObject
  {
    public readonly string Prefab;
    public Vector3 Pos;
    public Quaternion Rot;
    public readonly string Data;
    public Vector3 Scale;
    public readonly float Chance;
    public readonly string ExtraInfo;

    public BlueprintObject(string name, Vector3 pos, Quaternion rot, Vector3 scale, string info, string data,
      float chance)
    {
      Prefab = name;
      Pos = pos;
      Rot = rot.normalized;
      Data = data;
      Scale = scale;
      Chance = chance;
      ExtraInfo = info;
    }
  }
public class Blueprint
{
  public string Name = "";
  public string Description = "";
  public string Creator = "";
  public Vector3 Coordinates = Vector3.zero;
  public Vector3 Rotation = Vector3.zero;
  public string CenterPiece = Configuration.BlueprintCenterPiece;
  public List<BlueprintObject> Objects = new();
  public List<Vector3> SnapPoints = new();
  public float Radius = 0f;
  public Vector3 Center(string centerPiece)
  {
    if (centerPiece != "")
      CenterPiece = centerPiece;
    Bounds bounds = new();
    var y = float.MaxValue;
    Quaternion rot = Quaternion.identity;
    foreach (var obj in Objects)
    {
      y = Mathf.Min(y, obj.Pos.y);
      bounds.Encapsulate(obj.Pos);
    }

    Vector3 center = new(bounds.center.x, y, bounds.center.z);
    foreach (var obj in Objects)
    {
      if (obj.Prefab == CenterPiece)
      {
        center = obj.Pos;
        rot = Quaternion.Inverse(obj.Rot);
        break;
      }
    }
    Radius = Utils.LengthXZ(bounds.extents);
    foreach (var obj in Objects)
      obj.Pos -= center;
    SnapPoints = SnapPoints.Select(p => p - center).ToList();
    if (rot != Quaternion.identity)
    {
      foreach (var obj in Objects)
      {
        obj.Pos = rot * obj.Pos;
        obj.Rot = rot * obj.Rot;
      }
      SnapPoints = SnapPoints.Select(p => rot * p).ToList();
    }
    return center;
  }
}
}
