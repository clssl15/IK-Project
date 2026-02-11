using UnityEngine;
using StarterAssets;
using UnityEngine.Animations.Rigging;

public class EquipWeapon : MonoBehaviour
{
    [Header("Current Weapon Transform Settings")]
    [SerializeField]
    private Transform equipPos;
    [SerializeField]
    private Transform aimingPos;
    [SerializeField]
    private Transform firePos;

    [Header("Right Hand Target")]
    [SerializeField]
    private TwoBoneIKConstraint rightHandIK;
    [SerializeField]
    private Transform rightHandTarget;

    [Header("Left Hand Target")]
    [SerializeField]
    private TwoBoneIKConstraint leftHandIK;
    [SerializeField]
    private Transform leftHandTarget;

    [Header("Smooth Settings")]
    [SerializeField]
    private float toAimSpeed = 10f;
    [SerializeField]
    private float toFireSpeed = 40f;
    [SerializeField]
    private float recoilDuration = 0.1f; // 반동 상태 유지 시간

    private StarterAssetsInputs _input;
    private Animator playerAnimator;

    private Weapon _currentWeapon;

    private Transform _targetTransform;

    private float _currentRecoilTimer = 0f; // 현재 남은 반동 시간

    private void Start()
    {
        _input = GetComponent<StarterAssetsInputs>();
        playerAnimator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (_currentWeapon == null) return;

        if (_input.fire && _input.aim && _currentWeapon.TryShoot())
        {
            _currentRecoilTimer = recoilDuration;
        }

        float smoothSpeed = default;
        if (_currentRecoilTimer > 0)
        {
            _targetTransform = firePos;
            smoothSpeed = toFireSpeed;

            _currentRecoilTimer -= Time.deltaTime;
        }
        else
        {
            _targetTransform = _input.aim ? aimingPos : equipPos;
            smoothSpeed = toAimSpeed;
        }

        UpdateAimingState(_input.aim);

        // 무기 위치 갱신
        SmoothUpdateWeaponTransform(smoothSpeed);

        // IK 갱신
        leftHandIK.weight = _input.aim ? 1f : 0f;
        SetIKPos();
    }

    /// <summary>
    /// 기존 SetWeaponPos 대신 이 함수를 사용
    /// 부드러운 전환을 위함
    /// </summary>
    private void SmoothUpdateWeaponTransform(float speed)
    {
        if (_targetTransform == null) return;

        if (_currentWeapon.transform.parent != _targetTransform)
        {
            _currentWeapon.transform.SetParent(_targetTransform);
        }

        _currentWeapon.transform.position = Vector3.Lerp(
            _currentWeapon.transform.position,
            _targetTransform.position,
            Time.deltaTime * speed
        );

        _currentWeapon.transform.rotation = Quaternion.Slerp(
            _currentWeapon.transform.rotation,
            _targetTransform.rotation,
            Time.deltaTime * speed
        );
    }

    /// <summary>
    /// 무기 위치를 바꿔주는 함수
    /// </summary>
    /// <param name="newPos"></param>
    void SetWeaponPos(Transform newPos)
    {
        _currentWeapon.transform.SetParent(newPos.transform);
        _currentWeapon.transform.position = newPos.position;
        _currentWeapon.transform.rotation = newPos.rotation;
    }
    private void UpdateAimingState(bool isAiming)
    {
        switch (_currentWeapon.Type)
        {
            case WeaponType.pistol:
                playerAnimator.SetBool("PistolAim", isAiming);
                break;
            case WeaponType.assault:
                playerAnimator.SetBool("AssaultAim", isAiming);
                break;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!_input.interact) return;

        if (_input.aim) return;

        if (other.CompareTag("Weapon"))
        {
            Equip(other.GetComponent<Weapon>());
        }
    }

    void Equip(Weapon weapon)
    {
        if (_currentWeapon != null)
        {
            // 현재 장착중인 무기 떨어트리기
            _currentWeapon.transform.parent = null;
            _currentWeapon.transform.position = transform.position + transform.forward;
        }

        _currentWeapon = weapon;

        rightHandIK.weight = 1f;

        playerAnimator.SetBool("HasWeapon", true);
    }

    void SetIKPos()
    {
        rightHandTarget.position = _currentWeapon.RightHandPos.position;
        rightHandTarget.rotation = _currentWeapon.RightHandPos.rotation;

        leftHandTarget.position = _currentWeapon.LeftHandPos.position;
        leftHandTarget.rotation = _currentWeapon.LeftHandPos.rotation;
    }
}
