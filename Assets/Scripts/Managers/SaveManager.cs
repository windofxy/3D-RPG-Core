using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : Singleton<SaveManager>
{
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }

    public void SavePlayerData()
    {
        Save("PlayerData", GameManager.Instance.playerStats.characterData);
    }

    public void LoadPlayerData()
    {
        Load("PlayerData", GameManager.Instance.playerStats.characterData);
    }

    public void Save(string key, Object data)
    {
        var jsonData = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(key, jsonData);
        PlayerPrefs.Save();
    }

    public void Load(string key, Object data)
    {
        if (PlayerPrefs.HasKey(key))
        {
            JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString(key), data);
        }
    }
}
