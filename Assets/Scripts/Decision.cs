using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Decision : MonoBehaviour
{
    [TextAreaAttribute]
    public string text;

    public GameObject textObject;

    public GameObject bubble;
    TextMeshPro textMesh;

    Boolean active;

    char decision = '0';


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
        if (Input.GetKeyDown(KeyCode.N) && active && decision == '0')
        {
            decision = 'n';
            Hide();
        }
        if (Input.GetKeyDown(KeyCode.Y) && active && decision == '0')
        {
            decision = 'y';
            Hide();
        }
    }

    public void SetText(string text)
    {
        textMesh.SetText(text);
        textMesh.ForceMeshUpdate();
        /*Debug.Log(textMesh.textBounds);*/
        // Set bubble size according to text size
    }

    public void Hide()
    {
        /*if(transform.parent.CompareTag("Player")){
            transform.parent.GetComponent<Movement>().UnlockMovement();
        }*/
        transform.gameObject.SetActive(false);
        active = false;
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    public void Show()
    {
        /*if(transform.parent.CompareTag("Player")){
            transform.parent.GetComponent<Movement>().LockMovement();
        }*/
        active = true;
        transform.gameObject.SetActive(true);   
        foreach (Transform child in transform)
        {
            Debug.Log(child);
            child.gameObject.SetActive(true);
        }
    }

    public Boolean IsActive()
    {
        return active;
    }

    public char GetDecision()
    {
        return decision;
    }
}
