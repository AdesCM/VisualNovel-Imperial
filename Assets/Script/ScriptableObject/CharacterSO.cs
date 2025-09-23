using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Character", menuName = "Card Game/Character")]
public class CharacterSO : ScriptableObject
{
    public string characterId;
    public string characterName;
    [TextArea] public string description;

    // --- 캐릭터의 '순수 기본' 능력치 ---
    public int baseMaxHealth;
    public int baseAttackPower;
    public int baseDefense;
    public float baseCritRate = 0.1f;
    public float baseCritMultiplier = 1.5f;
    public float baseResistanceSlash = 1.0f;
    public float baseResistancePierce = 1.0f;
    public float baseResistanceBlunt = 1.0f;

    // ★★★ 캐릭터의 고유 스킬(카드) 4개를 여기에 등록 ★★★
    public List<CardDataSO> innateSkills;
}