using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinManager : MonoBehaviour
{
    public int enemies;
    [SerializeField] private GameObject ui;

    public static WinManager Instance;
    
    private void Awake() 
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start() 
    {
        EnemyNavigation[] navs = FindObjectsOfType<EnemyNavigation>();
        enemies = navs.Length;

        ui.SetActive(false);
    }
    
    private void Update() 
    {
        if (enemies == 0) 
        {
            ui.SetActive(true);
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
        }
    }
    
    public void Retry() 
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
    
    public void Quit() 
    {
        Application.Quit();
    }
}
