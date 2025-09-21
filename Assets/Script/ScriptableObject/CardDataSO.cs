using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EffectParameter
{
    public string key;
    public string value;
}

[System.Serializable]
public class EffectData
{
    public string effectId;
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
    public CardStateData revelationState;   // corruptedState -> revelationState
    public CardStateData encroachmentState; // ascendedState -> encroachmentState
    public CardStateData corrosionState;    // abyssalState -> corrosionState

    public bool HasEncroachmentState => encroachmentState != null && !string.IsNullOrEmpty(encroachmentState.stateName);
    public bool HasCorrosionState => corrosionState != null && !string.IsNullOrEmpty(corrosionState.stateName);
}