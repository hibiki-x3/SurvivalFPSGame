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
    public AudioClip M249Shot;
    public AudioClip M249Reload;
    public AudioClip UziShot;
    public AudioClip UziReload;

    public AudioSource throwablesChannel;
    public AudioClip grenadeSound;

    public AudioClip AxeZombHit;
    [Range(0f, 2f)] public float AxeZombHitVolume = 1.75f;
    public AudioClip hitTickSound;
    [Range(0f, 2f)] public float hitTickVolume = 0.35f;

    public AudioSource emptyMagazineSoundM1911;

    private AudioClip fallbackHitTickClip;

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
            case WeaponModel.Uzi:
                ShootingChannel.PlayOneShot(UziShot);
                break;
            case WeaponModel.M249:
                ShootingChannel.PlayOneShot(M249Shot);
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
            case WeaponModel.Uzi:
                ReloadingChannel.PlayOneShot(UziReload);
                break;
            case WeaponModel.M249:
                ReloadingChannel.PlayOneShot(M249Reload);
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

    public void PlayHitTickSound()
    {
        if (ShootingChannel == null)
        {
            return;
        }

        AudioClip clipToPlay = hitTickSound != null ? hitTickSound : GetFallbackHitTickClip();
        if (clipToPlay == null)
        {
            return;
        }

        ShootingChannel.PlayOneShot(clipToPlay, hitTickVolume);
    }

    private AudioClip GetFallbackHitTickClip()
    {
        if (fallbackHitTickClip != null)
        {
            return fallbackHitTickClip;
        }

        const int sampleRate = 44100;
        const float durationSeconds = 0.03f;
        int sampleCount = Mathf.CeilToInt(sampleRate * durationSeconds);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float time = i / (float)sampleRate;
            float envelope = Mathf.Exp(-time * 90f);
            float noise = Random.Range(-1f, 1f) * 0.15f;
            float click = Mathf.Sin(2f * Mathf.PI * 1800f * time) * 0.55f;
            samples[i] = (click + noise) * envelope;
        }

        fallbackHitTickClip = AudioClip.Create("FallbackHitTick", sampleCount, 1, sampleRate, false);
        fallbackHitTickClip.SetData(samples, 0);
        return fallbackHitTickClip;
    }

}
