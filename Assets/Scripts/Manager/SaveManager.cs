using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : Singleton<SaveManager>
{
    bool canSaveOrLoad;
    bool isSave;
    bool isLoad;
    bool isEscape;

    string sceneName = "anyScene";

    public string SceneName
    {
        get
        {
            return PlayerPrefs.GetString(sceneName);
        }
    }

    protected override void Awake()
    {
        base.Awake();
        //DontDestroyOnLoad(this);
    }

    private void Update()
    {
        if (GameManager.Instance.playerController != null)
        {
            isSave = GameManager.Instance.playerController.isSave;
            isLoad = GameManager.Instance.playerController.isLoad;
            isEscape = GameManager.Instance.playerController.isEscape;
        }
        SaveOrLoadPlayerStats();
        TransitionToMain();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canSaveOrLoad = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canSaveOrLoad = false;
        }
    }

    private void SaveOrLoadPlayerStats()
    {
        if (isSave && canSaveOrLoad)
        {
            SavePlayerData();
        }
        if (isLoad && canSaveOrLoad)
        {
            LoadPlayerData();
        }
    }

    public void SavePlayerData()
    {
        Save(GameManager.Instance.playerStats.characterData, GameManager.Instance.playerStats.characterData.name);
    }

    public void LoadPlayerData()
    {
        Load(GameManager.Instance.playerStats.characterData, GameManager.Instance.playerStats.characterData.name);
    }

    public void Save(object data, string key)
    {
        var jsonData = JsonUtility.ToJson(data, true);
        PlayerPrefs.SetString(key, jsonData);
        PlayerPrefs.SetString(sceneName, SceneManager.GetActiveScene().name);
        PlayerPrefs.Save();
    }

    public void Load(object data, string key)
    {
        if (PlayerPrefs.HasKey(key))
        {
            JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString(key), data);
        }
    }

    public void TransitionToMain()
    {
        if (isEscape)
        {
            SceneController.Instance.TransitionToMain();
        }
    }
}
