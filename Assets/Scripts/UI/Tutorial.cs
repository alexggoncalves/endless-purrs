using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    int step = 0;

    private GameObject start;
    private GameObject movement;
    private GameObject buttonQ;
    private GameObject buttonE;
    private GameObject catCounterMsg;

    private GameObject loading;
    private GameObject catPointer;
    //private GameObject catCounter;

    void Start()
    {
        start = transform.Find("Start").gameObject;
        movement = transform.Find("Movement").gameObject;
        buttonQ = transform.Find("Q").gameObject;
        buttonE = transform.Find("E").gameObject;
        catCounterMsg = transform.Find("Counter").gameObject;

        loading = GameObject.Find("LoadingScreen").gameObject;
        catPointer = GameObject.Find("CatPointer").gameObject;
        //catCounter = GameObject.Find("Cat Counter").gameObject;

        catPointer.SetActive(false);
        //catCounter.SetActive(false);
    }
    void Update()
    {
        if (!loading.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                step++;
            }
            if (step == 0) {
                movement.SetActive(false);
                buttonQ.SetActive(false);
                buttonE.SetActive(false);
                catCounterMsg.SetActive(false);
            }
            else if (step == 1)
            {
                start.SetActive(false);
                movement.SetActive(true);
                buttonQ.SetActive(true);
                buttonE.SetActive(true);
            }
            else if (step == 2)
            {
                movement.SetActive(false);
                buttonQ.SetActive(false);
                buttonE.SetActive(false);
                catCounterMsg.SetActive(true);

            }
            else if (step == 3)
            {
                gameObject.SetActive(false);
                catPointer.SetActive(true);
                //catCounter.SetActive(true);
            }
        }
    }
}