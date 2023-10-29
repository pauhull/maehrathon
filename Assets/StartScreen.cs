using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartScreen : MonoBehaviour
{

    public GameObject startScreen;
    public GameObject startScene;

    private double videoTime = 0;
    private bool skip = false;

    // Update is called once per frame
    void Update()
    {
        if (videoTime > 0 && (videoTime < (Time.time - 21.5) || skip))
        {
            SceneManager.LoadScene("MainScene");
            SceneManager.LoadScene("Level1", LoadSceneMode.Additive);
        }
        if (Input.GetKeyDown(KeyCode.Return)
            || Input.touches.Length > 0)
        {

            if (videoTime == 0)
            {
                startScreen.SetActive(false);
                startScene.SetActive(true);
                videoTime = Time.time;
            }
            else
            {
                skip = true;
            }
        }
    }
}