using UnityEngine;

#if UNITY_MLAGENTS
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
#endif

[RequireComponent(typeof(DroneController))]
[RequireComponent(typeof(RadioTransceiver))]
[RequireComponent(typeof(CommsSensor))]
public class DroneAgent : 
#if UNITY_MLAGENTS
    Agent
#else
    MonoBehaviour
#endif
{
    public float yawCmdDegPerSec = 0f;    // 정책으로 학습하고 싶으면 액션에 추가하면 됨.
    DroneController ctrl;
    RadioTransceiver radio;
    CommsSensor sensor;

    Vector3 prevPos;
    public float LastStepDistance { get; private set; }

    void Awake()
    {
        ctrl = GetComponent<DroneController>();
        radio = GetComponent<RadioTransceiver>();
        sensor = GetComponent<CommsSensor>();
        prevPos = transform.position;
    }

#if UNITY_MLAGENTS
    public override void OnEpisodeBegin()
    {
        // 간단 리셋: 위치 무작위, 속도 초기화
        Vector3 p = new Vector3(Random.Range(-200f, 200f), Random.Range(40f, 120f), Random.Range(-200f, 200f));
        transform.position = p;
        transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        prevPos = transform.position;
        LastStepDistance = 0f;
    }

    public override void CollectObservations(VectorSensor s)
    {
        // 최소 관측 (필요시 CoverageGrid 패치/이웃 드론 정보 추가)
        s.AddObservation(transform.position / 500f);         // roughly normalized
        s.AddObservation(ctrl.Altitude() / 200f);
        s.AddObservation(ctrl.CurrentVelocity() / 20f);
        s.AddObservation(sensor.linkableCount / 50f);        // 정규화 예시
        s.AddObservation(Mathf.Clamp((sensor.bestRssiDbm + 120f) / 60f, 0f, 1f)); // (-120..-60dBm)->0..1
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var a = actions.ContinuousActions;
        // a[0] = strafe (-1..1), a[1] = climb (-1..1), a[2] = forward (-1..1)
        Vector3 cmdLocal = new Vector3(a[0], a[1], a[2]);
        // 스케일링: DroneController가 속도/상승 제한 내에서 처리
        ctrl.SetCommand(new Vector3(cmdLocal.x * ctrl.maxHorizontalSpeed,
                                    cmdLocal.y * ctrl.maxClimbRate,
                                    cmdLocal.z * ctrl.maxHorizontalSpeed),
                        yawCmdDegPerSec);

        LastStepDistance = Vector3.Distance(prevPos, transform.position);
        prevPos = transform.position;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var a = actionsOut.ContinuousActions;
        a[0] = Input.GetAxis("Horizontal");                 // A/D
        a[2] = Input.GetAxis("Vertical");                   // W/S
        a[1] = (Input.GetKey(KeyCode.E) ? 1f : 0f) + (Input.GetKey(KeyCode.Q) ? -1f : 0f);
    }
#else
    // Fallback 모드 (ML-Agents 미설치 시 키보드 조종)
    void Update()
    {
        float strafe = Input.GetAxis("Horizontal");        // A/D
        float forward = Input.GetAxis("Vertical");         // W/S
        float climb = 0f; if (Input.GetKey(KeyCode.E)) climb += 1f; if (Input.GetKey(KeyCode.Q)) climb -= 1f;

        ctrl.SetCommand(new Vector3(strafe * ctrl.maxHorizontalSpeed,
                                    climb * ctrl.maxClimbRate,
                                    forward * ctrl.maxHorizontalSpeed),
                        yawCmdDegPerSec);

        LastStepDistance = Vector3.Distance(prevPos, transform.position);
        prevPos = transform.position;
    }
#endif
}
