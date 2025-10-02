using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DroneController : MonoBehaviour
{
    [Header("Speed / Accel Limits")]
    public float maxHorizontalSpeed = 15f;
    public float maxClimbRate = 5f;         // m/s up/down
    public float horizontalAccel = 30f;     // m/s^2
    public float yawRateDegPerSec = 120f;

    [Header("Flight Envelope")]
    public float minAltitude = 10f;
    public float maxAltitude = 200f;
    public Vector2 xBounds = new Vector2(-500f, 500f);
    public Vector2 zBounds = new Vector2(-500f, 500f);

    Rigidbody rb;
    Vector3 desiredVelLocal;   // (forward, right, up) in local frame
    float desiredYawRate;      // deg/s

    void Awake() { rb = GetComponent<Rigidbody>(); rb.useGravity = false; rb.drag = 1.0f; }

    /// <summary>
    /// inputLocal.x = strafe (right +), inputLocal.y = climb (up +), inputLocal.z = forward (+)
    /// yawRateCmdDeg = desired yaw rate in deg/s
    /// </summary>
    public void SetCommand(Vector3 inputLocal, float yawRateCmdDeg)
    {
        // Clamp inputs to limits
        float fwd = Mathf.Clamp(inputLocal.z, -maxHorizontalSpeed,  maxHorizontalSpeed);
        float str = Mathf.Clamp(inputLocal.x, -maxHorizontalSpeed,  maxHorizontalSpeed);
        float up  = Mathf.Clamp(inputLocal.y, -maxClimbRate,        maxClimbRate);

        desiredVelLocal = new Vector3(str, up, fwd);
        desiredYawRate = Mathf.Clamp(yawRateCmdDeg, -yawRateDegPerSec, yawRateDegPerSec);
    }

    void FixedUpdate()
    {
        // Rotate (yaw only here; pitch/roll은 속도 추종으로 암묵적으로 가정)
        float yawDelta = desiredYawRate * Mathf.Deg2Rad * Time.fixedDeltaTime;
        transform.rotation = Quaternion.AngleAxis(yawDelta * Mathf.Rad2Deg, Vector3.up) * transform.rotation;

        // Local desired velocity -> world
        Vector3 desiredWorldVel = transform.TransformDirection(new Vector3(desiredVelLocal.x, 0f, desiredVelLocal.z));
        Vector3 currentWorldVel = rb.velocity;
        Vector3 horizVel = new Vector3(currentWorldVel.x, 0f, currentWorldVel.z);

        // Horizontal acceleration toward desired
        Vector3 horizErr = desiredWorldVel - horizVel;
        Vector3 horizAcc = Vector3.ClampMagnitude(horizErr.normalized * horizontalAccel, horizontalAccel);
        rb.AddForce(horizAcc, ForceMode.Acceleration);

        // Vertical (climb)
        float climbErr = desiredVelLocal.y - currentWorldVel.y;
        float climbAcc = Mathf.Clamp(climbErr * horizontalAccel, -horizontalAccel, horizontalAccel);
        rb.AddForce(new Vector3(0f, climbAcc, 0f), ForceMode.Acceleration);

        // Soft clamp altitude
        float y = rb.position.y;
        if (y < minAltitude) rb.position = new Vector3(rb.position.x, minAltitude, rb.position.z);
        if (y > maxAltitude) rb.position = new Vector3(rb.position.x, maxAltitude, rb.position.z);

        // Soft clamp XY bounds
        float x = Mathf.Clamp(rb.position.x, xBounds.x, xBounds.y);
        float z = Mathf.Clamp(rb.position.z, zBounds.x, zBounds.y);
        rb.position = new Vector3(x, rb.position.y, z);
    }

    public Vector3 CurrentVelocity() => rb.velocity;
    public float Altitude() => rb.position.y;
}
