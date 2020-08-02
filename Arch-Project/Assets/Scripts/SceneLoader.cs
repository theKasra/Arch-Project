using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] Image aboutPanel;

    private void Start()
    {
        aboutPanel.gameObject.SetActive(false);
    }

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
    public void ShowAbout()
    {
        aboutPanel.gameObject.SetActive(true);
    }
    public void CloseAbout()
    {
        aboutPanel.gameObject.SetActive(false);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
