using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public enum BehaviourState
{
    Idle,
    Move,
    Attack,
    Defend
}
public class BehaviourAI : MonoBehaviour
{
    private Character character;
    private BehaviourState currentState = BehaviourState.Idle;
    private Character target;
    private float attackRange;
    private CharacterClass charClass;
    void Awake()
    {
        character = GetComponent<Character>();
        target = null;
    }

    void Start()
    {
        charClass = character.characterData.classChar;
        if (charClass == CharacterClass.Mage)
        {
            attackRange = 100f;
        }
        else
        {
            attackRange = 10f;
        }
    }
    private void Update()
    {
        // check xem có defend được không, nếu được thì tạo 1 biến cờ từ character để check
        if (target == null)
            currentState = BehaviourState.Idle;
        else
        {
            float range = Vector3.Distance(transform.position, target.transform.position);
            if (attackRange < range)
                currentState = BehaviourState.Move;
            else
                currentState = BehaviourState.Attack;
        }

        if (currentState == BehaviourState.Attack)
        {
            if (target == BattleManager.Instance.GetClosestEnemy(transform.position))
                return;
            else
                target = BattleManager.Instance.GetClosestEnemy(transform.position);
        }

        float dist = Vector3.Distance(transform.position, target.transform.position);
        if (dist > 2f && currentState != BehaviourState.Move)
        {
            ChangeState(BehaviourState.Move);
        }
        else if (dist <= 2f && currentState != BehaviourState.Attack)
        {
            ChangeState(BehaviourState.Attack);
        }

        HandleStateChange(currentState);
    }

    private void ChangeState(BehaviourState newState)
    {
        currentState = newState;
    }

    private void HandleStateChange(BehaviourState state)
    {
        switch (state)
        {
            case BehaviourState.Move:
                Move();
                break;
            case BehaviourState.Attack:
                Attack(target);
                break;
            case BehaviourState.Defend:
                //defend
                break;
            case BehaviourState.Idle:
                // Có thể clear queue nếu muốn dừng
                //
                break;
        }
    }
    private void Attack(Character enemy)
    {
        if (charClass == CharacterClass.Mage)
        {
            enemy.ApplyDamage(character.characterData.attack, DamageType.Magical, 0);
        }
        else
        {
            enemy.ApplyDamage(character.characterData.attack, DamageType.Physical, 0);
        }
    }

   private void Move()
    {
        transform.position = Vector3.MoveTowards(transform.position,target.transform.position,character.characterData.moveSpeed * Time.deltaTime);
    }
}
