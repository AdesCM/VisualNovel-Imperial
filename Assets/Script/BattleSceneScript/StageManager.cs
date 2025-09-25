using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// CSV의 한 줄에 해당하는 데이터를 담을 클래스
public class StageData
{
    public string StageID;
    public string StageName;
    public string AIPatternID;
    // 여러 웨이브 정보를 담을 수 있도록 List의 List로 구성
    public List<List<string>> Waves = new List<List<string>>(); 
}

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    private Dictionary<string, StageData> stageDatabase = new Dictionary<string, StageData>();
    private Dictionary<string, AIPatternSO> patternDatabase = new Dictionary<string, AIPatternSO>();

    // 외부(예: 월드맵 씬)에서 플레이할 스테이지 ID를 설정
    public string currentStageId; 

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadStageDataFromCSV();
        LoadAllAIPatterns();

        // 테스트를 위한 임시 코드
        if (string.IsNullOrEmpty(currentStageId))
        {
            currentStageId = "1-7"; // 기본 스테이지 ID
        }
    }

    void LoadStageDataFromCSV()
    {
        // 이 부분은 CSV 파일을 읽고 파싱하여 stageDatabase를 채우는 로직입니다.
        // TextAsset을 통해 CSV 파일을 불러오고, 한 줄씩 읽어 StageData를 생성합니다.
        TextAsset csvFile = Resources.Load<TextAsset>("Data/StageData"); // 예시 경로: Resources/Data/StageData.csv
        if (csvFile == null) 
        {
            Debug.LogError("StageData.csv 파일을 찾을 수 없습니다!");
            return;
        }

        string[] lines = csvFile.text.Split('\n');
        if (lines.Length <= 1) return;

        string[] header = lines[0].Trim().Split(',');
        var headerMap = new Dictionary<string, int>();
        for (int i = 0; i < header.Length; i++)
        {
            headerMap[header[i]] = i;
        }

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;
            
            string[] values = line.Split(',');
            var stageData = new StageData();
            
            stageData.StageID = values[headerMap["StageID"]];
            stageData.StageName = values[headerMap["StageName"]];
            stageData.AIPatternID = values[headerMap["AIPatternID"]];

            // 웨이브 데이터 파싱
            for(int w = 1; w < 10; w++) // 최대 9웨이브까지 확인
            {
                string waveColumnName = $"Wave{w}_Enemies";
                if (headerMap.ContainsKey(waveColumnName) && values.Length > headerMap[waveColumnName] && !string.IsNullOrEmpty(values[headerMap[waveColumnName]]))
                {
                    List<string> enemyIds = values[headerMap[waveColumnName]].Split(';').ToList();
                    stageData.Waves.Add(enemyIds);
                }
                else
                {
                    break; 
                }
            }
            stageDatabase.Add(stageData.StageID, stageData);
        }
    }

    void LoadAllAIPatterns()
    {
        var loadedPatterns = Resources.LoadAll<AIPatternSO>("SO/AIPatterns");
        foreach(var pattern in loadedPatterns)
        {
            if (!patternDatabase.ContainsKey(pattern.patternId))
            {
                patternDatabase.Add(pattern.patternId, pattern);
            }
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