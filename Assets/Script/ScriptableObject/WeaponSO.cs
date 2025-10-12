using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Card Game/Equipment/Weapon")]
public class WeaponSO : EquipmentSO
{
    // 무기는 특정 캐릭터 전용일 수 있음 (향후 CharacterSO와 연결 가능)
    // public CharacterSO requiredCharacter; 
}