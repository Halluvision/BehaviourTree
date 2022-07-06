using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public static Transform _transform;
    
    [SerializeField]
    private float _radius = 10f;
    [SerializeField]
    private float _speed = 1f;

    private void Awake()
    {
        _transform = transform;
    }

    private void Update()
    {
        float angle = Time.time * _speed;
        Vector3 position = Vector3.zero;
        position.x = _radius * Mathf.Sin(angle);
        position.y = _transform.position.y;
        position.z = _radius * Mathf.Cos(angle);
        _transform.position = position;
    }
}
