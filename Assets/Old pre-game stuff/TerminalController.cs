using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerminalController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public LayerMask obstacleLayer;
    private Vector3 targetPosition;
    private bool isMoving = false;

    void Start()
    {
        targetPosition = transform.position;
    }

    void Update()
    {
        if (!isMoving)
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                Move(Vector3.forward);
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                Move(Vector3.back);
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                Move(Vector3.left);
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                Move(Vector3.right);
            }
        }


        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            if (transform.position == targetPosition)
            {
                isMoving = false;
            }
        }
    }

    void Move(Vector3 direction)
    {
        Vector3 newPosition = targetPosition + direction;

        if (!isMoving && !IsObstacle(newPosition))
        {
            targetPosition = newPosition;
            isMoving = true;
        }
    }

    bool IsObstacle(Vector3 position)
    {
        Collider[] hitColliders = Physics.OverlapBox(position, Vector3.one * 0.45f, Quaternion.identity, obstacleLayer);
        
        if (hitColliders.Length > 0)
        {
            foreach (Collider hitCollider in hitColliders)
            {
                Debug.Log("Hit obstacle: " + hitCollider.gameObject.name);
            }
            return true;
        }
        else
        {
            return false;
        }
    }


//Visualize Collision detection
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(targetPosition, Vector3.one * 0.9f);
    }
}
