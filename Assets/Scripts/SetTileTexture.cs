using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetTileTexture : MonoBehaviour
{
    // Start is called before the first frame update

    private MeshRenderer meshRenderer;
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();/*
        meshRenderer.material.SetVector("_Offset", new Vector2(transform.position.x, transform.position.z));*/
    }

}
