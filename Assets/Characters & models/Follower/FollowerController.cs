using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;
using System;

public class FollowerController : MonoBehaviour
{
    [SerializeField] Transform[] points;
    private int current;

    [SerializeField] Animator anim;
    [SerializeField] Transform player;
    private NavMeshAgent agent;
    private Vector3 velocity;
    [SerializeField] private LayerMask groundLayer, playerLayer;
    [SerializeField] public LayerMask obstacleMask;
    [SerializeField] public StatsScreen statsScreenController;

    private float sightRange = 25f; // How far does enemy see
    private float attackRange = 2f; // How far does enemy attack ("Arm length")

    private float walkSpeed = 3.5f; // Walking speed, used for patrolling
    private float runSpeed = 8.5f; // Run speed, used for chasing

    public bool isPlayerInSight, isPlayerInAttackRange;
    public bool wasChasing = false; // If returning to patrol mode, helps to determine if searching for player is next step
    public bool isSearching = false; // Helps to know what is happening right now
    public bool isIdling = true; // Helps to know what is happening right now
    public bool isPatrolling = false;// Helps to know what is happening right now
    public bool isChasing = false;
    bool isAttacking = false;

    private Vector3 lastKnownPosition; // Saving players position for better chasing experience
    private float idleTimer = 0f; // "Where is he" effect timer when enemy loses player out of sight
    private float chaseMemoryTimer = 0f;  // Timer helps enemy to not immediately lose player (corners, random glitches)
    private float predictTimer = 0f; // Timer for predict mode
    private float waitBeforePatrol = 2f;  // Time to wait before returning to patrol
    private float patrolWaitTimer = 0f;
    private bool isWaitingToPatrol = false;  // Flag to control waiting state

    [SerializeField] AudioSource footstepAudioSource;
    [SerializeField] AudioSource musicAudioSource;
    [SerializeField] AudioClip footstepSound;
    [SerializeField] AudioClip runningFootstepSound;
    [SerializeField] AudioClip chasingSound;
    [SerializeField] AudioClip findingSound;
    [SerializeField] AudioClip jumpscareSound;

    public float charge;
    public TextMeshProUGUI chargeText;
    Coroutine waitingCoroutine;
    bool hasDeactivatedTower = false;

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

        InvokeRepeating(nameof(CheckPlayerDetection), 0f, 0.2f);
        IdleInf(); // Start in idle
    }

    public void Charge(float ammount)
    {
        if (isIdling && GameController.Instance.gameStarted)
        {
            charge += ammount;
            chargeText.text = $"{charge:F1}%";
            

            if (charge >= 100)
            {
                isIdling = false;
                charge = 0;
                chargeText.text = "NULL";
                isPatrolling = true;
                current = 0;
            }
        }
    }

    void CheckPlayerDetection()
    {
        if ((isPlayerInSight | Vector3.Distance(this.gameObject.transform.position,player.transform.position) < 4) && isPatrolling)
        {
            waitingCoroutine=null;
            isPatrolling = false;
            isChasing = true;
            Chase();
            agent.speed = runSpeed;
        }

        // Check player sight and attack range
        isPlayerInSight = Physics.CheckSphere(transform.position, sightRange, playerLayer) && CanSeePlayer();
        isPlayerInAttackRange = Physics.CheckSphere(transform.position, attackRange, playerLayer);

        if (isPlayerInSight && isChasing)
        {
            lastKnownPosition = player.position; // Update last seen position
            chaseMemoryTimer = 0f; // Reset chase memory timer

            if (isPlayerInAttackRange)
            {
                Attack();
            }
            else
            {
                if (!musicAudioSource.isPlaying || musicAudioSource.clip == findingSound)
                {
                    StartCoroutine(FadeOutAndIn(chasingSound));
                }
                isPatrolling = false;
                isChasing = true;
                Chase();
            }
        }
        else if (chaseMemoryTimer < 2f && wasChasing)
        {
            chaseMemoryTimer += 0.2f; // Increment based on detection interval

            if (!musicAudioSource.isPlaying || musicAudioSource.clip == findingSound)
            {
                StartCoroutine(FadeOutAndIn(chasingSound));
            }
            isPatrolling = false;
            isChasing = true;
            Chase();
        }
        else if (wasChasing)
        {
            if (!musicAudioSource.isPlaying || musicAudioSource.clip == chasingSound)
            {
                StartCoroutine(FadeOut(findingSound));
            }

            Predict();
        }
        else if (isSearching)
        {
            Search();
        }
        else if (isIdling)
        {
            Idle();
        }else if(isPatrolling){
            Patrol();
        }
    }

    void Chase()
    {
        if(!footstepAudioSource.isPlaying){
            footstepAudioSource.PlayOneShot(runningFootstepSound);
        }
        
        wasChasing = true;

        if (agent.velocity.sqrMagnitude > 0.5f) 
        {
            anim.SetInteger("walkMode", 3); // Running animation
        }
        else
        {
            anim.SetInteger("walkMode", 1); // Standing/idle animation
        }
        agent.speed = runSpeed;
        if (isPlayerInSight) // Checking if player is still on the NavMesh
        {
            agent.SetDestination(player.position);
        }
        else
        {
            agent.SetDestination(lastKnownPosition); // Go to last known position
            waitingCoroutine = StartCoroutine(WaitBeforePatrol()); // Start the patrol delay timer
            //Debug.Log("Player out of NavMesh - waiting at last position.");
        }
    }

    IEnumerator WaitBeforePatrol(){
        yield return new WaitForSeconds(3);
        isPatrolling = true;
        Patrol();
    }

    void Attack()
    {
        wasChasing = false;
        anim.SetInteger("walkMode", 5);
        agent.SetDestination(transform.position);
        footstepAudioSource.volume = 0;
        if(musicAudioSource.isPlaying && !isAttacking){
            musicAudioSource.Stop();
            isAttacking = true;
        }
        if(!musicAudioSource.isPlaying){
            musicAudioSource.PlayOneShot(jumpscareSound);
        }
        GameController.Instance.Jumpscare("Follower");
    }

    public void PlayFootstep(){
        footstepAudioSource.PlayOneShot(footstepSound);
    }

    void Predict()
    {
        predictTimer += Time.deltaTime;

        if (agent.velocity.sqrMagnitude > 0.1f) // Determine if enemy is actually moving
        {
            anim.SetInteger("walkMode", 3); // Running animation
        }
        else
        {
            anim.SetInteger("walkMode", 1); // Standing/idle animation
        }

        agent.speed = runSpeed;
        agent.SetDestination(lastKnownPosition);

        if (predictTimer >= 0.3f) // Predict for 4 seconds
        {
            predictTimer = 0;
            wasChasing = false;
            isSearching = true;
            Search();
        }
    }

    void Search()
    {
        if(!musicAudioSource.isPlaying && musicAudioSource.clip == chasingSound){
            musicAudioSource.clip = findingSound;
            musicAudioSource.Play();
        }

        isSearching = true;
        agent.speed = runSpeed;
        agent.SetDestination(lastKnownPosition);
        anim.SetInteger("walkMode", 2); // Walking animation

        if (Vector3.Distance(transform.position, lastKnownPosition) <= 2)
        {
            isSearching = false;
            isPatrolling = true;
            Patrol();
        }
    }

    void StartIdle() // Prepare Idle mode
    {
        anim.SetInteger("walkMode", 1);
        isIdling = true;
        idleTimer = UnityEngine.Random.Range(2f, 3f);
        if(musicAudioSource.isPlaying){
            StartCoroutine(FadeOutAndStop());
        }
        agent.SetDestination(transform.position);
    }

    void IdleInf(){
        anim.SetInteger("walkMode", 1);
        isIdling = true;
        agent.SetDestination(transform.position);
            
        if(musicAudioSource.isPlaying){
            StartCoroutine(FadeOutAndStop());
        }
    }

private IEnumerator FadeOutAndIn(AudioClip newClip)
    {
        for (float t = 0; t < 0.5f; t += Time.deltaTime)
        {
            musicAudioSource.volume = Mathf.Lerp(1, 0, t / 0.5f);
            yield return null;
        }
        musicAudioSource.volume = 0;

        // Switch to the new clip and play
        musicAudioSource.clip = newClip;
        musicAudioSource.Play();

        // Fade in
        for (float t = 0; t < 0.2f; t += Time.deltaTime)
        {
            musicAudioSource.volume = Mathf.Lerp(0, 1, t / 0.2f);
            yield return null;
        }
        musicAudioSource.volume = 1;
    }
    private IEnumerator FadeOut(AudioClip newClip)
    {
        yield return new WaitForSeconds(5);
        for (float t = 0; t < 0.5f; t += Time.deltaTime)
        {
            musicAudioSource.volume = Mathf.Lerp(1, 0, t / 0.5f);
            yield return null;
        }
        musicAudioSource.volume = 0;
    }

    private IEnumerator FadeOutAndStop()
    {
        for (float t = 0; t < 2; t += Time.deltaTime)
        {
            musicAudioSource.volume = Mathf.Lerp(1, 0, t / 2);
            yield return null;
        }
        musicAudioSource.Stop();
        musicAudioSource.volume = 1;
    }

    void Idle()
    {
        if(musicAudioSource.isPlaying){
            FadeOutAndStop();
        }

        anim.SetInteger("walkMode", 1);
        idleTimer -= Time.deltaTime;
        if (idleTimer <= 0)
        {
            if (isPatrolling)
            {
                isPatrolling = true;
                Patrol(); // Resume patrolling if enabled
            }
        }
    }

    void Patrol()
    {
        anim.SetInteger("walkMode", 2);
        agent.speed = walkSpeed;
        lastKnownPosition = player.position; // This shouldn't be here, but let's make it fun
        if (points.Length == 0) return;

        if (wasChasing)
        {
            int closestIndex = GetClosestPointIndex(); // Based on when stops chasing player, go to closest patrol point
            current = (closestIndex + 1) % points.Length;
            wasChasing = false;
        }

        agent.SetDestination(points[current].position);

        if (!agent.pathPending && agent.remainingDistance <= 1f)
        {
            current++;

            if(current == 2 && !hasDeactivatedTower && UnityEngine.Random.Range(1,10) != 2){ // 90% chance of deactivating
                statsScreenController.BreakAntenna();
                hasDeactivatedTower = true; // When returning from chasing player, might turn off the antenna again, that would be annoying
            }

            if (current >= points.Length)
            {
                current = 0;
                agent.SetDestination(points[current+1].position);
                isPatrolling = false;
                hasDeactivatedTower = false;
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

            // Adjust field of view angle to prevent backward detection
            if (angleToPlayer <= 160 / 2) // Decreased from 170 for a narrower FOV
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
        float minDist = Mathf.Infinity;
        int index = 0;
        for (int i = 0; i < points.Length; i++)
        {
            float dist = Vector3.Distance(points[i].position, transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                index = i;
            }
        }
        return index;
    }
}  