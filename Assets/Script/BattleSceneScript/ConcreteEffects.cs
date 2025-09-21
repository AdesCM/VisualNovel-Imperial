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


public class ModifyResistanceEffect : CardEffect
{
    // 이 효과는 즉시 발동하는 경우가 많으므로 PreCombat 페이즈가 적합합니다.
    public override GamePhase TriggerPhase => GamePhase.PreCombat; 
    
    private AttackType targetType; // 변경할 속성 (참, 관, 충)
    private float amount;          // 변경할 수치 (예: -0.5, 0.3)
    // private int duration;       // (나중에 추가 가능) 지속 시간 (예: 2턴)

    public override void Initialize(Dictionary<string, object> parameters)
    {
        // Inspector에서 입력한 문자열을 enum으로 변환
        System.Enum.TryParse<AttackType>(parameters["type"].ToString(), true, out targetType);
        
        // C#은 소수점 표기를 . 으로 하므로, , 를 . 으로 바꿔줍니다.
        string amountString = parameters["amount"].ToString().Replace(',', '.');
        amount = float.Parse(amountString, System.Globalization.CultureInfo.InvariantCulture);
    }
    
    // 효과 실행: 대상 캐릭터의 보너스 저항력을 변경합니다.
    public override void Execute(Character ownerUser, Character opponentUser)
    {
        // 이 효과는 보통 상대방에게 사용됩니다.
        if (opponentUser == null) return;
        
        switch (targetType)
        {
            case AttackType.Slash:
                opponentUser.bonusResistanceSlash += amount;
                Debug.Log($"{opponentUser.characterName}의 참격 내성이 {amount}만큼 변경되었습니다!");
                break;
            case AttackType.Pierce:
                opponentUser.bonusResistancePierce += amount;
                Debug.Log($"{opponentUser.characterName}의 관통 내성이 {amount}만큼 변경되었습니다!");
                break;
            case AttackType.Blunt:
                opponentUser.bonusResistanceBlunt += amount;
                Debug.Log($"{opponentUser.characterName}의 타격 내성이 {amount}만큼 변경되었습니다!");
                break;
        }
    }
}