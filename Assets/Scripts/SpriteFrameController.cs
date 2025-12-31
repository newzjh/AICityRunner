using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.U2D;

public class SpriteFrameController : MonoBehaviour
{
    SpriteRenderer r;
    Sprite[] ss;
    public SpriteAtlas atlas;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        var global = GameObject.FindFirstObjectByType<Global>(FindObjectsInactive.Include);

        r = GetComponent<SpriteRenderer>();
        if (atlas != null && atlas.spriteCount>0)
        {
            ss = new Sprite[atlas.spriteCount];
            atlas.GetSprites(ss);
            List<Sprite> ss2 = new();
            ss2.AddRange(ss);
            ss2.OrderBy(n => n.name);
            ss = ss2.ToArray();
        }
        if (global!=null && global.CurrentUser != null)
        {
            ss = global.CurrentUser.sprites;
            speed2 = global.CurrentSpeed;
        }
    }

    private float time = 0;
    public float speed = 0.02f;
    public float speed2 = 24.0f;
    public int index1 = 0;
    public int index2 = 0;
    // Update is called once per frame
    void Update()
    {
        if (ss == null || ss.Length <= 0)
            return;

        time += Time.deltaTime;
        int frameCount = Mathf.RoundToInt (time * speed2);
        index1 = frameCount % ss.Length;

        //if (speed > 0)
        //{
        //    while (time > speed)
        //    {
        //        time -= speed;
        //        index1 = (index1 + 1) % ss.Length;
        //    }
        //}

        //index2 = index1;
        //index1 = (index2 - 1 + ss.Length) % ss.Length;
        r.sprite = ss[index1];
    }
}
