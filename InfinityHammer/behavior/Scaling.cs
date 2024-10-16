using ServerDevcommands;
using UnityEngine;
namespace InfinityHammer
{
public class Scaling
{
  private static readonly ScalingData PieceScaling = new(true, false, true, Vector3.one);
  private static readonly ScalingData ToolScaling = new(false, true, false, new(10f, 0f, 10f));
  public static void Set(Vector3 value) => Get().SetScale(value);
  public static void Set(GameObject value) => Get().SetScale(value.transform.localScale);
  public static void Print(Terminal terminal) => Get().Print(terminal);
  public static ScalingData Get() => Selection.Get().IsTool ? ToolScaling : PieceScaling;
  public static ScalingData Get(bool tool) => tool ? ToolScaling : PieceScaling;
  public static Vector3 Value => Get().Vec3;
}


public class ScalingData
{
  private readonly bool OnlyPositiveHeight;
  private readonly bool MinXZ;
  private Vector3 Value;
  private readonly bool PrintChanges;
  
  public Vector3 Vec3 => Value;
  public float X => MinXZ ? Mathf.Max(0.25f, Value.x) : Value.x;
  public float Y => Value.y;
  public float Z => MinXZ ? Mathf.Max(0.25f, Value.z) : Value.z;
  
  public ScalingData(bool sanityY, bool minXZ, bool printChanges, Vector3 value)
  {
    OnlyPositiveHeight = sanityY;
    MinXZ = minXZ;
    Value = value;
    PrintChanges = printChanges;
  }

  public void Print(Terminal terminal)
  {
    if (!PrintChanges) return;
    if (Configuration.DisableScaleMessages) return;
    if (X != Y || X != Z)
      HammerHelper.Message(terminal, $"Scale set to X: {X:P0}, Z: {Z:P0}, Y: {Y:P0}.");
    else
      HammerHelper.Message(terminal, $"Scale set to {Y:P0}.");
  }
  private void Sanity()
  {
    Value.x = Mathf.Max(0f, Value.x);
    if (OnlyPositiveHeight)
      Value.y = Mathf.Max(0f, Value.y);
    Value.z = Mathf.Max(0f, Value.z);

    Value.x = Helper.Round(Value.x);
    Value.y = Helper.Round(Value.y);
    Value.z = Helper.Round(Value.z);
  }

  public void SetPrecisionXZ(float min, float precision)
  {
    Value.x = min + precision * Mathf.Floor((Value.x - min) / precision);
    Value.z = min + precision * Mathf.Floor((Value.z - min) / precision);
    AfterScaling();
  }
  public void Zoom(float amount, float percentage)
  {
    Value += new Vector3(amount, amount, amount);
    if (percentage < 0f) Value /= 1f - percentage;
    else Value *= 1f + percentage;
    Sanity();
    AfterScaling();
  }
  public void ZoomX(float amount, float percentage)
  {
    Value.x += amount;
    if (percentage < 0f) Value.x /= 1f - percentage;
    else Value.x *= 1f + percentage;
    Sanity();
    AfterScaling();
  }
  public void ZoomY(float amount, float percentage)
  {
    Value.y += amount;
    if (percentage < 0f) Value.y /= 1f - percentage;
    else Value.y *= 1f + percentage;
    Sanity();
    AfterScaling();
  }

  public void ZoomZ(float amount, float percentage)
  {
    Value.z += amount;
    if (percentage < 0f) Value.z /= 1f - percentage;
    else Value.z *= 1f + percentage;
    Sanity();
    AfterScaling();
  }
  public void SetScale(float value)
  {
    Value = value * Vector3.one;
    Sanity();
    AfterScaling();
  }
  public void SetScaleX(float value)
  {
    Value.x = value;
    Sanity();
    AfterScaling();
  }
  public void SetScaleY(float value)
  {
    Value.y = value;
    Sanity();
    AfterScaling();
  }
  public void SetScaleZ(float value)
  {
    Value.z = value;
    Sanity();
    AfterScaling();
  }
  public void SetScale(Vector3 value)
  {
    Value = value;
    Sanity();
    AfterScaling();
  }
  private void AfterScaling()
  {
    var player = Helper.GetPlayer();
    if (player.m_placementGhost)
      player.m_placementGhost.transform.localScale = Value;
  }
}
}
