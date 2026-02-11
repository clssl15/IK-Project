using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType
{
    none,
    undefined,
    pistol,
    assault,
    sniper,
    shotgun,
}
public class Weapon : MonoBehaviour
{
    [SerializeField]
    private WeaponType _type;

    [Header("Shooting Settings")]
    [SerializeField]
    private float fireRate = 0.1f; // 연사 간격

    [Header("IK Hand Position")]
    [SerializeField]
    private Transform _rightHandPos;
    [SerializeField]
    private Transform _leftHandPos;

    // 프로퍼티
    public WeaponType Type => _type;
    public Transform RightHandPos => _rightHandPos;
    public Transform LeftHandPos => _leftHandPos;

    // internal
    private float lastFireTime;

    public bool TryShoot()
    {
        if (Time.time >= lastFireTime + fireRate)
        {
            lastFireTime = Time.time;
            Shoot();
            return true;
        }

        return false;
    }

    /// <summary>
    /// 발사 로직
    /// </summary>
    private void Shoot()
    {

    }
}
