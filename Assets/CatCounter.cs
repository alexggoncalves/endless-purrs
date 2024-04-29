using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CatCounter : MonoBehaviour
{
    public TextMeshProUGUI text;
    public TextMeshProUGUI successMessage;
    int totalCatAmount = 3;
    int catCount = 0;
    

    // Start is called before the first frame update
    void Start()
    {
        successMessage.transform.gameObject.SetActive(false);
        text.SetText(catCount.ToString() + "/" + totalCatAmount);
    }

    void addCat()
    {
        catCount++;
        text.SetText(catCount.ToString() + "/" + totalCatAmount);

        if(catCount >= totalCatAmount)
        {
            successMessage.transform.gameObject.SetActive(true);
        }
    }

    int getCatCount()
    {
        return catCount;
    }
}
