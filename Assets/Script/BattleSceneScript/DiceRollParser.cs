using UnityEngine;

public class DiceRoll
{
    public int baseValue = 0;
    public int numberOfDice = 0;
    public int sidesOfDice = 0;
    public char operation = '+';

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

        if (firstPart.Contains("d")) { ParseDicePart(firstPart, roll); }
        else { roll.baseValue = int.Parse(firstPart); }

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