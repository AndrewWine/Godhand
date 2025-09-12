using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : Singleton<BattleManager>
{
    public List<Character> characterList = new List<Character>();

    void Start()
    {

    }

    private void AddCharacterToList()
    {
        characterList.Add(FindAnyObjectByType<Character>());
    }

    public Character GetClosestEnemy(Vector3 you)
    {
        Character closest = null;
        float minDist = Mathf.Infinity;

        foreach (var character in characterList)
        {
            if (character == null) continue;
            if (!character.CompareTag("Enemy")) continue;
            float dist = Vector3.Distance(you, character.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = character;
            }
        }

        return closest;
    }

    public Character GetClosestAlly(Vector3 you)
    {
        Character closest = null;
        float minDist = Mathf.Infinity;

        foreach (var character in characterList)
        {
            if (character == null) continue;
            if (character.CompareTag("Enemy")) continue;
            float dist = Vector3.Distance(you, character.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = character;
            }
        }

        return closest;
    }

}
