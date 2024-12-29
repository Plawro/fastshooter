using System.Collections;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class SentinelController : MonoBehaviour
{
    public Animator anim;
    private NavMeshAgent navAgent;
    
    [SerializeField] private LayerMask groundLayer, playerLayer, obstacleMask;
    private float walkSpeed = 3f;

    public AudioSource footstepAudioSource;
    public AudioSource musicAudioSource;
    public AudioClip footstepSound;
    public AudioClip outsidePlayerSound;
    public Transform player;

    private Vector3 switchPos;
    private Vector3 basePos;
    bool isPlayerInSight;

    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        navAgent.speed = walkSpeed;
        navAgent.updateRotation = true;

        basePos = transform.position;
    }

    void Update(){ // Sends info signals, after 3 of them, goes to switch antenna off, if sees player, returns back
        isPlayerInSight = Physics.CheckSphere(transform.position, 10, playerLayer) && CanSeePlayer();

        if(isPlayerInSight){
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToPlayer.x, 0, directionToPlayer.z));
            transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, navAgent.angularSpeed * Time.deltaTime);
        }
            
        navAgent.SetDestination(player.position);
    }

    bool CanSeePlayer()
    {
        Vector3 directionToPlayer = player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer <= 10)
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

    void UpdateAnimation()
    {
        float velocityMagnitude = navAgent.velocity.magnitude;

        if (velocityMagnitude > 0.1f)
        {
            anim.SetInteger("walkMode", 2);
            if (!footstepAudioSource.isPlaying)
            {
                footstepAudioSource.clip = footstepSound;
                footstepAudioSource.Play();
            }
        }
        else
        {
            anim.SetInteger("walkMode", 1);
            footstepAudioSource.Stop();
        }
    }
}
