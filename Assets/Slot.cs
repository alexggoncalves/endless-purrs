using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slot : MonoBehaviour
{
    private Boolean isOccupied = false;
    private GameObject catInstance;

    public Boolean IsOccupied()
    {
        return isOccupied;
    }

    public void SetOccupied(Boolean isOccupied)
    {
        this.isOccupied = isOccupied;
    }

    public void SetInstance(GameObject catInstance)
    {
        this.catInstance = catInstance;
    }

    public GameObject GetInstance()
    {
        return catInstance;
    }
}
