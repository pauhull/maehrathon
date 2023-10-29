using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class StartScreenAnimation : MonoBehaviour
{

    public GameObject[] worlds;
    public TMP_Text startText;

    private void Start()
    {
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            startText.text = "tap to start";
        }
    }

    void Update()
    {
        for (var i = 0; i < worlds.Length; i++)
        {
            var world = worlds[i];
            var t = Time.time - i * 0.5f;
            var dy = (Mathf.Sin(t) - Mathf.Sin(t - Time.deltaTime)) * 7.5f;
            world.transform.position += Vector3.up * dy;
            var dr = (Mathf.Cos(t) - Mathf.Cos(t - Time.deltaTime)) * 0.5f;
            world.transform.Rotate(0, 0, dr);
        }

        var col = startText.color;
        col.a = Mathf.Abs(Mathf.Sin(Time.time * 1.5f));
        startText.color = col;
    }
}
