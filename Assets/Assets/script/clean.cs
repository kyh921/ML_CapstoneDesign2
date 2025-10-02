using UnityEngine;

public class DemandCleanup : MonoBehaviour
{
    public Transform[] roots; // Buildings 같은 상위

    [ContextMenu("Cleanup Demand/Receiver on children (keep on group roots)")]
    public void Cleanup()
    {
        foreach (var root in roots)
        {
            if (!root) continue;
            var areas = root.GetComponentsInChildren<DemandArea>(true);
            foreach (var da in areas)
            {
                // 그룹 루트에만 남기고, 그 ‘자식’에 달린 건 제거
                if (da.transform != root && da.transform.parent != null)
                {
                    // 자식인데 그룹 루트가 아님 → 제거
                    DestroyImmediate(da);
                }
            }

            var rxs = root.GetComponentsInChildren<ReceiverNode>(true);
            foreach (var rn in rxs)
            {
                if (rn.transform != root && rn.transform.parent != null)
                {
                    DestroyImmediate(rn);
                }
            }
        }
        Debug.Log("[Cleanup] Done.");
    }
}
