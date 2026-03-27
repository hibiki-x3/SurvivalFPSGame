using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using static Weapon;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance{ get; set; }

    public AudioSource ShootingChannel;
    public AudioSource ReloadingChannel;

    public AudioClip M1911Shot;
    public AudioClip M16Shot;
    public AudioClip M1911Reload;
    public AudioClip M16Reload;

    public AudioClip AxeZombHit;
    [Range(0f, 2f)] public float AxeZombHitVolume = 1.75f;

    public AudioSource emptyMagazineSoundM1911;

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

    public void PlayShootingSound(WeaponModel weapon)
    {
        switch (weapon)
        {
            case WeaponModel.PistolM1911:
                ShootingChannel.PlayOneShot(M1911Shot);
                break;
            case WeaponModel.M16:
                ShootingChannel.PlayOneShot(M16Shot);
                break;
        }
    }

    public void PlayReloadSound(WeaponModel weapon)
    {
         switch (weapon)
        {
            case WeaponModel.PistolM1911:
                ReloadingChannel.PlayOneShot(M1911Reload);
                break;
            case WeaponModel.M16:
                ReloadingChannel.PlayOneShot(M16Reload);
                break;
        }
    }

    public void PlayAxeZombHitSound()
    {
        if (AxeZombHit == null || ShootingChannel == null)
        {
            return;
        }

        ShootingChannel.PlayOneShot(AxeZombHit, AxeZombHitVolume);
    }

}
