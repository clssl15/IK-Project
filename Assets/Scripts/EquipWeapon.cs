using System.Collections.Generic;
using UnityEngine;
using StarterAssets;
using UnityEngine.Animations.Rigging;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class EquipWeapon : MonoBehaviour
{
    private const int MaxWeapons = 2;

    [Header("Pistol Weapon Transform Settings")]
    [SerializeField]
    private Transform equipPos;
    [SerializeField]
    private Transform aimingPos;
    [SerializeField]
    private Transform firePos;

    [Header("Assault Weapon Transform Settings")]
    [SerializeField]
    private Transform assaultEquipPos;
    [SerializeField]
    private Transform assaultAimingPos;
    [SerializeField]
    private Transform assaultFirePos;

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

    [Header("Pistol Smooth Settings")]
    [SerializeField]
    private float toAimSpeed = 10f;
    [SerializeField]
    private float toFireSpeed = 40f;
    [SerializeField]
    private float recoilDuration = 0.1f; // 반동 상태 유지 시간

    [Header("Assault Recoil Settings")]
    [SerializeField]
    private float assaultToFireSpeed = 35f;
    [SerializeField]
    private float assaultRecoilDuration = 0.045f;

    [Header("Switch Settings")]
    [SerializeField]
    private float switchCooldown = 0.2f;

    private StarterAssetsInputs _input;
    private Animator playerAnimator;

    private Weapon _currentWeapon;
    private readonly List<Weapon> _ownedWeapons = new List<Weapon>(MaxWeapons);
    private int _currentIndex = -1;
    private float _switchCooldownTimer;

    // 현재 장착한 무기 타입에 맞춰 선택된 위치 세트
    private Transform _equipPos;
    private Transform _aimingPos;
    private Transform _firePos;
    private float _toFireSpeed;
    private float _recoilDuration;

    private Transform _targetTransform;

    private float _currentRecoilTimer = 0f; // 현재 남은 반동 시간

    private void Start()
    {
        _input = GetComponent<StarterAssetsInputs>();
        playerAnimator = GetComponent<Animator>();
    }

    private void Update()
    {
        HandleWeaponSwitch();

        if (_currentWeapon == null) return;

        if (_input.fire && _input.aim && _currentWeapon.TryShoot())
        {
            _currentRecoilTimer = _recoilDuration;
        }

        float smoothSpeed = default;
        if (_currentRecoilTimer > 0)
        {
            _targetTransform = _firePos;
            smoothSpeed = _toFireSpeed;

            _currentRecoilTimer -= Time.deltaTime;
        }
        else
        {
            _targetTransform = _input.aim ? _aimingPos : _equipPos;
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
        bool pistolAim = false;
        bool assaultAim = false;

        if (isAiming && _currentWeapon != null)
        {
            switch (_currentWeapon.Type)
            {
                case WeaponType.pistol:
                    pistolAim = true;
                    break;
                case WeaponType.assault:
                    assaultAim = true;
                    break;
            }
        }

        playerAnimator.SetBool("PistolAim", pistolAim);
        playerAnimator.SetBool("AssaultAim", assaultAim);
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
        if (weapon == null) return;
        if (_ownedWeapons.Contains(weapon)) return;

        // 슬롯이 가득 차면 현재 들고 있는 무기만 버리고 새 무기로 교체
        if (_ownedWeapons.Count >= MaxWeapons)
        {
            DropCurrentWeapon();
        }
        else if (_currentWeapon != null)
        {
            HolsterWeapon(_currentWeapon);
        }

        _ownedWeapons.Add(weapon);
        _currentIndex = _ownedWeapons.Count - 1;
        ActivateWeapon(weapon);
    }

    private void HandleWeaponSwitch()
    {
        if (_switchCooldownTimer > 0f)
        {
            _switchCooldownTimer -= Time.deltaTime;
            return;
        }

        if (_ownedWeapons.Count < 2) return;

        float scroll = ReadScrollDelta();
        if (Mathf.Abs(scroll) < 0.1f) return;

        int nextIndex = 1 - _currentIndex;
        SwitchToIndex(nextIndex);
        _switchCooldownTimer = switchCooldown;
    }

    private static float ReadScrollDelta()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            return Mouse.current.scroll.ReadValue().y;
        }
#endif
        return Input.GetAxis("Mouse ScrollWheel");
    }

    private void SwitchToIndex(int index)
    {
        if (index < 0 || index >= _ownedWeapons.Count) return;
        if (index == _currentIndex) return;

        HolsterWeapon(_currentWeapon);
        _currentIndex = index;
        ActivateWeapon(_ownedWeapons[_currentIndex]);
    }

    private void ActivateWeapon(Weapon weapon)
    {
        _currentWeapon = weapon;
        _currentWeapon.gameObject.SetActive(true);
        SetPickupEnabled(_currentWeapon, false);

        SelectWeaponPositions(_currentWeapon.Type);

        Transform spawnPos = (_input != null && _input.aim && _aimingPos != null)
            ? _aimingPos
            : _equipPos;

        _currentWeapon.transform.SetParent(spawnPos);
        _currentWeapon.transform.SetPositionAndRotation(spawnPos.position, spawnPos.rotation);

        _targetTransform = spawnPos;
        _currentRecoilTimer = 0f;

        rightHandIK.weight = 1f;
        playerAnimator.SetBool("HasWeapon", true);
    }

    private void HolsterWeapon(Weapon weapon)
    {
        if (weapon == null) return;

        weapon.transform.SetParent(transform);
        weapon.gameObject.SetActive(false);
    }

    private void DropCurrentWeapon()
    {
        if (_currentWeapon == null) return;

        SetPickupEnabled(_currentWeapon, true);
        _currentWeapon.transform.SetParent(null);
        _currentWeapon.transform.position = transform.position + transform.forward;
        _currentWeapon.gameObject.SetActive(true);

        _ownedWeapons.Remove(_currentWeapon);
        _currentWeapon = null;
        _currentIndex = _ownedWeapons.Count > 0 ? _ownedWeapons.Count - 1 : -1;
    }

    private static void SetPickupEnabled(Weapon weapon, bool enabled)
    {
        var colliders = weapon.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = enabled;
        }
    }

    /// <summary>
    /// 무기 타입에 따라 장착/조준/사격 위치 세트를 고른다.
    /// </summary>
    private void SelectWeaponPositions(WeaponType type)
    {
        switch (type)
        {
            case WeaponType.assault:
                _equipPos = assaultEquipPos;
                _aimingPos = assaultAimingPos;
                _firePos = assaultFirePos;
                _toFireSpeed = assaultToFireSpeed;
                _recoilDuration = assaultRecoilDuration;
                break;
            default: // pistol 및 기타
                _equipPos = equipPos;
                _aimingPos = aimingPos;
                _firePos = firePos;
                _toFireSpeed = toFireSpeed;
                _recoilDuration = recoilDuration;
                break;
        }
    }

    void SetIKPos()
    {
        // 손 위치가 지정되지 않은 무기는 IK 갱신을 건너뛴다 (null 참조 방지)
        if (_currentWeapon.RightHandPos != null)
        {
            rightHandTarget.position = _currentWeapon.RightHandPos.position;
            rightHandTarget.rotation = _currentWeapon.RightHandPos.rotation;
        }

        if (_currentWeapon.LeftHandPos != null)
        {
            leftHandTarget.position = _currentWeapon.LeftHandPos.position;
            leftHandTarget.rotation = _currentWeapon.LeftHandPos.rotation;
        }
    }
}
