using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

public class Intro_GUI_Manager : MonoBehaviour
{
	public Fade _fade;
	public Texture Play_btn;
	GUIStyle guiRectStyle;
	float screenX;
	float screenY;
	public bool _click;

    public void Awake()
    {
		var global = GameObject.Find("Global");
		if (global == null)
		{
			global = new GameObject("Global");
            global.AddComponent<Global>();
        }
        DontDestroyOnLoad(global);
		Application.targetFrameRate = 60;
    }

    void Start ()
	{
		_fade.FadeIn ();
		SCREENSETTING ();
		_click = false;

    }
	
	void Update ()
	{
	}

	public async void StartPlay()
	{
        Debug.Log("gotoPlay");
		_click = true;

        await Global.BuildAISceneContent("π„÷›");
		_fade.FadeOut();
		await UniTask.WaitForSeconds(_fade.Fade_Time);

        await SceneManager.LoadSceneAsync("[PlayScene]");

        _click = false;
    }
	
	void SCREENSETTING ()
	{
		
		screenX = Screen.width;
		screenY = Screen.height;	
		guiRectStyle = new GUIStyle ();
		guiRectStyle.border = new RectOffset (0, 0, 0, 0);
        
	}
	
	void OnGUI ()
	{
  //      screenX = Screen.width;
  //      screenY = Screen.height;
  //      if (!_click)
		//{
		//	if (GUI.Button(new Rect(screenX * 0.5f - Play_btn.width * 0.5f, screenY * 0.5f + Play_btn.height * 0.5f + 100f, Play_btn.width, Play_btn.height), Play_btn, guiRectStyle))
		//	{
		//		StartPlay();
  //          }
		//}
	}
}