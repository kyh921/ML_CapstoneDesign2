using UnityEngine;

/// <summary>
/// 간단한 전파 모델: FSPL + 경로손실 지수 + 건물 차폐.
/// </summary>
public class RadioLinkModel : MonoBehaviour
{
    [Header("RF Parameters")]
    public float frequencyMHz = 2400f;
    public float pathLossExponent = 2.6f;        // 도심: 2.4~3.2
    public float obstacleLossPerHitDb = 12f;     // 건물 관통시 감쇠(dB)
    public LayerMask obstacleMask;

    /// <summary>
    /// 송신 전력(txPowerDbm)과 두 위치(txPos→rxPos)를 받아 RSSI(dBm) 계산
    /// </summary>
    public float EstimateRssiDbm(Vector3 txPos, float txPowerDbm, Vector3 rxPos)
    {
        float d = Mathf.Max(0.1f, Vector3.Distance(txPos, rxPos));
        // 기본 FSPL 식: PL(dB) = 20log10(d) + 20log10(f_MHz) + 32.44
        float pl = 20f * Mathf.Log10(d) + 20f * Mathf.Log10(frequencyMHz) + 32.44f;

        // 경로손실 지수 보정
        float n = Mathf.Max(1.0f, pathLossExponent);
        if (Mathf.Abs(n - 2.0f) > 1e-3f)
        {
            float baseTerm = 20f * Mathf.Log10(d);
            float rest = pl - baseTerm;
            pl = baseTerm * (n / 2.0f) + rest;
        }

        // 차폐 감쇠
        Vector3 dir = (rxPos - txPos).normalized;
        float dist = Vector3.Distance(txPos, rxPos);
        int hits = Physics.RaycastAll(txPos, dir, dist, obstacleMask, QueryTriggerInteraction.Ignore).Length;
        pl += hits * obstacleLossPerHitDb;

        return txPowerDbm - pl;  // RSSI(dBm)
    }
}
