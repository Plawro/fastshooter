using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FollowerController : MonoBehaviour
{
    public Animator anim;
    public Transform player;
    NavMeshAgent agent;
    [SerializeField] LayerMask groundLayer, playerLayer;
    
    bool isPlayerInSight, isPlayerInAttackRange;
    [SerializeField] float sightRange, attackRange;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

		player = GameObject.FindWithTag("Player").GetComponent<Transform>();

    }

    void Update()
    {
        isPlayerInSight = Physics.CheckSphere(transform.position, sightRange, playerLayer);
        isPlayerInAttackRange = Physics.CheckSphere(transform.position, attackRange, playerLayer);

        if(!isPlayerInSight && !isPlayerInAttackRange) Idle();
        if(isPlayerInSight && !isPlayerInAttackRange) Chase();
        if(isPlayerInSight && isPlayerInAttackRange) Attack();
	}

    void Chase(){
        agent.SetDestination(player.transform.position);
        anim.SetBool("isRunning", true);
    }

    void Attack(){

    }

    void Idle(){
        anim.SetBool("isRunning", false);
        agent.SetDestination(transform.position);
    }

}
