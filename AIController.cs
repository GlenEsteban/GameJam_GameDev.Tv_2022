using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class AIController : MonoBehaviour
{
    [Header("Attack Configurations")]
    [SerializeField] float chaseDistance = 6f;
    [SerializeField] float timeBetweenAttacks = 1f;
    bool isCurrentCharacter;
    bool isStaying = false;
    bool isAttacking = false;
    float timeSinceLastAttack;
    int specialAttackChargeUp;
    CharacterManager characterManager;
    Health health;
    Abilities abilities;
    NavMeshAgent navMeshAgent;
    Transform targetDestination;

    public bool GetIsStaying()
    {
        return isStaying;
    }

    void Start()
    {
        characterManager = FindObjectOfType<CharacterManager>();
        health = GetComponent<Health>();
        abilities = GetComponent<Abilities>();
        navMeshAgent = GetComponent<NavMeshAgent>();

        GetComponent<SphereCollider>().radius = chaseDistance;

        if (transform.parent.tag == "Necromancer")
        {
            tag = "Follower";
            transform.parent = FindObjectOfType<CharacterManager>().transform;
            GetComponent<PlayerInput>().enabled = true;
            GetComponent<PlayerController>().enabled = true;
        }
        else
        {
            tag = "Enemy";
        }
    }
    void Update()
    {
        if (health.GetIsDead()) {return;}

        CheckIfCurrentCharacter();
        timeSinceLastAttack += Time.deltaTime;

        if (!isCurrentCharacter && !isStaying && !isAttacking)
        {
            UpdateTarget();
            StartFollowing();
        }
        else if (isCurrentCharacter || isStaying && !isAttacking)
        {
            StopFollowing();
        }
    }

    private void CheckIfCurrentCharacter()
    {
        if (gameObject == characterManager.GetCurrentCharacter())
        {
            isCurrentCharacter = true;
        }
        else
        {
            isCurrentCharacter = false;
        }
    }

    void OnStay(InputValue value)
    {
        if(value.isPressed)
        {
            isStaying = !isStaying;
        }
    }

    void OnTriggerStay(Collider other) 
    {
        if (isCurrentCharacter) {return;}

        if (other.GetComponent<Health>() != null && other.GetComponent<Health>().GetIsDead())
        {
            isAttacking = false;
            return;
        }

        if (tag == "Follower" && other.tag == "Enemy")
        {
            isAttacking = true;
            StartAttacking(other.gameObject);
        }
        else if (tag == "Enemy")
        {
            if (other.tag == "Necromancer" || other.tag == "Follower")
            {
                float distanceToTarget = Vector2.Distance(transform.position, targetDestination.position);
                if (distanceToTarget < navMeshAgent.stoppingDistance)
                {
                    isAttacking = true;
                    StartAttacking(other.gameObject);
                }
            }
        }
        else
        {
            isAttacking = false;
        }
    }
    void OnTriggerExit(Collider other) 
    {
        isAttacking = false;
        StartFollowing();
    }

    void UpdateTarget()
    {
        if (tag == "Follower" && targetDestination != characterManager.GetCurrentCharacter())
        {
            targetDestination = characterManager.GetCurrentCharacter().transform;
        }
        else if (tag == "Enemy")
        {
            targetDestination = transform;
        }
    }

    void StartFollowing()
    {
        navMeshAgent.enabled = true;
        navMeshAgent.destination = targetDestination.position;
        //FaceTarget(targetDestination); // Fix bug where it rotates instantly
    }
    void FaceTarget(Transform target)
    {
        transform.LookAt(target.transform); // Fix bug where it rotates instantly
    }
    void StartAttacking(GameObject target)
    {
        if (navMeshAgent.enabled != false)
        {
            navMeshAgent.destination = target.transform.position;
            FaceTarget(target.transform);
        }

        if (timeSinceLastAttack > timeBetweenAttacks)
        {
            if (specialAttackChargeUp == abilities.GetChargeRequiredTillSpecial())
            {
                BroadcastMessage("SpecialAbility");
                specialAttackChargeUp = 0;
            }
            else
            {
                BroadcastMessage("Ability");
                specialAttackChargeUp ++;
            }

            timeSinceLastAttack = 0;
        }
    }
    void StopFollowing()
    {
        navMeshAgent.enabled = false;
    }
}