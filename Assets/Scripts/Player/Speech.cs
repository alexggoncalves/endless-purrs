using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Speech : MonoBehaviour
{
    [TextAreaAttribute]
    public string text;

    public GameObject textObject;

    public GameObject bubble;
    TextMeshPro textMesh;

    bool active;
   
    int step = 0;
    Boolean startText = true;

    // Start is called before the first frame update
    void Start()
    {
        textMesh = textObject.GetComponent<TextMeshPro>();
        Hide();
    }

    private void LateUpdate()
    {
        transform.rotation = Quaternion.identity;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && active)
        {
            Hide();
        }
    }

    public void SetText(string text)
    {
        textMesh.SetText(text);
        textMesh.ForceMeshUpdate();
    }

    public void Hide()
    {
        if (transform.parent.CompareTag("Player"))
        {
            transform.parent.GetComponent<PlayerController>().UnlockMovement();
        }
        active = false;
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    public void Show()
    {
        if (transform.parent.CompareTag("Player"))
        {
            transform.parent.GetComponent<PlayerController>().LockMovement();
        }
        active = true;
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
    }

    public Boolean IsActive()
    {
        return active;
    }
}
