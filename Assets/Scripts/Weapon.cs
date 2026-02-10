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
    public WeaponType Type => _type;
}
