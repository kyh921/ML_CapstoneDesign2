using System;
using System.Collections.Generic;
using UnityEngine;

public enum NodeRole { Drone, UE }

[DisallowMultipleComponent]
public class RadioTransceiver : MonoBehaviour
{
    // === Static registry for quick lookup ===
    private static readonly HashSet<RadioTransceiver> s_all = new HashSet<RadioTransceiver>();
    public static IReadOnlyCollection<RadioTransceiver> All => s_all;

    [Header("Identity / Role")]
    public NodeRole role = NodeRole.Drone;
    [SerializeField] private int _nodeId = 0;
    public int NodeId => _nodeId;

    [Header("Radio Params")]
    public int channel = 1;
    public float txPowerDbm = 10f;     // 10 dBm = 10 mW
    public float rxSensitivityDbm = -85f;
    public float dataRateMbps = 6f;

    [Header("Runtime (readonly)")]
    public int sentPackets = 0;
    public int recvPackets = 0;

    public event Action<int, byte[], float> OnReceive; // (srcId, payload, rssiDbm)

    RadioLinkModel linkModel;

    void OnEnable()
    {
        s_all.Add(this);
        if (_nodeId == 0) _nodeId = GetInstanceID();
        if (!linkModel) linkModel = FindAnyObjectByType<RadioLinkModel>();
    }
    void OnDisable() { s_all.Remove(this); }

    public float EstimateRssiTo(RadioTransceiver other)
    {
        if (!linkModel || other == null) return -999f;

        Vector3 txPos = this.GetAntennaPosition();
        Vector3 rxPos = other.GetAntennaPosition();

        return linkModel.EstimateRssiDbm(txPos, txPowerDbm, rxPos);
    }

    /// <summary> Convenience: 모든 노드 중 자신을 제외하고 channel이 같은 대상만 나열 </summary>
    public IEnumerable<RadioTransceiver> NeighborsSameChannel()
    {
        foreach (var n in s_all)
            if (n != this && n.channel == this.channel) yield return n;
    }

    // === Minimal send API (no NetMedium yet) ===
    public void SendBroadcast(byte[] payload)
    {
        sentPackets++;
        // 더미 전달: 실제 실제 전파/충돌 모델은 NetMedium에 연결해 구현
        foreach (var rx in NeighborsSameChannel())
        {
            float rssi = EstimateRssiTo(rx);
            if (rssi >= rx.rxSensitivityDbm)
            {
                rx.recvPackets++;
                rx.OnReceive?.Invoke(this.NodeId, payload, rssi);
            }
        }
    }

    Vector3 GetAntennaPosition()
    {
        var rn = GetComponent<ReceiverNode>();
        return rn ? rn.GetRxPosition() : transform.position;
    }


    public void SendUnicast(int dstNodeId, byte[] payload)
    {
        sentPackets++;
        foreach (var rx in NeighborsSameChannel())
        {
            if (rx.NodeId != dstNodeId) continue;
            float rssi = EstimateRssiTo(rx);
            if (rssi >= rx.rxSensitivityDbm)
            {
                rx.recvPackets++;
                rx.OnReceive?.Invoke(this.NodeId, payload, rssi);
            }
        }
    }
}
