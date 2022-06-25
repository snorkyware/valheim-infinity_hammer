using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using ServerSync;
using Service;
namespace InfinityHammer;
public class Configuration {
#nullable disable
  public static bool IsCheats => Enabled && ((ZNet.instance && ZNet.instance.IsServer()) || Console.instance.IsCheatsEnabled());

  public static ConfigEntry<string> configBinds;
  public static string Binds => configBinds.Value;

  public static ConfigEntry<bool> configNoBuildCost;
  public static bool NoBuildCost => configNoBuildCost.Value && IsCheats;
  public static ConfigEntry<bool> configIgnoreWards;
  public static bool IgnoreWards => configIgnoreWards.Value && IsCheats;
  public static ConfigEntry<bool> configIgnoreNoBuild;
  public static bool IgnoreNoBuild => configIgnoreNoBuild.Value && IsCheats;
  public static ConfigEntry<bool> configNoStaminaCost;
  public static bool NoStaminaCost => configNoStaminaCost.Value && IsCheats;
  public static ConfigEntry<bool> configNoDurabilityLoss;
  public static bool NoDurabilityLoss => configNoDurabilityLoss.Value && IsCheats;
  public static ConfigEntry<bool> configAllObjects;
  public static bool AllObjects => configAllObjects.Value && IsCheats;
  public static ConfigEntry<bool> configCopyState;
  public static bool CopyState => configCopyState.Value && IsCheats;
  public static ConfigEntry<bool> configAllowInDungeons;
  public static bool AllowInDungeons => configAllowInDungeons.Value && IsCheats;
  public static ConfigEntry<bool> configIgnoreOtherRestrictions;
  public static bool IgnoreOtherRestrictions => configIgnoreOtherRestrictions.Value && IsCheats;
  public static ConfigEntry<bool> configRemoveAnything;
  public static bool RemoveAnything => configRemoveAnything.Value && IsCheats;
  public static ConfigEntry<bool> configDisableMessages;
  public static bool DisableMessages => configDisableMessages.Value;
  public static ConfigEntry<bool> configDisableSelectMessages;
  public static bool DisableSelectMessages => configDisableSelectMessages.Value;
  public static ConfigEntry<bool> configDisableOffsetMessages;
  public static bool DisableOffsetMessages => configDisableOffsetMessages.Value;
  public static ConfigEntry<bool> configDisableScaleMessages;
  public static bool DisableScaleMessages => configDisableScaleMessages.Value;
  public static ConfigEntry<bool> configChatOutput;
  public static bool ChatOutput => configChatOutput.Value;
  public static ConfigEntry<bool> configDisableLoot;
  public static bool DisableLoot => configDisableLoot.Value && IsCheats;
  public static ConfigEntry<bool> configRepairAnything;
  public static bool RepairAnything => configRepairAnything.Value && IsCheats;
  public static ConfigEntry<bool> configEnableUndo;
  public static bool EnableUndo => configEnableUndo.Value && IsCheats;
  public static ConfigEntry<bool> configNoCreator;
  public static bool NoCreator => configNoCreator.Value && IsCheats;
  public static ConfigEntry<bool> configResetOffsetOnUnfreeze;
  public static bool ResetOffsetOnUnfreeze => configResetOffsetOnUnfreeze.Value;
  public static ConfigEntry<bool> configUnfreezeOnUnequip;
  public static bool UnfreezeOnUnequip => configUnfreezeOnUnequip.Value;
  public static ConfigEntry<bool> configUnfreezeOnSelect;
  public static bool UnfreezeOnSelect => configUnfreezeOnSelect.Value;
  public static ConfigEntry<string> configOverwriteHealth;
  public static float OverwriteHealth => IsCheats ? InfiniteHealth ? 10E20f : Helper.ParseFloat(configOverwriteHealth.Value, 0f) : 0f;
  public static ConfigEntry<string> configPlanBuildFolder;
  public static string PlanBuildFolder => configPlanBuildFolder.Value;
  public static ConfigEntry<string> configBuildShareFolder;
  public static string BuildShareFolder => configBuildShareFolder.Value;
  public static ConfigEntry<bool> configInfiniteHealth;
  public static bool InfiniteHealth => configInfiniteHealth.Value && IsCheats;
  public static ConfigEntry<bool> configCopyRotation;
  public static bool CopyRotation => configCopyRotation.Value && Enabled;
  public static ConfigEntry<string> configRemoveArea;
  public static float RemoveArea => Enabled ? Helper.ParseFloat(configRemoveArea.Value, 0f) : 0f;
  public static ConfigEntry<string> configSelectRange;
  public static float SelectRange => Enabled ? Helper.ParseFloat(configSelectRange.Value, 0f) : 0f;
  public static ConfigEntry<string> configRemoveRange;
  public static float RemoveRange => IsCheats ? Helper.ParseFloat(configRemoveRange.Value, 0f) : 0f;
  public static ConfigEntry<string> configRepairRange;
  public static float RepairRange => IsCheats ? Helper.ParseFloat(configRepairRange.Value, 0f) : 0f;
  public static ConfigEntry<string> configBuildRange;
  public static float BuildRange => IsCheats ? Helper.ParseFloat(configBuildRange.Value, 0f) : 0f;
  public static ConfigEntry<bool> configRemoveEffects;
  public static bool RemoveEffects => configRemoveEffects.Value && Enabled;
  public static ConfigEntry<bool> configRepairTaming;
  public static bool RepairTaming => configRepairTaming.Value && IsCheats;
  public static ConfigEntry<bool> configHidePlacementMarker;
  public static bool HidePlacementMarker => configHidePlacementMarker.Value && Enabled;
  public static ConfigEntry<bool> configEnabled;
  public static bool Enabled => configEnabled.Value;
  public static ConfigEntry<bool> configServerDevcommandsUndo;
  public static bool ServerDevcommandsUndo => configServerDevcommandsUndo.Value;
  private static HashSet<string> ParseList(string value) => value.Split(',').Select(s => s.Trim().ToLower()).ToHashSet();
  private static List<string> ParseCommands(string value) => value.Split('|').Select(s => s.Trim()).ToList();
  public static ConfigEntry<string> configRemoveBlacklist;
  public static HashSet<string> RemoveBlacklist = new();
  public static ConfigEntry<string> configSelectBlacklist;
  public static HashSet<string> SelectBlacklist = new();
  public static ConfigEntry<string> configHammerTools;
  public static HashSet<string> HammerTools = new();
  public static ConfigEntry<string> configHoeTools;
  public static HashSet<string> HoeTools = new();
  public static ConfigEntry<string> configHammerCommands;
  public static List<string> HammerCommands = new();
  public static ConfigEntry<string> configHoeCommands;
  public static List<string> HoeCommands = new();
  public static ConfigEntry<int> configHammerMenuTab;
  public static int HammerMenuTab => configHammerMenuTab.Value;
  public static ConfigEntry<int> configHammerMenuIndex;
  public static int HammerMenuIndex => configHammerMenuIndex.Value;
  public static ConfigEntry<int> configHoeMenuTab;
  public static int HoeMenuTab => configHoeMenuTab.Value;
  public static ConfigEntry<int> configHoeMenuIndex;
  public static int HoeMenuIndex => configHoeMenuIndex.Value;
  public static ConfigEntry<string> configCommandDefaultSize;
  public static float CommandDefaultSize => ConfigWrapper.TryParseFloat(configCommandDefaultSize);
  public static void AddHammerCommand(string command) {
    HammerCommands.Add(command);
    configHammerCommands.Value = string.Join("|", HammerCommands);
  }
  public static void AddHoeCommand(string command) {
    HoeCommands.Add(command);
    configHoeCommands.Value = string.Join("|", HoeCommands);
  }
  public static void Init(ConfigSync configSync, ConfigFile configFile) {
    ConfigWrapper wrapper = new("hammer_config", configFile, configSync);
    var defaultHammerCommands = new[] {
      "hammer_command cmd_icon=hammer cmd_name=Pipette cmd_desc=Shift_to_select_entire_buildings. hammer keys=-leftshift;hammer keys=leftshift connect",
      "hammer_command cmd_icon=hammer cmd_name=Area_pipette cmd_desc=Select_multiple_objects. hammer radius=r from=x,z,y",
    };
    var defaultHoeCommands = new[] {
      "hoe_terrain cmd_icon=mud_road cmd_name=Level cmd_desc=Flattens_terrain. circle=r level",
      "hoe_terrain cmd_icon=raise cmd_name=Raise cmd_desc=Raises_terrain. circle=r raise=h",
      "hoe_terrain cmd_icon=paved_road cmd_name=Pave cmd_desc=Paves_terrain. circle=r paint=paved",
      "hoe_terrain cmd_icon=replan cmd_name=Grass cmd_desc=Grass. circle=r paint=grass",
      "hoe_terrain cmd_icon=KnifeBlackMetal cmd_name=Reset cmd_desc=Resets_terrain. circle=r reset",
      "hoe_object cmd_icon=softdeath cmd_name=Remove cmd_desc=Removes_objects. radius=r remove id=*",
      "hoe_object cmd_icon=Burning cmd_name=Tame cmd_desc=Tames_creatures. radius=r tame",
    };
    var section = "General";
    configEnabled = wrapper.Bind(section, "Enabled", true, "Whether this mod is enabled at all.");
    var defaultBinds = new[] {
      "wheel,leftshift hammer_scale build 5%",
      "wheel,leftshift hammer_scale_x command 5",
      "wheel,leftshift,leftcontrol hammer_scale_y command 0.5",
      "wheel,leftshift,leftalt hammer_scale_z command 5",
    };
    configBinds = wrapper.BindList(section, "Binds", string.Join("|", defaultBinds), "Binds separated by ; that are set on the game start.");
    configHammerTools = wrapper.BindList(section, "Hammer tools", "hammer", "List of hammers.");
    configHammerTools.SettingChanged += (s, e) => HammerTools = ParseList(configHammerTools.Value);
    HammerTools = ParseList(configHammerTools.Value);
    configHoeTools = wrapper.Bind(section, "Hoe tools", "hoe", "List of hoes.");
    configHoeTools.SettingChanged += (s, e) => HoeTools = ParseList(configHoeTools.Value);
    HoeTools = ParseList(configHoeTools.Value);
    configServerDevcommandsUndo = wrapper.Bind(section, "Server Devcommands undo", true, "If disabled, uses Infinity Hammer's own undo system even if Server Devcommands is installed.");
    section = "Commands";
    configCommandDefaultSize = wrapper.Bind(section, "Command default size", "10", "Default size for commands.");
    configCommandDefaultSize.SettingChanged += (s, e) => {
      Scaling.Command.SetScaleX(CommandDefaultSize);
      Scaling.Command.SetScaleZ(CommandDefaultSize);
    };
    Scaling.Command.SetScaleX(CommandDefaultSize);
    Scaling.Command.SetScaleZ(CommandDefaultSize);

    configHammerCommands = wrapper.Bind(section, "Hammer commands", string.Join("|", defaultHammerCommands), "Available commands.");
    configHammerCommands.SettingChanged += (s, e) => HammerCommands = ParseCommands(configHammerCommands.Value);
    HammerCommands = ParseCommands(configHammerCommands.Value);
    configHammerMenuTab = wrapper.Bind(section, "Hammer menu tab", 0, "Index of the menu tab.");
    configHammerMenuIndex = wrapper.Bind(section, "Hammer menu index", 1, "Index on the menu.");
    configHoeCommands = wrapper.Bind(section, "Hoe commands", string.Join("|", defaultHoeCommands), "Available commands.");
    configHoeCommands.SettingChanged += (s, e) => HoeCommands = ParseCommands(configHoeCommands.Value);
    HoeCommands = ParseCommands(configHoeCommands.Value);
    configHoeMenuTab = wrapper.Bind(section, "Hoe menu tab", 0, "Index of the menu tab.");
    configHoeMenuIndex = wrapper.Bind(section, "Hoe menu index", 5, "Index on the menu.");
    section = "Powers";
    configRemoveArea = wrapper.Bind(section, "Remove area", "0", "Removes same objects within the radius.");
    configSelectRange = wrapper.Bind(section, "Select range", "50", "Range for selecting objects.");
    configRemoveRange = wrapper.Bind(section, "Remove range", "0", "Range for removing objects (0 = default).");
    configRepairRange = wrapper.Bind(section, "Repair range", "0", "Range for repairing objects (0 = default).");
    configBuildRange = wrapper.Bind(section, "Build range", "0", "Range for placing objects (0 = default)");
    configRepairTaming = wrapper.Bind(section, "Repair taming", false, "Repairing full health creatures tames/untames them.");
    configRemoveEffects = wrapper.Bind(section, "Remove effects", false, "Removes visual effects of building, etc.");
    configEnableUndo = wrapper.Bind(section, "Enable undo", true, "Enabled undo and redo for placing/removing.");
    configCopyRotation = wrapper.Bind(section, "Copy rotation", true, "Copies rotation of the selected object.");
    configNoBuildCost = wrapper.Bind(section, "No build cost", true, "Removes build costs and requirements.");
    configIgnoreWards = wrapper.Bind(section, "Ignore wards", true, "Ignores ward restrictions.");
    configIgnoreNoBuild = wrapper.Bind(section, "Ignore no build", true, "Ignores no build areas.");
    configNoStaminaCost = wrapper.Bind(section, "No stamina cost", true, "Removes hammer stamina usage.");
    configNoDurabilityLoss = wrapper.Bind(section, "No durability loss", true, "Removes hammer durability usage.");
    configAllObjects = wrapper.Bind(section, "All objects", true, "Allows placement of non-default objects.");
    configCopyState = wrapper.Bind(section, "Copy state", true, "Copies object's internal state.");
    configAllowInDungeons = wrapper.Bind(section, "Allow in dungeons", true, "Allows building in dungeons.");
    configRemoveAnything = wrapper.Bind(section, "Remove anything", false, "Allows removing anything.");
    configDisableLoot = wrapper.Bind(section, "Disable loot", false, "Prevents creatures and structures dropping loot when removed with the hammer.");
    configRepairAnything = wrapper.Bind(section, "Repair anything", false, "Allows reparing anything.");
    configOverwriteHealth = wrapper.Bind(section, "Overwrite health", "0", "Overwrites the health of built or repaired objects.");
    configInfiniteHealth = wrapper.Bind(section, "Infinite health", false, "Sets the Overwrite health to 10E30.");
    configNoCreator = wrapper.Bind(section, "No creator", false, "Build without setting the creator (ignored by enemies).");
    configUnfreezeOnSelect = wrapper.Bind(section, "Unfreeze on select", false, "Removes the placement freeze when selecting a new object.");
    configResetOffsetOnUnfreeze = wrapper.Bind(section, "Reset offset on unfreeze", true, "Removes the placement offset when unfreezing the placement.");
    configUnfreezeOnUnequip = wrapper.Bind(section, "Unfreeze on unequip", true, "Removes the placement freeze when unequipping the hammer.");
    configHidePlacementMarker = wrapper.Bind(section, "No placement marker", false, "Hides the yellow placement marker (also affects Gizmo mod).");
    configIgnoreOtherRestrictions = wrapper.Bind(section, "Ignore other restrictions", true, "Ignores any other restrictions (material, biome, etc.)");
    section = "Items";
    configRemoveBlacklist = wrapper.BindList(section, "Remove blacklist", "", "Object ids separated by , that can't be removed.");
    configRemoveBlacklist.SettingChanged += (s, e) => RemoveBlacklist = ParseList(configRemoveBlacklist.Value);
    RemoveBlacklist = ParseList(configRemoveBlacklist.Value);
    configSelectBlacklist = wrapper.BindList(section, "Select blacklist", "", "Object ids separated by , that can't be selected.");
    configSelectBlacklist.SettingChanged += (s, e) => SelectBlacklist = ParseList(configSelectBlacklist.Value);
    SelectBlacklist = ParseList(configSelectBlacklist.Value);
    configPlanBuildFolder = wrapper.Bind(section, "Plan Build folder", "BepInEx/config/PlanBuild", "Folder relative to the Valheim.exe.");
    configBuildShareFolder = wrapper.Bind(section, "Build Share folder", "BuildShare/Builds", "Folder relative to the Valheim.exe.");
    section = "Messages";
    configDisableMessages = wrapper.Bind(section, "Disable messages", false, "Disables all messages from this mod.");
    configDisableOffsetMessages = wrapper.Bind(section, "Disable offset messages", false, "Disables messages from changing placement offset.");
    configDisableScaleMessages = wrapper.Bind(section, "Disable scale messages", false, "Disables messages from changing the scale.");
    configDisableSelectMessages = wrapper.Bind(section, "Disable select messages", false, "Disables messages from selecting objects.");
    configChatOutput = wrapper.Bind(section, "Chat output", false, "Sends messages to the chat window from bound keys.");
  }
}
