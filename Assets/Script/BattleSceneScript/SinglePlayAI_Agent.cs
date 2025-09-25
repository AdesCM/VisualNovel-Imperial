using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SinglePlayAI_Agent : MonoBehaviour, IPlayerAgent
{
    private Player controlledPlayer;
    private GameManager gameManager;

    private AIPatternSO currentPattern; 

    //시전자와 스킬을 함께 묶어주는 내부 구조체 사용
    private class InnateSkillInfo
    {
        public CardDataSO Card { get; set; }
        public Character Caster { get; set; }
    }
    
    // Inspector에서 받던 List 대신, 내부에서 동적으로 생성할 스킬 풀
    private List<InnateSkillInfo> dynamicSkillPool = new List<InnateSkillInfo>();

    public void Setup(Player player, GameManager gm, AIPatternSO pattern)
    {
        controlledPlayer = player;
        gameManager = gm;
        currentPattern = pattern; // 전달받은 패턴을 저장
        BuildDynamicSkillPool();
    }

    private void BuildDynamicSkillPool()
    {
        dynamicSkillPool.Clear();
        // 현재 제어 중인 플레이어(Player2)의 모든 캐릭터를 순회
        foreach (var character in controlledPlayer.characters)
        {
            // 각 캐릭터의 설계도(SO)에 등록된 고유 스킬들을 가져옴
            foreach (var skill in character.blueprint.innateSkills)
            {
                // 스킬과 그 스킬의 주인(시전자)을 짝지어 스킬 풀에 추가
                dynamicSkillPool.Add(new InnateSkillInfo { Card = skill, Caster = character });
            }
        }
        if (GameConstants.DEBUG_MODE) Debug.Log($"{controlledPlayer.playerName}의 스킬 풀 구성 완료. 총 {dynamicSkillPool.Count}개의 스킬 보유.");
    }

    public void PrepareTurn()
    {
        if (GameConstants.DEBUG_MODE) Debug.Log($"{controlledPlayer.playerName}(AI)가 행동을 결정 중입니다...");
        controlledPlayer.registeredSlots.Clear();

        // ★★★ 1. 현재 턴 번호를 가져옴 ★★★
        int currentTurn = gameManager.GetCurrentTurn();

        // ★★★ 2. 현재 턴에 예정된 행동이 있는지 패턴에서 검색 ★★★
        var actionsForThisTurn = currentPattern.scheduledActions.Where(a => a.turnNumber == currentTurn).ToList();

        if (actionsForThisTurn.Count > 0)
        {
            // ★★★ 3. 예정된 행동이 있다면, 그대로 실행 ★★★
            if (GameConstants.DEBUG_MODE) Debug.Log($"[AI] {currentTurn}턴 패턴 행동 실행!");
            foreach (var action in actionsForThisTurn)
            {
                // 지정된 캐릭터를 시전자로 설정 (여기서는 첫 번째 캐릭터로 고정)
                Character caster = controlledPlayer.characters[0];
                controlledPlayer.registeredSlots.Add(new RegisteredCardSlot
                {
                    cardSO = action.cardToUse,
                    user = caster
                });
                // (향후 targetSlotIndex도 활용 가능)
            }
        }
        else
        {
            // ★★★ 4. 예정된 행동이 없다면, 기존의 무작위 방식으로 행동 (Fallback) ★★★
            if (GameConstants.DEBUG_MODE) Debug.Log($"[AI] {currentTurn}턴에 예정된 행동 없음. 무작위 행동 실행.");
            var shuffledPool = dynamicSkillPool.OrderBy(x => Random.value).ToList();
            for (int i = 0; i < 2 && i < shuffledPool.Count; i++) // 예: 2장 등록
            {
                var chosenSkillInfo = shuffledPool[i];
                controlledPlayer.registeredSlots.Add(new RegisteredCardSlot
                {
                    cardSO = chosenSkillInfo.Card,
                    user = chosenSkillInfo.Caster 
                });
            }
        }
    }
}