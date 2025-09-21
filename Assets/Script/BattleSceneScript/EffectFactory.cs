using System.Collections.Generic;
using UnityEngine;

public static class EffectFactory
{
    public static CardEffect CreateEffect(string effectId, Dictionary<string, object> parameters)
    {
        CardEffect effect = null;
        switch (effectId)
        {
            case "DealDamage": effect = new DamageEffect(); break;
            case "HealSelf": effect = new HealEffect(); break;
            case "ExpandSlots": effect = new ExpandSlotsEffect(); break;
            case "ModifyResistance": effect = new ModifyResistanceEffect(); break;
            case "BuffNextTurnSlot": effect = new BuffNextTurnSlotEffect(); break;
            default: Debug.LogError($"Unknown effectId: {effectId}"); break;
        }
        effect?.Initialize(parameters);
        return effect;
    }
}