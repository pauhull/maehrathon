using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using TMPro;
using Unity.Properties;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

[Serializable]
public class Level
{
    public GameObject backgroundPrefab;
    public GameObject[] obstaclePrefabs;
    public float nextLevelScore;
    public Texture2D groundTexture;
    public AudioClip music;
}

public class GameController : MonoBehaviour
{

    public Level[] levels;

    private const float InitalSpeed = 30.0f;
    private float _speed = InitalSpeed;
    private float _score = 0.0f;
    public TMP_Text scoreText;
    
    private readonly List<GameObject> _obstacles = new();
    
    private float _timeUntilNextObstacle;

    private bool _gameStopped = false;
    private bool _transitionActive = false;

    public GameObject gameEndedPanel;
    public GameObject changeLevelPanel;
    
    public GameObject transitionPanel;
    
    public PlayerController playerController;

    private GameObject _backgroundGroup;

    private float _bgWidth;

    public float gameOverTime = 0;
    private float _transitionTime = 0;

    [CanBeNull] private Level currentLevel;

    public AudioSource levelUp;
    
    public AudioSource[] maehen;
    private float _maehenTime;
    private float _maehenWaitTime;

    private bool _changeLevels = true;

    public int fixLevel = -1;

    public GameObject ground;
    public Material groundMat;
    
    private int _currentLevelIndex = 0;

    private AudioSource _audioSource;
    
    private void Start()
    {

        _audioSource = GetComponent<AudioSource>();
        
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            gameEndedPanel.transform.Find("Mobile").gameObject.SetActive(true);
        }
        else
        {
            gameEndedPanel.transform.Find("Desktop").gameObject.SetActive(true);
        }
        
        SetLevel(0);
        
        SpawnObstacle();
        PickNextObstacleTime();
    }

    public bool GameIsRunning()
    {
        return !_gameStopped;
    }

    void playMaehSound()
    {
        _maehenTime = Time.time;
        _maehenWaitTime = Random.Range(5, 10);
        maehen[Random.Range(0, maehen.Length)].Play();
    }
    void Update()
    {

        if (_gameStopped)
        {
            return;
        }

        if (_transitionActive)
        {
            if (_transitionTime < Time.time - 1.5)
            {
                transitionPanel.SetActive(false);
                _transitionActive = false;
            }
        }

        if (_changeLevels && _score > currentLevel.nextLevelScore)
        {
            _currentLevelIndex++;
            SetLevel(_currentLevelIndex);
        }

        if (_maehenTime < Time.time - _maehenWaitTime)
        {
            playMaehSound();
        }

        if (_changeLevels)
        {
            _score += Time.deltaTime * _speed * 0.5f;
        }
        _timeUntilNextObstacle -= Time.deltaTime;
        _speed += Time.deltaTime * 0.4f;
        _speed = Mathf.Min(180f, _speed);

        _backgroundGroup.transform.position += Vector3.left * (Time.deltaTime * _speed * 0.01f);
        if (_backgroundGroup.transform.position.x <= -_bgWidth)
        {
            _backgroundGroup.transform.position += Vector3.right * _bgWidth;
        }
        
        if (_timeUntilNextObstacle <= 0)
        {
            SpawnObstacle();
            PickNextObstacleTime();
        }
        
        ground.transform.Rotate(Vector3.down, _speed * Time.deltaTime);
        
        var toRemove = new List<GameObject>();
        foreach (var obstacle in _obstacles)
        {
            obstacle.transform.RotateAround(ground.transform.position, Vector3.forward, _speed * Time.deltaTime);

            if (obstacle.transform.rotation.eulerAngles.z is > 90 and < 180)
            {
                toRemove.Add(obstacle);
                Destroy(obstacle);
            }
        }
        _obstacles.RemoveAll(toRemove.Contains);

        scoreText.text = "Score:\n" + Math.Round(_score);
    }

    void SpawnObstacle()
    {
        var prefab = currentLevel.obstaclePrefabs[Random.Range(0, currentLevel.obstaclePrefabs.Length)];
        var obstacle = Instantiate(prefab);
        obstacle.transform.RotateAround(ground.transform.position, Vector3.forward, -90f); 
        _obstacles.Add(obstacle);
    }

    void PickNextObstacleTime()
    {
        _timeUntilNextObstacle = Random.Range(2.0f, 4.0f);
    }
    
    public void StopGame()
    {
        _gameStopped = true;
        gameOverTime = Time.time;
        
        float highScore = 0;
        if(PlayerPrefs.HasKey("highScore"))
        {
            highScore = PlayerPrefs.GetFloat("highScore");
            if(_score > highScore)
            {
                highScore = _score;
                PlayerPrefs.SetFloat("highScore", highScore);
                PlayerPrefs.Save();
            }
        }
        else
        {   
            if(_score > highScore)
            {
                highScore = _score;
                PlayerPrefs.SetFloat("highScore", highScore);
                PlayerPrefs.Save();
            }
        }

        _audioSource.mute = true;
        
        if (ChangeLevelsEnabled())
        {
            gameEndedPanel.SetActive(true);
        }
        else
        {
            changeLevelPanel.SetActive(true);
        }
        
        gameEndedPanel.transform.Find("Final Score").GetComponent<TMP_Text>().text = "Your Score: " + Math.Round(_score);
        gameEndedPanel.transform.Find("Highscore").GetComponent<TMP_Text>().text = "Highscore: " + Math.Round(highScore);
        scoreText.enabled = false;
        
        
        playerController.StopAnimation();
    }

    public void RestartGame(int nextLevel = 0)
    {
        gameEndedPanel.SetActive(false);
        playerController.StartAnimation();
        ClearObstacles();
        _score = 0;
        scoreText.enabled = true;
        _speed = InitalSpeed;
        _gameStopped = false;

        SetLevel(nextLevel);
    }
    
    public bool ChangeLevelsEnabled()
    {
        return _changeLevels;
    }
    
    public void editChangeLevel(bool enabled)
    {
        _changeLevels = enabled;
    }
    
    public void ClearObstacles()
    {
        foreach (var obstacle in GameObject.FindGameObjectsWithTag("obstacle"))
        {
            Destroy(obstacle);
        }
        _obstacles.Clear();
    }

    void startTransition()
    {
        transitionPanel.SetActive(true);
        transitionPanel.transform.Find("LevelName").GetComponent<TMP_Text>().text = "Level: " + (_currentLevelIndex+1);
        _transitionActive = true;
        _transitionTime = Time.time;
    }

    public void SetLevel(int index)
    {
        _currentLevelIndex = index;
        ClearObstacles();
        currentLevel = levels[index];

        groundMat.mainTexture = currentLevel.groundTexture;
        
        if (_backgroundGroup)
        {
            Destroy(_backgroundGroup);
        }
        
        _backgroundGroup = new GameObject
        {
            name = "BackgroundGroup"
        };
        
        var bg = Instantiate(currentLevel.backgroundPrefab, _backgroundGroup.transform, true);
        _bgWidth = bg.GetComponent<SpriteRenderer>().bounds.size.x;
        var bg2 = Instantiate(currentLevel.backgroundPrefab, _backgroundGroup.transform, true);
        bg2.transform.position += Vector3.right * _bgWidth;
        
        levelUp.Play();
        
        startTransition();

        _audioSource.mute = false;
        _audioSource.clip = currentLevel.music;
        _audioSource.Play();
    }
}
