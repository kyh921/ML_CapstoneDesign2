using UnityEngine;

[DisallowMultipleComponent]
public class ReceiverNode : MonoBehaviour
{
    [Tooltip("있으면 이 위치를 수신점으로 사용, 없으면 자동 계산")]
    public Transform rxAnchor;

    [Header("Auto Anchor (fallback)")]
    public float upOffset = 1.0f;   // 중심에서 위로 띄우기(건물 중심이 바닥이면  높여줌)
    public bool drawGizmo = true;

    public Vector3 GetRxPosition()
    {
        if (rxAnchor) return rxAnchor.position;

        // Collider 우선, 없으면 MeshRenderer로 bounds 계산
        var col = GetComponentInParent<Collider>();
        if (col)
            return col.bounds.center + Vector3.up * upOffset;

        var r = GetComponentInParent<MeshRenderer>();
        if (r)
            return r.bounds.center + Vector3.up * upOffset;

        return transform.position + Vector3.up * upOffset;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!drawGizmo) return;
        Gizmos.color = Color.yellow;
        Vector3 p = GetRxPosition();
        Gizmos.DrawSphere(p, 0.4f);
    }
#endif
}
