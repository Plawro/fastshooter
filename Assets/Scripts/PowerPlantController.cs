using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerPlantController : MonoBehaviour
{
    public float minPower;
    public float maxPower;
// -60 & 60

    public float addedPower;
    public float decreasedPower;
    public float power;
    public GameObject arrow;

    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        power -= 0.002f;
        arrow.transform.eulerAngles = new Vector3(
        arrow.transform.eulerAngles.x,
        arrow.transform.eulerAngles.y,
        power
        );

        

        if(power < minPower){
            //No energy for ya
        }

        if(power > maxPower){
            //KABOOM
        }
    }

    public void addPower(float ammount){
        if(ammount == 1){
            power += 0.03f + Mathf.Clamp((power + 60) * 0.002f, 0, 0.2f);
            power = Mathf.Clamp(power, minPower, maxPower);
        }else if(ammount == -1){
            power -= 0.05f + Mathf.Clamp((power + 60) * 0.005f, 0, 1f);
            power = Mathf.Clamp(power, minPower, maxPower);
        }
    }
}
