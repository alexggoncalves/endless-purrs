using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{ 

    // Update is called once per frame
    public void StartGame()
    {
        SceneManager.LoadScene("prototype_1", LoadSceneMode.Single);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}

