using NUnit.Framework.Internal;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.AppUI.UI;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class RadarHUD : MonoBehaviour
{
    [SerializeField] private PlayerAbilities playerAbilities;
    [SerializeField] private Transform player;

    [SerializeField] private float locaterRange = 200f;
    [SerializeField] private float radarRange = 30f;
    [SerializeField] private float radarDuration = 5f;
    [SerializeField] private VisualTreeAsset blipTemplate;

    private VisualElement root;
    private VisualElement radar;
    private VisualElement home;

    private float radarRadius = 0;

    private Coroutine radarCoroutine;
    private readonly List<(VisualElement blip, CatController cat)> activeBlips = new();

    private Collider houseArea = null;

    private void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        radar = root.Q<VisualElement>("Radar");
        home = root.Q<VisualElement>("HomeLocation");

        playerAbilities.OnAbilityUsed += OnAbilityUsed;

        TryFindHouse();
    }
    private void OnDisable()
    {
        playerAbilities.OnAbilityUsed -= OnAbilityUsed;
    }

    private void Update()
    {
        radarRadius = radar.resolvedStyle.width * 0.5f;
        if (radarRadius == 0) return;

        UpdateHomeLocation();
        UpdateBlipPositions();
    }

    /// <summary>
    /// Updates the home position relative to the player on the radar
    /// </summary>
    private void UpdateHomeLocation()
    {
        TryFindHouse();
        if (houseArea == null) return;

        // Get closest point from the player to the house collider
        Vector3 closestPoint = houseArea.ClosestPoint(player.position);

        bool isInside = (closestPoint - player.position).sqrMagnitude < 0.0001f;
        Vector2 offset = isInside
            ? Vector2.zero
            : GetRelativeOffsetToPoint(closestPoint);

        float halfW = home.resolvedStyle.width * 0.5f;
        float halfH = home.resolvedStyle.height * 0.5f;

        home.style.translate = new Translate(
            radarRadius + offset.x * radarRadius - halfW,
            radarRadius + offset.y * radarRadius - halfH,
            0f);
    }

    private void TryFindHouse()
    {
        if (houseArea != null) return;

        GameObject house = GameObject.FindGameObjectWithTag("HouseInterior");
        if (house != null)
            houseArea = house.GetComponent<Collider>();
    }

    /// <summary>
    /// Updates each blip's position relative to the player on the radar
    /// </summary>
    private void UpdateBlipPositions()
    {
        if (activeBlips.Count == 0) return;

        foreach (var (blip, cat) in activeBlips)
        {
            if (cat == null) continue;
            Vector2 offset = GetRelativeOffsetToPoint(cat.transform.position);

            float halfW = blip.resolvedStyle.width * 0.5f;
            float halfH = blip.resolvedStyle.height * 0.5f;

            blip.style.translate = new Translate(
                radarRadius + offset.x * radarRadius - halfW,
                radarRadius + offset.y * radarRadius - halfH,
                0f);
        }
    }

    private Vector2 GetRelativeOffsetToPoint(Vector3 pos)
    {
        Vector3 diff = pos - player.position;
        float nx = Mathf.Clamp(diff.x / radarRange, -1f, 1f);
        float nz = Mathf.Clamp(diff.z / radarRange, -1f, 1f);

        Vector2 offset = new(nx, -nz);
        if (offset.magnitude > 1f)
            offset = offset.normalized;

        return offset;
    }

    private void OnAbilityUsed(AbilityType type)
    {
        if (type == AbilityType.Call)
            ActivateRadar();
    }

    private void ActivateRadar()
    {
        ClearBlips();

        foreach (CatController cat in FindCatsInRange())
        {
            VisualElement blipContainer = blipTemplate.Instantiate().ElementAt(0);
            VisualElement blip = blipContainer.Children().ElementAt(0);

            // Set color of blip as cat's coat color
            Transform catMesh = cat.transform.Find("Cat");
            Material catMaterial = catMesh.GetComponent<Renderer>().material;

            blip.style.unityBackgroundImageTintColor = catMaterial.GetColor("_Coat_Colour");

            radar.Add(blipContainer);
            activeBlips.Add((blipContainer, cat));
        }

        if (radarCoroutine != null)
            StopCoroutine(radarCoroutine);

        radarCoroutine = StartCoroutine(HideRadarAfterDelay());
    }

    private void ClearBlips()
    {
        foreach (var (blip, _) in activeBlips)
            blip.RemoveFromHierarchy();

        activeBlips.Clear();

        if (radarCoroutine != null)
        {
            StopCoroutine(radarCoroutine);
            radarCoroutine = null;
        }
    }

    private IEnumerator HideRadarAfterDelay()
    {
        yield return new WaitForSeconds(radarDuration);
        ClearBlips();
    }


    /// <summary>
    /// Returns all cats within locaterRange that are not following and not scaredy.
    /// </summary>
    private List<CatController> FindCatsInRange()
    {
        List<CatController> result = new();
        Vector3 playerPos = player.transform.position;

        foreach (CatController cat in CatController.AllCats)
        {
            var catIdentity = cat.GetIdentity();
            if (cat == null || catIdentity == null) continue;

            float dist = (cat.transform.position - playerPos).magnitude;
            if (dist > locaterRange) continue;

            if (!cat.IsAtHome()
                && catIdentity.behaviour != BehaviourType.Scaredy
                && !cat.IsFollowing())
            {
                result.Add(cat);
            }
        }

        return result;
    }


}
