using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class EquipmentDataImporter
{
    private static string csvPath = "/Resources/Data/EquipmentData.csv";
    private static string cardSOFolderPath = "Assets/Resources/SO/Cards";
    private static string saveFolderPath = "Assets/Resources/SO/Equipments";

    [MenuItem("Tools/Data Importer/Update Equipment from CSV")]
    public static void ImportEquipment()
    {
        string filePath = Application.dataPath + csvPath;
        if (!File.Exists(filePath)) { /* ... */ return; }

        var lines = File.ReadAllLines(filePath);
        if (lines.Length <= 1) return;

        var headerMap = new Dictionary<string, int>();
        var header = lines[0].Trim().Split(',');
        for (int i = 0; i < header.Length; i++) { headerMap[header[i]] = i; }

        for (int i = 1; i < lines.Length; i++)
        {
            var values = lines[i].Split(',');
            string equipId = values[headerMap["EquipmentID"]];
            string equipTypeStr = values[headerMap["EquipmentType"]];
            string assetPath = $"{saveFolderPath}/{equipId}.asset";
            
            EquipmentSO equipSO = AssetDatabase.LoadAssetAtPath<EquipmentSO>(assetPath);
            
            if (equipSO == null)
            {
                // 타입에 맞는 SO를 생성
                System.Enum.TryParse<EquipmentType>(equipTypeStr, true, out EquipmentType type);
                switch(type)
                {
                    case EquipmentType.Weapon:    equipSO = ScriptableObject.CreateInstance<WeaponSO>(); break;
                    case EquipmentType.Armor:     equipSO = ScriptableObject.CreateInstance<ArmorSO>(); break;
                    case EquipmentType.Accessory: equipSO = ScriptableObject.CreateInstance<AccessorySO>(); break;
                    case EquipmentType.Artifact:  equipSO = ScriptableObject.CreateInstance<ArtifactSO>(); break;
                }
                AssetDatabase.CreateAsset(equipSO, assetPath);
            }

            // --- 데이터 채우기 ---
            equipSO.equipmentId = equipId;
            equipSO.name = values[headerMap["EquipmentName"]];
            equipSO.description = values[headerMap["Description"]];

            string iconPath = values[headerMap["IconPath"]];
            if (!string.IsNullOrEmpty(iconPath))
            {
                equipSO.icon = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
            }
            
            System.Enum.TryParse<EquipmentType>(equipTypeStr, true, out equipSO.type);
            
            string skillId = values[headerMap["GrantedSkillID"]];
            if (!string.IsNullOrEmpty(skillId))
            {
                equipSO.grantedSkill = AssetDatabase.LoadAssetAtPath<CardDataSO>($"{cardSOFolderPath}/{skillId}.asset`");
            }

            // 스탯 보너스 파싱
            equipSO.statModifiers = new List<StatModifier>();
            string modifiersString = values[headerMap["StatModifiers"]];
            if (!string.IsNullOrEmpty(modifiersString))
            {
                var pairs = modifiersString.Split(';');
                foreach (var pair in pairs)
                {
                    var keyValue = pair.Split(':');
                    System.Enum.TryParse<StatType>(keyValue[0], true, out StatType stat);
                    float value = float.Parse(keyValue[1], System.Globalization.CultureInfo.InvariantCulture);
                    equipSO.statModifiers.Add(new StatModifier { statToModify = stat, value = value });
                }
            }
            
            EditorUtility.SetDirty(equipSO);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("CSV로부터 EquipmentSO 에셋 업데이트 완료!");
    }
}