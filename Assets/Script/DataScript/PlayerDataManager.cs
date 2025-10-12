using UnityEngine;
using System.Collections.Generic;
using System.IO;

// 카드 ID와 시전자 ID를 묶어주는 데이터 구조
[System.Serializable]
public class DeckCardInfo
{
    public string CardID;
    public string CasterID;
}

[System.Serializable]
public class PlayerSaveData
{
    //저장 데이터도 DeckCardInfo 리스트로 변경
    public List<DeckCardInfo> PlayerDeck = new List<DeckCardInfo>();
}

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance;

    //덱 정보 리스트의 타입을 DeckCardInfo로 변경
    public List<DeckCardInfo> PlayerDeck = new List<DeckCardInfo>();
    
    private string saveFilePath;

    void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        saveFilePath = Path.Combine(Application.persistentDataPath, "playerData.json");
        LoadData();
    }

    public void SaveData()
    {
        PlayerSaveData dataToSave = new PlayerSaveData();
        dataToSave.PlayerDeck = this.PlayerDeck; // 변경된 리스트 저장
        string json = JsonUtility.ToJson(dataToSave, true);
        File.WriteAllText(saveFilePath, json);
    }

    public void LoadData()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            PlayerSaveData loadedData = JsonUtility.FromJson<PlayerSaveData>(json);
            this.PlayerDeck = loadedData.PlayerDeck; // 변경된 리스트 불러오기
        }
    }
    
    private void OnApplicationQuit()
    {
        SaveData();
    }
}