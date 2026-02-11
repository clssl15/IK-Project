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

    [Header("IK Hand Position")]
    [SerializeField]
    private Transform _rightHandPos;
    [SerializeField]
    private Transform _leftHandPos;

    // 프로퍼티
    public WeaponType Type => _type;
    public Transform RightHandPos => _rightHandPos;
    public Transform LeftHandPos => _leftHandPos;
}
