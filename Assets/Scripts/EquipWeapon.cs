using UnityEngine;
using StarterAssets;

public class EquipWeapon : MonoBehaviour
{
    [Header("Current Weapon Transform Settings")]
    [SerializeField]
    private Transform currentWeaponPos;

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

        if (_input.aim)
        {
            switch (_currentWeapon.Type)
            {
                case WeaponType.pistol:
                    playerAnimator.SetBool("PistolAim", true);
                    break;

                case WeaponType.assault:
                    playerAnimator.SetBool("AssaultAim", true);
                    break;
            }
        }
        else if (!_input.aim)
        {
            switch (_currentWeapon.Type)
            {
                case WeaponType.pistol:
                    playerAnimator.SetBool("PistolAim", false);
                    break;
                case WeaponType.assault:
                    playerAnimator.SetBool("AssaultAim", false);
                    break;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!_input.interact) return;

        if (other.CompareTag("Weapon"))
        {
            Equip(other.GetComponent<Weapon>());
        }
    }

    void Equip(Weapon weapon)
    {
        _currentWeapon = weapon;

        _currentWeapon.transform.parent = currentWeaponPos.transform;
        _currentWeapon.transform.position = currentWeaponPos.position;
        _currentWeapon.transform.rotation = currentWeaponPos.rotation;
    }
}
