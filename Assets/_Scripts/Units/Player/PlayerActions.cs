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

    [Header("Pet positioning")]
    [SerializeField] private float petStandDistance = 0.8f;  // how far from the cat the player stands
    [SerializeField] private float approachSpeed = 4f;        // units/sec while moving into place
    [SerializeField] private float approachTurnSpeed = 720f;  // deg/sec while aligning
    [SerializeField] private float alignThreshold = 2f;       // degrees; close enough to start the anim
    [SerializeField] private float catTurnSpeed = 360f;
    [SerializeField] private float backBias = 0f;

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

    public void TryPetCat(Interactable target)
    {
        if (!player.IsFree) return;
        if (!player.IsGrounded()) return;
        if (target == null) return;

        var cat = target.TryGetComponent<CatController>(out var catController) ? catController : null;

        if(cat != null)
            StartCoroutine(PetRoutine(cat));
    }

    private IEnumerator PetRoutine(CatController cat)
    {
        player.SetState(PlayerState.Acting);

        Vector3 catPos = cat.transform.position;

        Vector3 catFwd = cat.transform.forward;
        catFwd.y = 0f;
        catFwd.Normalize();

        Vector3 catRight = Vector3.Cross(Vector3.up, catFwd);
        Vector3 toPlayer = player.transform.position - catPos;
        toPlayer.y = 0f;
        float side = Vector3.Dot(toPlayer, catRight) >= 0f ? 1f : -1f;

        Vector3 standPos = catPos
                           + catRight * (side * petStandDistance)
                           + catFwd * backBias;
        standPos.y = player.transform.position.y;

        // Cat squares up to present its side to the player — nearer flank, short rotation.
        Vector3 flatToPlayer = -catRight * side; // the direction from cat out to the player's side
        Vector3 broadside = Vector3.Cross(Vector3.up, flatToPlayer);
        Quaternion optionA = Quaternion.LookRotation(broadside, Vector3.up);
        Quaternion optionB = Quaternion.LookRotation(-broadside, Vector3.up);
        Quaternion catTargetRot =
            Quaternion.Angle(cat.transform.rotation, optionA) <=
            Quaternion.Angle(cat.transform.rotation, optionB) ? optionA : optionB;

        while (true)
        {
            Vector3 toStand = standPos - player.transform.position;
            toStand.y = 0f;
            float dist = toStand.magnitude;

            Vector3 lookDir = catPos - player.transform.position;
            lookDir.y = 0f;
            Quaternion playerTargetRot = Quaternion.LookRotation(lookDir, Vector3.up);
            player.transform.rotation = Quaternion.RotateTowards(
                player.transform.rotation, playerTargetRot, approachTurnSpeed * Time.deltaTime);
            float playerAngleLeft = Quaternion.Angle(player.transform.rotation, playerTargetRot);

            if (dist > 0.05f)
                player.transform.position = Vector3.MoveTowards(
                    player.transform.position, standPos, approachSpeed * Time.deltaTime);

            // --- cat rotation (this is what went missing) ---
            cat.transform.rotation = Quaternion.RotateTowards(
                cat.transform.rotation, catTargetRot, catTurnSpeed * Time.deltaTime);
            float catAngleLeft = Quaternion.Angle(cat.transform.rotation, catTargetRot);

            if (dist <= 0.05f && playerAngleLeft <= alignThreshold && catAngleLeft <= alignThreshold)
                break;

            yield return null;
        }

        animator.SetTrigger(PetCatHash);
        yield return new WaitForSeconds(3.5f);

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
