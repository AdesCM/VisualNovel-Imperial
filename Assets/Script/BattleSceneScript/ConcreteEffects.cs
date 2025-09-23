using UnityEngine;
using System.Collections.Generic;

// --- 피해 및 회복 효과 ---
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

// --- 플레이어 대상 효과 ---
public class ExpandSlotsEffect : CardEffect
{
    public override GamePhase TriggerPhase => GamePhase.TurnEnd;
    public override void Initialize(Dictionary<string, object> parameters) { }
    public override void Execute(Player ownerPlayer, Player opponentPlayer, Character ownerUser, Character opponentUser)
    {
        if (ownerPlayer != null) ownerPlayer.bonusSlots += 1;
    }
}

// --- 캐릭터 임시 능력치(버프/디버프) 변경 효과 ---
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
            case AttackType.Slash: opponentUser.bonusResistanceSlash += amount; break;
            case AttackType.Pierce: opponentUser.bonusResistancePierce += amount; break;
            case AttackType.Blunt: opponentUser.bonusResistanceBlunt += amount; break;
        }
    }
}

public class ModifySanityEffect : CardEffect
{
    public override GamePhase TriggerPhase => GamePhase.PreCombat;
    private string target;
    private int amount;
    public override void Initialize(Dictionary<string, object> parameters)
    {
        target = parameters["target"].ToString();
        amount = System.Convert.ToInt32(parameters["amount"]);
    }
    public override void Execute(Player ownerPlayer, Player opponentPlayer, Character ownerUser, Character opponentUser)
    {
        if (target.Equals("Self", System.StringComparison.OrdinalIgnoreCase) && ownerUser != null) ownerUser.ChangeSanity(amount);
        else if (target.Equals("Opponent", System.StringComparison.OrdinalIgnoreCase) && opponentUser != null) opponentUser.ChangeSanity(amount);
    }
}


// --- 슬롯 대상 효과 ---
public class BuffNextTurnSlotEffect : CardEffect
{
    public override GamePhase TriggerPhase => GamePhase.PostCombat;
    private int targetSlotIndex;
    private int diceCountBonus;
    public override void Initialize(Dictionary<string, object> parameters)
    {
        targetSlotIndex = System.Convert.ToInt32(parameters["targetSlot"]) - 1;
        diceCountBonus = System.Convert.ToInt32(parameters["bonusDice"]);
    }
    public override void Execute(Player ownerPlayer, Player opponentPlayer, Character ownerUser, Character opponentUser)
    {
        if (ownerPlayer == null) return;
        if (ownerPlayer.slotDiceCountBuffs.ContainsKey(targetSlotIndex)) { ownerPlayer.slotDiceCountBuffs[targetSlotIndex] += diceCountBonus; }
        else { ownerPlayer.slotDiceCountBuffs.Add(targetSlotIndex, diceCountBonus); }
    }
}

public class BuffNextTurnSlotDiceMaxEffect : CardEffect
{
    public override GamePhase TriggerPhase => GamePhase.PostCombat;
    private int targetSlotIndex;
    private int diceMaxBonus;
    public override void Initialize(Dictionary<string, object> parameters)
    {
        targetSlotIndex = System.Convert.ToInt32(parameters["targetSlot"]) - 1;
        diceMaxBonus = System.Convert.ToInt32(parameters["bonusMax"]);
    }
    public override void Execute(Player ownerPlayer, Player opponentPlayer, Character ownerUser, Character opponentUser)
    {
        if (ownerPlayer == null) return;
        if (ownerPlayer.slotDiceMaxBuffs.ContainsKey(targetSlotIndex)) { ownerPlayer.slotDiceMaxBuffs[targetSlotIndex] += diceMaxBonus; }
        else { ownerPlayer.slotDiceMaxBuffs.Add(targetSlotIndex, diceMaxBonus); }
    }
}