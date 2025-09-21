using UnityEngine;
using System.Collections.Generic;

public class DamageEffect : CardEffect
{
    public override GamePhase TriggerPhase => GamePhase.PostCombat;
    private int damageAmount;
    public override void Initialize(Dictionary<string, object> parameters) { damageAmount = System.Convert.ToInt32(parameters["amount"]); }
    public override void Execute(Player ownerPlayer, Player opponentPlayer, Character ownerUser, Character opponentUser)
    {
        if (opponentUser != null) opponentUser.TakeDamage(damageAmount);
    }
}

public class HealEffect : CardEffect
{
    public override GamePhase TriggerPhase => GamePhase.PostCombat;
    private int healAmount;
    public override void Initialize(Dictionary<string, object> parameters) { healAmount = System.Convert.ToInt32(parameters["amount"]); }
    public override void Execute(Player ownerPlayer, Player opponentPlayer, Character ownerUser, Character opponentUser)
    {
        if (ownerUser != null) ownerUser.Heal(healAmount);
    }
}

public class ExpandSlotsEffect : CardEffect
{
    public override GamePhase TriggerPhase => GamePhase.TurnEnd;
    public override void Initialize(Dictionary<string, object> parameters) { }
    public override void Execute(Player ownerPlayer, Player opponentPlayer, Character ownerUser, Character opponentUser)
    {
        if (ownerPlayer != null)
        {
            ownerPlayer.bonusSlots += 1;
            Debug.Log($"{ownerPlayer.playerName}의 다음 턴 최대 슬롯이 1 증가합니다!");
        }
    }
}

public class ModifyResistanceEffect : CardEffect
{
    public override GamePhase TriggerPhase => GamePhase.PreCombat;
    private AttackType targetType;
    private float amount;
    public override void Initialize(Dictionary<string, object> parameters)
    {
        System.Enum.TryParse<AttackType>(parameters["type"].ToString(), true, out targetType);
        string amountString = parameters["amount"].ToString().Replace(',', '.');
        amount = float.Parse(amountString, System.Globalization.CultureInfo.InvariantCulture);
    }
    public override void Execute(Player ownerPlayer, Player opponentPlayer, Character ownerUser, Character opponentUser)
    {
        if (opponentUser == null) return;
        switch (targetType)
        {
            case AttackType.Slash: opponentUser.resistanceSlash += amount; break;
            case AttackType.Pierce: opponentUser.resistancePierce += amount; break;
            case AttackType.Blunt: opponentUser.resistanceBlunt += amount; break;
        }
    }
}

// 다음 턴 특정 슬롯의 주사위 굴림 횟수를 증가시키는 효과
public class BuffNextTurnSlotEffect : CardEffect
{
    // 이 효과는 전투가 끝난 후 다음 턴을 위해 발동하는 것이 자연스럽습니다.
    public override GamePhase TriggerPhase => GamePhase.PostCombat;
    
    private int targetSlotIndex; // 버프를 적용할 슬롯 인덱스 (1번 슬롯 -> 인덱스 0)
    private int diceCountBonus;  // 주사위 굴림 횟수 증가량

    public override void Initialize(Dictionary<string, object> parameters)
    {
        // Inspector에서 입력한 슬롯 번호(1)를 인덱스(0)로 변환
        targetSlotIndex = System.Convert.ToInt32(parameters["targetSlot"]) - 1;
        diceCountBonus = System.Convert.ToInt32(parameters["bonusDice"]);
    }
    
    // 효과 실행: 소유자(owner) Player의 버프 Dictionary에 정보를 기록합니다.
    public override void Execute(Player ownerPlayer, Player opponentPlayer, Character ownerUser, Character opponentUser)
    {
        if (ownerPlayer == null) return;

        // 이미 해당 슬롯에 다른 버프가 있다면 값을 더하고, 없다면 새로 추가합니다.
        if (ownerPlayer.slotDiceCountBuffs.ContainsKey(targetSlotIndex))
        {
            ownerPlayer.slotDiceCountBuffs[targetSlotIndex] += diceCountBonus;
        }
        else
        {
            ownerPlayer.slotDiceCountBuffs.Add(targetSlotIndex, diceCountBonus);
        }
        
        Debug.Log($"{ownerPlayer.playerName}의 다음 턴 {targetSlotIndex + 1}번 슬롯의 주사위 굴림 횟수가 {diceCountBonus}만큼 증가합니다!");
    }
}