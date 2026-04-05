using UnityEngine;
using UnityEngine.UI;

//게임의 진행중인 상태를 정의
public enum GameState
{
	Play,
	Pause,
	End,
}

public class Game_Manager : MonoBehaviour
{
	public GameState GS;
	public int GameLv;
	public float GameSpeed;
	public Box_Loop _BL;
	public Scroll_Mapping _SM;

	//계속 유동적인 수

	public float Meter;
	public int GetMoney = 0;
	public int Lives = 2;
	public int MaxLives = 5;
	public int CheckInCount = 0;
	public float SpeedBoostAmount;
	public float SpeedBoostTime;
	public float HitInvulnerableTime = 1.0f;

	float nextHitAvailableTime;
	string currentCity;


	//GUI 관련
	
	public UnityEngine.UI.Text Gold_Label;
	public UnityEngine.UI.Text Meter_Label;
	public Image Black_screen;
	public UnityEngine.UI.Text result_Gold_Label;
	public UnityEngine.UI.Text result_Meter_Label;
	GUIStyle guiRectStyle;
	public Fade _fade;
	public Texture Pause_btn;
	public Texture Go_btn;
	public Texture Replay_btn;
	public Texture Main_btn;
	public GameObject result_window;
	float screenX;
	float screenY;

    public void Awake()
    {
        var global = GameObject.Find("Global");
        if (global == null)
        {
            global = new GameObject("Global");
			global.AddComponent<Global>();
        }
        DontDestroyOnLoad(global);
    }
    void Start ()
	{		
		GameObject a = GameObject.Find("02_Box_Maker");
		if(a!=null)_BL = a.GetComponent<Box_Loop>();		
		GameObject b = GameObject.Find("01_Sky");
		if(b!=null)_SM = b.GetComponent<Scroll_Mapping>();

		if (Global.bg)
		{
			var mr = b.GetComponent<MeshRenderer>();
            var mat = new Material(mr.sharedMaterial);
            mat.mainTexture = Global.bg;
			mr.material = mat;
			_SM.Calc();
        }
		
		currentCity = Global.CurrentCity;
		GameSpeed = _BL != null ? _BL.Speed : GameSpeed;
		SCREENSETTING ();
		RefreshHUD();
	}

	void Update ()
	{
		if (GS == GameState.Play) {
			METERUPDATE ();
			UPDATESPEEDBOOST();
		}

	}
	
	void SCREENSETTING ()
	{
		screenX = Screen.width;
		screenY = Screen.height;
		guiRectStyle = new GUIStyle ();
		guiRectStyle.border = new RectOffset (0, 0, 0, 0);
		_fade.FadeIn ();
	}
	
	void OnGUI ()
	{
		//플레이상태일때 존재 할 일시정지버튼

		if (GS == GameState.Play) {

			if (GUI.Button (new Rect (20, 20, Pause_btn.width, Pause_btn.height), Pause_btn, guiRectStyle)) {
				Black_screen.color = new Color (0, 0, 0, 0.4f);
				GS = GameState.Pause;
				Time.timeScale = 0;
			}
		}


		//일시정지 상태에 들어가면 버튼을 띄웁니다.

		if (GS == GameState.Pause) {

			if (GUI.Button (new Rect (screenX * 0.5f - Go_btn.width * 0.5f, screenY * 0.5f + Replay_btn.height * 0.5f + 10f, Go_btn.width, Go_btn.height), Go_btn, guiRectStyle)) {
				Black_screen.color = new Color (0, 0, 0, 0);
				Time.timeScale = 1;
				GS = GameState.Play;

			}

			if (GUI.Button (new Rect (screenX * 0.5f - Replay_btn.width * 0.5f, screenY * 0.5f - Replay_btn.height * 0.5f, Replay_btn.width, Replay_btn.height), Replay_btn, guiRectStyle)) {
				Time.timeScale = 1;
				Application.LoadLevel ("[PlayScene]");
			}

			if (GUI.Button (new Rect (screenX * 0.5f - Main_btn.width * 0.5f, screenY * 0.5f - Replay_btn.height * 0.5f - Main_btn.height - 10f, Main_btn.width, Main_btn.height), Main_btn, guiRectStyle)) {
				Time.timeScale = 1;
				Application.LoadLevel ("[IntroScene]");
			}
		}

		//게임이 종료됐을시 띄울 버튼

		if (GS == GameState.End) {
			if (GUI.Button (new Rect (screenX * 0.5f - Replay_btn.width * 0.5f, screenY * 0.5f + 10f, Replay_btn.width, Replay_btn.height), Replay_btn, guiRectStyle)) {
				Application.LoadLevel ("[PlayScene]");
			}

			if (GUI.Button (new Rect (screenX * 0.5f - Main_btn.width * 0.5f, screenY * 0.5f + Replay_btn.height + 20f, Main_btn.width, Main_btn.height), Main_btn, guiRectStyle)) {
				Application.LoadLevel ("[IntroScene]");
			}
		}
	}
   
	public void GAMEOVER ()
	{
		GS = GameState.End;
		_fade.FadeOut ();
		result_window.gameObject.SetActive (true);
		result_Gold_Label.text = string.Format ("{0:N0}", GetMoney);
		result_Meter_Label.text = string.Format ("{0:N0}", Meter);
	}
	
	public void GETCOIN ()
	{
		ADDSCORE (1);
	}

	public void ADDSCORE (int amount)
	{
		GetMoney += amount;
		RefreshHUD ();
	}

	public void GAINLIFE (int amount)
	{
		Lives = Mathf.Clamp (Lives + amount, 0, MaxLives);
		RefreshHUD ();
	}

	public void REGISTERCHECKIN (string placeName)
	{
		CheckInCount += 1;
		ADDSCORE (3);
		Debug.Log ($"[CheckIn] {Global.CurrentCity} - {placeName}");
	}

	public void APPLYSPEEDBOOST (float bonusSpeed, float duration)
	{
		SpeedBoostAmount = Mathf.Max (SpeedBoostAmount, bonusSpeed);
		SpeedBoostTime = Mathf.Max (SpeedBoostTime, duration);
		REFRESHRUNNERSPEED ();
	}

	public bool TAKEHIT (int damage, string sourceName)
	{
		//if (GS == GameState.End) {
		//	return true;
		//}

		if (Time.time < nextHitAvailableTime) {
			return false;
		}

		nextHitAvailableTime = Time.time + HitInvulnerableTime;
		Lives = Mathf.Clamp (Lives - damage, 0, MaxLives);
		Debug.Log ($"[Hit] {sourceName}, hp={Lives}");
		RefreshHUD ();

		if (Lives <= 0) {
			return true;
		}

		return false;
	}

	public void METERUPDATE ()
	{
		float currentSpeed = GameSpeed + SpeedBoostAmount;
		Meter += Time.deltaTime * currentSpeed;
		Meter_Label.text = string.Format ("{0:N0}<color=#ff3366> m</color>", Meter);

		//시간이 지날수록 속도가 점점 빨라지게 한다.

		if (Meter >= 50 && GameLv == 1) {
			GameLevelUp ();
		}

		if (Meter >= 100 && GameLv == 2) {
			GameLevelUp ();
		}

		if (Meter >= 150 && GameLv == 3) {
			GameLevelUp ();
		}

		if (Meter >= 200 && GameLv == 4) {
			GameLevelUp ();
		}

		if (Meter >= 250 && GameLv == 5) {
			GameLevelUp ();
		}

		if (Meter >= 300 && GameLv == 6) {
			GameLevelUp ();
		}
	}

	public void GameLevelUp ()
	{
		GameLv += 1;
		//GameSpeed += 3;
		//_SM.ScrollSpeed += 0.1f;
		REFRESHRUNNERSPEED ();
	}

	void UPDATESPEEDBOOST ()
	{
		if (SpeedBoostTime <= 0f) {
			return;
		}

		SpeedBoostTime -= Time.deltaTime;
		if (SpeedBoostTime <= 0f) {
			SpeedBoostTime = 0f;
			SpeedBoostAmount = 0f;
			REFRESHRUNNERSPEED ();
		}
	}

	void REFRESHRUNNERSPEED ()
	{
		if (_BL != null) {
			_BL.Speed = GameSpeed + SpeedBoostAmount;
		}
	}

	void RefreshHUD ()
	{
		if (Gold_Label != null) {
			if (string.IsNullOrEmpty (currentCity))
				Gold_Label.text = string.Format ("{0:N0}  HP:{1}  打卡:{2}", GetMoney, Lives, CheckInCount);
			else
				Gold_Label.text = string.Format ("{0:N0}  HP:{1}  打卡:{2}  {3}", GetMoney, Lives, CheckInCount, currentCity);
		}
	}
}
