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

    private StarterAssetsInputs _input;
    private Animator playerAnimator;

    private Weapon _currentWeapon;

    private void Start()
    {
        _input = GetComponent<StarterAssetsInputs>();
        playerAnimator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (_currentWeapon == null) return;

        Transform newPos = default;

        if (_input.aim)
        {
            switch (_currentWeapon.Type)
            {
                case WeaponType.pistol:
                    playerAnimator.SetBool("PistolAim", true);
                    newPos = aimingPos;
                    break;

                case WeaponType.assault:
                    playerAnimator.SetBool("AssaultAim", true);
                    newPos = aimingPos;
                    break;
            }

            leftHandIK.weight = 1f;
        }
        else if (!_input.aim)
        {
            switch (_currentWeapon.Type)
            {
                case WeaponType.pistol:
                    playerAnimator.SetBool("PistolAim", false);
                    newPos = equipPos;
                    break;
                case WeaponType.assault:
                    newPos = equipPos;
                    playerAnimator.SetBool("AssaultAim", false);
                    break;
            }

            leftHandIK.weight = 0f;
        }

        SetWeaponPos(newPos);
        SetIKPos();
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

    void SetWeaponPos(Transform newPos)
    {
        _currentWeapon.transform.SetParent(newPos.transform);
        _currentWeapon.transform.position = newPos.position;
        _currentWeapon.transform.rotation = newPos.rotation;
    }

    void SetIKPos()
    {
        rightHandTarget.position = _currentWeapon.RightHandPos.position;
        rightHandTarget.rotation = _currentWeapon.RightHandPos.rotation;

        leftHandTarget.position = _currentWeapon.LeftHandPos.position;
        leftHandTarget.rotation = _currentWeapon.LeftHandPos.rotation;
    }
}
