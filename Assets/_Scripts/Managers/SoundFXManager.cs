using UnityEngine;

public class SoundFXManager : PersistentSingleton<SoundFXManager>
{
    [SerializeField] private AudioSource soundFXObject;
    [SerializeField] private AudioSource soundFXObject2D;

    public void PlaySoundFXClip(AudioClip clip, Vector3 position, float volume)
    {
        // Spawn gameObject at spawn position
        AudioSource audioSource = Instantiate(soundFXObject, position, Quaternion.identity);

        // Assign the audio clip and volume
        audioSource.clip = clip;
        audioSource.volume = volume;

        // Play sound
        audioSource.Play();

        float clipLength = clip.length;

        // Destroy the clip after it's done playing
        Destroy(audioSource.gameObject, clipLength);
    }

    public void PlayRandomSoundFXClip(AudioClip[] clips, Vector3 position, float volume)
    {
        if (clips.Length == 0) return;

        int index = Random.Range(0, clips.Length);
        AudioClip clip = clips[index];

        // Spawn gameObject at spawn position
        AudioSource audioSource = Instantiate(soundFXObject, position, Quaternion.identity);

        // Assign the audio clip and volume
        audioSource.clip = clip;
        audioSource.volume = volume;

        // Play sound
        audioSource.Play();

        float clipLength = clip.length;

        // Destroy the clip after it's done playing
        Destroy(audioSource.gameObject, clipLength);
    }

    public void Play2DSoundFXClip(AudioClip clip, float volume)
    {
        // Spawn gameObject at spawn position
        AudioSource audioSource = Instantiate(soundFXObject2D, Vector3.zero, Quaternion.identity);

        // Assign the audio clip and volume
        audioSource.clip = clip;
        audioSource.volume = volume;

        // Play sound
        audioSource.Play();

        float clipLength = clip.length;

        // Destroy the clip after it's done playing
        Destroy(audioSource.gameObject, clipLength);
    }
}
