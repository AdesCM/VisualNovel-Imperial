using UnityEngine;
using UnityEditor; // 에디터 스크립트용 네임스페이스
using System.IO;   
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CardDataImporter
{
    private static string csvPath = "/Resources/Data/CardData.csv"; 
    private static string saveFolderPath = "Assets/Resources/SO/Cards";

    // 유니티 메뉴에 버튼 생성
    [MenuItem("Tools/Card Importer/Update All Cards from CSV")]
    public static void ImportCards()
    {
        string filePath = Application.dataPath + csvPath;
        if (!File.Exists(filePath))
        {
            Debug.LogError("CardData.csv 파일을 찾을 수 없습니다: " + filePath);
            return;
        }

        string[] lines = File.ReadAllLines(filePath);
        if (lines.Length <= 1) return;

        // 헤더(첫 번째 줄) 분석
        string[] header = lines[0].Trim().Split(',');
        var headerMap = new Dictionary<string, int>();
        for (int i = 0; i < header.Length; i++) { headerMap[header[i]] = i; }

        // 데이터 처리
        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');
            string cardId = values[headerMap["CardID"]];
            string assetPath = saveFolderPath + "/" + cardId + ".asset";
            
            CardDataSO cardSO = AssetDatabase.LoadAssetAtPath<CardDataSO>(assetPath);

            if (cardSO == null)
            {
                cardSO = ScriptableObject.CreateInstance<CardDataSO>();
                AssetDatabase.CreateAsset(cardSO, assetPath);
            }

            // SO 데이터 채우기
            cardSO.cardId = cardId;
            cardSO.name = values[headerMap["CardName"]];
            cardSO.speed = int.Parse(values[headerMap["Speed"]]);

            // 각 상태별 데이터 채우기
            cardSO.awakenedState = ParseStateData(values, headerMap, "Awakened");
            cardSO.revelationState = ParseStateData(values, headerMap, "Revelation");
            cardSO.encroachmentState = ParseStateData(values, headerMap, "Encroachment");
            cardSO.corrosionState = ParseStateData(values, headerMap, "Corrosion");

            EditorUtility.SetDirty(cardSO); // 변경사항 저장 표시
        }

        AssetDatabase.SaveAssets(); // 모든 변경사항 실제 파일에 저장
        AssetDatabase.Refresh();    // 프로젝트 창 새로고침
        Debug.Log("CSV로부터 CardDataSO 에셋 업데이트 완료!");
    }
    
    // 상태 데이터를 파싱하는 헬퍼 함수
    private static CardStateData ParseStateData(string[] values, Dictionary<string, int> headerMap, string statePrefix)
    {
        var stateData = new CardStateData();
        stateData.stateName = values[headerMap[statePrefix + "_Name"]];
        stateData.attackDice = values[headerMap[statePrefix + "_Dice"]];
        System.Enum.TryParse<AttackType>(values[headerMap[statePrefix + "_Type"]], true, out stateData.attackType);
        
        string artPathKey = statePrefix + "_ArtPath";
        if (headerMap.ContainsKey(artPathKey))
        {
            string artPath = values[headerMap[artPathKey]];
            if (!string.IsNullOrEmpty(artPath))
            {
                stateData.cardArt = AssetDatabase.LoadAssetAtPath<Sprite>(artPath);
            }
        }

        stateData.effects = ParseEffects(values[headerMap[statePrefix + "_Effects"]]);
        return stateData;
    }

    // Effects 문자열을 파싱하는 헬퍼 함수
    private static List<EffectData> ParseEffects(string effectsString)
    {
        var effects = new List<EffectData>();
        if (string.IsNullOrEmpty(effectsString)) return effects;

        string[] effectTokens = effectsString.Split(';');
        foreach (var token in effectTokens)
        {
            var effectData = new EffectData();
            var paramList = new List<EffectParameter>();
            
            Match match = Regex.Match(token, @"(\w+)\((.*)\)");
            if (match.Success)
            {
                effectData.effectId = match.Groups[1].Value;
                string paramsString = match.Groups[2].Value;

                if (!string.IsNullOrEmpty(paramsString))
                {
                    string[] paramTokens = paramsString.Split(',');
                    foreach (var param in paramTokens)
                    {
                        string[] keyValue = param.Split(':');
                        paramList.Add(new EffectParameter { key = keyValue[0].Trim(), value = keyValue[1].Trim() });
                    }
                }
            }
            effectData.parameters = paramList;
            effects.Add(effectData);
        }
        return effects;
    }
}