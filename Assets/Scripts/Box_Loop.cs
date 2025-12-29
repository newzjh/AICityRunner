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
        if (global == null || global.Coins == null || global.Coins.Length < 1)
            return;

        var rbs = zonego.GetComponentsInChildren<Collider>();
        foreach (var rb in rbs)
        {
            if (rb == null)
                continue;

			if (rb.gameObject.tag == "coin")
			{

				var mr = rb.GetComponentInChildren<MeshRenderer>();

				if (mr == null)
					continue;

				GameObject.DestroyImmediate(mr.gameObject);

				rb.transform.localPosition += Vector3.down;

				GameObject spritego = new GameObject("Sprite");
				spritego.transform.parent = rb.transform;
				spritego.transform.localScale = Vector3.one * 0.3f;
				spritego.transform.localEulerAngles = new Vector3(-15, 0, 0);
				spritego.transform.localPosition = Vector3.zero;
				var sr = spritego.AddComponent<SpriteRenderer>();
				sr.material = global.CoinMat;
				sr.sprite = global.Coins[UnityEngine.Random.Range(0, global.Coins.Length - 1)];

			}

            else if (rb.gameObject.tag == "Tile")
            {

                var mr = rb.GetComponentInChildren<MeshRenderer>();

                if (mr == null)
                    continue;

				if (Global.bridge)
					mr.material.mainTexture = Global.bridge;

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
            mr.transform.localPosition = new Vector3(0, 0.3f, 3.3f);
            mr.transform.localScale = new Vector3(1, 1, 0.4f);
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
