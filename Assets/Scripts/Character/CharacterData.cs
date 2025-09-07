using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Role
{
    Tank,       // Tiền tuyến, chịu đòn
    MeleeDPS,   // Cận chiến gây sát thương đơn mục tiêu
    Mage        // Pháp sư, gây sát thương phép
   
}

public enum CharacterClass
{
    Warrior,//Tank
    Paladin,//Tank
    Berserker,//MelleDPS
    Assassin,//MelleDPS
    Rogue,//MelleDPS
    Slayer,//MelleDPS
    Mage,//Mage
    Healer,//Mage
    ElementorMaster//Mage
}

[CreateAssetMenu(fileName = "NewCharacterData", menuName = "Game/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("Identifiers")]
    public int characterID;   

    [Header("Information")]
    public string characterName;
    public Role role;               
    public CharacterClass classChar; 

    [Header("Stats")]
    public float maxHealth = 100;
    public float maxMana = 10f;
    public float attack = 10;
    public float health = 100;
    public float mana = 10f;
    public float magic = 1;
    public float armor = 1f;
    public float magicResist = 1;

    public float lifeSteal = 0f;  
    public float attackSpeed = 1f;
    public float moveSpeed = 5f;
    [Header("Attributes")]
    public float STR = 1;
    public float INT = 1;
    public float DEX = 1;

    public ExperienceCurve overrideCurve;

}
