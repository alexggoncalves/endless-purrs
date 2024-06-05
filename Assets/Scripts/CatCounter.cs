using System;
using TMPro;
using UnityEngine;

public class CatCounter : MonoBehaviour
{
    public TextMeshProUGUI text;
    public TextMeshProUGUI successMessage;
    [SerializeField,Range(1,10)]
    public int totalCatAmount = 1;
    int catCount = 0;
    

    // Start is called before the first frame update
    void Start()
    {
        text.SetText(catCount.ToString() + "/" + totalCatAmount);
    }

    private void Update()
    {
        /*if (catCount >= totalCatAmount)
        {
            successMessage.transform.gameObject.SetActive(true);
        } else successMessage.transform.gameObject.SetActive(false);*/
    }

    public void AddCat()
    {
        catCount++;
        text.SetText(catCount.ToString() + "/" + totalCatAmount);

    }

    public void RemoveCat()
    {
        catCount--;
        text.SetText(catCount.ToString() + "/" + totalCatAmount);
    }

    int GetCatCount()
    {
        return catCount;
    }

    public void SetMessage(string text)
    {
        successMessage.SetText(text);
    }
}
