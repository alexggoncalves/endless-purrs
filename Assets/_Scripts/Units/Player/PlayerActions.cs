using System.Collections;
using UnityEngine;
[RequireComponent(typeof(PlayerController))]

[RequireComponent(typeof(PlayerAbilities))]
public class PlayerActions : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private WorldGenerator worldGenerator;
    private PlayerController player;
    private PlayerAbilities abilities;

    // TELEPORT
    [SerializeField] private ParticleSystem starTeleportEffect;
    [SerializeField] private IrisWipeController irisWipe;
    [SerializeField] private AudioClip teleportDisappearAudio;
    [SerializeField] private AudioClip teleportAppearAudio;
    private readonly WaitForSeconds teleportDelay = new(1f);

    private static readonly int PetCatHash = Animator.StringToHash("PetCat");

    private void Start()
    {
        worldGenerator = WorldGenerator.Instance;

        player = GetComponent<PlayerController>();
        abilities = GetComponent<PlayerAbilities>();

        abilities.OnAbilityUsed += HandleAbility;
    }

    private void OnDestroy()
    {
        abilities.OnAbilityUsed -= HandleAbility;
    }

    private void HandleAbility(AbilityType ability, bool locked)
    {
        if (ability == AbilityType.Home)
        {
            if (!player.IsFree) return;
            StartCoroutine(TeleportHome());
        }
    }

    private IEnumerator TeleportHome()
    {
        // Set player teleporting state
        player.SetState(PlayerState.Teleporting);
        GameManager.Instance.ChangeState(GameState.Teleporting);

        // Spawn particle system
        ParticleSystem starsIn = Instantiate(starTeleportEffect, player.transform.position, Quaternion.Euler(-90, 0, 0));
        StartCoroutine(FollowPlayer(starsIn.transform, player.transform));
        Destroy(starsIn.gameObject, 10f);

        SoundFXManager.Instance.PlaySoundFXClip(teleportDisappearAudio, player.transform.position,0.8f);

        // Delay teleport 
        yield return teleportDelay;

        // Close Iris Wipe
        if (irisWipe != null)
            yield return StartCoroutine(irisWipe.CloseIris(2f));

        // Clear wfc shift, move to origin and reset move offset
        var wfc = worldGenerator.GetWFC();
        yield return wfc.MoveToOriginRoutine();

        // Move player to the spaw point
        StartCoroutine(player.TeleportToSpawnPoint());

        // Resync map generator after teleport and resume
        worldGenerator.ForceResyncAfterTeleport();
        yield return null;
        wfc.Resume();
        yield return null;

        // Teleport followers with the player
        CatController.TeleportFollowersTo(player.transform.position);

        // Play Sound
        SoundFXManager.Instance.PlaySoundFXClip(teleportAppearAudio, player.transform.position, 0.8f);

        yield return new WaitForSeconds(1f);

        // Open Iris Wipe
        if (irisWipe != null)
            yield return StartCoroutine(irisWipe.OpenIris(2f));

        // Set player state to Free
        player.SetState(PlayerState.Free);
        GameManager.Instance.ChangeState(GameState.Playing);
    }

    public void TryPetCat()
    {
        if (!player.IsFree) return;
        if (!player.IsGrounded()) return;

        if (CatController.AllCats.Count == 0)
            return;

        CatController closest = null;
        float best = Mathf.Infinity;
        Vector3 pos = player.transform.position;

        foreach (CatController cat in CatController.AllCats)
        {
            if (cat == null) continue;
            if (!cat.IsFollowing()) continue; // only followers

            float dist = (cat.transform.position - pos).sqrMagnitude;

            if (dist < 9f && dist < best)
            {
                best = dist;
                closest = cat;
            }
        }

        if (closest == null)
            return;

        animator.SetTrigger(PetCatHash);

        StartCoroutine(PetRoutine(closest));
    }

    private IEnumerator PetRoutine(CatController cat)
    {
        player.SetState(PlayerState.Acting);

        Vector3 target = cat.transform.position;

        float duration = 3.5f;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;

            Vector3 lookPos = new(target.x, player.transform.position.y, target.z);
            Quaternion rot = Quaternion.LookRotation(lookPos - player.transform.position, Vector3.up);

            player.transform.rotation = Quaternion.RotateTowards(
                player.transform.rotation,
                rot,
                400 * Time.deltaTime
            );

            yield return null;
        }


        player.SetState(PlayerState.Free);
    }

    /// <summary>
    /// Makes effect follow player's position and ignores rotation
    /// </summary>
    IEnumerator FollowPlayer(Transform effect, Transform player)
    {
        while (effect != null)
        {
            effect.position = player.position;
            yield return null;
        }
    }
}
