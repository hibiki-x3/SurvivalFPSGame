using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bullet : MonoBehaviour
{
    public int bulletDamage;
    [SerializeField] private float zombieBloodEffectScale = 0.2f;

    private void OnCollisionEnter(Collision objectWeHit)
    {
        AxeZomb zombie = ResolveZombie(objectWeHit.collider);
        if (zombie != null)
        {
            print("hit a zombie");
            SoundManager.Instance?.PlayAxeZombHitSound();
            SoundManager.Instance?.PlayHitTickSound();
            HitMarkerUI.Instance?.ShowHitMarker();
            CreateBloodEffect(objectWeHit);
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

    private void OnTriggerEnter(Collider other)
    {
        AxeZomb zombie = ResolveZombie(other);
        if (zombie != null)
        {
            print("hit a zombie (trigger)");
            SoundManager.Instance?.PlayAxeZombHitSound();
            SoundManager.Instance?.PlayHitTickSound();
            HitMarkerUI.Instance?.ShowHitMarker();
            zombie.TakeDamage(bulletDamage);
            Destroy(gameObject);
            return;
        }

        if (other.CompareTag("Target") || other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }

    private AxeZomb ResolveZombie(Collider hitCollider)
    {
        if (hitCollider == null)
        {
            return null;
        }

        AxeZomb zombie = hitCollider.GetComponent<AxeZomb>();
        if (zombie != null)
        {
            return zombie;
        }

        zombie = hitCollider.GetComponentInParent<AxeZomb>();
        if (zombie != null)
        {
            return zombie;
        }

        if (hitCollider.attachedRigidbody != null)
        {
            zombie = hitCollider.attachedRigidbody.GetComponent<AxeZomb>();
            if (zombie != null)
            {
                return zombie;
            }

            zombie = hitCollider.attachedRigidbody.GetComponentInParent<AxeZomb>();
            if (zombie != null)
            {
                return zombie;
            }
        }

        return hitCollider.transform.root.GetComponentInChildren<AxeZomb>(true);
    }
    void CreateBloodEffect(Collision objectWeHit)
    {
        ContactPoint contact = objectWeHit.contacts[0];
        GameObject bloodPrefab = GlobalReferences.Instance != null ? GlobalReferences.Instance.bloodEffectPrefab : null;

        GameObject blood = bloodPrefab != null
            ? Instantiate(
                bloodPrefab,
                contact.point,
                Quaternion.LookRotation(contact.normal)
            )
            : CreateFallbackBloodEffect(contact.point, contact.normal);

        if (blood != null)
        {
            // Keep blood effect subtle for zombie hits.
            blood.transform.localScale *= zombieBloodEffectScale;
            blood.transform.SetParent(objectWeHit.gameObject.transform);
        }
    }
    void CreateBulletImpactEffect(Collision objectWeHit)
    {
        ContactPoint contact = objectWeHit.contacts[0];

        GameObject impactPrefab = GlobalReferences.Instance != null ? GlobalReferences.Instance.bulletImpactEffectPrefab : null;

        GameObject hole = impactPrefab != null
            ? Instantiate(
                impactPrefab,
                contact.point,
                Quaternion.LookRotation(contact.normal)
            )
            : CreateFallbackImpactEffect(contact.point, contact.normal);

        if (hole != null)
        {
            hole.transform.SetParent(objectWeHit.gameObject.transform);
        }

    }

    private GameObject CreateFallbackBloodEffect(Vector3 position, Vector3 normal)
    {
        GameObject effect = new GameObject("FallbackBloodEffect");
        effect.transform.position = position;
        effect.transform.rotation = Quaternion.LookRotation(normal);

        ParticleSystem particleSystem = effect.AddComponent<ParticleSystem>();
        var main = particleSystem.main;
        main.loop = false;
        main.startLifetime = 0.25f;
        main.startSpeed = 2.5f;
        main.startSize = 0.08f;
        main.startColor = new Color(0.55f, 0f, 0f, 1f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = particleSystem.emission;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 12) });

        var shape = particleSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 20f;
        shape.radius = 0.02f;

        Destroy(effect, 1f);
        return effect;
    }

    private GameObject CreateFallbackImpactEffect(Vector3 position, Vector3 normal)
    {
        GameObject effect = new GameObject("FallbackImpactEffect");
        effect.transform.position = position;
        effect.transform.rotation = Quaternion.LookRotation(normal);

        ParticleSystem particleSystem = effect.AddComponent<ParticleSystem>();
        var main = particleSystem.main;
        main.loop = false;
        main.startLifetime = 0.2f;
        main.startSpeed = 1.5f;
        main.startSize = 0.04f;
        main.startColor = new Color(0.85f, 0.85f, 0.85f, 1f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = particleSystem.emission;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 6) });

        var shape = particleSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 10f;
        shape.radius = 0.01f;

        Destroy(effect, 0.75f);
        return effect;




    }


}
