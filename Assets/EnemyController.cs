using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float health;
    void Start()
    {
        health = 100;
    }

    // Update is called once per frame
    void Update()
    {
        if(health < 0){
            Destroy(gameObject);
        }
    }

    public void Damage(int damage){
        health -= damage;
    }
}
