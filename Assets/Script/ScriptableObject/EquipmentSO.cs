using UnityEngine;
using System.Collections.Generic;

// 장비의 종류를 나타내는 Enum
public enum EquipmentType { Weapon, Armor, Accessory, Artifact }

// 캐릭터의 어떤 스탯을 변경할지 나타내는 Enum
public enum StatType { MaxHealth, AttackPower, Defense, CritRate, CritMultiplier, ResistanceSlash, ResistancePierce, ResistanceBlunt }

// 스탯을 어떻게 변경할지 (고정 값 합산, 곱연산 등)
// public enum ModifierType { Additive, Multiplicative } // -> 향후 확장을 위해 미리 구상

// 스탯 변경 정보를 담는 작은 클래스
[System.Serializable]
public class StatModifier
{
    public StatType statToModify;
    public float value;
    // public ModifierType type; // -> 향후 확장을 위해 미리 구상
}

// 모든 장비 SO의 부모 클래스 (추상 클래스)
public abstract class EquipmentSO : ScriptableObject
{
    public string equipmentId;
    public string equipmentName;
    [TextArea] public string description;
    public Sprite icon;
    public EquipmentType type;

    // 이 장비가 부여하는 스킬(카드)
    public CardDataSO grantedSkill;

    // 이 장비가 제공하는 스탯 보너스 목록
    public List<StatModifier> statModifiers;
}

// --- 각 장비 종류별 클래스 ---

[CreateAssetMenu(fileName = "New Weapon", menuName = "Card Game/Equipment/Weapon")]
public class WeaponSO : EquipmentSO
{
    // 무기는 특정 캐릭터 전용일 수 있음 (향후 CharacterSO와 연결 가능)
    // public CharacterSO requiredCharacter; 
}

[CreateAssetMenu(fileName = "New Armor", menuName = "Card Game/Equipment/Armor")]
public class ArmorSO : EquipmentSO
{
    // public CharacterSO requiredCharacter;
}

[CreateAssetMenu(fileName = "New Accessory", menuName = "Card Game/Equipment/Accessory")]
public class AccessorySO : EquipmentSO
{
    // 액세서리는 공용이므로 특정 캐릭터 요구사항 없음
}

[CreateAssetMenu(fileName = "New Artifact", menuName = "Card Game/Equipment/Artifact")]
public class ArtifactSO : EquipmentSO
{
    // public CharacterSO requiredCharacter;
}