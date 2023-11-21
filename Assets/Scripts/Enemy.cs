using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public bool GoingLeft;
    public float Speed;
    Vector3 InitialPosition;
    private void Start()
    {
        InitialPosition = transform.position;
    }
    private void FixedUpdate()
    {
        if (GoingLeft) 
        {
            var Position = transform.position;
            Position.x -= Speed * Time.deltaTime;
            transform.position = Position;
        }
        else
        {
            var Position = transform.position;
            Position.x += Speed * Time.deltaTime;
            transform.position = Position;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Bounce")
        {
            GoingLeft = !GoingLeft;
        }
    }
    public void RestorePosition()
    {
        transform.position = InitialPosition;
    }
}
