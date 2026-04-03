using UnityEngine;

public class AmmoBox : MonoBehaviour
{
    public enum AmmoType
    {
        PistolAmmo,
        RifleAmmo,
        MachineGunAmmo,
        UniversalAmmo
    }

    public AmmoType ammoType;
    public int ammoAmount = 300;

    [Header("Pickup Behavior")]
    [SerializeField] private bool useActiveWeaponAmmoType = true;

    public AmmoType GetEffectiveAmmoType()
    {
        if (!useActiveWeaponAmmoType || WeaponManager.Instance == null || WeaponManager.Instance.activeWeaponSlot == null)
        {
            return ammoType;
        }

        Weapon activeWeapon = WeaponManager.Instance.activeWeaponSlot.GetComponentInChildren<Weapon>();
        if (activeWeapon == null)
        {
            return ammoType;
        }

        switch (activeWeapon.thisWeaponModel)
        {
            case Weapon.WeaponModel.PistolM1911:
                return AmmoType.PistolAmmo;
            case Weapon.WeaponModel.M16:
                return AmmoType.RifleAmmo;
            case Weapon.WeaponModel.M249:
                return AmmoType.MachineGunAmmo;
            default:
                return ammoType;
        }
    }
}
