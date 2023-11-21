using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    public int InitialPopulation;
    public double ElitismRate;
    public double MutationRate;
    public GameObject SquarePrefab;
    public GameObject ChadPrefab;
    public GameObject DagerPrefab;
    public GameObject WinZone;
    public GameObject Checkpoint;
    Population SquarePopulation;
    UnityEngine.Vector2 TargetVector;
    UnityEngine.Vector2 CheckpointVector;
    public Enemy[] Enemies;
    public TextMeshProUGUI InfoLabel;
    void Start()
    {

        TargetVector = new UnityEngine.Vector2(WinZone.transform.position.x, WinZone.transform.position.y);
        CheckpointVector = new UnityEngine.Vector2(Checkpoint.transform.position.x, Checkpoint.transform.position.y);

        this.SquarePopulation = gameObject.AddComponent<Population>();
        this.SquarePopulation.Init(InitialPopulation, this.SquarePrefab, this.ChadPrefab, TargetVector, GenesPerCreature: 5,
            CheckpointVector, MutationRate, ElitismRate);
    }
    float CurrentFrame = 0;
    private void FixedUpdate()
    {
        Tick();
    }
    void Tick()
    {

        if (CurrentFrame >= SquarePopulation.GenesPerCreature) 
        {
            if(SquarePopulation.CurrentGeneration % 5 == 0 && SquarePopulation.GenesPerCreature <= 100)
            {
                SquarePopulation.GenesPerCreature += 1;
                Debug.Log("Current genes per creature: " + SquarePopulation.GenesPerCreature);
            }
            PerformSelection();
            RestoreEnemiesPosition();
            CurrentFrame = 0;
            InfoLabel.text = @$"Generación: {SquarePopulation.CurrentGeneration}; genes por criatura: {SquarePopulation.GenesPerCreature}; población total: {InitialPopulation}.";
        }
        else
        {
            CurrentFrame += Global.StepSubdivisionFactor;
        }

    }

    void RestoreEnemiesPosition()
    {
        foreach (Enemy enemy in Enemies)
        {
            enemy.RestorePosition();
        }
    }

    void PerformSelection()
    {
        if (SquarePopulation.ToleranceFunction())
        {
            //CancelInvoke(nameof(Tick)); //Uncomment if you wanna stop it from running after a certain threshhold (e.g., reached goal).
        }
        else
        {
            SquarePopulation.NaturalSelection();
        }
    }

}
