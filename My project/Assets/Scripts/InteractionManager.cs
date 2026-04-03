using UnityEngine;

public class InteractionManager : MonoBehaviour
{

    public static InteractionManager Instance{ get; set; }

    public Weapon hoveredWeapon = null;
    public AmmoBox hoveredAmmoBox = null;
    public Throwable hoveredThrowable = null;

    public float interactionRange = 5.0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Update()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit, interactionRange))
        {
            GameObject objectHitByRaycast = hit.transform.gameObject;
            Weapon weapon = objectHitByRaycast.GetComponentInParent<Weapon>();

            //checking if the weapon displays outline whenever we are looking at it
            if (weapon && weapon.isActiveWeapon == false)
            {
                hoveredWeapon = weapon;
                Outline outline = hoveredWeapon.GetComponent<Outline>();
                if (outline != null)
                {
                    outline.enabled = true;
                }

                if (Input.GetKeyDown(KeyCode.F))
                {
                    WeaponManager.Instance.PickupWeapon(hoveredWeapon.gameObject);
                }
            }
            else
            {
                if (hoveredWeapon)
                {
                    Outline outline = hoveredWeapon.GetComponent<Outline>();
                    if (outline != null)
                    {
                        outline.enabled = false;
                    }
                }
            }

            //AmmoBox
            AmmoBox ammoBox = objectHitByRaycast.GetComponentInParent<AmmoBox>();
            if (ammoBox)
            {
                hoveredAmmoBox = ammoBox;
                hoveredAmmoBox.GetComponent<Outline>().enabled = true;
                if (Input.GetKeyDown(KeyCode.F))
                {
                    WeaponManager.Instance.PickupAmmo(hoveredAmmoBox);
                    //Destroy the ammo box after picking it up
                    Destroy(hoveredAmmoBox.gameObject);
                }
            }
            else
            {
                if (hoveredAmmoBox)
                {
                    hoveredAmmoBox.GetComponent<Outline>().enabled = false;
                }
            }

            //Throwable
            if (objectHitByRaycast.GetComponent<Throwable>())
            {
                hoveredThrowable = objectHitByRaycast.gameObject.GetComponent<Throwable>();
                hoveredThrowable.GetComponent<Outline>().enabled = true;
                if (Input.GetKeyDown(KeyCode.F))
                {
                    WeaponManager.Instance.PickupThrowable(hoveredThrowable);
                    //Destroy the throwable after picking it up
                    Destroy(objectHitByRaycast.gameObject);
                }
            }
            else
            {
                if (hoveredThrowable)
                {
                    hoveredThrowable.GetComponent<Outline>().enabled = false;
                }
            }
        }
        else 
    {
        // If we look at the sky/nothing, turn off all outlines
        if (hoveredWeapon && hoveredWeapon.GetComponent<Outline>() != null)
            hoveredWeapon.GetComponent<Outline>().enabled = false;
            
        if (hoveredAmmoBox && hoveredAmmoBox.GetComponent<Outline>() != null)
            hoveredAmmoBox.GetComponent<Outline>().enabled = false;
            
        if (hoveredThrowable && hoveredThrowable.GetComponent<Outline>() != null)
            hoveredThrowable.GetComponent<Outline>().enabled = false;
    }
    }
}
