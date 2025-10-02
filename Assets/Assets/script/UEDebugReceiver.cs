using UnityEngine;


public class UEDebugReceiver : MonoBehaviour
{
    RadioTransceiver rt;        // 부모에서 찾음
    Renderer ren;
    Color orig;
    bool hasBaseColor, hasColor;

    void Awake()
    {
        // 부모/자식 어디서든 RadioTransceiver 찾기 (우선 부모)
        rt = GetComponent<RadioTransceiver>();
        if (!rt) rt = GetComponentInParent<RadioTransceiver>();
        if (!rt) rt = GetComponentInChildren<RadioTransceiver>();

        ren = GetComponent<Renderer>();
        if (!ren) ren = GetComponentInChildren<Renderer>();

        if (ren && ren.sharedMaterial)
        {
            hasBaseColor = ren.sharedMaterial.HasProperty("_BaseColor");
            hasColor     = ren.sharedMaterial.HasProperty("_Color");

            // 원래 색 저장
            var mpb = new MaterialPropertyBlock();
            ren.GetPropertyBlock(mpb);
            if (hasBaseColor) orig = mpb.GetColor("_BaseColor");
            else if (hasColor) orig = mpb.GetColor("_Color");
            else orig = Color.white;
        }

        if (rt) rt.OnReceive += OnRx;
        // 태그 보장
        gameObject.tag = "UEViz";
    }

    void OnDestroy(){ if (rt != null) rt.OnReceive -= OnRx; }

    void OnRx(int srcId, byte[] payload, float rssiDbm)
    {
        if (!ren) return;
        StopAllCoroutines();
        StartCoroutine(Blink());
    }

    System.Collections.IEnumerator Blink()
    {
        var mpb = new MaterialPropertyBlock();
        ren.GetPropertyBlock(mpb);

        if (hasBaseColor) mpb.SetColor("_BaseColor", Color.cyan);
        else if (hasColor) mpb.SetColor("_Color", Color.cyan);
        ren.SetPropertyBlock(mpb);

        yield return new WaitForSeconds(0.2f);

        if (hasBaseColor) mpb.SetColor("_BaseColor", orig);
        else if (hasColor) mpb.SetColor("_Color", orig);
        ren.SetPropertyBlock(mpb);
    }
}
