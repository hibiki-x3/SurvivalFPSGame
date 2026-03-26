using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class HUDManager : MonoBehaviour
{

    public static HUDManager Instance{ get; set; }

    [Header("Ammo")]
    public TextMeshProUGUI magazineAmmoUI;
    public TextMeshProUGUI totalAmmoUI;
    public Image ammoTypeUI;

    [Header("Weapon")]
    public Image activeWeaponUI;
    public Image unActiveWeaponUI;

    [Header("Throwables")]
    public Image lethalUI;
    public TextMeshProUGUI lethalAmountUI;

    public Image tacticalUI;
    public TextMeshProUGUI tacticalAmountUI;

    public Sprite emptySlot;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Update()
{
    Weapon activeWeapon = WeaponManager.Instance.activeWeaponSlot.GetComponentInChildren<Weapon>();
    // Get the other slot
    GameObject unActiveSlot = GetUnActiveWeaponSlot();
    Weapon unActiveWeapon = unActiveSlot != null ? unActiveSlot.GetComponentInChildren<Weapon>() : null;

    if (activeWeapon != null)
    {
        // 1. Always update Active Weapon Info
        magazineAmmoUI.text = $"{activeWeapon.bulletsLeft}";
        totalAmmoUI.text = $"{WeaponManager.Instance.CheckAmmoLeftFor(activeWeapon.thisWeaponModel)}";

        Weapon.WeaponModel model = activeWeapon.thisWeaponModel;
        ammoTypeUI.sprite = (Sprite)GetAmmoSprite(model);
        activeWeaponUI.sprite = (Sprite)GetWeaponSprite(model);

        // 2. Handle the Secondary Weapon Slot separately
        if (unActiveWeapon != null)
        {
            unActiveWeaponUI.sprite = (Sprite)GetWeaponSprite(unActiveWeapon.thisWeaponModel);
        }
        else
        {
            unActiveWeaponUI.sprite = emptySlot;
        }
    }
    else
    {
        // 3. If NO weapon is active at all, clear everything
        magazineAmmoUI.text = "";
        totalAmmoUI.text = "";
        ammoTypeUI.sprite = emptySlot;
        activeWeaponUI.sprite = emptySlot;
        unActiveWeaponUI.sprite = emptySlot;
    }
    
}

    private object GetWeaponSprite(Weapon.WeaponModel model)
    {
        switch (model)
        {
            case Weapon.WeaponModel.PistolM1911:
                return Resources.Load<GameObject>("PistolM1911_Weapon").GetComponent<SpriteRenderer>().sprite;
            case Weapon.WeaponModel.M16:
                return Resources.Load<GameObject>("M16_Weapon").GetComponent<SpriteRenderer>().sprite;
            default:
                return null;
        }
    }

    private object GetAmmoSprite(Weapon.WeaponModel model)
    {
        switch (model)
        {
            case Weapon.WeaponModel.PistolM1911:
                return Resources.Load<GameObject>("Pistol_Ammo").GetComponent<SpriteRenderer>().sprite;
            case Weapon.WeaponModel.M16:
                return Resources.Load<GameObject>("Rifle_Ammo").GetComponent<SpriteRenderer>().sprite;
            default:
                return null;
        }
    }

    private GameObject GetUnActiveWeaponSlot()
    {
        foreach(GameObject weaponSlot in WeaponManager.Instance.weaponSlots)
        {
            if(weaponSlot != WeaponManager.Instance.activeWeaponSlot)
            {
                return weaponSlot;
            }
        }
        return null;
    }
}
