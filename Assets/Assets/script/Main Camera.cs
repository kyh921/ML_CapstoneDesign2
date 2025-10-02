using UnityEngine;

public class FollowCam : MonoBehaviour
{
    public Transform Target;
    public Vector3 offset = new Vector3(0, 5, -10);  // 드론 기준 위치
    public float followSpeed = 5f;                   // 따라가는 속도
    public float lookSpeed = 7f;                     // 시선 전환 속도

    void LateUpdate()
    {
        if (!Target) return;

        // 목표 위치
        Vector3 desiredPos = Target.position + Target.TransformDirection(offset);
        transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * followSpeed);

        // 목표 바라보기
        Quaternion lookRot = Quaternion.LookRotation(Target.position - transform.position, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * lookSpeed);
    }
}
