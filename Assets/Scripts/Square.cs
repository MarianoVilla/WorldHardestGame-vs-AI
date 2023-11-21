using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Square : MonoBehaviour
{
    public GameObject GameObj;
    public Vector2[] DNA { get; set; }
    public float Speed;
    public float StepsTaken = 0f;
    public bool Won;
    public bool ReachedCheckpoint;
    public bool Died;
    public Square(Vector2[] DNA)
    {
        this.DNA = DNA;
    }
    private void FixedUpdate()
    {
        if (StepsTaken < DNA.Length && !Died)
        {
            Act();
        }
    }
    public void Act()
    {
        var ClosestIndex = (int)Math.Floor(StepsTaken);
        Vector3 Position = GameObj.transform.position;
        Vector2 Action = DNA[ClosestIndex];

        Position.x += (Action.x * Global.StepSubdivisionFactor) * Speed * Time.deltaTime;
        Position.y += (Action.y * Global.StepSubdivisionFactor) * Speed * Time.deltaTime;
        GameObj.transform.position = Position;

        StepsTaken += Global.StepSubdivisionFactor;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Enemy")
        {
            Died = true;
            gameObject.SetActive(false);
        }
        else if(collision.tag == "Finish")
        {
            Won = true;
        }
        else if(collision.tag == "Checkpoint")
        {
            ReachedCheckpoint = true;
        }
    }

}
