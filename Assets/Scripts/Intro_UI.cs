using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Intro_UI : MonoBehaviour
{
    public RawImage[] Maps;
    public GameObject progressUI;
    public GameObject mainMenu;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (mainMenu)
            mainMenu.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Go(int mapIndex)
    {
        Maps[mapIndex].gameObject.SetActive(true);
    }

    public void SwitchRuntimeGeneration(bool value)
    {
        Global.runtimegeneration = value;
    }

    public async void StartPlay(string street)
    {
        if (progressUI)
            progressUI.SetActive(true);

        if (mainMenu)
            mainMenu.SetActive(false);

        await Global.BuildAISceneContent(street);

        if (progressUI)
            progressUI.SetActive(false);

        //if (mainMenu)
        //    mainMenu.SetActive(true);

        await SceneManager.LoadSceneAsync("[PlayScene]");

    }

}
