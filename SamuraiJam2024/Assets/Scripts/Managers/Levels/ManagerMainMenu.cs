using UnityEngine;
using UnityEngine.SceneManagement; 


public enum Menu
{
   Main, 
   Settings, 
}

public class ManagerMainMenu : MonoBehaviour
{

    public Menu SelectedMenu;
    public Camera Camera; 

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 eulers = Camera.main.transform.eulerAngles;
        switch (SelectedMenu)
        {
            case Menu.Main:
                Camera.transform.eulerAngles = Vector3.Lerp(eulers, Vector3.zero, Time.deltaTime * 10.0f);

                break; 
            case Menu.Settings:
                Camera.transform.eulerAngles = Vector3.Lerp(eulers, Vector3.up * 90, Time.deltaTime * 10.0f);
                break; 
        } 
    }

    public void TransitionSettings()
    {
        
        SelectedMenu = Menu.Settings; 
    }

    public void TransitionMain()
    {
        SelectedMenu = Menu.Main; 
        
    }

    public void Play()
    {
        SceneManager.LoadScene(1); 
    }

    public void Quit()
    {
        Application.Quit();
    }
}
