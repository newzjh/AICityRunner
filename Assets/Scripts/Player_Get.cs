using UnityEngine;

public class Player_Get : MonoBehaviour
{
	
	public Sound_Player _SP;
	public Player_Move _PM;
	public int Get_Coin_Count;
	public Game_Manager _gm;
	public Fade _fade;
	
	void Start(){
	
		_gm = GameObject.FindFirstObjectByType<Game_Manager>(FindObjectsInactive.Include);
		
		GameObject b = GameObject.Find("Black_Screen");
		if(b!=null)_fade = b.GetComponent<Fade>();
		
	}
	
	void OnTriggerEnter (Collider Get)
	{
		DynamicStreetRuntimeItem runtimeItem = Get.GetComponentInParent<DynamicStreetRuntimeItem>();
		if (runtimeItem != null && runtimeItem.TryConsume()) {
			HandleRuntimeItem(Get, runtimeItem);
			return;
		}
       
		if (Get.tag == "coin") {	
			Get.gameObject.SetActive (false);
			Get_Coin_Count += 1;
			if (_gm != null)
				_gm.GETCOIN ();
			
			if (_SP != null)
				_SP.SoundPlay (1);
		}
		
		
		bool gameOver = false;

		if (Get.tag == "DeathZone") {
			gameOver = true;
		}

		//if (!gameOver)
		//{
		//	if (_gm != null)
		//	{
		//		gameOver = _gm.TAKEHIT(1, "默认障碍");
		//	}
		//}

		if (gameOver) {
			HandleGameOver ();
		}

	}

	void HandleRuntimeItem(Collider target, DynamicStreetRuntimeItem runtimeItem)
	{
		switch (runtimeItem.ItemType) {
			case DynamicStreetItemType.ScorePickup:
				target.gameObject.SetActive (false);
				Get_Coin_Count += runtimeItem.ScoreValue;
				if (_gm != null)
					_gm.ADDSCORE (runtimeItem.ScoreValue);
				if (_SP != null)
					_SP.SoundPlay (1);
				break;
			case DynamicStreetItemType.LifePickup:
				target.gameObject.SetActive (false);
				if (_gm != null)
					_gm.GAINLIFE (Mathf.Max (1, runtimeItem.HealthDelta));
				if (_SP != null)
					_SP.SoundPlay (1);
				break;
			case DynamicStreetItemType.SpeedPickup:
				target.gameObject.SetActive (false);
				if (_gm != null)
					_gm.APPLYSPEEDBOOST (runtimeItem.SpeedDelta, runtimeItem.SpeedDuration);
				if (_SP != null)
					_SP.SoundPlay (1);
				break;
			case DynamicStreetItemType.CheckInPickup:
				target.gameObject.SetActive (false);
				if (_gm != null)
					_gm.REGISTERCHECKIN (runtimeItem.ItemLabel);
				if (_SP != null)
					_SP.SoundPlay (1);
				break;
			case DynamicStreetItemType.Obstacle:
				target.gameObject.SetActive (false);
				bool gameOver = true;
				if (_gm != null)
					gameOver = _gm.TAKEHIT (1, runtimeItem.ItemLabel);
				if (gameOver)
					HandleGameOver ();
				break;
		}
	}

	void HandleGameOver ()
	{
		Debug.Log ("Die");
		if (_PM.status != PlayerMoveStatus.Die) {
			_PM.status = PlayerMoveStatus.Die;
			GetComponent<Rigidbody>().AddForce (0, -50f, 0);
			if (_gm != null)
				_gm.GAMEOVER ();
			if (_SP != null)
				_SP.SoundPlay (2);
			if (_fade != null)
				_fade.FadeOut ();
		}
	}
		
}
