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
    public float resistanceSlash;
    public float resistancePierce;
    public float resistanceBlunt;

    // 경험치
    public int experience;
    public int maxExperience;

    // ★★★ 누락되었던 정신력 변수 ★★★
    public int sanity;

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
        resistanceSlash = 1.0f;
        resistancePierce = 1.0f;
        resistanceBlunt = 1.0f;
        experience = 0;
        
        // ★★★ 생성자에 정신력 초기화 추가 ★★★
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

    // ★★★ 누락되었던 정신력 변경 메소드 ★★★
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
            case AttackType.Slash: return resistanceSlash;
            case AttackType.Pierce: return resistancePierce;
            case AttackType.Blunt: return resistanceBlunt;
            default: return 1.0f;
        }
    }
}