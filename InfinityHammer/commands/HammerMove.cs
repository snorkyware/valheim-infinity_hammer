using System;
using Service;
using UnityEngine;
namespace InfinityHammer;
public class HammerMoveCommand
{
  private static void Command(string direction, Action<Terminal.ConsoleEventArgs, GameObject> action)
  {
    CommandWrapper.Register($"hammer_move_{direction}", (int index) =>
    {
      if (index == 0) return CommandWrapper.Info($"Meters towards the {direction} direction (<color=yellow>number</color> or <color=yellow>number*auto</color> for automatic step size).");
      return null;
    });
    Helper.Command($"hammer_move_{direction}", $"[value=auto] - Moves the placement towards the {direction} direction.", (args) =>
    {
      var ghost = Helper.GetPlacementGhost();
      action(args, ghost);
      Position.Print(args.Context);
    });
  }
  public HammerMoveCommand()
  {
    Command("left", (args, ghost) =>
    {
      var amount = Helper.TryParseSize(ghost, args.Args, 1).x;
      Position.MoveLeft(Parse.Direction(args.Args, 2) * amount);
    });
    Command("right", (args, ghost) =>
    {
      var amount = Helper.TryParseSize(ghost, args.Args, 1).x;
      Position.MoveRight(Parse.Direction(args.Args, 2) * amount);
    });
    Command("down", (args, ghost) =>
    {
      var amount = Helper.TryParseSize(ghost, args.Args, 1).y;
      Position.MoveDown(Parse.Direction(args.Args, 2) * amount);
    });
    Command("up", (args, ghost) =>
    {
      var amount = Helper.TryParseSize(ghost, args.Args, 1).y;
      Position.MoveUp(Parse.Direction(args.Args, 2) * amount);
    });
    Command("backward", (args, ghost) =>
    {
      var amount = Helper.TryParseSize(ghost, args.Args, 1).z;
      Position.MoveBackward(Parse.Direction(args.Args, 2) * amount);
    });
    Command("forward", (args, ghost) =>
    {
      var amount = Helper.TryParseSize(ghost, args.Args, 1).z;
      Position.MoveForward(Parse.Direction(args.Args, 2) * amount);
    });
    CommandWrapper.Register("hammer_move", (int index, int subIndex) =>
    {
      if (index == 0) return CommandWrapper.FRU("Meters to move the placement", subIndex);
      return null;
    });
    Helper.Command("hammer_move", "[forward,up,right] - Moves the placement.", (args) =>
    {
      var ghost = Helper.GetPlacementGhost();
      var amount = Helper.TryParseSizesZYX(ghost, args.Args, 1, "0");
      Position.Move(amount);
      Position.Print(args.Context);
    });
  }
}
