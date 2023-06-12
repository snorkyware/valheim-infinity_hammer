using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Service;
using UnityEngine;
namespace InfinityHammer;


public class HammerSaveCommand {

  private static string GetExtraInfo(GameObject obj, ZDOData data) {
    var info = "";
    if (obj.GetComponent<Sign>())
      info = data.GetString(Hash.Text, "");
    if (obj.GetComponent<TeleportWorld>())
      info = data.GetString(Hash.Tag, "");
    if (obj.GetComponent<Tameable>())
      info = data.GetString(Hash.TamedName, "");

    if (obj.GetComponent<ItemStand>() && data.GetString(Hash.Item) != "") {
      var item = data.GetString(Hash.Item);
      var variant = data.GetInt(Hash.Variant);
      if (variant != 0)
        info = $"{item}:{variant}";
      else
        info = $"{item}";
    }
    if (obj.TryGetComponent<ArmorStand>(out var armorStand)) {
      info = $"{armorStand.m_pose}:";
      info += $"{armorStand.m_slots.Count}:";
      var slots = armorStand.m_slots.Select(slot => $"{slot.m_visualName}:{slot.m_visualVariant}");
      info += string.Join(":", slots);
    }
    return info;
  }
  private static void AddSingleObject(Blueprint bp, GameObject obj) {
    var data = Selection.GetData();
    var info = GetExtraInfo(obj, data);
    bp.Objects.Add(new BlueprintObject(Utils.GetPrefabName(obj), Vector3.zero, Quaternion.identity, obj.transform.localScale, info, data.Save()));
  }
  private static void AddObject(Blueprint bp, GameObject obj, int index = 0) {
    var data = Selection.GetData(index);
    var info = GetExtraInfo(obj, data);
    bp.Objects.Add(new BlueprintObject(Utils.GetPrefabName(obj), obj.transform.localPosition, obj.transform.localRotation, obj.transform.localScale, info, data.Save()));
  }
  private static Blueprint BuildBluePrint(Player player, GameObject obj, string centerPiece) {
    Blueprint bp = new() {
      Name = Utils.GetPrefabName(obj),
      Creator = player.GetPlayerName()
    };
    var piece = obj.GetComponent<Piece>();
    if (piece) {
      bp.Name = Localization.instance.Localize(piece.m_name);
      bp.Description = piece.m_description;
    }
    if (Selection.Type == SelectedType.Object || Selection.Type == SelectedType.Default) {
      AddSingleObject(bp, obj);
      // Snap points are sort of useful for single objects.
      // Since single objects should just have custom data but otherwise the original behavior.
      foreach (Transform child in obj.transform) {
        if (Helper.IsSnapPoint(child.gameObject))
          bp.SnapPoints.Add(child.localPosition);
      }
    }
    if (Selection.Type == SelectedType.Multiple || Selection.Type == SelectedType.Location) {
      var i = 0;
      foreach (Transform tr in obj.transform) {
        // Snap points aren't that useful for multiple objects.
        // Especially when they conflict with the center piece.
        // In the future, snapPiece could be added to allow users to select the snap points.
        if (Helper.IsSnapPoint(tr.gameObject)) continue;
        AddObject(bp, tr.gameObject, i);
        i += 1;
      }
    }
    bp.Center(centerPiece);
    return bp;
  }

  private static string[] GetPlanBuildFile(Blueprint bp) {
    List<string> lines = new() {
      $"#Name:{bp.Name}",
      $"#Creator:{bp.Creator}",
      $"#Description:{bp.Description}",
      $"#Category:InfinityHammer",
      $"#Center:{bp.CenterPiece}",
      $"#SnapPoints"
    };
    lines.AddRange(bp.SnapPoints.Select(GetPlanBuildSnapPoint));
    lines.Add($"#Pieces");
    lines.AddRange(bp.Objects.Select(GetPlanBuildObject));
    return lines.ToArray();
  }
  private static string InvariantString(float f) {
    return f.ToString(NumberFormatInfo.InvariantInfo);
  }
  private static string GetPlanBuildSnapPoint(Vector3 pos) {
    var x = InvariantString(pos.x);
    var y = InvariantString(pos.y);
    var z = InvariantString(pos.z);
    return $"{x};{y};{z}";
  }
  private static string GetPlanBuildObject(BlueprintObject obj) {
    var name = obj.Prefab;
    var posX = InvariantString(obj.Pos.x);
    var posY = InvariantString(obj.Pos.y);
    var posZ = InvariantString(obj.Pos.z);
    var rotX = InvariantString(obj.Rot.x);
    var rotY = InvariantString(obj.Rot.y);
    var rotZ = InvariantString(obj.Rot.z);
    var rotW = InvariantString(obj.Rot.w);
    var scaleX = InvariantString(obj.Scale.x);
    var scaleY = InvariantString(obj.Scale.y);
    var scaleZ = InvariantString(obj.Scale.z);
    var info = obj.ExtraInfo;
    var data = "";
    if (obj.Data != null) {
      data = obj.Data.GetBase64();
      if (data == "AAAAAA==") data = "";
    }
    return $"{name};;{posX};{posY};{posZ};{rotX};{rotY};{rotZ};{rotW};{info};{scaleX};{scaleY};{scaleZ};{data}";
  }

  public HammerSaveCommand() {
    CommandWrapper.Register("hammer_save", (int index) => {
      if (index == 0) return CommandWrapper.Info("File name.");
      if (index == 1) return CommandWrapper.ObjectIds();
      return null;
    });
    Helper.Command("hammer_save", "[file name] [center piece] - Saves the selection to a blueprint.", (args) => {
      Helper.CheatCheck();
      Helper.ArgsCheck(args, 2, "Blueprint name is missing.");
      var player = Helper.GetPlayer();
      var ghost = Helper.GetPlacementGhost();
      var center = args.Length > 2 ? args[2] : "";
      var bp = BuildBluePrint(player, ghost, center);
      var lines = GetPlanBuildFile(bp);
      var name = Path.GetFileNameWithoutExtension(args[1]) + ".blueprint";
      var path = Path.Combine(Configuration.SaveBlueprintsToProfile ? Configuration.BlueprintLocalFolder : Configuration.BlueprintGlobalFolder, name);
      Directory.CreateDirectory(Path.GetDirectoryName(path));
      File.WriteAllLines(path, lines);
      args.Context.AddString($"Blueprint saved to {path}");
    });
  }
}
