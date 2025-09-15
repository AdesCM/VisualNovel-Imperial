using System.Collections.Generic;

public abstract class CardEffect
{
    public abstract GamePhase TriggerPhase { get; }
    public abstract void Initialize(Dictionary<string, object> parameters);
    public abstract void Execute(Character ownerUser, Character opponentUser);
    public virtual int GetCombatBonus(RegisteredCardSlot ownerSlot, RegisteredCardSlot opponentSlot)
    {
        return 0;
    }
}