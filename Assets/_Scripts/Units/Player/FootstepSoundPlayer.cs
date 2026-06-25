using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class FootstepSoundPlayer : MonoBehaviour
{
    private static readonly int FootstepHash = Animator.StringToHash("Footstep");

    [SerializeField] private Animator animator;

    [SerializeField] private AudioClip[] grassFootsteps;
    [SerializeField] private AudioClip[] sandFootsteps;
    [SerializeField] private AudioClip[] waterFootsteps;
    [SerializeField] private AudioClip[] woodFootsteps;
    [SerializeField] private AudioClip[] stoneFootsteps;
    [SerializeField] private AudioClip[] clothFootsteps;

    [SerializeField] private float waterY = -0.3f;
    [SerializeField] private float footstepVolume = 0.05f;

    [SerializeField] private LayerMask Ground;
    private static readonly RaycastHit[] hitBuffer = new RaycastHit[8];

    private float lastFootstep;

    private SoundFXManager soundFXManager;

    private void Start()
    {
        soundFXManager = SoundFXManager.Instance;

        if (animator == null)
            Debug.LogError("No animator component was set.");
    }

    private void Update()
    {
        if (animator == null || soundFXManager == null) return;

        // Get current footstep curve value
        var footstep = animator.GetFloat(FootstepHash);
        if (Mathf.Abs(footstep) < .0001f) footstep = 0f;

        // Play footstep sound if value crosses 0 in the animation curve
        if (lastFootstep > 0 && footstep < 0 || lastFootstep < 0 && footstep > 0)
        {
            PlayFootstep();
        }

        lastFootstep = footstep;
    }

    private void PlayFootstep()
    {
        SurfaceType surface = GetCurrentSurface();

        switch (surface)
        {
            case SurfaceType.Grass:
                soundFXManager.PlayRandomSoundFXClip(grassFootsteps, transform, footstepVolume);
                break;
            case SurfaceType.Sand:
                soundFXManager.PlayRandomSoundFXClip(sandFootsteps, transform, footstepVolume);
                break;
            case SurfaceType.Wood:
                soundFXManager.PlayRandomSoundFXClip(woodFootsteps, transform, footstepVolume);
                break;
            case SurfaceType.Water:
                soundFXManager.PlayRandomSoundFXClip(waterFootsteps, transform, footstepVolume/2f);
                break;
            case SurfaceType.Stone:
                soundFXManager.PlayRandomSoundFXClip(stoneFootsteps, transform, footstepVolume);
                break;
            case SurfaceType.Cloth:
                soundFXManager.PlayRandomSoundFXClip(clothFootsteps, transform, footstepVolume);
                break;
            default:
                break;
        }
    }

    private SurfaceType GetCurrentSurface()
    {
        if (transform.position.y <= waterY)
            return SurfaceType.Water;

        // Perform downwards raycast and cache all hits in hitBuffer
        int count = Physics.RaycastNonAlloc(
            transform.position + Vector3.up * 0.01f + transform.forward * 0.1f,
            Vector3.down,
            hitBuffer,
            .2f,
            Ground);

        if (count > 0)
        {
            // Sort hits by distance
            System.Array.Sort(hitBuffer, 0, count, Comparer<RaycastHit>.Create((a, b) => a.distance.CompareTo(b.distance)));

            // Find closest hit gameobject containing a surface definition component
            for (int i = 0; i < count; i++)
            {
                var surface = hitBuffer[i].collider.GetComponent<SurfaceDefinition>();
                if (surface) return surface.surfaceType;
            }
        }

        return SurfaceType.Stone;
    }
}
