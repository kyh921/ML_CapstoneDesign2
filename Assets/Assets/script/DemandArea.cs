using System.Linq;
using UnityEngine;

public enum AreaKind { Building, Road }

[DisallowMultipleComponent]
public class DemandArea : MonoBehaviour
{
    [Header("Type & Demand")]
    public AreaKind kind = AreaKind.Building;
    public int demand;
    public bool covered = false;

    [Header("Colors")]
    public Color colorLow = Color.yellow;                 // 0~14
    public Color colorMid = new Color(1f, 0.55f, 0f, 1f); // 15~30
    public Color colorHigh = Color.red;                   // 30+
    public Color colorCovered = Color.green;              // Ŀ�� ��

    [Header("Exclusions")]
    public string excludeTag = "UEViz";

    Renderer[] rends;

    void Awake() { EnsureInit(); }
    void Start()
    {
        // SetupNow���� �ٷ� ApplyColor�� ȣ���ص� �����ϵ���
        if (demand == 0 && kind == AreaKind.Building) demand = Random.Range(10, 51);
        if (demand == 0 && kind == AreaKind.Road) demand = Random.Range(0, 31);
        ApplyColor();
    }

    void EnsureInit()
    {
        if (rends == null || rends.Length == 0)
            rends = GetComponentsInChildren<Renderer>(true);
    }

    public void RandomizeDemand()
    {
        demand = (kind == AreaKind.Building)
            ? Random.Range(10, 51)   // [10,50]
            : Random.Range(0, 31);   // [0,30]
    }

    public void SetCovered(bool on)
    {
        covered = on;
        ApplyColor();
    }

    public void ApplyColor()
    {
        EnsureInit();
        if (rends == null || rends.Length == 0) return;

        foreach (var r in rends)
        {
            if (!r) continue;
            // 🔴 UEViz 태그는 건드리지 않음
            if (!string.IsNullOrEmpty(excludeTag) && r.CompareTag(excludeTag)) continue;

            var mat = r.sharedMaterial;
            if (!mat) continue;

            Color c;
            if (covered) c = colorCovered;
            else c = (demand <= 14) ? colorLow : (demand <= 30) ? colorMid : colorHigh;

            var mpb = new MaterialPropertyBlock();
            r.GetPropertyBlock(mpb);
            if (mat.HasProperty("_BaseColor")) mpb.SetColor("_BaseColor", c);
            else if (mat.HasProperty("_Color")) mpb.SetColor("_Color", c);
            else continue;

            r.SetPropertyBlock(mpb);
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        ApplyColor(); // �ν����Ϳ��� �� �ٲٸ� ��� �ݿ�
    }
#endif
}
