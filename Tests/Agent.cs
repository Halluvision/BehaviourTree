using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
    [SerializeField]
    float _speed = 5f;

    Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public bool MoveToPoint(Vector3 position, float maxDistance)
    {
        Vector3 direction = (position - transform.position).normalized;
        _rb.velocity = direction * _speed;
        if (Vector3.Distance(transform.position , position) > maxDistance)
            return false;
        return true;
    }

    public bool Teleport(Vector3 position)
    {
        Vector3 direction = (position - transform.position);
        float distance = direction.magnitude;
        direction.Normalize();

        transform.position += direction * (distance - 2f);
        
        return true;
    }
}
