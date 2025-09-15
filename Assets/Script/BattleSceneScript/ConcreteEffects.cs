using UnityEngine;
using System.Collections.Generic;

public class DamageEffect : CardEffect
{
    public override GamePhase TriggerPhase => GamePhase.PostCombat;
    private int damageAmount;
    public override void Initialize(Dictionary<string, object> parameters) { damageAmount = System.Convert.ToInt32(parameters["amount"]); }
    public override void Execute(Character ownerUser, Character opponentUser) 
    {
        if (opponentUser != null) opponentUser.TakeDamage(damageAmount);
    }
}

public class HealEffect : CardEffect
{
    public override GamePhase TriggerPhase => GamePhase.PostCombat;
    private int healAmount;
    public override void Initialize(Dictionary<string, object> parameters) { healAmount = System.Convert.ToInt32(parameters["amount"]); }
    public override void Execute(Character ownerUser, Character opponentUser) 
    {
        if (ownerUser != null) ownerUser.Heal(healAmount);
    }
}

public class ExpandSlotsEffect : CardEffect
{
    public override GamePhase TriggerPhase => GamePhase.TurnEnd;
    public override void Initialize(Dictionary<string, object> parameters) { }
    public override void Execute(Character ownerUser, Character opponentUser) { /* Player object needed */ }
}

public class CombatBonusEffect : CardEffect
{
    public override GamePhase TriggerPhase => GamePhase.DuringCombat;
    private int bonusAmount;
    public override void Initialize(Dictionary<string, object> parameters) { bonusAmount = System.Convert.ToInt32(parameters["amount"]); }
    public override void Execute(Character ownerUser, Character opponentUser) { }
    public override int GetCombatBonus(RegisteredCardSlot ownerSlot, RegisteredCardSlot opponentSlot)
    {
        CardStateData ownerStateData = GetStateData(ownerSlot);
        CardStateData opponentStateData = GetStateData(opponentSlot);
        if (ownerStateData == null || opponentStateData == null) return 0;
        
        int ownerBaseAttack = DiceParser.Roll(ownerStateData.attackDice);
        int opponentBaseAttack = DiceParser.Roll(opponentStateData.attackDice);
        
        if (opponentBaseAttack > ownerBaseAttack) return bonusAmount;
        return 0;
    }

    private CardStateData GetStateData(RegisteredCardSlot slot)
    {
        if (slot == null || slot.cardSO == null) return null;
        switch (slot.state)
        {
            case SlotState.Ascended: return slot.cardSO.ascendedState;
            case SlotState.Abyssal: return slot.cardSO.abyssalState;
            case SlotState.Corrupted: return slot.cardSO.corruptedState;
            default: return slot.cardSO.awakenedState;
        }
    }
}