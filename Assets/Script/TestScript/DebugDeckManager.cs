using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class DebugDeckManager : MonoBehaviour
{
    [Header("디버깅용 덱 설정")]
    [SerializeField] private List<DeckCardInfo> debugDeck = new List<DeckCardInfo>();
    
    [Header("저장된 덱 불러오기")]
    [SerializeField] private string savedDeckName = "debug_deck.json";
    
    void Start()
    {
        // 디버깅용 덱이 설정되어 있으면 PlayerDataManager에 로드
        if (debugDeck.Count > 0)
        {
            LoadDebugDeck();
        }
    }
    
    /// <summary>
    /// 디버깅용 덱을 PlayerDataManager에 로드
    /// </summary>
    public void LoadDebugDeck()
    {
        // PlayerDataManager가 없으면 자동으로 생성
        if (PlayerDataManager.Instance == null)
        {
            Debug.Log("PlayerDataManager가 없습니다. 자동으로 생성합니다.");
            CreatePlayerDataManager();
        }
        
        // PlayerDataManager가 초기화되지 않은 경우 대기
        if (PlayerDataManager.Instance == null)
        {
            Debug.LogWarning("PlayerDataManager.Instance가 여전히 null입니다. 1프레임 후 다시 시도합니다.");
            StartCoroutine(WaitForPlayerDataManager());
            return;
        }
        
        PlayerDataManager.Instance.PlayerDeck.Clear();
        PlayerDataManager.Instance.PlayerDeck.AddRange(debugDeck);
        
        Debug.Log($"디버깅용 덱 로드 완료: {debugDeck.Count}장의 카드");
        foreach (var card in debugDeck)
        {
            Debug.Log($"- {card.CardID} (시전자: {card.CasterID})");
        }
    }
    
    void CreatePlayerDataManager()
    {
        // PlayerDataManager GameObject 생성
        GameObject playerDataManagerGO = new GameObject("PlayerDataManager");
        playerDataManagerGO.AddComponent<PlayerDataManager>();
        
        Debug.Log("PlayerDataManager가 자동으로 생성되었습니다.");
    }
    
    System.Collections.IEnumerator WaitForPlayerDataManager()
    {
        yield return null; // 1프레임 대기
        LoadDebugDeck(); // 다시 시도
    }
    
    /// <summary>
    /// 현재 덱을 JSON 파일로 저장
    /// </summary>
    [ContextMenu("현재 덱을 JSON으로 저장")]
    public void SaveCurrentDeckToJSON()
    {
        if (PlayerDataManager.Instance == null || PlayerDataManager.Instance.PlayerDeck.Count == 0)
        {
            Debug.LogWarning("저장할 덱이 없습니다.");
            return;
        }
        
        string json = JsonUtility.ToJson(new PlayerSaveData { PlayerDeck = PlayerDataManager.Instance.PlayerDeck }, true);
        string filePath = Path.Combine(Application.persistentDataPath, savedDeckName);
        File.WriteAllText(filePath, json);
        
        Debug.Log($"덱이 저장되었습니다: {filePath}");
    }
    
    /// <summary>
    /// JSON 파일에서 덱을 불러와서 Inspector에서 설정
    /// </summary>
    [ContextMenu("JSON에서 덱 불러오기")]
    public void LoadDeckFromJSON()
    {
        string filePath = Path.Combine(Application.persistentDataPath, savedDeckName);
        
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"저장된 덱 파일을 찾을 수 없습니다: {filePath}");
            return;
        }
        
        string json = File.ReadAllText(filePath);
        PlayerSaveData loadedData = JsonUtility.FromJson<PlayerSaveData>(json);
        
        debugDeck.Clear();
        debugDeck.AddRange(loadedData.PlayerDeck);
        
        Debug.Log($"JSON에서 덱을 불러왔습니다: {debugDeck.Count}장의 카드");
    }
    
    /// <summary>
    /// Inspector에서 설정된 덱을 JSON으로 저장
    /// </summary>
    [ContextMenu("Inspector 덱을 JSON으로 저장")]
    public void SaveInspectorDeckToJSON()
    {
        if (debugDeck.Count == 0)
        {
            Debug.LogWarning("Inspector에 설정된 덱이 없습니다.");
            return;
        }
        
        string json = JsonUtility.ToJson(new PlayerSaveData { PlayerDeck = debugDeck }, true);
        string filePath = Path.Combine(Application.persistentDataPath, savedDeckName);
        File.WriteAllText(filePath, json);
        
        Debug.Log($"Inspector 덱이 저장되었습니다: {filePath}");
    }
}
