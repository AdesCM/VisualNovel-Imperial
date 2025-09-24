using UnityEngine;
using System.Collections.Generic;

public class PlayerDataManager : MonoBehaviour
{
    // ★★★ 싱글톤(Singleton) 패턴: 게임 내에 단 하나만 존재하도록 보장 ★★★
    public static PlayerDataManager Instance;

    // 플레이어의 모든 캐릭터 데이터를 여기에 저장
    public List<Character> playerCharacters = new List<Character>();
    // public int gold; // 골드, 아이템 등 다른 데이터도 여기에 저장

    void Awake()
    {
        // 이미 PlayerDataManager가 존재하면 새로 생긴 것은 파괴
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        
        // 없다면 이것을 유일한 인스턴스로 지정
        Instance = this;
        
        // ★★★ 이 오브젝트는 씬이 바뀌어도 파괴되지 않도록 설정 ★★★
        DontDestroyOnLoad(this.gameObject);
        
        // 게임 시작 시 초기 캐릭터 생성 (최초 한 번만 실행됨)
        InitializePlayerData();
    }

    private void InitializePlayerData()
    {
        // CharacterSO를 불러와서 초기 캐릭터 데이터를 생성하는 로직
        // 예: CharacterSO knightSO = Resources.Load<CharacterSO>("SO/Characters/briram_spear");
        // playerCharacters.Add(new Character(knightSO));
    }
}