using UnityEngine;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    public string playerName;
    public int health = 30;

    // 등록된 카드를 담는 리스트 (ScriptableObject 참조)
    public List<CardDataSO> registeredCards = new List<CardDataSO>();

    // 기본 최대 슬롯 (영구적)
    public int baseSlots = 4;
    
    // 다음 턴에만 적용될 보너스 슬롯 (임시적)
    public int bonusSlots = 0;

    public void TakeDamage(int amount)
    {
        health -= amount;
        Debug.Log($"{playerName}의 현재 체력: {health}");
    }

    public void Heal(int amount)
    {
        health += amount;
        Debug.Log($"{playerName}의 현재 체력: {health}");
    }
}