using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SinglePlayAI_Agent : MonoBehaviour, IPlayerAgent
{
    private Player controlledPlayer;
    private GameManager gameManager;

    private AIPatternSO currentPattern; 

     private List<RuntimeCard> dynamicSkillPool = new List<RuntimeCard>();

    public void Setup(Player player, GameManager gm, AIPatternSO pattern)
    {
        controlledPlayer = player;
        gameManager = gm;
        currentPattern = pattern; // 전달받은 패턴을 저장
        BuildDynamicSkillPool();
    }

    public void BuildDynamicSkillPool()
    {
        dynamicSkillPool.Clear();
        // 현재 제어 중인 플레이어(Player2)의 모든 캐릭터를 순회
        foreach (var character in controlledPlayer.characters)
        {
            // 각 캐릭터의 설계도(SO)에 등록된 고유 스킬들을 가져옴
            foreach (var skill in character.blueprint.innateSkills)
            {
                // 스킬과 그 스킬의 주인(시전자)을 짝지어 스킬 풀에 추가
                dynamicSkillPool.Add(new RuntimeCard { CardSO = skill, Caster = character });
            }
        }
        if (GameConstants.DEBUG_MODE) Debug.Log($"{controlledPlayer.playerName}의 스킬 풀 구성 완료. 총 {dynamicSkillPool.Count}개의 스킬 보유.");
    }

    public void PrepareTurn()
    {
        if (GameConstants.DEBUG_MODE) Debug.Log($"{controlledPlayer.playerName}(AI)가 행동을 결정 중입니다...");
        controlledPlayer.registeredCards.Clear();

        bool hasExecutedPattern = false;

        // ★★★ 1. AI 패턴이 있는지(null이 아닌지) 먼저 확인 ★★★
        if (currentPattern != null)
        {
            int currentTurn = gameManager.GetCurrentTurn();
            var actionsForThisTurn = currentPattern.scheduledActions.Where(a => a.turnNumber == currentTurn).ToList();

            if (actionsForThisTurn.Count > 0)
            {
                // ★★★ 2. 패턴이 있고, 현재 턴에 할 일이 있다면 -> 패턴대로 행동 ★★★
                foreach (var action in actionsForThisTurn)
                {
                    Character caster = controlledPlayer.characters.FirstOrDefault();
                    if (caster != null)
                    {
                        // ★★★ InnateSkillInfo 대신 RuntimeCard 객체를 생성하여 추가 ★★★
                        controlledPlayer.registeredCards.Add(new RuntimeCard { CardSO = action.cardToUse, Caster = caster });
                    }
                }
                hasExecutedPattern = true; // 패턴을 실행했다고 표시
            }
        }

        // ★★★ 3. 패턴이 없거나(null), 현재 턴에 할 일이 없다면 -> 무작위로 행동 ★★★
        if (!hasExecutedPattern)
        {
            if (GameConstants.DEBUG_MODE) Debug.Log($"[AI] 지정된 패턴 없음. 무작위 행동 실행.");
            var shuffledPool = dynamicSkillPool.OrderBy(x => Random.value).ToList();
            // 예시: 2장 등록
            for (int i = 0; i < 4 && i < shuffledPool.Count; i++)
            {
                var chosenSkillInfo = shuffledPool[i];
                controlledPlayer.registeredCards.Add(shuffledPool[i]);
            }
        }
    }
}