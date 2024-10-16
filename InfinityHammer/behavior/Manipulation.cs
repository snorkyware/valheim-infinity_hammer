
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection.Emit;
using HarmonyLib;
using ServerDevcommands;
using UnityEngine;
namespace InfinityHammer
{


[HarmonyPatch(typeof(Player), nameof(Player.FindClosestSnapPoints))]
public class DisableSnapsWhenFrozen
{
  static bool Prefix() => !Position.Override.HasValue;
}

[HarmonyPatch(typeof(Player), nameof(Player.PieceRayTest))]
public class FreezePlacementMarker
{
  static Vector3 CurrentNormal = Vector3.up;
  static void Postfix(ref Vector3 point, ref Vector3 normal, ref Piece piece, ref Heightmap heightmap, ref Collider waterSurface, ref bool __result)
  {
    if (__result && Grid.Enabled)
    {
      point = Grid.Apply(point, heightmap ? Vector3.up : normal);
      if (heightmap)
      {
        // +2 meters so that floors and small objects will be hit by the collision check.
        point.y = ZoneSystem.instance.GetGroundHeight(point) + 2f;
        if (Physics.Raycast(point, Vector3.down, out var raycastHit, 50f, Player.m_localPlayer.m_placeRayMask))
        {
          point = raycastHit.point;
          normal = raycastHit.normal;
        }
      }
    }
    if (Position.Override.HasValue)
    {
      point = Position.Override.Value;
      normal = CurrentNormal;
      __result = true;
#nullable disable
      piece = null;
      heightmap = null;
      waterSurface = null;
#nullable enable
    }
    else
    {
      CurrentNormal = normal;
    }
  }
}

[HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacementGhost))]
public class OverridePlacementGhost
{
  ///<summary>Then override snapping and other modifications for the final result (and some rules are checked too).</summary>
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions)
          .MatchForward(
              useEnd: false,
              new CodeMatch(
                  OpCodes.Call,
                  AccessTools.Method(typeof(Location), nameof(Location.IsInsideNoBuildLocation))))
          .Advance(-2)
          // If-branches require using ops from the IsInsideBuildLocation so just duplicate the used ops afterwards.
          .Insert(new CodeInstruction(OpCodes.Call, Transpilers.EmitDelegate<Action<GameObject>>(Position.Apply).operand),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Player), nameof(Player.m_placementGhost)))
          )
          .InstructionEnumeration();
  }
}


public static class Grid
{
  public static bool Enabled => Precision != 0f || Selection.Get().TerrainGrid;
  private static float Precision;
  private static Vector3 Center;
  private static Vector3 PreciseCenter;
  public static Vector3 Apply(Vector3 point, Vector3 normal)
  {
    var precision = Precision;
    var center = Center;
    if (precision == 0f && Selection.Get().TerrainGrid)
    {
      precision = 1f;
      center = PreciseCenter;
    }
    if (precision == 0f) return point;
    var rotation = Quaternion.FromToRotation(Vector3.up, normal);
    point = rotation * point;
    var c = rotation * center;
    point.x = c.x + Mathf.Round((point.x - c.x) / precision) * precision;
    point.z = c.z + Mathf.Round((point.z - c.z) / precision) * precision;
    return Quaternion.Inverse(rotation) * point;
  }
  public static void Toggle(float precision, Vector3 center)
  {
    if (Precision == precision) Precision = 0f;
    else
    {
      Center = center;
      Precision = precision;
    }
  }
  public static void SetPreciseMode(Vector3 center)
  {
    PreciseCenter = center;
  }
}
public static class Position
{
  public static Vector3? Override = null;
  public static Vector3 Offset = Vector3.zero;
  public static void ToggleFreeze()
  {
    if (Override.HasValue)
      Unfreeze();
    else
      Freeze();
  }
  public static void Freeze(Vector3 position)
  {
    Override = position;
  }
  public static void Freeze()
  {
    var player = Helper.GetPlayer();
    var ghost = player.m_placementGhost;
    Override = ghost ? Deapply(ghost.transform.position, ghost.transform.rotation) : player.transform.position;
  }
  public static void Unfreeze()
  {
    Override = null;
    if (Configuration.ResetOffsetOnUnfreeze) Offset = Vector3.zero;
  }
  public static void Apply(GameObject ghost)
  {
    if (Selection.Get().PlayerHeight)
    {
      var player = Helper.GetPlayer();
      ghost.transform.position = new Vector3(ghost.transform.position.x, player.transform.position.y, ghost.transform.position.z);
    }
    ghost.transform.position = Apply(ghost.transform.position, ghost.transform.rotation);
  }
  public static Vector3 Apply(Vector3 point, Quaternion rotation)
  {
    if (Override.HasValue)
      point = Override.Value;
    if (Selection.Get().TerrainGrid)
      rotation = Quaternion.identity;
    point += rotation * Vector3.right * Offset.x;
    point += rotation * Vector3.up * Offset.y;
    point += rotation * Vector3.forward * Offset.z;
    return point;
  }
  public static Vector3 Deapply(Vector3 point, Quaternion rotation)
  {
    if (Override.HasValue)
      point = Override.Value;
    if (Selection.Get().TerrainGrid)
      rotation = Quaternion.identity;
    point -= rotation * Vector3.right * Offset.x;
    point -= rotation * Vector3.up * Offset.y;
    point -= rotation * Vector3.forward * Offset.z;
    return point;
  }
  public static void SetX(float value)
  {
    Offset.x = value;
  }
  public static void SetY(float value)
  {
    Offset.y = value;
  }
  public static void SetZ(float value)
  {
    Offset.z = value;
  }
  public static void MoveLeft(float value)
  {
    if (Selection.Get().TerrainGrid) value = Mathf.Max(value, 1f);
    Offset.x = Helper.Round(Offset.x - value);
  }
  public static void MoveRight(float value)
  {
    if (Selection.Get().TerrainGrid) value = Mathf.Max(value, 1f);
    Offset.x = Helper.Round(Offset.x + value);
  }
  public static void MoveDown(float value)
  {
    Offset.y = Helper.Round(Offset.y - value);
  }
  public static void MoveUp(float value)
  {
    Offset.y = Helper.Round(Offset.y + value);
  }
  public static void MoveBackward(float value)
  {
    if (Selection.Get().TerrainGrid) value = Mathf.Max(value, 1f);
    Offset.z = Helper.Round(Offset.z - value);
  }
  public static void MoveForward(float value)
  {
    if (Selection.Get().TerrainGrid) value = Mathf.Max(value, 1f);
    Offset.z = Helper.Round(Offset.z + value);
  }
  public static void Set(Vector3 value)
  {
    Offset = value;
  }
  public static void Move(Vector3 value)
  {
    Offset += value;
  }

  public static void Print(Terminal terminal)
  {
    if (Configuration.DisableOffsetMessages) return;
    HammerHelper.Message(terminal, $"Offset set to forward: {Offset.z.ToString("F1", CultureInfo.InvariantCulture)}, up: {Offset.y.ToString("F1", CultureInfo.InvariantCulture)}, right: {Offset.x.ToString("F1", CultureInfo.InvariantCulture)}.");
  }
}
}
