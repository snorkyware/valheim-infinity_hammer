using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using Service;
namespace InfinityHammer
{

public partial class Configuration
{
#nullable disable

  public static ConfigEntry<bool> configSaveBlueprintData;
  public static bool SaveBlueprintData => configSaveBlueprintData.Value;
  public static ConfigEntry<string> configBlueprintCenterPiece;
  public static string BlueprintCenterPiece => configBlueprintCenterPiece.Value;
  public static ConfigEntry<string> configBlueprintSnapPiece;
  public static string BlueprintSnapPiece => configBlueprintSnapPiece.Value;
  public static ConfigEntry<bool> configSaveSimplerBlueprints;
  public static bool SimplerBlueprints => configSaveSimplerBlueprints.Value;
  public static ConfigEntry<bool> configUseBlueprintChance;
  public static bool UseBlueprintChance => configUseBlueprintChance.Value;
  public static ConfigEntry<string> configBlueprintFolder;
  public static string BlueprintGlobalFolder => Path.Combine("BepInEx", "config", configBlueprintFolder.Value);
  public static string BlueprintLocalFolder => Path.Combine(Paths.ConfigPath, configBlueprintFolder.Value);

  public static ConfigEntry<bool> configSaveBlueprintsToProfile;
  public static bool SaveBlueprintsToProfile => configSaveBlueprintsToProfile.Value;
  public static ConfigEntry<string> configSavedObjectData;
  public static HashSet<string> SavedObjectData = new();
  private static void UpdateSavedObjectData()
  {
    SavedObjectData = ParseHashList(configSavedObjectData.Value);
  }
  private static void InitBlueprint(ConfigWrapper wrapper)
  {
    var section = "6. Blueprints";
    configBlueprintFolder = wrapper.Bind(section, "Blueprint folder", "PlanBuild", "Folder relative to the config folder.");
    configSaveBlueprintsToProfile = wrapper.Bind(section, "Save blueprints to profile", false, "If enabled, blueprints are saved to the profile folder instead of base Valheim folder.");
    configSaveBlueprintData = wrapper.Bind(section, "Save data to blueprints", true, "If enabled, object data values are saved to blueprints.");
    configSaveSimplerBlueprints = wrapper.Bind(section, "Save simpler blueprints", false, "If enabled, only mandatory information is saved.");
    configSavedObjectData = wrapper.Bind(section, "Save object data blueprints", "", "Object ids that save extra data if save data is disabled.");
    configSavedObjectData.SettingChanged += (s, e) => UpdateSavedObjectData();
    configUseBlueprintChance = wrapper.Bind(section, "Use blueprint chance", false, "If enabled, chance field is checked from the blueprint.");
    configBlueprintCenterPiece = wrapper.Bind(section, "Blueprint center piece", "", "Piece name that is used as the center point when saving a blueprint.");
    configBlueprintSnapPiece = wrapper.Bind(section, "Blueprint snap piece", "", "Piece name that is used as the snap point when saving a blueprint.");
    UpdateSavedObjectData();
  }
}
}
