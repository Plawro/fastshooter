using System.Collections;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class DriftController : MonoBehaviour
{
    public Animator anim;
    public Transform player;
    private NavMeshAgent navAgent;
    
    [SerializeField] private LayerMask groundLayer, playerLayer, obstacleMask;

    private float sightRange = 80f;
    private float attackRange = 1.5f;
    private float walkSpeed = 3.5f;
    private float runSpeed = 9f;

    public bool isPlayerInSight, isPlayerInAttackRange;

    public AudioSource footstepAudioSource;
    public AudioSource musicAudioSource;
    public AudioClip footstepSound;
    public AudioClip runningFootstepSound;
    public AudioClip jumpscareSound;

    private Vector3 patrolTarget;
    private bool isPatrolling = true;
    private bool wasChasing = false;

    public float patrolRadius = 10f;
    public Vector3 spawnPoint;

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        navAgent = GetComponent<NavMeshAgent>();
        navAgent.speed = walkSpeed;
        navAgent.updateRotation = true;

        spawnPoint = transform.position;

        patrolTarget = GetRandomPatrolPoint();
    }

    void Update()
    {
        UpdateState();
        HandleMovement();
        UpdateAnimation();
    }

    void UpdateState()
    {
        isPlayerInSight = Physics.CheckSphere(transform.position, sightRange, playerLayer) && CanSeePlayer();
        isPlayerInAttackRange = Physics.CheckSphere(transform.position, attackRange, playerLayer);
    }

    void HandleMovement()
    {
        if (isPlayerInSight)
        {
            wasChasing = true;
            isPatrolling = false;

            Vector3 playerPositionAtFoot = new Vector3(player.position.x, player.transform.position.y, player.position.z);
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            navAgent.speed = distanceToPlayer > 10 ? 16f : runSpeed;

            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToPlayer.x, 0, directionToPlayer.z));
            transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, navAgent.angularSpeed * Time.deltaTime);
    
            
            navAgent.SetDestination(player.position);

            if (distanceToPlayer <= attackRange)
            {
                Attack();
            }
        }
        else if (wasChasing)
        {
            Patrol();
        }
        else if (isPatrolling)
        {
            Patrol();
        }
    }

    void UpdateAnimation()
    {
        float velocityMagnitude = navAgent.velocity.magnitude;

        if (velocityMagnitude > 0.1f)
        {
            anim.SetInteger("walkMode", navAgent.speed > walkSpeed ? 3 : 2);

            if (!footstepAudioSource.isPlaying)
            {
                footstepAudioSource.clip = navAgent.speed > walkSpeed ? runningFootstepSound : footstepSound;
                footstepAudioSource.Play();
            }
        }
        else
        {
            anim.SetInteger("walkMode", 1);
            footstepAudioSource.Stop();
        }
    }

    void Patrol()
    {
        
        if (Vector3.Distance(new Vector3(transform.position.x,0,transform.position.z), new Vector3(patrolTarget.x,0,patrolTarget.z)) < 2f)
        {
            patrolTarget = GetRandomPatrolPoint();
        }

        navAgent.speed = walkSpeed;
        navAgent.SetDestination(patrolTarget);
        isPatrolling = true;
    }

    void Attack()
    {
        anim.SetInteger("walkMode", 5);
        footstepAudioSource.Stop();

        if (!musicAudioSource.isPlaying)
        {
            musicAudioSource.PlayOneShot(jumpscareSound);
        }
        navAgent.velocity = Vector3.zero;
        navAgent.angularSpeed = 0;

        GameController.Instance.jumpscareCameraDrift = this.transform.Find("Virtual Camera").GetComponent<CinemachineVirtualCamera>();
        GameController.Instance.Jumpscare("Drift");
    }

    private Vector3 GetRandomPatrolPoint()
    {
        Vector2 randomPoint = Random.insideUnitCircle * patrolRadius;
        return spawnPoint + new Vector3(randomPoint.x, 0, randomPoint.y);
    }

    bool CanSeePlayer()
    {
        Vector3 directionToPlayer = player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer <= sightRange)
        {
            directionToPlayer.Normalize();
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

            if (angleToPlayer <= 100)
            {
                if (!Physics.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleMask))
                {
                    return true;
                }
            }
        }
        return false;
    }
}
