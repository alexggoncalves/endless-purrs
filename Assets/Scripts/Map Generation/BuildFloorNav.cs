using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using NavMeshBuilder = UnityEngine.AI.NavMeshBuilder;

public class AreaFloorBaker : MonoBehaviour
{
    [SerializeField] private NavMeshSurface surface;
    [SerializeField] private PlayerController player;
    [SerializeField] private float updateRate = 0.1f;
    [SerializeField] private float movementThreshold = 3f;
    [SerializeField] private Vector3 navMeshSize = new(50, 4, 50);
    [SerializeField] private Vector3 navMeshOffset = new(0, 0, 5);

    [Header("Layers")]
    [SerializeField] private LayerMask bakeLayers;

    private Vector3 worldAnchor;
    private NavMeshData navMeshData;
    private NavMeshBuildSettings buildSettings;
    private Transform playerTransform;

    private readonly List<NavMeshBuildSource> sources = new();
    private readonly List<NavMeshBuildMarkup> markups = new();

    private AsyncOperation pendingBake = null;
    private readonly List<NavMeshModifier> cachedModifiers = new();
    private bool modifiersDirty = true;

    private float sqrThreshold;

    private void Start()
    {
        playerTransform = player.transform;
        sqrThreshold = movementThreshold * movementThreshold;

        navMeshData = new NavMeshData();
        buildSettings = surface.GetBuildSettings();
        NavMesh.AddNavMeshData(navMeshData);
        BuildNavMesh(false);
        StartCoroutine(CheckPlayerMovement());
    }

    private IEnumerator CheckPlayerMovement()
    {
        var wait = new WaitForSeconds(updateRate);

        while (true)
        {
            yield return wait;

            if ((worldAnchor - playerTransform.position).sqrMagnitude > sqrThreshold)
            {
                worldAnchor = playerTransform.position;
                BuildNavMesh(true);
            }
        }
    }

    private void BuildNavMesh(bool async)
    {
        if (async && pendingBake != null && !pendingBake.isDone) return;

        var center = playerTransform.position + navMeshOffset;
        var bounds = new Bounds(center, navMeshSize);

        if (modifiersDirty)
        {
            RebuildMarkups();
            modifiersDirty = false;
        }

        if (surface.collectObjects == CollectObjects.Children)
            NavMeshBuilder.CollectSources(transform, bakeLayers, surface.useGeometry, surface.defaultArea, markups, sources);
        else
            NavMeshBuilder.CollectSources(bounds, bakeLayers, surface.useGeometry, surface.defaultArea, markups, sources);

        if (async)
            pendingBake = NavMeshBuilder.UpdateNavMeshDataAsync(navMeshData, buildSettings, sources, bounds);
        else
            NavMeshBuilder.UpdateNavMeshData(navMeshData, buildSettings, sources, bounds);
    }

    private void RebuildMarkups()
    {
        cachedModifiers.Clear();
        if (surface.collectObjects == CollectObjects.Children)
            cachedModifiers.AddRange(GetComponentsInChildren<NavMeshModifier>());
        else
            cachedModifiers.AddRange(NavMeshModifier.activeModifiers);

        markups.Clear();
        for (int i = 0; i < cachedModifiers.Count; i++)
        {
            var mod = cachedModifiers[i];
            if ((bakeLayers & (1 << mod.gameObject.layer)) != 0
                && mod.AffectsAgentType(surface.agentTypeID))
            {
                markups.Add(new NavMeshBuildMarkup
                {
                    root = mod.transform,
                    overrideArea = mod.overrideArea,
                    area = mod.area,
                    ignoreFromBuild = mod.ignoreFromBuild
                });
            }
        }
    }

    public void InvalidateModifiers()
    {
        modifiersDirty = true;
    }
}