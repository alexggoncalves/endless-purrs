using System;
using UnityEngine;
using UnityEngine.Audio;

public class SoundMixerManager : PersistentSingleton<SoundMixerManager>
{
    [SerializeField] private AudioMixer audioMixer;

    public void SetMasterVolume(float level)
    {
        if (level == 0) level = 0.0001f; // Prevent infinity on Log10(0)
        audioMixer.SetFloat("masterVolume", Mathf.Log10(level) * 20f);
    }

    public void SetSoundFXVolume(float level)
    {
        if (level == 0) level = 0.0001f;
        audioMixer.SetFloat("soundFXVolume", Mathf.Log10(level) * 20f);
    }

    public void SetMusicVolume(float level)
    {
        if (level == 0) level = 0.0001f;
        audioMixer.SetFloat("musicVolume", Mathf.Log10(level) * 20f);
    }

    public float GetMasterVolume()
    {
        audioMixer.GetFloat("masterVolume", out float db);
        return Mathf.Pow(10f, db / 20f);
    }

    public float GetSoundFXVolume()
    {
        audioMixer.GetFloat("soundFXVolume", out float db);
        return Mathf.Pow(10f, db / 20f);
    }

    public float GetMusicVolume()
    {
        audioMixer.GetFloat("musicVolume", out float db);
        return Mathf.Pow(10f, db / 20f);
    }
}
