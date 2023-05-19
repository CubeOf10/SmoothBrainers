using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Helicopter : Entity
{
    //Missiles
    public GameObject missile;
    public float missileAmount = 10;
    public float missileMaxAmount = 10;
    private float missileFireRate = 7f;
    private float missileFireTime;
    public float missileRegenRate = 5.0f;
    //private float missileRegenTimer;
    public GameObject missileLauncher;



    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    protected override void Idle()
    {
        base.Idle();
    }

    protected override void Attacking()
    {
        base.Attacking();

        //Fire Missile
        if (inRange)
        {
            if (Time.time > missileFireTime && missileAmount > 0)
            {
                Instantiate(missile, missileLauncher.transform.position, missileLauncher.transform.rotation);
                missileAmount--;
                missileFireTime = Time.time + missileFireRate;
            }

            ////Regenerate Missiles
            //if (Time.time > missileRegenTimer && missileAmount < missileMaxAmount)
            //{
            //    missileAmount++;
            //    missileRegenTimer = Time.time + missileRegenRate;
            //}
        }
    }

    protected override void Relocating()
    {
        base.Relocating();
    }
}
