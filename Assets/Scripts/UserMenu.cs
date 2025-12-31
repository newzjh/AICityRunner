using Cysharp.Threading.Tasks;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class UserMenu : MonoBehaviour
{
    public AIGenerateUserDialog newDialog;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnDefault()
    {
        var global = GameObject.FindFirstObjectByType<Global>(FindObjectsInactive.Include);
        if (global != null) 
        {
            global.CurrentUser = global.DefaultUser;
            global.CurrentSpeed = 24;
        }

        var controller = GameObject.FindFirstObjectByType<SpriteFrameController>(FindObjectsInactive.Include);
        if (controller != null)
        {
            controller.Start();
        }
    }

    public async void OnNew()
    {
        if (newDialog == null)
            return;

        newDialog.gameObject.SetActive(true);

        await UniTask.WaitUntil(() => newDialog.gameObject.activeSelf == false);

        var controller = GameObject.FindFirstObjectByType<SpriteFrameController>(FindObjectsInactive.Include);
        if (controller != null)
        {
            controller.Start();
        }
    }
}
