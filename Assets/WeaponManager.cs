using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [SerializeField]
    public int currentWeapon;
    private int weaponCount;
    public GameObject[] weapons;

    void Start()
    {
        weaponCount = weapons.Length;
        Debug.Log("All w." + weaponCount);
		SwitchWeapon(0);
    }

    void Update()
    {
        HandleWeaponSwitching();

        // Tell current weapon to shoot & reload
        if (Input.GetMouseButtonDown(0))
        {
            Primary();
        }

        if (Input.GetMouseButtonDown(1))
        {
            Secondary();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Reload();
        }
    }

    void HandleWeaponSwitching()
{
    float scrollWheel = Input.GetAxis("Mouse ScrollWheel");

    if (scrollWheel > 0f)
    {
        SwitchWeapon(currentWeapon - 1);
    }
    else if (scrollWheel < 0f)
    {
        SwitchWeapon(currentWeapon + 1);
    }
}

void SwitchWeapon(int num)
{
    if (num > weaponCount)
    {
        currentWeapon = 1;
    }
    else if (num < 1)
    {
        currentWeapon = weaponCount;
    }
    else
    {
        currentWeapon = num;
    }

    Debug.Log("Switched to weapon " + currentWeapon);

    for (int i = 0; i < weaponCount; i++)
    {
        if (i == currentWeapon - 1)
        {
            weapons[i].gameObject.SetActive(true);
        }
        else
        {
            weapons[i].gameObject.SetActive(false);
        }
    }
}



    void Primary()
    {
            IWeapon weaponScript = weapons[currentWeapon-1].GetComponent<IWeapon>();

            if (weaponScript != null)
            {
                weaponScript.Primary();
            }
            else
            {
                Debug.LogError("Selected weapon does not implement the IWeapon interface.");
            }
        
    }

    void Secondary()
    {
            IWeapon weaponScript = weapons[currentWeapon-1].GetComponent<IWeapon>();

            if (weaponScript != null)
            {
                weaponScript.Secondary();
            }
            else
            {
                Debug.LogError("Selected weapon does not implement the IWeapon interface.");
            }
        
    }

    void Reload()
    {
            IWeapon weaponScript = weapons[currentWeapon-1].GetComponent<IWeapon>();

            if (weaponScript != null)
            {
                weaponScript.Reload();
            }
            else
            {
                Debug.LogError("Selected weapon does not implement the IWeapon interface.");
            }
        
    }
}
