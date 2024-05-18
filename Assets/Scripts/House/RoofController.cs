using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoofController : MonoBehaviour
{
    Boolean isPlayerInside;

    BoxCollider[] houseInterior;
    Transform player;

    // Start is called before the first frame update
    void Start()
    {
        houseInterior = GetComponents<BoxCollider>();
        player = GameObject.FindWithTag("Player").GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        Boolean inside = false;
        foreach (BoxCollider b in houseInterior)
        {
            if (b.bounds.Contains(player.position)) { inside = true; break; }
        }

        if (isPlayerInside && !inside) {
            ShowHouseTop();
            isPlayerInside = false;
        }
        else if (!isPlayerInside && inside)
        {
            HideHouseTop();
            isPlayerInside = true;
        }
    }

    void HideHouseTop()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        }
    }

    void ShowHouseTop()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        }
    }

    public Boolean IsPlayerInside()
    {
        return isPlayerInside;
    }

    public bool IsPlayerInsideCheck
    {
        get { return isPlayerInside; }
    }
}
