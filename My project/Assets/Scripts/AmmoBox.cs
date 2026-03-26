using UnityEngine;

public class AmmoBox : MonoBehaviour
{
    public enum AmmoType
    {
        PistolAmmo,
        RifleAmmo,
        UniversalAmmo
    }

    public AmmoType ammoType;
    public int ammoAmount = 200;
}
