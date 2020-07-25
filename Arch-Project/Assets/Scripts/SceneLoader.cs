using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void GoToDirectMap()
    {
        SceneManager.LoadScene("Direct Map");
    }
    public void GoToSetAssociative()
    {
        SceneManager.LoadScene("Set Associative");
    }
    public void GoToFullyAssociative()
    {
        SceneManager.LoadScene("Fully Associative");
    }

    public void Quit()
    {
        Application.Quit();
    }
}
