using UnityEngine;

// 주사위 롤의 구성요소를 담는 클래스
public class DiceRoll
{
    public int baseValue = 0;
    public int numberOfDice = 0;
    public int sidesOfDice = 0;
    public char operation = '+'; // +, -

    // 이 객체의 현재 설정으로 주사위를 굴림
    public int Roll()
    {
        int diceResult = 0;
        for (int i = 0; i < numberOfDice; i++)
        {
            diceResult += Random.Range(1, sidesOfDice + 1);
        }

        if (operation == '+') return baseValue + diceResult;
        if (operation == '-') return baseValue - diceResult;
        return baseValue;
    }
}

// 문자열을 파싱하여 DiceRoll 객체를 생성하는 정적 클래스
public static class DiceRollParser
{
    public static DiceRoll Parse(string diceNotation)
    {
        DiceRoll roll = new DiceRoll();
        if (string.IsNullOrEmpty(diceNotation)) return roll;

        string notation = diceNotation.Replace(" ", "");
        char op = notation.Contains("+") ? '+' : (notation.Contains("-") ? '-' : '+');
        
        string[] parts = notation.Split(op);

        string firstPart = parts[0];
        string secondPart = parts.Length > 1 ? parts[1] : null;

        // "10+2d6" -> first="10", second="2d6"
        // "2d6" -> first="2d6", second=null
        // "10" -> first="10", second=null

        if (firstPart.Contains("d"))
        {
            ParseDicePart(firstPart, roll);
        }
        else
        {
            roll.baseValue = int.Parse(firstPart);
        }

        if (secondPart != null)
        {
            roll.operation = op;
            ParseDicePart(secondPart, roll);
        }
        
        return roll;
    }

    private static void ParseDicePart(string dicePart, DiceRoll roll)
    {
        string[] diceComponents = dicePart.ToLower().Split('d');
        roll.numberOfDice = int.Parse(diceComponents[0]);
        roll.sidesOfDice = int.Parse(diceComponents[1]);
    }
}