using UnityEngine;
using UnityEngine.UI;

public class Intro_UI : MonoBehaviour
{
    public RawImage[] Maps;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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

}
