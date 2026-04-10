using UnityEngine;
using System;
using System.Collections.Generic;

public class Box_Loop : MonoBehaviour {
	
	public GameObject[] Box;
	public GameObject A_Zone;
	public GameObject B_Zone;
	
	public float Speed = 5f;
	const string DynamicRootName = "__DynamicStreetContent";
	const string FloorBricksRootName = "__FloorBricks";
	const float LaneOffset = 5f;
	const float SpawnXMin = 4.5f;
	const float SpawnXMax = 24.5f;
	const float MinLaneSpacing = 3f;

	Player_Move player = null;

    public void Start()
    {
		player = GameObject.FindFirstObjectByType<Player_Move>(FindObjectsInactive.Include);
		ModifyZoneCoin(A_Zone);
        ModifyZoneCoin(B_Zone);
    }

    void Update () {
	
		MOVE();
	}
	
	//만들자
	
	public void MAKE(){
		
		B_Zone=A_Zone;
		int a = UnityEngine.Random.Range(0,5);
        A_Zone = Instantiate(Box[a], new Vector3(30,0,0), transform.rotation) as GameObject;

		PostMake();
    }

	public void PostMake()
	{
		ModifyZoneCoin(A_Zone);
    }

	public void ModifyZoneCoin(GameObject zonego)
	{
		if (zonego == null)
			return;

		Global global = GameObject.FindFirstObjectByType<Global>();

		var rbs = zonego.GetComponentsInChildren<Rigidbody>();
		foreach (var rb in rbs)
		{
			if (rb == null)
				continue;

            int way = UnityEngine.Random.Range(0, 3);

            if (rb.gameObject.tag == "coin")
			{
				DynamicStreetRuntimeItem runtimeItem = rb.GetComponent<DynamicStreetRuntimeItem>();
				if (runtimeItem == null)
				{
					runtimeItem = rb.gameObject.AddComponent<DynamicStreetRuntimeItem>();
					runtimeItem.ItemType = DynamicStreetItemType.ScorePickup;
					runtimeItem.CityName = Global.CurrentCity;
					runtimeItem.ScoreValue = 1;
				}

                runtimeItem.BobAmplitude = UnityEngine.Random.Range(0.1f, 0.25f);
                runtimeItem.BobFrequency = UnityEngine.Random.Range(1.0f, 3.0f);
				runtimeItem.SpinSpeed = UnityEngine.Random.Range(0,100)>50? UnityEngine.Random.Range(45.0f, 125.0f) : UnityEngine.Random.Range(-45.0f, -125.0f);

                runtimeItem.EnableBob = true;
				runtimeItem.EnableSpin = true;

				var mr = rb.GetComponentInChildren<MeshRenderer>();

				if (mr == null)
					continue;

				//rb.transform.localPosition += Vector3.up * UnityEngine.Random.Range(0,5);
				rb.transform.localPosition += Vector3.back * 5.0f * way;

				if (global != null && global.Coins != null && global.Coins.Length > 0)
				{
                    DestroyRuntimeObject(mr.gameObject);

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

			BuildFloorBricks(box, global);
        }

		if (!zonego.name.Contains("box_Loop_Coin"))
			BuildDynamicCityContent(zonego);
    }

	void BuildFloorBricks(BoxCollider tileBox, Global global)
	{
		if (tileBox == null)
			return;

		Transform tileTransform = tileBox.transform;
		Transform oldRoot = tileTransform.Find(FloorBricksRootName);
		if (oldRoot != null)
			DestroyRuntimeObject(oldRoot.gameObject);

		GameObject rootGo = new GameObject(FloorBricksRootName);
		rootGo.transform.SetParent(tileTransform, false);

		Bounds bounds = tileBox.bounds;
		float totalLen = bounds.size.x;
		float totalWidth = bounds.size.z;

		float brickLen = Mathf.Clamp(totalLen / 18f, 0.6f, 1.6f);
		float brickThickness = Mathf.Clamp(bounds.size.y * 0.2f, 0.06f, 0.2f);
		float laneWidth = Mathf.Clamp(totalWidth / 3f, 1.5f, 4.5f);

		float startX = bounds.min.x + brickLen * 0.5f;
		float endX = bounds.max.x - brickLen * 0.5f;
		int brickCount = Mathf.Clamp(Mathf.FloorToInt((endX - startX) / brickLen) + 1, 0, 240);

		float y = bounds.max.y - brickThickness * 0.5f - 0.01f;

		Vector3 desiredWorldScale = new Vector3(brickLen, brickThickness, laneWidth);
		Vector3 parentWorldScaleAbs = GetWorldScaleAbs(rootGo.transform);
		Vector3 localScale = Vector3.one * 0.45f;// GetLocalScaleForDesiredWorldScale(desiredWorldScale, parentWorldScaleAbs);
		localScale.z = 0.30f;

		float zMin = bounds.min.z + laneWidth * 0.5f;
		float zMax = bounds.max.z - laneWidth * 0.5f;
		if (zMax < zMin)
		{
			zMin = bounds.center.z;
			zMax = bounds.center.z;
		}
		zMin = 0;
		zMax = -10;

		for (int lane = 0; lane < 3; lane++)
		{
			float t = lane / 2f;
			float z = Mathf.Lerp(zMax, zMin, t);

			for (int i = 0; i < brickCount; i++)
			{
				float x = startX + i * brickLen;
				Vector3 worldPos = new Vector3(x, y, z);

				GameObject brick = GameObject.CreatePrimitive(PrimitiveType.Cube);
				brick.name = "FloorBrick";
				brick.transform.SetParent(rootGo.transform, true);
				brick.transform.position = worldPos;
				brick.transform.rotation = Quaternion.identity;
				brick.transform.localScale = localScale;

				Collider collider = brick.GetComponent<Collider>();
				if (collider != null)
					collider.enabled = false;

				Renderer renderer = brick.GetComponent<Renderer>();
				if (renderer != null && global != null && global.FloorMat != null)
					renderer.sharedMaterial = global.FloorMat;
			}
		}
	}

	[NonSerialized]
	public float detectdistance = 0.6f;
	//움직이자
	
	public void MOVE(){

		bool block = false;
		if (player)
		{
			//player.transform.position
			var playerbounds = player.GetComponent<CapsuleCollider>();
			var worldpos = player.transform.TransformPoint(playerbounds.center + Vector3.down * playerbounds.height * 0.4f);
			var hits = Physics.RaycastAll(worldpos, Vector3.right, detectdistance);
			if (hits!=null)
			{
				foreach(var hit in hits)
				{
					if (hit.collider == null)
						continue;

					if (hit.collider.gameObject.tag=="Tile")
						block = true;
				}
			}
		}

		if (!block)
		{
			A_Zone.transform.Translate(Vector3.left * Speed * Time.deltaTime, Space.World);
			B_Zone.transform.Translate(Vector3.left * Speed * Time.deltaTime, Space.World);
		}

		if (A_Zone.transform.position.x<=0){
				DEATH();
		}
	}
	
	//없애자
	
	public void DEATH(){
		Destroy(B_Zone);
		MAKE();
			
	}

	void BuildDynamicCityContent(GameObject zonego)
	{
		Transform oldRoot = zonego.transform.Find(DynamicRootName);
		if (oldRoot != null)
			DestroyRuntimeObject(oldRoot.gameObject);

		GameObject rootGo = new GameObject(DynamicRootName);
		rootGo.transform.SetParent(zonego.transform, false);

		CityRuntimeProfile profile = CityRuntimeContent.ResolveProfile(Global.CurrentStreet);
		List<float>[] laneUsage = new List<float>[3]
		{
			new List<float>(),
			new List<float>(),
			new List<float>()
		};

		int scoreCount = UnityEngine.Random.Range(2, 5);
		for (int i = 0; i < scoreCount; i++)
		{
			SpawnInteractiveItem(rootGo.transform, laneUsage, profile, DynamicStreetItemType.ScorePickup);
		}

		int obstacleCount = UnityEngine.Random.Range(1, 3);
		for (int i = 0; i < obstacleCount; i++)
		{
			SpawnInteractiveItem(rootGo.transform, laneUsage, profile, DynamicStreetItemType.Obstacle);
		}

		if (UnityEngine.Random.value < 0.45f)
			SpawnInteractiveItem(rootGo.transform, laneUsage, profile, DynamicStreetItemType.LifePickup);

		if (UnityEngine.Random.value < 0.55f)
			SpawnInteractiveItem(rootGo.transform, laneUsage, profile, DynamicStreetItemType.SpeedPickup);

		if (UnityEngine.Random.value < 0.65f)
			SpawnInteractiveItem(rootGo.transform, laneUsage, profile, DynamicStreetItemType.CheckInPickup);

		List<DynamicStreetItemType> extraTypes = new List<DynamicStreetItemType>(CityRuntimeContent.ExtraSceneryTypes);
		Shuffle(extraTypes);
		foreach (DynamicStreetItemType extraType in extraTypes)
		{
			SpawnDecorationItem(rootGo.transform, laneUsage, profile, extraType);
		}
	}

	void SpawnInteractiveItem(Transform root, List<float>[] laneUsage, CityRuntimeProfile profile, DynamicStreetItemType itemType)
	{
		int lane = UnityEngine.Random.Range(0, 2);
		if (!TryReserveLanePosition(laneUsage, lane, out Vector3 localPosition, 10, MinLaneSpacing))
			return;

		//localPosition.y = itemType == DynamicStreetItemType.Obstacle ? 0.8f : 1.4f;

		PrimitiveType primitiveType = PrimitiveType.Sphere;
		Vector3 scale = Vector3.one * 0.9f;
		switch (itemType)
		{
			case DynamicStreetItemType.Obstacle:
				primitiveType = PrimitiveType.Cube;
				scale = new Vector3(1.2f, 1.3f, 1.2f);
				break;
			case DynamicStreetItemType.LifePickup:
				primitiveType = PrimitiveType.Capsule;
				scale = new Vector3(0.9f, 1.1f, 0.9f);
				break;
			case DynamicStreetItemType.SpeedPickup:
				primitiveType = PrimitiveType.Cylinder;
				scale = new Vector3(0.9f, 0.35f, 0.9f);
				break;
			case DynamicStreetItemType.CheckInPickup:
				primitiveType = PrimitiveType.Cube;
				scale = new Vector3(0.95f, 0.95f, 0.95f);
				break;
		}
        scale = Vector3.one * 0.5f;

        string label = CityRuntimeContent.PickLabel(profile, itemType);
		Color bodyColor = GetBodyColor(profile, itemType);
		Color accentColor = GetAccentColor(profile, itemType);

		GameObject prop = GameObject.CreatePrimitive(primitiveType);
		prop.name = itemType.ToString();
		prop.transform.SetParent(root, false);
		prop.transform.localPosition = localPosition;
		prop.transform.localScale = scale;
		prop.tag = "coin";// itemType == DynamicStreetItemType.Obstacle ? "Obstacle" : "coin";

		Collider collider = prop.GetComponent<Collider>();
		if (collider != null)
			collider.isTrigger = true;

		Rigidbody rigidbody = prop.AddComponent<Rigidbody>();
		rigidbody.useGravity = false;
		rigidbody.isKinematic = true;

		Renderer renderer = prop.GetComponent<Renderer>();
		ConfigureRenderer(renderer, bodyColor);
		SetRendererVisible(renderer, false);

		DynamicStreetRuntimeItem runtimeItem = prop.AddComponent<DynamicStreetRuntimeItem>();
		runtimeItem.ItemType = itemType;
		runtimeItem.ItemLabel = label;
		runtimeItem.CityName = profile.CityName;
		runtimeItem.ScoreValue = itemType == DynamicStreetItemType.ScorePickup ? 1 : 2;
		runtimeItem.HealthDelta = itemType == DynamicStreetItemType.LifePickup ? 1 : 0;
		runtimeItem.SpeedDelta = itemType == DynamicStreetItemType.SpeedPickup ? 3f : 0f;
		runtimeItem.SpeedDuration = itemType == DynamicStreetItemType.SpeedPickup ? 4f : 0f;
		runtimeItem.EnableSpin = itemType != DynamicStreetItemType.Obstacle;
		runtimeItem.EnableBob = itemType != DynamicStreetItemType.Obstacle;
		CreateGeneratedSpriteVisual(prop.transform, itemType, label, bodyColor, accentColor, false);
	}

	void SpawnDecorationItem(Transform root, List<float>[] laneUsage, CityRuntimeProfile profile, DynamicStreetItemType itemType)
	{
		int lane = UnityEngine.Random.Range(0, 3);
		if (!TryReserveLanePosition(laneUsage, lane, out Vector3 localPosition, 8, MinLaneSpacing - 0.5f))
			return;

		localPosition.y = 0.7f;
		//localPosition.z += UnityEngine.Random.value > 0.5f ? 1.7f : -1.7f;

		PrimitiveType primitiveType = PrimitiveType.Cube;
		Vector3 scale = new Vector3(1.4f, 0.8f, 0.6f);
		switch (itemType)
		{
			case DynamicStreetItemType.ArcadeSign:
				scale = new Vector3(0.45f, 2.1f, 0.35f);
				break;
			case DynamicStreetItemType.SharedBikeSpot:
				scale = new Vector3(1.6f, 0.45f, 0.5f);
				break;
			case DynamicStreetItemType.FlowerMarket:
				scale = new Vector3(1.2f, 0.7f, 0.7f);
				break;
			case DynamicStreetItemType.TransitStop:
				scale = new Vector3(0.5f, 1.9f, 0.5f);
				break;
		}

		string label = CityRuntimeContent.PickLabel(profile, itemType);
		GameObject prop = GameObject.CreatePrimitive(primitiveType);
		prop.name = itemType.ToString();
		prop.transform.SetParent(root, false);
		prop.transform.localPosition = localPosition;
		prop.transform.localScale = scale;

		Collider collider = prop.GetComponent<Collider>();
		if (collider != null)
			collider.enabled = false;

		Renderer renderer = prop.GetComponent<Renderer>();
		ConfigureRenderer(renderer, GetBodyColor(profile, itemType));
		SetRendererVisible(renderer, false);
		CreateGeneratedSpriteVisual(prop.transform, itemType, label, GetBodyColor(profile, itemType), GetAccentColor(profile, itemType), true);
	}

	bool TryReserveLanePosition(List<float>[] laneUsage, int lane, out Vector3 localPosition, int attempts, float minSpacing)
	{
		for (int i = 0; i < attempts; i++)
		{
			float x = UnityEngine.Random.Range(SpawnXMin, SpawnXMax);
			bool occupied = false;
			for (int j = 0; j < laneUsage[lane].Count; j++)
			{
				if (Mathf.Abs(laneUsage[lane][j] - x) < minSpacing)
				{
					occupied = true;
					break;
				}
			}

			if (occupied)
				continue;

			laneUsage[lane].Add(x);
			localPosition = new Vector3(x, 0f, lane * -LaneOffset);
			return true;
		}

		localPosition = Vector3.zero;
		return false;
	}

	void ConfigureRenderer(Renderer renderer, Color color)
	{
		if (renderer == null)
			return;

		Material material = new Material(renderer.sharedMaterial);
		material.color = color;
		renderer.material = material;
	}

	void SetRendererVisible(Renderer renderer, bool visible)
	{
		if (renderer == null)
			return;

		renderer.enabled = visible;
	}

	void CreateGeneratedSpriteVisual(Transform parent, DynamicStreetItemType itemType, string label, Color bodyColor, Color accentColor, bool decorative)
	{
		GameObject visualGo = new GameObject("GeneratedSprite");
		visualGo.transform.SetParent(parent, false);
		visualGo.transform.localPosition = Vector3.zero;// decorative ? new Vector3(0f, 0.85f, 0.4f) : new Vector3(0f, 0.1f, 0.45f);
		visualGo.transform.localEulerAngles = new Vector3(-5f, 0f, 0f);
		Vector3 desiredWorldScale = Vector3.one * 0.4f;// decorative ? new Vector3(2.2f, 1.8f, 1f) : new Vector3(1.6f, 1.6f, 1f);
		Vector3 parentWorldScaleAbs = GetWorldScaleAbs(parent);
		visualGo.transform.localScale = GetLocalScaleForDesiredWorldScale(desiredWorldScale, parentWorldScaleAbs);

		SpriteRenderer spriteRenderer = visualGo.AddComponent<SpriteRenderer>();
		spriteRenderer.sprite = DynamicStreetSpriteFactory.GetSprite(itemType, label, bodyColor, accentColor, decorative);
		spriteRenderer.sortingOrder = decorative ? 10 : 20;
		var resolver = visualGo.AddComponent<DynamicStreetVisualResolver>();
		resolver.TargetRenderer = spriteRenderer;
		resolver.ItemType = itemType;
		resolver.ItemLabel = label;
		resolver.PrimaryColor = bodyColor;
		resolver.AccentColor = accentColor;
		resolver.Decorative = decorative;

		if (!string.IsNullOrEmpty(label))
			CreateLabel(parent, label, Color.white, decorative);
	}

	Vector3 GetLocalScaleForDesiredWorldScale(Vector3 desiredWorldScale, Vector3 parentWorldScaleAbs)
	{
		const float eps = 0.00001f;
		float px = Mathf.Abs(parentWorldScaleAbs.x) < eps ? 1f : parentWorldScaleAbs.x;
		float py = Mathf.Abs(parentWorldScaleAbs.y) < eps ? 1f : parentWorldScaleAbs.y;
		float pz = Mathf.Abs(parentWorldScaleAbs.z) < eps ? 1f : parentWorldScaleAbs.z;
		return new Vector3(desiredWorldScale.x / px, desiredWorldScale.y / py, desiredWorldScale.z / pz);
	}

	Vector3 GetWorldScaleAbs(Transform t)
	{
		Vector3 scale = Vector3.one;
		Transform current = t;
		while (current != null)
		{
			Vector3 local = current.localScale;
			scale = Vector3.Scale(scale, new Vector3(Mathf.Abs(local.x), Mathf.Abs(local.y), Mathf.Abs(local.z)));
			current = current.parent;
		}

		return scale;
	}

	void AddAccent(Transform parent, Color accentColor, DynamicStreetItemType itemType)
	{
		GameObject accent = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		accent.name = "Accent";
		accent.transform.SetParent(parent, false);
		accent.transform.localPosition = Vector3.up * 0.8f;
		accent.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);

		switch (itemType)
		{
			case DynamicStreetItemType.Obstacle:
				accent.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
				accent.transform.localPosition = Vector3.up * 0.95f;
				break;
			case DynamicStreetItemType.SpeedPickup:
				accent.transform.localScale = new Vector3(0.55f, 0.18f, 0.55f);
				break;
			case DynamicStreetItemType.SharedBikeSpot:
				accent.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
				accent.transform.localPosition = new Vector3(-0.35f, -0.15f, 0f);
				CreateSideAccent(parent, new Vector3(0.35f, -0.15f, 0f), accentColor);
				break;
			case DynamicStreetItemType.TransitStop:
				accent.transform.localScale = new Vector3(1.2f, 0.18f, 0.55f);
				accent.transform.localPosition = new Vector3(0f, 0.75f, 0f);
				break;
		}

		Collider collider = accent.GetComponent<Collider>();
		if (collider != null)
			collider.enabled = false;

		ConfigureRenderer(accent.GetComponent<Renderer>(), accentColor);
	}

	void CreateSideAccent(Transform parent, Vector3 localPosition, Color accentColor)
	{
		GameObject accent = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		accent.name = "Accent2";
		accent.transform.SetParent(parent, false);
		accent.transform.localPosition = localPosition;
		accent.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
		Collider collider = accent.GetComponent<Collider>();
		if (collider != null)
			collider.enabled = false;
		ConfigureRenderer(accent.GetComponent<Renderer>(), accentColor);
	}

	void CreateLabel(Transform anchor, string label, Color color, bool decorative)
	{
		GameObject labelGo = new GameObject("Label");
		TextMesh textMesh = labelGo.AddComponent<TextMesh>();
		Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
		if (font == null)
			font = Resources.GetBuiltinResource<Font>("Arial.ttf");
		textMesh.font = font;
		textMesh.text = label;
		textMesh.fontSize = 40;
		textMesh.characterSize = 0.12f;
		textMesh.anchor = TextAnchor.MiddleCenter;
		textMesh.alignment = TextAlignment.Center;
		textMesh.color = color;
		MeshRenderer renderer = labelGo.GetComponent<MeshRenderer>();
		if (renderer != null && font != null)
		{
			renderer.sharedMaterial = font.material;
			renderer.sortingOrder = decorative ? 30 : 40;
		}
		var labelFollow = labelGo.AddComponent<DynamicStreetWorldLabel>();
		labelFollow.Anchor = anchor;
		labelFollow.WorldOffset = decorative ? new Vector3(0f, 1.8f, 0f) : new Vector3(0f, 1.25f, 0f);
		labelFollow.UniformScale = decorative ? 0.03f : 0.026f;
	}

	Color GetBodyColor(CityRuntimeProfile profile, DynamicStreetItemType itemType)
	{
		switch (itemType)
		{
			case DynamicStreetItemType.Obstacle:
				return Color.Lerp(profile.PrimaryColor, Color.black, 0.35f);
			case DynamicStreetItemType.LifePickup:
				return Color.Lerp(profile.AccentColor, Color.white, 0.35f);
			case DynamicStreetItemType.SpeedPickup:
				return Color.Lerp(profile.PrimaryColor, Color.cyan, 0.25f);
			case DynamicStreetItemType.CheckInPickup:
				return Color.Lerp(profile.AccentColor, new Color(1f, 0.6f, 0.1f), 0.4f);
			default:
				return profile.PrimaryColor;
		}
	}

	Color GetAccentColor(CityRuntimeProfile profile, DynamicStreetItemType itemType)
	{
		switch (itemType)
		{
			case DynamicStreetItemType.Obstacle:
				return new Color(0.95f, 0.42f, 0.18f);
			case DynamicStreetItemType.ScorePickup:
				return profile.AccentColor;
			case DynamicStreetItemType.LifePickup:
				return new Color(0.38f, 0.9f, 0.52f);
			case DynamicStreetItemType.SpeedPickup:
				return new Color(0.3f, 0.9f, 1f);
			default:
				return profile.AccentColor;
		}
	}

	void Shuffle<T>(List<T> values)
	{
		for (int i = values.Count - 1; i > 0; i--)
		{
			int swapIndex = UnityEngine.Random.Range(0, i + 1);
			T temp = values[i];
			values[i] = values[swapIndex];
			values[swapIndex] = temp;
		}
	}

	void DestroyRuntimeObject(UnityEngine.Object target)
	{
		if (target == null)
			return;

		if (Application.isPlaying)
			Destroy(target);
		else
			DestroyImmediate(target);
	}
}
