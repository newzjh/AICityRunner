using UnityEngine;
using System.Collections;

public class Scroll_Mapping : MonoBehaviour
{
	
	public float ScrollSpeed = 0.03f;
	float Offset;

	Renderer r;
    private void Awake()
    {
        r = GetComponent<Renderer>();
    }

	public void Calc()
	{
        r = GetComponent<Renderer>();

        var tex = r.material.mainTexture;
        float s = 2.0f / ((float)tex.width / (float)tex.height) * 2.0f;
        r.material.mainTextureScale = new Vector2(s, 1.0f);
    }

    void Update ()
	{
		if (r == null)
			return;
	
		Offset += Time.deltaTime * ScrollSpeed;
        r.material.mainTextureOffset = new Vector2 (Offset, 0.01f);
		
	}
}



