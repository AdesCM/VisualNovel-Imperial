using UnityEngine;
using System.Collections.Generic;

// ★★★ Inspector에서 편집 가능한 Key-Value 클래스 새로 정의 ★★★
[System.Serializable]
public class EffectParameter
{
    public string key;
    public string value; // 값을 문자열로 받아 코드에서 변환 (가장 유연한 방식)
}

[System.Serializable]
public class EffectData
{
    public string effectId;
    // ★★★ Dictionary 대신 위에서 만든 클래스의 List를 사용 ★★★
    public List<EffectParameter> parameters; 
}

[System.Serializable]
public class CardStateData
{
    public string stateName;
    public string attackDice;
    public AttackType attackType;
    public List<EffectData> effects;
}

[CreateAssetMenu(fileName = "New CardData", menuName = "Card Game/Card Data")]
public class CardDataSO : ScriptableObject
{
    public string cardId;
    public Sprite cardArt;
    public int speed;
    
    public CardStateData awakenedState;
    public CardStateData corruptedState;
    public CardStateData ascendedState;
    public CardStateData abyssalState;

    public bool HasAscendedState => ascendedState != null && !string.IsNullOrEmpty(ascendedState.stateName);
    public bool HasAbyssalState => abyssalState != null && !string.IsNullOrEmpty(abyssalState.stateName);
}