using System.Collections;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
[RequireComponent(typeof(PlayerController))]

[RequireComponent(typeof(PlayerAbilities))]
public class PlayerActions : MonoBehaviour
{
    [SerializeField] private Game game;
    [SerializeField] private Animator animator;
    [SerializeField] private MapGenerator mapGenerator;
    [SerializeField] private CameraController cameraController;

    private PlayerController player;
    private Cloth playerCape;
    private PlayerAbilities abilities;

    // TELEPORT
    private Vector3 spawnPoint;
    [SerializeField] private ParticleSystem starTeleportEffect;
    [SerializeField] private TeleportTransition teleportTransition;

    private static readonly int PetCatHash = Animator.StringToHash("PetCat");

    private void Start()
    {
        player = GetComponent<PlayerController>();
        playerCape = GameObject.FindGameObjectWithTag("Cape").GetComponent<Cloth>();
        abilities = GetComponent<PlayerAbilities>();

        abilities.OnAbilityUsed += HandleAbility;
    }

    private void OnDisable()
    {
        abilities.OnAbilityUsed -= HandleAbility;
    }

    private void HandleAbility(AbilityType ability)
    {
        if (ability == AbilityType.Home)
        {
            if (!player.IsFree) return;
            StartCoroutine(TeleportHome());
        }

        if (ability == AbilityType.Call)
        {

        }
    }

    private IEnumerator TeleportHome()
    {
        // Set player teleporting state
        player.SetState(PlayerState.Teleporting);

        // Spawn particle system
        ParticleSystem effect = Instantiate(starTeleportEffect, player.transform.position, Quaternion.Euler(-90, 0, 0));
        Destroy(effect.gameObject, 10f);

        yield return new WaitForSeconds(1);

        // Close Iris Wipe 
        yield return StartCoroutine(teleportTransition.CloseIris());

        // stop wfc movement systems
        var wfc = mapGenerator.GetWFC();
        wfc.ClearPendingShift();

        // Move wfc to origin and reset move offset
        yield return wfc.MoveToOriginRoutine();
        wfc.ResetMoveOffset();

        // Find Spawn Point
        GameObject spawnPointObject = GameObject.FindGameObjectWithTag("SpawnPoint");
        if (spawnPointObject != null)
            spawnPoint = spawnPointObject.transform.position;
        else spawnPoint = Vector3.zero;

        // Move player and followers to spawn
        cameraController.PauseSmoothing();
        playerCape.enabled = false;
        player.transform.position = spawnPoint;
        game.MoveFollowersHome();

        // Resync map generator after teleport
        mapGenerator.ForceResyncAfterTeleport();
        yield return null;

        // Resume wfc generation
        wfc.Resume();

        // Spawn particle system
        ParticleSystem effect2 = Instantiate(starTeleportEffect, player.transform.position, Quaternion.Euler(-90, 0, 0));
        Destroy(effect2.gameObject, 10f);

        // Open Iris Wipe 
        yield return StartCoroutine(teleportTransition.OpenIris());

        // Set player state to Free
        playerCape.enabled = true;
        cameraController.ResumeSmoothing();
        player.SetState(PlayerState.Free);
    }

    public void TryPetCat()
    {
        if (!player.IsFree) return;
        if (!player.IsGrounded()) return;

        if (game == null || game.followers == null)
            return;

        CatController closest = null;
        float best = Mathf.Infinity;
        Vector3 pos = player.transform.position;

        foreach (CatController cat in game.followers)
        {
            if (cat == null) continue;
            if (cat.identity.behaviour == BehaviourType.Scaredy) continue;

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
}
