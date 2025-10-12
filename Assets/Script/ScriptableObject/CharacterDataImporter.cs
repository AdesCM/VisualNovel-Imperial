using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class CharacterDataImporter
{
    private static string csvPath = "/Resources/Data/CharacterData.csv";
    private static string cardSOFolderPath = "Assets/Resources/SO/Cards"; // 카드 SO 참조를 위해 필요
    private static string saveFolderPath = "Assets/Resources/SO/Characters";

    [MenuItem("Tools/Data Importer/Update Characters from CSV")]
    public static void ImportCharacters()
    {
        string filePath = Application.dataPath + csvPath;
        if (!File.Exists(filePath)) { /* ... 파일 없음 에러 처리 ... */ return; }

        string[] lines = File.ReadAllLines(filePath);
        if (lines.Length <= 1) return;

        var headerMap = new Dictionary<string, int>();
        var header = lines[0].Trim().Split(',');
        for (int i = 0; i < header.Length; i++) { headerMap[header[i]] = i; }

        for (int i = 1; i < lines.Length; i++)
        {
            var values = lines[i].Split(',');
            string charId = values[headerMap["CharacterID"]];
            string assetPath = saveFolderPath + "/" + charId + ".asset";

            CharacterSO charSO = AssetDatabase.LoadAssetAtPath<CharacterSO>(assetPath);
            if (charSO == null)
            {
                charSO = ScriptableObject.CreateInstance<CharacterSO>();
                AssetDatabase.CreateAsset(charSO, assetPath);
            }

            // --- 데이터 채우기 ---
            charSO.characterId = charId;
            charSO.name = values[headerMap["CharacterName"]];
            charSO.baseMaxHealth = int.Parse(values[headerMap["baseMaxHealth"]]);
            charSO.baseAttackPower = int.Parse(values[headerMap["baseAttackPower"]]);
            charSO.baseDefense = int.Parse(values[headerMap["baseDefense"]]);
            // ... (나머지 모든 스탯 파싱)

            // 고유 스킬 연결
            string skillIDsString = values[headerMap["InnateSkillIDs"]];
            charSO.innateSkills = new List<CardDataSO>();
            if (!string.IsNullOrEmpty(skillIDsString))
            {
                var skillIds = skillIDsString.Split(';');
                foreach (var skillId in skillIds)
                {
                    CardDataSO card = AssetDatabase.LoadAssetAtPath<CardDataSO>($"{cardSOFolderPath}/{skillId}.asset");
                    if (card != null)
                    {
                        charSO.innateSkills.Add(card);
                    }
                    else
                    {
                        // 카드를 찾지 못했을 경우를 대비한 디버그 로그 추가 (권장)
                        Debug.LogWarning($"Innate Skill ID '{skillId}'에 해당하는 CardDataSO 에셋을 찾을 수 없습니다.");
                    }
                }
            }

            EditorUtility.SetDirty(charSO);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("CSV로부터 CharacterSO 에셋 업데이트 완료!");
    }
}