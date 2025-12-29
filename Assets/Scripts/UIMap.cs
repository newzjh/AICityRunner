using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIMap : MonoBehaviour
{
    public GameObject progressUI;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public async void StartPlay(string street)
    {
        gameObject.SetActive(false);

        if (progressUI)
            progressUI.SetActive(true);

        await Global.BuildAISceneContent(street);

        if (progressUI)
            progressUI.SetActive(false);

        await SceneManager.LoadSceneAsync("[PlayScene]");

    }

    public void OnClose()
    {
        gameObject.SetActive(false);
    }
}
