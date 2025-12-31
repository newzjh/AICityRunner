using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class Box_Loop : MonoBehaviour {
	
	public GameObject[] Box;
	public GameObject A_Zone;
	public GameObject B_Zone;
	
	public float Speed = 5f;

    public void Start()
    {
		ModifyZoneCoin(A_Zone);
        ModifyZoneCoin(B_Zone);
    }

    void Update () {
	
		MOVE();
	}
	
	//만들자
	
	public void MAKE(){
		
		B_Zone=A_Zone;
		int a = Random.Range(0,5);
        A_Zone = Instantiate(Box[a], new Vector3(30,0,0), transform.rotation) as GameObject;

		PostMake();
    }

	public void PostMake()
	{
		ModifyZoneCoin(A_Zone);
    }

	public void ModifyZoneCoin(GameObject zonego)
	{
        Global global = GameObject.FindFirstObjectByType<Global>();

		var rbs = zonego.GetComponentsInChildren<Rigidbody>();
		foreach (var rb in rbs)
		{
			if (rb == null)
				continue;

            int way = UnityEngine.Random.Range(0, 3);

            if (rb.gameObject.tag == "coin")
			{

				var mr = rb.GetComponentInChildren<MeshRenderer>();

				if (mr == null)
					continue;

				//rb.transform.localPosition += Vector3.up * UnityEngine.Random.Range(0,5);
				rb.transform.localPosition += Vector3.back * 5.0f * way;

				if (global != null && global.Coins != null && global.Coins.Length > 0)
				{
                    GameObject.DestroyImmediate(mr.gameObject);

                    GameObject spritego = new GameObject("Sprite");
					spritego.transform.parent = rb.transform;
					spritego.transform.localScale = Vector3.one * (0.3f - way * 0.03f);
					spritego.transform.localEulerAngles = new Vector3(-5, 0, 0);
					spritego.transform.localPosition = Vector3.down * way * 0.2f + Vector3.forward * way * 1.0f;
					var sr = spritego.AddComponent<SpriteRenderer>();
					sr.material = global.CoinMat;
					sr.sprite = global.Coins[UnityEngine.Random.Range(0, global.Coins.Length - 1)];
				}
			}
		}

		var boxs = zonego.GetComponentsInChildren<BoxCollider>();
		foreach(var box in boxs)
		{
			if (box == null)
				continue;

            if (box.gameObject.tag != "Tile")
                continue;

            var mr = box.GetComponentInChildren<MeshRenderer>();

            if (mr == null)
                continue;

            //mr.transform.localEulerAngles = new Vector3(-10, 0, 0);
            //mr.transform.localPosition = new Vector3(0, 0, 3);

            //         mr.transform.localEulerAngles = new Vector3(-20, 0, 0);
            //         mr.transform.localPosition = new Vector3(0, 0.310000002f, 4.65999985f);
            //mr.transform.localScale = new Vector3(1, 1, 0.45f);

            mr.transform.localEulerAngles = new Vector3(0, 0, 0);
            mr.transform.localPosition = new Vector3(0, 0.3f, 3.08f);
            mr.transform.localScale = new Vector3(1, 1, 0.4f);

            if (Global.bridge)
                mr.material.mainTexture = Global.bridge;
        }
    }

	//움직이자
	
	public void MOVE(){
		
		A_Zone.transform.Translate(Vector3.left * Speed *Time.deltaTime, Space.World);
		B_Zone.transform.Translate(Vector3.left * Speed *Time.deltaTime, Space.World);
			
		if(A_Zone.transform.position.x<=0){
				DEATH();
		}
	}
	
	//없애자
	
	public void DEATH(){
		Destroy(B_Zone);
		MAKE();
			
	}
}
