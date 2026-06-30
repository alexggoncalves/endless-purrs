using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable]
public struct SurfaceSounds
{
    public AudioClip[] footsteps;
    public AudioClip[] jump;
    public AudioClip[] land;
}

public class FootstepSoundPlayer : MonoBehaviour
{
    private static readonly int FootstepHash = Animator.StringToHash("Footstep");

    [SerializeField] private Animator animator;
    [SerializeField] private PlayerController playerController;

    [SerializeField] private SurfaceSounds grass;
    [SerializeField] private SurfaceSounds sand;
    [SerializeField] private SurfaceSounds water;
    [SerializeField] private SurfaceSounds wood;
    [SerializeField] private SurfaceSounds stone;
    [SerializeField] private SurfaceSounds cloth;

    [SerializeField] private float waterY = -0.3f;
    [SerializeField] private float footstepVolume = 0.5f;

    [SerializeField] private LayerMask Ground;
    private static readonly RaycastHit[] hitBuffer = new RaycastHit[8];

    private float lastFootstep;
    private bool wasGrounded = true;

    private SoundFXManager soundFXManager;

    private void Start()
    {
        soundFXManager = SoundFXManager.Instance;

        if (animator == null)
            Debug.LogError("No animator component was set.");
    }

    private void Update()
    {
        if (animator == null || soundFXManager == null || playerController == null) return;
        if (GameManager.Instance.State == GameState.InitialLoading) return;

        bool grounded = playerController.IsGrounded();

        if (grounded)
            HandleFootsteps();

        HandleJumpAndLand(grounded);
    }

    private void HandleFootsteps()
    {
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

    private void HandleJumpAndLand(bool grounded)
    {
        // Leaving the ground -> jump
        if (wasGrounded && !grounded)
            PlayJumpSound();

        // Returning to the ground -> land
        if (!wasGrounded && grounded)
            PlayLandingSound();

        wasGrounded = grounded;
    }

    private void PlayFootstep()
    {
        SurfaceType surface = GetCurrentSurface();

        switch (surface)
        {
            case SurfaceType.Grass:
                soundFXManager.PlayRandomSoundFXClip(grass.footsteps, transform.position, footstepVolume);
                break;
            case SurfaceType.Sand:
                soundFXManager.PlayRandomSoundFXClip(sand.footsteps, transform.position, footstepVolume);
                break;
            case SurfaceType.Wood:
                soundFXManager.PlayRandomSoundFXClip(wood.footsteps, transform.position, footstepVolume);
                break;
            case SurfaceType.Water:
                soundFXManager.PlayRandomSoundFXClip(water.footsteps, transform.position, footstepVolume / 2f);
                break;
            case SurfaceType.Stone:
                soundFXManager.PlayRandomSoundFXClip(stone.footsteps, transform.position, footstepVolume);
                break;
            case SurfaceType.Cloth:
                soundFXManager.PlayRandomSoundFXClip(cloth.footsteps, transform.position, footstepVolume);
                break;
            default:
                break;
        }
    }

    private void PlayJumpSound()
    {
        SurfaceType surface = GetCurrentSurface();

        switch (surface)
        {
            case SurfaceType.Grass:
                soundFXManager.PlayRandomSoundFXClip(grass.jump, transform.position, footstepVolume);
                break;
            case SurfaceType.Sand:
                soundFXManager.PlayRandomSoundFXClip(sand.jump, transform.position, footstepVolume);
                break;
            case SurfaceType.Wood:
                soundFXManager.PlayRandomSoundFXClip(wood.jump, transform.position, footstepVolume);
                break;
            case SurfaceType.Water:
                soundFXManager.PlayRandomSoundFXClip(water.jump, transform.position, footstepVolume / 2f);
                break;
            case SurfaceType.Stone:
                soundFXManager.PlayRandomSoundFXClip(stone.jump, transform.position, footstepVolume);
                break;
            case SurfaceType.Cloth:
                soundFXManager.PlayRandomSoundFXClip(cloth.jump, transform.position, footstepVolume);
                break;
            default:
                break;
        }
    }

    private void PlayLandingSound()
    {
        SurfaceType surface = GetCurrentSurface();

        switch (surface)
        {
            case SurfaceType.Grass:
                soundFXManager.PlayRandomSoundFXClip(grass.land, transform.position, footstepVolume);
                break;
            case SurfaceType.Sand:
                soundFXManager.PlayRandomSoundFXClip(sand.land, transform.position, footstepVolume);
                break;
            case SurfaceType.Wood:
                soundFXManager.PlayRandomSoundFXClip(wood.land, transform.position, footstepVolume);
                break;
            case SurfaceType.Water:
                soundFXManager.PlayRandomSoundFXClip(water.land, transform.position, footstepVolume / 2f);
                break;
            case SurfaceType.Stone:
                soundFXManager.PlayRandomSoundFXClip(stone.land, transform.position, footstepVolume);
                break;
            case SurfaceType.Cloth:
                soundFXManager.PlayRandomSoundFXClip(cloth.land, transform.position, footstepVolume);
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
            transform.position + Vector3.up * 0.01f,
            Vector3.down,
            hitBuffer,
            1f,
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

        return SurfaceType.Grass;
    }
}
