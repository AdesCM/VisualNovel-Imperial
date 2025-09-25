using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// CSV의 한 줄에 해당하는 데이터를 담을 클래스
public class StageData
{
    public string StageID;
    public string StageName;
    public List<string> EnemyTeamIDs;
    public string AIPatternID;
}

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    private Dictionary<string, StageData> stageDatabase = new Dictionary<string, StageData>();
    private Dictionary<string, AIPatternSO> patternDatabase = new Dictionary<string, AIPatternSO>();

    public string currentStageId; // 현재 플레이할 스테이지 ID

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadStageDataFromCSV();
        LoadAllAIPatterns();
    }

    void LoadStageDataFromCSV()
    {
        // CSV 파일을 읽고 파싱하여 stageDatabase를 채우는 로직
        // (이전 답변의 CSV 파싱 로직과 유사하게 구현)
    }

    void LoadAllAIPatterns()
    {
        var loadedPatterns = Resources.LoadAll<AIPatternSO>("SO/AIPatterns");
        foreach(var pattern in loadedPatterns)
        {
            patternDatabase.Add(pattern.patternId, pattern);
        }
    }

    public StageData GetCurrentStageData()
    {
        return stageDatabase.ContainsKey(currentStageId) ? stageDatabase[currentStageId] : null;
    }
    
    public AIPatternSO GetAIPattern(string patternId)
    {
        return patternDatabase.ContainsKey(patternId) ? patternDatabase[patternId] : null;
    }
}