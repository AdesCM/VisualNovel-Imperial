using UnityEngine;
using System.Text.RegularExpressions;

public static class DiceParser
{
    public static int Roll(string diceNotation)
    {
        if (string.IsNullOrEmpty(diceNotation)) return 0;

        // "10 + 2d6" 처럼 공백이 있어도 처리할 수 있도록 공백을 모두 제거합니다.
        string notation = diceNotation.Replace(" ", "");

        // ★★★ 1. 덧셈(+) 형식 확인 ★★★
        if (notation.Contains("+"))
        {
            string[] parts = notation.Split('+');
            int baseValue = int.Parse(parts[0]);
            // 뒷부분("2d6")을 다시 Roll 함수에 넣어 주사위 값을 계산합니다. (재귀 호출)
            int diceResult = Roll(parts[1]); 
            return baseValue + diceResult;
        }
        // ★★★ 2. 뺄셈(-) 형식 확인 ★★★
        else if (notation.Contains("-"))
        {
            string[] parts = notation.Split('-');
            int baseValue = int.Parse(parts[0]);
            int diceResult = Roll(parts[1]);
            return baseValue - diceResult;
        }

        // ★★★ 3. 기존 로직 (순수 주사위 또는 고정 값) ★★★
        // "+"나 "-"가 없다면, 기존 방식대로 처리합니다.
        Match match = Regex.Match(notation.ToLower(), @"(\d+)d(\d+)");
        if (match.Success)
        {
            int numberOfDice = int.Parse(match.Groups[1].Value);
            int sidesOfDice = int.Parse(match.Groups[2].Value);
            int total = 0;
            for (int i = 0; i < numberOfDice; i++)
            {
                total += Random.Range(1, sidesOfDice + 1);
            }
            return total;
        }
        
        if(int.TryParse(notation, out int fixedValue))
        {
            return fixedValue;
        }

        Debug.LogError("잘못된 주사위 형식입니다: " + diceNotation);
        return 0;
    }
}