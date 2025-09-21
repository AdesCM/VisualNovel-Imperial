using UnityEngine;

public class Character
{
    public string characterName;

    // 능력치
    public int level;
    public int maxHealth;
    public int currentHp;
    public int attackPower;
    public int defense;
    public float critRate;
    public float critMultiplier;
    
    // 속성 내성
    public float baseResistanceSlash = 1.0f;
    public float baseResistancePierce = 1.0f;
    public float baseResistanceBlunt = 1.0f;

    // 경험치
    public int experience;
    public int maxExperience;
    public int sanity;

    // 보너스 내성
    public float bonusResistanceSlash = 0f;
    public float bonusResistancePierce = 0f;
    public float bonusResistanceBlunt = 0f;

    public Character(string name, int hp, int atk, int def, int initialSanity = 0)
    {
        characterName = name;
        level = 1;
        maxHealth = hp;
        currentHp = hp;
        attackPower = atk;
        defense = def;
        critRate = 0.1f;
        critMultiplier = 1.5f;
        baseResistanceSlash = 1.0f;
        baseResistancePierce = 1.0f;
        baseResistanceBlunt = 1.0f;
        experience = 0;
        sanity = initialSanity;
        
        CalculateMaxExperience();
    }

    public void TakeDamage(int amount)
    {
        currentHp -= amount;
        currentHp = Mathf.Clamp(currentHp, 0, maxHealth);
        Debug.Log($"{characterName}이(가) {amount}의 최종 피해! [HP: {currentHp}/{maxHealth}]");
    }

    public void Heal(int amount)
    {
        currentHp += amount;
        currentHp = Mathf.Clamp(currentHp, 0, maxHealth);
        Debug.Log($"{characterName}이(가) {amount}만큼 회복! [HP: {currentHp}/{maxHealth}]");
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
        if (experience >= maxExperience)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        experience -= maxExperience;
        level++;
        
        // [차후 수정] 레벨업 시 능력치 증가 공식
        maxHealth += 10;
        attackPower += 2;
        defense += 1;
        currentHp = maxHealth;

        CalculateMaxExperience();
        Debug.Log($"{characterName} 레벨 업! Level: {level}");
    }
    
    private void CalculateMaxExperience()
    {
        // [차후 수정] 최대 경험치 계산 공식
        maxExperience = 100 * level;
    }

     public float GetResistanceFor(AttackType attackType)
    {
        switch (attackType)
        {
            // (기본 내성 + 보너스 내성)을 합산하여 최종 값을 반환
            case AttackType.Slash: return baseResistanceSlash + bonusResistanceSlash;
            case AttackType.Pierce: return baseResistancePierce + bonusResistancePierce;
            case AttackType.Blunt: return baseResistanceBlunt + bonusResistanceBlunt;
            default: return 1.0f;
        }
    }

    
}