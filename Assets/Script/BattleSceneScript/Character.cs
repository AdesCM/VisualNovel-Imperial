using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Character
{
    public CharacterSO blueprint { get; private set; } // 이 캐릭터의 원본 설계도
    public string characterName;

    // --- 기본 능력치 (설계도에서 복사해 옴) ---
    public int level = 1;
    public int baseMaxHealth;
    public int baseAttackPower;
    public int baseDefense;
    public float baseCritRate;
    public float baseCritMultiplier;
    public float baseResistanceSlash;
    public float baseResistancePierce;
    public float baseResistanceBlunt;

    // --- 임시 효과(버프/디버프)를 저장할 변수들 ---
    public int bonusMaxHealth = 0;
    public int bonusAttackPower = 0;
    public int bonusDefense = 0;
    public float bonusCritRate = 0f;
    public float bonusCritMultiplier = 0f;
    public float bonusResistanceSlash = 0f;
    public float bonusResistancePierce = 0f;
    public float bonusResistanceBlunt = 0f;

    // --- 실시간 상태 ---
    public int currentHp;
    public int experience = 0;
    public int maxExperience;
    public int sanity = 0;

    // --- 장비 ---
    public Dictionary<EquipmentType, EquipmentSO> equippedItems = new Dictionary<EquipmentType, EquipmentSO>();

    // 생성자: 설계도(SO)를 받아와서 실제 캐릭터를 생성
    public Character(CharacterSO so, int initialSanity = 0)
    {
        // 원본 설계도 연결 및 기본 능력치 복사
        this.blueprint = so;
        this.characterName = so.characterName;
        this.baseMaxHealth = so.baseMaxHealth;
        this.baseAttackPower = so.baseAttackPower;
        this.baseDefense = so.baseDefense;
        this.baseCritRate = so.baseCritRate;
        this.baseCritMultiplier = so.baseCritMultiplier;
        this.baseResistanceSlash = so.baseResistanceSlash;
        this.baseResistancePierce = so.baseResistancePierce;
        this.baseResistanceBlunt = so.baseResistanceBlunt;

        // 실시간 상태 초기화
        this.currentHp = GetFinalMaxHealth();
        this.sanity = initialSanity;
        CalculateMaxExperience();
    }

    // =================================================================
    // 최종 능력치 계산 (GetFinal... 메소드들)
    // =================================================================

    public int GetFinalMaxHealth()
    {
        float equipmentBonus = 0;
        foreach (var item in equippedItems.Values)
            foreach (var modifier in item.statModifiers)
                if (modifier.statToModify == StatType.MaxHealth)
                    equipmentBonus += modifier.value;
        
        return baseMaxHealth + (int)equipmentBonus + bonusMaxHealth;
    }

    public int GetFinalAttackPower()
    {
        float equipmentBonus = 0;
        foreach (var item in equippedItems.Values)
            foreach (var modifier in item.statModifiers)
                if (modifier.statToModify == StatType.AttackPower)
                    equipmentBonus += modifier.value;

        return baseAttackPower + (int)equipmentBonus + bonusAttackPower;
    }

    public int GetFinalDefense()
    {
        float equipmentBonus = 0;
        foreach (var item in equippedItems.Values)
            foreach (var modifier in item.statModifiers)
                if (modifier.statToModify == StatType.Defense)
                    equipmentBonus += modifier.value;
        
        return baseDefense + (int)equipmentBonus + bonusDefense;
    }

    public float GetFinalCritRate()
    {
        float equipmentBonus = 0;
        foreach (var item in equippedItems.Values)
            foreach (var modifier in item.statModifiers)
                if (modifier.statToModify == StatType.CritRate)
                    equipmentBonus += modifier.value;
        
        return baseCritRate + equipmentBonus + bonusCritRate;
    }

    public float GetFinalCritMultiplier()
    {
        float equipmentBonus = 0;
        foreach (var item in equippedItems.Values)
            foreach (var modifier in item.statModifiers)
                if (modifier.statToModify == StatType.CritMultiplier)
                    equipmentBonus += modifier.value;
        
        return baseCritMultiplier + equipmentBonus + bonusCritMultiplier;
    }

    public float GetFinalResistanceFor(AttackType attackType)
    {
        float baseResistance = 1.0f;
        float bonusResistance = 0f;
        StatType statToLookFor;

        switch (attackType)
        {
            case AttackType.Slash:
                baseResistance = baseResistanceSlash; bonusResistance = bonusResistanceSlash;
                statToLookFor = StatType.ResistanceSlash; break;
            case AttackType.Pierce:
                baseResistance = baseResistancePierce; bonusResistance = bonusResistancePierce;
                statToLookFor = StatType.ResistancePierce; break;
            case AttackType.Blunt:
                baseResistance = baseResistanceBlunt; bonusResistance = bonusResistanceBlunt;
                statToLookFor = StatType.ResistanceBlunt; break;
            default: return 1.0f;
        }

        float equipmentBonus = 0;
        foreach (var item in equippedItems.Values)
            foreach (var modifier in item.statModifiers)
                if (modifier.statToModify == statToLookFor)
                    equipmentBonus += modifier.value;

        return baseResistance + equipmentBonus + bonusResistance;
    }

    // =================================================================
    // 캐릭터 행동 및 상태 변화 메소드들
    // =================================================================

    public void EquipItem(EquipmentSO item)
    {
        if (equippedItems.ContainsKey(item.type)) { UnequipItem(item.type); }
        equippedItems[item.type] = item;
    }

    public void UnequipItem(EquipmentType type)
    {
        if (equippedItems.ContainsKey(type)) { equippedItems.Remove(type); }
    }

    public List<CardDataSO> GetAvailableSkills()
    {
        var skills = new List<CardDataSO>();
        
        // 1. 캐릭터 고유 스킬을 blueprint에서 가져옴
        if (blueprint != null && blueprint.innateSkills != null)
        {
            skills.AddRange(blueprint.innateSkills);
        }

        // 2. 장비가 부여하는 스킬 추가
        foreach (var item in equippedItems.Values)
        {
            if (item.grantedSkill != null) { skills.Add(item.grantedSkill); }
        }
        return skills;
    }

    public void TakeDamage(int amount)
    {
        currentHp -= amount;
        currentHp = Mathf.Clamp(currentHp, 0, GetFinalMaxHealth());
        Debug.Log($"{characterName}이(가) {amount}의 최종 피해! [HP: {currentHp}/{GetFinalMaxHealth()}]");
    }

    public void Heal(int amount)
    {
        currentHp += amount;
        currentHp = Mathf.Clamp(currentHp, 0, GetFinalMaxHealth());
        Debug.Log($"{characterName}이(가) {amount}만큼 회복! [HP: {currentHp}/{GetFinalMaxHealth()}]");
    }

    public void ChangeSanity(int amount)
    {
        sanity += amount;
        sanity = Mathf.Clamp(sanity, -15, 15);
        Debug.Log($"{characterName}의 정신력 {amount} 변화! [정신력: {sanity}]");
    }

    public void AddExperience(int amount)
    {
        experience += amount;
        if (experience >= maxExperience) { LevelUp(); }
    }

    private void LevelUp()
    {
        experience -= maxExperience;
        level++;
        
        // [차후 수정] 레벨업 시 능력치 증가 공식
        baseMaxHealth += 10;
        baseAttackPower += 2;
        baseDefense += 1;
        currentHp = GetFinalMaxHealth();

        CalculateMaxExperience();
        Debug.Log($"{characterName} 레벨 업! Level: {level}");
    }

    private void CalculateMaxExperience()
    {
        // [차후 수정] 최대 경험치 계산 공식
        maxExperience = 100 * level;
    }
}