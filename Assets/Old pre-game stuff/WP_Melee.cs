using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WP_Melee : MonoBehaviour, IWeapon
{
    Animator m_animator;

    void Start(){
        m_animator=GetComponent<Animator>();
    }
    public void Primary()
    {
        Debug.Log("SHOOT");
        m_animator.SetTrigger("Primary");
    }

    public void Secondary(){

    }

    public void Reload()
    {
        
    }

}
