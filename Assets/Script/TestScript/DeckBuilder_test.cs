using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;

public class DeckBuilder_Test : MonoBehaviour
{
    [Header("디버깅용 덱 설정")]
    [SerializeField] private List<DeckCardInfo> debugDeck = new List<DeckCardInfo>();
    [SerializeField] private bool useInspectorDeck = true;
    [SerializeField] private bool autoLoadFromJSON = true;
    [SerializeField] private string savedDeckFileName = "debug_deck.json";
    
    void Start()
    {
        SetupDebugDeck();
    }
    
    void SetupDebugDeck()
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
        
        if (useInspectorDeck && debugDeck.Count > 0)
        {
            // Inspector에서 설정된 덱 사용
            PlayerDataManager.Instance.PlayerDeck.AddRange(debugDeck);
            Debug.Log($"Inspector에서 설정된 디버깅용 덱 로드: {debugDeck.Count}장");
        }
        else if (autoLoadFromJSON)
        {
            // JSON에서 자동으로 덱 불러오기
            if (TryLoadDeckFromJSON())
            {
                Debug.Log("JSON에서 덱을 자동으로 불러왔습니다.");
            }
            else
            {
                Debug.LogWarning("JSON 파일을 찾을 수 없어 기본 덱을 사용합니다.");
                LoadDefaultDebugDeck();
            }
        }
        else
        {
            // 기존 하드코딩 방식 (기본값)
            LoadDefaultDebugDeck();
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
        SetupDebugDeck(); // 다시 시도
    }
    
    void LoadDefaultDebugDeck()
    {
        // 예시: "briram_spear" 캐릭터가 "debug_skill002" 카드를 4장 사용하도록 덱 구성
        for(int i = 0; i < 4; i++) 
        {
            PlayerDataManager.Instance.PlayerDeck.Add(
                new DeckCardInfo { CardID = "debug_skill002", CasterID = "briram_spear" }
            );
        }
        Debug.Log("기본 디버깅용 덱 로드 완료");
    }
    
    [ContextMenu("현재 덱을 JSON으로 저장")]
    public void SaveCurrentDeckToJSON()
    {
        if (PlayerDataManager.Instance.PlayerDeck.Count == 0)
        {
            Debug.LogWarning("저장할 덱이 없습니다.");
            return;
        }
        
        string json = JsonUtility.ToJson(new PlayerSaveData { PlayerDeck = PlayerDataManager.Instance.PlayerDeck }, true);
        string filePath = Path.Combine(Application.persistentDataPath, savedDeckFileName);
        File.WriteAllText(filePath, json);
        
        Debug.Log($"덱이 저장되었습니다: {filePath}");
    }
    
    [ContextMenu("JSON에서 덱 불러오기")]
    public void LoadDeckFromJSON()
    {
        if (TryLoadDeckFromJSON())
        {
            Debug.Log("JSON에서 덱을 불러왔습니다.");
        }
        else
        {
            Debug.LogWarning("JSON에서 덱을 불러오는데 실패했습니다.");
        }
    }
    
    bool TryLoadDeckFromJSON()
    {
        string filePath = Path.Combine(Application.persistentDataPath, savedDeckFileName);
        
        Debug.Log($"JSON 파일 경로: {filePath}");
        Debug.Log($"파일 존재 여부: {File.Exists(filePath)}");
        
        if (!File.Exists(filePath))
        {
            Debug.LogError($"JSON 파일을 찾을 수 없습니다: {filePath}");
            return false;
        }
        
        try
        {
            string json = File.ReadAllText(filePath);
            Debug.Log($"JSON 파일 내용 길이: {json.Length} 문자");
            Debug.Log($"JSON 파일 내용: {json}");
            
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogError("JSON 파일이 비어있습니다.");
                return false;
            }
            
            PlayerSaveData loadedData = JsonUtility.FromJson<PlayerSaveData>(json);
            
            if (loadedData == null)
            {
                Debug.LogError("JSON 파싱 결과가 null입니다.");
                return false;
            }
            
            if (loadedData.PlayerDeck == null)
            {
                Debug.LogError("로드된 데이터의 PlayerDeck이 null입니다.");
                return false;
            }
            
            if (loadedData.PlayerDeck.Count == 0)
            {
                Debug.LogWarning("로드된 덱에 카드가 없습니다.");
                return false;
            }
            
            if (PlayerDataManager.Instance != null)
            {
                PlayerDataManager.Instance.PlayerDeck.Clear();
                PlayerDataManager.Instance.PlayerDeck.AddRange(loadedData.PlayerDeck);
                Debug.Log($"JSON에서 덱을 불러왔습니다: {loadedData.PlayerDeck.Count}장의 카드");
                
                // 각 카드 정보 출력
                for (int i = 0; i < loadedData.PlayerDeck.Count; i++)
                {
                    var card = loadedData.PlayerDeck[i];
                    Debug.Log($"  카드 {i + 1}: {card.CardID} (시전자: {card.CasterID})");
                }
                
                return true;
            }
            else
            {
                Debug.LogError("PlayerDataManager.Instance가 null입니다.");
                return false;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"JSON 덱 로딩 중 오류 발생: {e.Message}");
            Debug.LogError($"스택 트레이스: {e.StackTrace}");
        }
        
        return false;
    }

    // (UI 버튼 등에 연결하여 호출할 함수)
    public void GoToBattleScene()
    {
        SceneManager.LoadScene("BattleScene"); // 전투 씬 이름으로 변경
    }
}