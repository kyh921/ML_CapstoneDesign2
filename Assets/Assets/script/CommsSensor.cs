using UnityEngine;

public class CommsSensor : MonoBehaviour
{
    public float scanInterval = 0.25f;     // seconds
    public bool onlyUE = true;             // 수신 후보를 UE로 제한할지 여부

    [Header("Read-only (debug)")]
    public int linkableCount;
    public float bestRssiDbm;
    public RadioTransceiver bestTarget;

    RadioTransceiver self;
    float t;

    void Awake() { self = GetComponent<RadioTransceiver>(); }
    void Update()
    {
        t += Time.deltaTime;
        if (t >= scanInterval)
        {
            t = 0f;
            Scan();
        }
    }

    void Scan()
    {
        linkableCount = 0;
        bestRssiDbm = -999f;
        bestTarget = null;

        if (self == null) return;

        foreach (var n in RadioTransceiver.All)
        {
            if (n == self) continue;
            if (n.channel != self.channel) continue;
            if (onlyUE && n.role != NodeRole.UE) continue;

            float rssi = self.EstimateRssiTo(n);
            if (rssi >= n.rxSensitivityDbm)
            {
                linkableCount++;
                if (rssi > bestRssiDbm)
                {
                    bestRssiDbm = rssi;
                    bestTarget = n;
                }
            }
        }
    }
}
