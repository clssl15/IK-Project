using UnityEngine;
using StarterAssets;

/// <summary>
/// Resident Evil (over-the-shoulder) camera.
/// Idle: character sits slightly left of center, closer than a standard third-person follow.
/// Aim: zoom in (closer offset + lower FOV).
/// </summary>
[RequireComponent(typeof(Camera))]
public class OverShoulderCamera : MonoBehaviour
{
    [Header("Idle")]
    [SerializeField]
    private Vector3 idleOffset = new Vector3(0.48f, -0.12f, -2.2f);
    [SerializeField]
    private float idleFov = 55f;

    [Header("Aim Zoom")]
    [SerializeField]
    private Vector3 aimOffset = new Vector3(0.42f, 0.02f, -1.1f);
    [SerializeField]
    private float aimFov = 38f;

    [Header("Blend")]
    [SerializeField]
    private float blendSpeed = 12f;

    [Header("Collision")]
    [SerializeField]
    private float collisionRadius = 0.18f;
    [SerializeField]
    private LayerMask collisionLayers = ~0;

    private StarterAssetsInputs _input;
    private Camera _camera;
    private int _collisionMask;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        _input = GetComponentInParent<StarterAssetsInputs>();
        _collisionMask = collisionLayers & ~LayerMask.GetMask("Player");
    }

    private void LateUpdate()
    {
        bool aiming = _input != null && _input.aim;
        Vector3 targetOffset = aiming ? aimOffset : idleOffset;
        float targetFov = aiming ? aimFov : idleFov;

        ApplyOffset(targetOffset);
        _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, targetFov, Time.deltaTime * blendSpeed);
    }

    private void ApplyOffset(Vector3 desiredLocal)
    {
        Transform pivot = transform.parent;
        if (pivot == null) return;

        Vector3 origin = pivot.position;
        Vector3 desiredWorld = pivot.TransformPoint(desiredLocal);
        Vector3 toCam = desiredWorld - origin;
        float dist = toCam.magnitude;

        if (dist > 0.001f &&
            Physics.SphereCast(origin, collisionRadius, toCam / dist, out RaycastHit hit, dist, _collisionMask, QueryTriggerInteraction.Ignore))
        {
            desiredWorld = origin + (toCam / dist) * Mathf.Max(hit.distance - collisionRadius, 0.05f);
        }

        transform.position = Vector3.Lerp(transform.position, desiredWorld, Time.deltaTime * blendSpeed);
    }
}
