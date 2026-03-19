using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bullet : MonoBehaviour
{
    public int bulletDamage;

    private void OnCollisionEnter(Collision objectWeHit)
    {
        AxeZomb zombie = objectWeHit.gameObject.GetComponentInParent<AxeZomb>();
        if (zombie != null)
        {
            print("hit a zombie");
            CreateBulletImpactEffect(objectWeHit);
            zombie.TakeDamage(bulletDamage);
            Destroy(gameObject);
            return;
        }

        if (objectWeHit.gameObject.CompareTag("Target"))
        {
            print("hit " + objectWeHit.gameObject.name + " !");
            CreateBulletImpactEffect(objectWeHit);
            Destroy(gameObject);
        }

        if (objectWeHit.gameObject.CompareTag("Wall"))
        {
            print("hit a wall");
            CreateBulletImpactEffect(objectWeHit);
            Destroy(gameObject);
        }
    }

    void CreateBulletImpactEffect(Collision objectWeHit)
    {
        ContactPoint contact = objectWeHit.contacts[0];

        GameObject hole = Instantiate(
            GlobalReferences.Instance.bulletImpactEffectPrefab,
            contact.point,
            Quaternion.LookRotation(contact.normal)
        );

        hole.transform.SetParent(objectWeHit.gameObject.transform);



    }



}
