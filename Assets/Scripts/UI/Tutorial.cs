using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    int step = 0;

    private GameObject movement;
    private GameObject buttonQ;
    private GameObject buttonE;
    private GameObject catCounter;

    void Start()
    {
        movement = transform.Find("Movement").gameObject;
        buttonQ = transform.Find("Q").gameObject;
        buttonE = transform.Find("E").gameObject;
        catCounter = transform.Find("Counter").gameObject;
    }
    void Update() {
        if (Input.GetKeyDown(KeyCode.E))
        {
            step++;
        }

        if (step == 0)
        {
            buttonQ.SetActive(false);
            buttonE.SetActive(false);
            catCounter.SetActive(false);
        } else if (step == 1)
        {
            movement.SetActive(false);
            buttonQ.SetActive(true);

        } else if (step == 2)
        {
            buttonQ.SetActive(false);
            buttonE.SetActive(true);

        } else if (step == 3 )
        {
            buttonE.SetActive(false);
            catCounter.SetActive(true);

        } else if (step == 4 ){
            gameObject.SetActive(false);
        }
    }
}