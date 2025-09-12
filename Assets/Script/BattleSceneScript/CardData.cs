using System.Collections.Generic;

// ScriptableObject가 아닌, 메모리에서 사용할 일반 데이터 클래스입니다.
public class CardData
{
    public string cardName;
    public int attackPower;
    public int speed;
    public List<CardEffect> effects; // 실제 로직 객체 리스트

    public CardData(string name, int power, int spd, List<CardEffect> cardEffects)
    {
        cardName = name;
        attackPower = power;
        speed = spd;
        effects = cardEffects;
    }
}