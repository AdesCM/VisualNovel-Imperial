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
