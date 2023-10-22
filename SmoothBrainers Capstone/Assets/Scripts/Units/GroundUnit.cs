using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Inherited from Entity incase different actions were going to occur.
//This changed over the course of development, and both Aerial Unit and 
// Ground Unit use the same, unchanged Entity script
public class GroundUnit : Entity
{

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
    }

    protected override void Relocating()
    {
        base.Relocating();
    }
    protected override void Follow()
    {
        base.Follow();
    }
}
