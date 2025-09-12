using UnityEngine;
using System.Collections.Generic;

// 효과 정보를 Inspector에서 편집할 수 있도록 [System.Serializable] 속성 추가
[System.Serializable]
public class EffectData
{
    public string effectId;
    public Dictionary<string, object> parameters; // 실제 구현 시 Key-Value 클래스 리스트로 대체 권장
}

[CreateAssetMenu(fileName = "New CardData", menuName = "Card Game/Card Data")]
public class CardDataSO : ScriptableObject
{
    public string cardId;
    public string cardName;
    [TextArea] public string description;
    public int attackPower;
    public int speed;
    public Sprite cardArt;
    
    public List<EffectData> effects;
}