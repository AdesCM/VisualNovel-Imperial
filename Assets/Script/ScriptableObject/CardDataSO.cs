using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EffectData
{
    public string effectId;
    public Dictionary<string, object> parameters;
}

[System.Serializable]
public class CardStateData
{
    public string stateName;
    public string attackDice;
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