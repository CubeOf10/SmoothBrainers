using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Rigidbody _rigidbody;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public void Throw(Vector3 throwVelocity)
    {
        _rigidbody.velocity = throwVelocity;
    }

}