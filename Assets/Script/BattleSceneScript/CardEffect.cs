using System.Collections.Generic;

public abstract class CardEffect
{
    public abstract GamePhase TriggerPhase { get; }
    public abstract void Initialize(Dictionary<string, object> parameters);
    public abstract void Execute(Player ownerPlayer, Player opponentPlayer, Character ownerUser, Character opponentUser);
}