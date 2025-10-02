#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class UEMarkerAutoPlacer
{
    [MenuItem("Tools/UE/Place Markers For All UE Buildings")]
    public static void PlaceMarkersForAll()
    {
        int made = 0, updated = 0;
        foreach (var rt in Object.FindObjectsOfType<RadioTransceiver>())
        {
            if (rt.role != NodeRole.UE) continue;

            // ReceiverNode에서 위치 얻기
            var rn = rt.GetComponent<ReceiverNode>();
            Vector3 pos = rn ? rn.GetRxPosition() : rt.transform.position;

            // 부모(=건물) 아래에 "UE_Marker"가 이미 있나?
            Transform parent = rt.transform; // UE가 붙은 그 오브젝트(건물 그룹)
            Transform marker = parent.Find("UE_Marker");
            if (!marker)
            {
                // 새 구체 생성
                var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.name = "UE_Marker";
                go.tag = "UEViz"; // DemandArea에서 제외되도록
                marker = go.transform;
                marker.SetParent(parent, true);
                Object.DestroyImmediate(go.GetComponent<Collider>());

                // 기본 비주얼 크기/머티리얼
                marker.localScale = Vector3.one * 0.8f;
                var ren = go.GetComponent<Renderer>();
                if (ren && ren.sharedMaterial) ren.sharedMaterial.EnableKeyword("_EMISSION");

                // 디버그 수신기(부모의 RadioTransceiver 이벤트를 사용)
                var rx = go.AddComponent<UEDebugReceiver>();
                // UEDebugReceiver는 부모/자식에서 Renderer/Transceiver를 자동 탐색하도록 작성해둘 것
                made++;
            }
            else
            {
                updated++;
            }

            // 위치/방향 업데이트
            marker.position = pos + Vector3.up * 10.0f; // 1m 위로 이동
            marker.rotation = Quaternion.identity;
        }
        Debug.Log($"[UE Marker] Created={made}, Updated={updated}");
    }
}
#endif
