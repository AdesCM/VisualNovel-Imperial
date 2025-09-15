using UnityEngine;

public class Character
{
    public string characterName;
    public int currentHp;
    public int maxHp;
    public int sanity;

    public Character(string name, int hp, int initialSanity = 0)
    {
        characterName = name;
        maxHp = hp;
        currentHp = hp;
        sanity = initialSanity;
    }

    public void TakeDamage(int amount)
    {
        currentHp -= amount;
        Debug.Log($"{characterName}이(가) {amount}의 피해! [HP: {currentHp}/{maxHp}]");
    }

    public void Heal(int amount)
    {
        currentHp += amount;
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);
        Debug.Log($"{characterName}이(가) {amount}만큼 회복! [HP: {currentHp}/{maxHp}]");
    }

    public void ChangeSanity(int amount)
    {
        sanity += amount;
        sanity = Mathf.Clamp(sanity, -15, 15);
        Debug.Log($"{characterName}의 정신력 {amount} 변화! [정신력: {sanity}]");
    }
}