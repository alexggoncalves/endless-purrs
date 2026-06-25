using CAC;
using System.Collections.Generic;
using UnityEngine;
public enum CatState { None, Following, Wandering, Fleeing }
[RequireComponent(typeof(CreateACatGenerator))]

[RequireComponent(typeof(CatSenses))]
[RequireComponent(typeof(CatMovement))]
[RequireComponent(typeof(CatHouseSystem))]
public class CatController : MonoBehaviour
{
    // --- Global cat registry -------------------------------
    public static List<CatController> AllCats = new();
    void OnEnable() { if (!AllCats.Contains(this)) AllCats.Add(this); }
    void OnDisable() { AllCats.Remove(this); }

    // --- STATE --------------------------------------------
    private CatState state = CatState.Wandering;
    private CatIdentity identity;

    // --- REFS ---------------------------------------------
    private CatSenses senses;
    private CatMovement movement;
    private Transform player;
    private CatHouseSystem houseSystem;


    void Start()
    {
        // Randomize cat and create it's identity
        GetComponent<CreateACatGenerator>().RandomizeCat();
        identity = gameObject.AddComponent<CatIdentity>();

        // Get senses component
        senses = GetComponent<CatSenses>();

        // Get movement component
        movement = GetComponent<CatMovement>();

        // Get house system
        houseSystem = GetComponent<CatHouseSystem>();
    }

    private void Update()
    {
        if (player == null)
        {
            TryFindPlayer();
            return;
        }

        senses.Tick();
        houseSystem.Tick();

        bool agentReady = movement.Tick();  // validates & disables agent if off-mesh

        UpdateState();

        if (agentReady)
            ExecuteState();
    }

    private void TryFindPlayer()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    private void UpdateState()
    {
        bool alert = senses.IsPlayerVisible;

        switch (state)
        {
            case CatState.Wandering:
                if (alert && !IsAtHome())
                    state = identity.IsFriendly() ? CatState.Following : CatState.Fleeing;
                break;

            case CatState.Following:
                if (IsAtHome())
                    state = CatState.Wandering;
                break;
            case CatState.Fleeing:
                if (!alert)
                    state = CatState.Wandering;
                break;
        }
    }

    private void ExecuteState()
    {
        switch (state)
        {
            case CatState.Wandering:
                movement.Wander();
                break;

            case CatState.Following:
                movement.Follow(player.position);
                break;

            case CatState.Fleeing:
                movement.Flee(player.position);
                break;
        }
    }

    public static void TeleportFollowersTo(Vector3 point)
    {
        foreach (CatController cat in CatController.AllCats)
        {
            if (cat == null) continue;
            if (!cat.IsFollowing()) continue;

            Vector3 offset = Random.insideUnitSphere * 3f;
            offset.y = 0f;

            cat.movement.TeleportTo(point + offset);
        }
    }

    public CatIdentity GetIdentity() => identity;
    public void SetState(CatState newState) => state = newState;
    public bool IsFollowing() => state == CatState.Following;
    public bool IsAtHome() => houseSystem.IsAtHome();
}
