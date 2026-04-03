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
}
