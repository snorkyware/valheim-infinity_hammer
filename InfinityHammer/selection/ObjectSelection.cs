using System;
using System.Collections.Generic;
using System.Linq;
using ServerDevcommands;
using Service;
using UnityEngine;

namespace InfinityHammer;

// This is quite messy because single and multiple objects behave differently.
// But they have to be the same because selection is changed when zooping.
public partial class ObjectSelection : BaseSelection
{

  public List<SelectedObject> Objects = [];


  public ObjectSelection(string name, bool singleUse)
  {
    var prefab = ZNetScene.instance.GetPrefab(name);
    if (!prefab) throw new InvalidOperationException("Invalid prefab.");
    if (prefab.GetComponent<Player>()) throw new InvalidOperationException("Players are not valid objects.");
    if (!Configuration.AllObjects && !HammerHelper.IsBuildPiece(prefab)) throw new InvalidOperationException("Only build pieces are allowed.");
    SingleUse = singleUse;
    SelectedObject = HammerHelper.SafeInstantiate(prefab);
    Objects.Add(new(name, prefab.GetComponent<ZNetView>().m_syncInitialScale, new()));
  }
  public ObjectSelection(ZNetView view, bool singleUse)
  {
    var originalPrefab = ZNetScene.instance.GetPrefab(view.GetZDO().GetPrefab());
    var name = Utils.GetPrefabName(originalPrefab);
    var prefab = Configuration.CopyState ? view.gameObject : originalPrefab;
    ZDOData data = Configuration.CopyState ? new(view.GetZDO()) : new();

    if (!prefab) throw new InvalidOperationException("Invalid prefab.");
    if (prefab.GetComponent<Player>()) throw new InvalidOperationException("Players are not valid objects.");
    if (!Configuration.AllObjects && !HammerHelper.IsBuildPiece(prefab)) throw new InvalidOperationException("Only build pieces are allowed.");
    SingleUse = singleUse;
    SelectedObject = HammerHelper.SafeInstantiate(prefab);
    // Reseted for bounds check.
    SelectedObject.transform.rotation = Quaternion.identity;
    ResetColliders(SelectedObject, originalPrefab);
    Objects.Add(new(name, view.m_syncInitialScale, data));
    Rotating.UpdatePlacementRotation(view.gameObject);
  }
  public ObjectSelection(IEnumerable<ZNetView> views, bool singleUse)
  {
    SingleUse = singleUse;
    SelectedObject = new GameObject();
    // Prevents children from disappearing.
    SelectedObject.SetActive(false);
    SelectedObject.name = $"Multiple ({views.Count()})";
    SelectedObject.transform.position = views.First().transform.position;
    ZNetView.m_forceDisableInit = true;
    foreach (var view in views)
    {
      var originalPrefab = ZNetScene.instance.GetPrefab(view.GetZDO().GetPrefab());
      var name = Utils.GetPrefabName(originalPrefab);
      var prefab = Configuration.CopyState ? view.gameObject : originalPrefab;
      ZDOData data = Configuration.CopyState ? new(view.GetZDO()) : new();
      var obj = HammerHelper.SafeInstantiate(prefab, SelectedObject);
      obj.SetActive(true);
      obj.transform.position = view.transform.position;
      obj.transform.rotation = view.transform.rotation;
      ResetColliders(obj, originalPrefab);
      SetExtraInfo(obj, "", data);
      Objects.Add(new(name, view.m_syncInitialScale, data));
      if (view == views.First() || Configuration.AllSnapPoints)
        AddSnapPoints(obj);
    }
    ZNetView.m_forceDisableInit = false;
    CountObjects();
    Rotating.UpdatePlacementRotation(SelectedObject);
  }


  public ObjectSelection(Terminal terminal, Blueprint bp, Vector3 scale)
  {
    SelectedObject = new GameObject();
    // Prevents children from disappearing.
    SelectedObject.SetActive(false);
    SelectedObject.name = bp.Name;
    SelectedObject.transform.localScale = scale;
    SelectedObject.transform.position = Helper.GetPlayer().transform.position;
    var piece = SelectedObject.AddComponent<Piece>();
    piece.m_name = bp.Name;
    piece.m_description = bp.Description;
    if (piece.m_description == "")
      ExtraDescription = "Center: " + bp.CenterPiece;
    ZNetView.m_forceDisableInit = true;
    foreach (var item in bp.Objects)
    {
      try
      {
        var obj = HammerHelper.SafeInstantiate(item.Prefab, SelectedObject);
        obj.SetActive(true);
        obj.transform.localPosition = item.Pos;
        obj.transform.localRotation = item.Rot;
        obj.transform.localScale = item.Scale;
        ZDOData data = new(item.Data);
        SetExtraInfo(obj, item.ExtraInfo, data);
        Objects.Add(new SelectedObject(item.Prefab, obj.GetComponent<ZNetView>()?.m_syncInitialScale ?? false, data));

      }
      catch (InvalidOperationException e)
      {
        HammerHelper.AddMessage(terminal, $"Warning: {e.Message}");
      }
    }
    foreach (var position in bp.SnapPoints)
    {
      SnapObj.SetActive(false);
      var snapObj = UnityEngine.Object.Instantiate(SnapObj, SelectedObject.transform);
      snapObj.transform.localPosition = position;
    }
    piece.m_clipEverything = HammerHelper.CountSnapPoints(SelectedObject) == 0;
    ZNetView.m_forceDisableInit = false;
    Scaling.Get().SetScale(SelectedObject.transform.localScale);
    Helper.GetPlayer().SetupPlacementGhost();
  }

  public void Mirror()
  {
    var i = 0;
    foreach (Transform tr in SelectedObject.transform)
    {
      var prefab = i < Objects.Count ? Objects[i].Prefab : "";
      i += 1;
      if (HammerHelper.IsSnapPoint(tr.gameObject))
      {
        prefab = "";
        i -= 1;
      }
      tr.localPosition = new(tr.localPosition.x, tr.localPosition.y, -tr.localPosition.z);

      var angles = tr.localEulerAngles;
      angles = new(angles.x, -angles.y, angles.z);
      if (Configuration.MirrorFlip.Contains(prefab))
        angles.y += 180;
      tr.localRotation = Quaternion.Euler(angles);
    }
    Helper.GetPlayer().SetupPlacementGhost();
  }
  public override void Postprocess(Vector3? scale)
  {
    if (Objects.Count == 1)
    {

      Postprocess(SelectedObject, GetData());
      HammerHelper.EnsurePiece(SelectedObject);
      if (HammerHelper.CountSnapPoints(SelectedObject) == 0)
      {
        SnapObj.SetActive(false);
        UnityEngine.Object.Instantiate(SnapObj, SelectedObject.transform);
      }
    }
    else
    {
      var i = 0;
      foreach (Transform tr in SelectedObject.transform)
      {
        if (HammerHelper.IsSnapPoint(tr.gameObject)) continue;
        Postprocess(tr.gameObject, GetData(i++));
      }
    }
    base.Postprocess(scale);
  }

  private void CountObjects()
  {
    if (Objects.Count < 2) return;
    SelectedObject.name = $"Multiple ({HammerHelper.CountActiveChildren(SelectedObject)})";
    var piece = SelectedObject.GetComponent<Piece>();
    if (!piece) piece = SelectedObject.AddComponent<Piece>();
    piece.m_clipEverything = HammerHelper.CountSnapPoints(SelectedObject) == 0;
    piece.m_name = SelectedObject.name;
    Dictionary<string, int> counts = Objects.GroupBy(obj => obj.Prefab).ToDictionary(kvp => kvp.Key, kvp => kvp.Count());
    var topKeys = counts.OrderBy(kvp => kvp.Value).Reverse().ToArray();
    if (topKeys.Length <= 5)
      ExtraDescription = string.Join("\n", topKeys.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
    else
    {
      ExtraDescription = string.Join("\n", topKeys.Take(4).Select(kvp => $"{kvp.Key}: {kvp.Value}"));
      ExtraDescription += $"\n{topKeys.Length - 4} other types: {topKeys.Skip(4).Sum(kvp => kvp.Value)}";
    }
  }
  private List<GameObject> AddSnapPoints(GameObject obj)
  {
    List<GameObject> added = [];
    // Null reference exception is sometimes thrown, no idea why but added some checks.
    if (!SelectedObject || !obj || !SnapObj) return added;
    foreach (Transform tr in obj.transform)
    {
      if (!tr || !HammerHelper.IsSnapPoint(tr.gameObject)) continue;
      SnapObj.SetActive(false);
      var snapObj = UnityEngine.Object.Instantiate(SnapObj, SelectedObject.transform);
      snapObj.transform.position = tr.position;
      added.Add(snapObj);
    }
    return added;
  }
  public override ZDOData GetData(int index = 0)
  {
    if (Objects.Count <= index) throw new InvalidOperationException("Invalid index.");
    return Objects[index].Data;
  }
  public override int GetPrefab(int index = 0)
  {
    if (Objects.Count <= index) throw new InvalidOperationException("Invalid index.");
    return Objects[index].Prefab.GetStableHashCode();
  }
  public override bool IsScalingSupported() => Objects.All(obj => obj.Scalable);
  public override void UpdateZDOs(Action<ZDOData> action)
  {
    Objects.ForEach(obj => action(obj.Data));
  }
  public override GameObject GetPrefab(GameObject obj)
  {
    if (Objects.Count == 1)
    {
      var name = Utils.GetPrefabName(obj);
      DataHelper.Init(name, obj.transform, GetData(0));
      return ZNetScene.instance.GetPrefab(name);
    }
    var dummy = new GameObject
    {
      name = "Blueprint"
    };
    dummy.AddComponent<Piece>();
    return dummy;
  }
  public override void AfterPlace(GameObject obj)
  {
    if (Objects.Count == 1)
    {

      var view = obj.GetComponent<ZNetView>();
      // Hoe adds pieces too.
      if (!view) return;
      view.m_body?.WakeUp();
      PostProcessPlaced(obj);
      Undo.CreateObject(obj);
    }
    else
    {
      HandleMultiple(SelectedObject);
      UnityEngine.Object.Destroy(obj);
    }
  }
  private void HandleMultiple(GameObject ghost)
  {
    Undo.StartTracking();
    var children = HammerHelper.GetChildren(ghost);
    ValheimRAFT.Handle(children);
    for (var i = 0; i < children.Count; i++)
    {
      var ghostChild = children[i];
      var name = Utils.GetPrefabName(ghostChild);
      if (ValheimRAFT.IsRaft(name)) continue;
      var prefab = ZNetScene.instance.GetPrefab(name);
      if (prefab)
      {
        DataHelper.Init(name, ghostChild.transform, GetData(i));
        var childObj = UnityEngine.Object.Instantiate(prefab, ghostChild.transform.position, ghostChild.transform.rotation);
        PostProcessPlaced(childObj);
      }
    }
    Undo.StopTracking();
  }

  public GameObject AddObject(GameObject baseObj, Vector3 pos)
  {
    if (Objects.Count == 1)
      ToMulti();
    var obj = HammerHelper.SafeInstantiate(baseObj, SelectedObject);
    obj.SetActive(true);
    obj.transform.rotation = baseObj.transform.rotation;
    obj.transform.localPosition = pos;
    if (Configuration.AllSnapPoints) AddSnapPoints(obj);
    Objects.Add(new SelectedObject(Objects[0].Prefab, Objects[0].Scalable, Objects[0].Data));
    return obj;
  }
  private void ToMulti()
  {
    var obj = SelectedObject;
    SelectedObject = new GameObject();
    // Prevents children from disappearing.
    SelectedObject.SetActive(false);
    SelectedObject.transform.position = obj.transform.position;
    SelectedObject.transform.rotation = obj.transform.rotation;
    obj.transform.parent = SelectedObject.transform;
    obj.transform.localScale = Vector3.one;
    obj.SetActive(true);
    AddSnapPoints(obj);
    if (obj.TryGetComponent<Piece>(out var piece))
    {
      var name2 = Utils.GetPrefabName(obj);
      var prefab = ZNetScene.instance.GetPrefab(name2);
      if (prefab && !prefab.GetComponent<Piece>())
        UnityEngine.Object.Destroy(piece);
    }
  }
  public void RemoveObject(GameObject obj)
  {
    if (Objects.Count == 1)
      return;
    obj.SetActive(false);
    obj.transform.parent = null;
    UnityEngine.Object.Destroy(obj);
    Objects.RemoveAt(Objects.Count - 1);
    if (Objects.Count == 1)
      ToSingle();
  }
  private void ToSingle()
  {
    var obj = SelectedObject.transform.GetChild(0).gameObject;
    obj.SetActive(false);
    HammerHelper.EnsurePiece(obj);
    obj.transform.parent = null;
    UnityEngine.Object.Destroy(SelectedObject);
    SelectedObject = obj;
    Objects = Objects.Take(1).ToList();
  }
}