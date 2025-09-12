using UnityEngine;
using System.Collections.Generic;

// 데미지를 주는 효과
public class DamageEffect : CardEffect
{
    public override GamePhase TriggerPhase => GamePhase.PostCombat;
    private int damageAmount;
    public override void Initialize(Dictionary<string, object> parameters) { damageAmount = System.Convert.ToInt32(parameters["amount"]); }
    public override void Execute(Player owner, Player opponent) { opponent.TakeDamage(damageAmount); }
}

// 생명력을 회복하는 효과
public class HealEffect : CardEffect
{
    public override GamePhase TriggerPhase => GamePhase.PostCombat;
    private int healAmount;
    public override void Initialize(Dictionary<string, object> parameters) { healAmount = System.Convert.ToInt32(parameters["amount"]); }
    public override void Execute(Player owner, Player opponent) { owner.Heal(healAmount); }
}

// 다음 턴 슬롯을 확장하는 효과
public class ExpandSlotsEffect : CardEffect
{
    public override GamePhase TriggerPhase => GamePhase.TurnEnd;
    public override void Initialize(Dictionary<string, object> parameters) { }
    public override void Execute(Player owner, Player opponent) { owner.bonusSlots += 1; }
}

// 전투 중 특정 조건에 따라 공격력 보너스를 주는 효과
public class CombatBonusEffect : CardEffect
{
    public override GamePhase TriggerPhase => GamePhase.DuringCombat; // 페이즈는 DuringCombat과 관련됨
    private int bonusAmount;
    public override void Initialize(Dictionary<string, object> parameters) { bonusAmount = System.Convert.ToInt32(parameters["amount"]); }
    public override void Execute(Player owner, Player opponent) { } // 직접적인 행동은 없음
    public override int GetCombatBonus(Player owner, Player opponent, CardDataSO ownerCard, CardDataSO opponentCard)
    {
        if (opponentCard.attackPower > ownerCard.attackPower)
        {
            return bonusAmount;
        }
        return 0;
    }
}