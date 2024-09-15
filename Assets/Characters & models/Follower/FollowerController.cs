using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FollowerController : MonoBehaviour
{
    public Transform[] points;
    private int current;

    public Animator anim;
    public Transform player;
    private NavMeshAgent agent;
    private Vector3 velocity;
    [SerializeField] private LayerMask groundLayer, playerLayer;
    [SerializeField] public LayerMask obstacleMask;

    private float sightRange = 25f; // How far does enemy see
    private float attackRange = 1.2f; // How far does enemy attack ("Arm length")

    private float walkSpeed = 3.5f; // Walking speed, used for patrolling
    private float runSpeed = 9f; // Run speed, used for chasing

    private bool isPlayerInSight, isPlayerInAttackRange;
    private bool wasChasing = false; // If returning to patrol mode, helps to determine if searching for player is next step
    private bool isSearching = false; // Helps to know what is happening right now
    private bool isIdling = false; // Helps to know what is happening right now
    private bool isPatrolling = false;// Helps to know what is happening right now

    private Vector3 lastKnownPosition; // Saving players position for better chasing experience
    private float idleTimer = 0f; // "Where is he" effect timer when enemy loses player out of sight
    private float chaseMemoryTimer = 0f;  // Timer helps enemy to not immedeately lose player (corners, random glitches)
    float predictTimer = 0; // Timer for predict mode
    public AudioSource footstepAudioSource;
    public AudioClip footstepSound;
    public AudioClip runningFootstepSound;
    public AudioClip chasingSound;
    public AudioClip findingSound;


    // Character has no gravity + animations based on velocity are only on predict & chase mode



    void Start()
    {
        current = 0;
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindWithTag("Player").GetComponent<Transform>();
        lastKnownPosition = transform.position;

        // Initialize NPC status
        isPlayerInSight = false;
        isPlayerInAttackRange = false;
        wasChasing = false;
        isSearching = false;
        isIdling = true;  // Auto idling on start
        isPatrolling = false;
        chaseMemoryTimer = 0f;

        StartIdle(); // Start in idle
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))  // Used for Debug to manually start a patrol
        {
            isPatrolling = !isPatrolling; // Start/stop patrolling (stop = stops in current position)
            current = 0;  // Start from the first point
            wasChasing = false;  // Reset chasing state
            isSearching = false; // Reset searching state
            isIdling = false;    // Reset idling state
        }

        // Check player sight and attack range
        isPlayerInSight = Physics.CheckSphere(transform.position, sightRange, playerLayer) && CanSeePlayer();
        isPlayerInAttackRange = Physics.CheckSphere(transform.position, attackRange, playerLayer);

        if (isPlayerInSight) // System for switching between modes
        {
            lastKnownPosition = player.position; // Update last seen position
            chaseMemoryTimer = 0f; // Reset chase memory timer

            if (isPlayerInAttackRange)
            {
                Attack();
            }
            else
            {
                Chase();
            }
        }
        else if (chaseMemoryTimer < 2f && wasChasing)
        {
            // Continue chasing if within the chase memory duration
            chaseMemoryTimer += Time.deltaTime;
            Chase();
        }
        else if (wasChasing)
        {
            // Player lost, start predicting movement
            Predict();
        }
        else if (isSearching)
        {
            Search();
        }
        else if (isIdling)
        {
            Idle();
        }
        else if (isPatrolling)
        {
            Patrol();
        }
        else
        {
            StartIdle();
        }
    }

    void Chase()
    {
        wasChasing = true;
        isSearching = false;
        isIdling = false;

        if (agent.velocity.sqrMagnitude > 0.5f) // Determine if enemy is actually moving (last position can take less seconds to get to)
        {
            anim.SetInteger("walkMode", 3); // Running animation
        }
        else
        {
            anim.SetInteger("walkMode", 1); // Standing/idle animation
        }

        agent.speed = runSpeed;
        agent.SetDestination(player.position);
    }

    void Attack()
    {
        wasChasing = false;
        anim.SetInteger("walkMode", 1); // Attack animation
        agent.SetDestination(transform.position);
    }



    void Predict()
    {
        predictTimer += Time.deltaTime;

        if (agent.velocity.sqrMagnitude > 0.1f) // Determine if enemy is actually moving (last position can take less seconds to get to)
        {
            anim.SetInteger("walkMode", 3); // Running animation
        }
        else
        {
            anim.SetInteger("walkMode", 1); // Standing/idle animation
        }

        agent.speed = runSpeed;
        agent.SetDestination(lastKnownPosition);

        if (predictTimer >= 4f) // Predict for 4 seconds
        {
            predictTimer = 0;
            wasChasing = false;
            Search();
        }

    }


    void Search()
    {
        isSearching = true;
        agent.SetDestination(lastKnownPosition);
        anim.SetInteger("walkMode", 2); // Walking animation

        if (Vector3.Distance(transform.position, lastKnownPosition) <= 2)
        {
            isSearching = false;
            StartIdle();
        }
    }

    void StartIdle() // Prepare Idle mode
    {
        anim.SetInteger("walkMode", 1);
        isIdling = true;
        idleTimer = Random.Range(2f, 3f);
        agent.SetDestination(transform.position);
    }

    void Idle()
    {
        anim.SetInteger("walkMode", 1);
        idleTimer -= Time.deltaTime;
        if (idleTimer <= 0)
        {
            isIdling = false;
            if (isPatrolling)
            {
                Patrol(); // Resume patrolling if enabled
            }
        }
    }

    void Patrol()
    {
        anim.SetInteger("walkMode", 2);
        agent.speed = walkSpeed;
        lastKnownPosition = player.position; // This shouldn't be here, but let's make it fun (gets players position later, defaultly last position is set when last time player in sight)
        if (points.Length == 0) return;

        if (wasChasing)
        {
            int closestIndex = GetClosestPointIndex(); // Based on when stops chasing player, go to closest patrol point
            current = (closestIndex + 1) % points.Length;
            wasChasing = false;
        }

        agent.SetDestination(points[current].position);

        if (!agent.pathPending && agent.remainingDistance <= 0.5f)
        {
            current++;
            if (current >= points.Length)
            {
                current = 0;
                isPatrolling = false;
                StartIdle();
            }
            else
            {
                agent.SetDestination(points[current].position);
            }
        }
    }

    bool CanSeePlayer()
    {
        Vector3 directionToPlayer = player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer <= sightRange)
        {
            directionToPlayer.Normalize();
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

            // Only for scene view for Debug use
            Debug.DrawLine(transform.position, transform.position + Quaternion.Euler(0, 170 / 2, 0) * transform.forward * sightRange, Color.red); // Right edge of FOV
            Debug.DrawLine(transform.position, transform.position + Quaternion.Euler(0, -170 / 2, 0) * transform.forward * sightRange, Color.red); // Left edge of FOV

            if (angleToPlayer <= 170 / 2)
            {
                if (!Physics.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleMask))
                {
                    return true;
                }
            }
        }
        return false;
    }

    int GetClosestPointIndex()
    {
        int closestIndex = 0;
        float closestDistance = Vector3.Distance(transform.position, points[0].position);
        for (int i = 1; i < points.Length; i++)
        {
            float distanceToPoint = Vector3.Distance(transform.position, points[i].position);
            if (distanceToPoint < closestDistance)
            {
                closestIndex = i;
                closestDistance = distanceToPoint;
            }
        }

        return closestIndex;
    }
}
