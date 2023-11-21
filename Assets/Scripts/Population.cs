using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class Population : MonoBehaviour
{
    public List<Square> Squares { get; private set; }
    public List<GameObject> GameObjects { get; private set; }
    double MutationRate;
    double ElitismRate;
    int InitialPopulation;
    public GameObject PussyPrefab;
    public GameObject ChadPrefab;

    public Vector2 Target { get; private set; }
    public Vector2 Checkpoint { get; private set; }
    public int CurrentGeneration { get; private set; }
    public int GenesPerCreature { get; set; }

    public Population()
    {

    }
    public void Init(int InitialPopulation, 
        GameObject PussyPrefab, GameObject ChadPrefab, Vector2 Target, int GenesPerCreature,
        Vector2 Checkpoint,
        double MutationRate = 0.01, double ElitismRate = 0.01)
    {
        this.InitialPopulation = InitialPopulation;
        this.MutationRate = MutationRate;
        this.ElitismRate = ElitismRate;
        this.CurrentGeneration = 0;
        this.GenesPerCreature = GenesPerCreature;
        Squares = new List<Square>(InitialPopulation);
        GameObjects = new List<GameObject>(InitialPopulation);
        this.PussyPrefab = PussyPrefab;
        this.ChadPrefab = ChadPrefab;
        this.Target = Target;
        this.Checkpoint = Checkpoint;
        CreateInitialPopulation();

    }
    Vector2 RandomVector()
    {
        float MinMax = 5f;
        float RandomX = Random.Range(-MinMax, MinMax);
        float RandomY = Random.Range(-MinMax, MinMax);
        return new Vector2(RandomX, RandomY);
    }
    void CreateInitialPopulation()
    {
        while (Squares.Count < InitialPopulation)
        {
           var CurrentSquare = InstantiateSquare(GetRandomDNA());
           Squares.Add(CurrentSquare);
        }
    }
    Square InstantiateSquare(Vector2[] DNA, GameObject Prefab = null)
    {
        Prefab ??= this.PussyPrefab;
        GameObject SquareGameObject = Instantiate(Prefab);
        Square Square = SquareGameObject.GetComponent<Square>();
        Square.GameObj = SquareGameObject;
        Square.DNA = DNA;
        GameObjects.Add(SquareGameObject);
        return Square;
    }
    Vector2[] GetRandomDNA()
    {
        Vector2[] Dna = new Vector2[GenesPerCreature];
        for(int i = 0; i < GenesPerCreature; i++)
        {
            Dna[i] = RandomVector();
        }
        return Dna;
    }
    double FitnessFunction(Square s)
    {
        double Fitness;
        float DistanceFromWin = Vector2.Distance(s.transform.position, Target);
        float DistanceFromCheckpoint = Vector2.Distance(s.transform.position, Checkpoint);
        
        if (s.Won)
        {
            Fitness = 1.0 / (s.StepsTaken * s.StepsTaken);
        }
        else if (!s.ReachedCheckpoint)
        {
            Fitness = 1.0 / ((DistanceFromCheckpoint * DistanceFromCheckpoint) + DistanceFromWin);
        }
        else
        {
            Fitness = 1.0 / DistanceFromWin;
        }
        if (s.Died && s.StepsTaken < s.DNA.Length)
        {
            Fitness *= .9;//Punish death;
        }
        return Fitness;

    }
    public bool ToleranceFunction()
    {
        foreach (var Square in Squares)
        {
            if (Square == null) continue;

            float Distance = Vector2.Distance(Square.transform.position, Target);

            if (Distance <= 0)
            {
                return true;
            }
        }

        return false;
    }
    public void NaturalSelection()
    {
        var CurrentGenElite = Squares.OrderByDescending(x => FitnessFunction(x)).Take((int)(InitialPopulation*ElitismRate));

        List<List<Vector2>> EliteDna = new List<List<Vector2>>();
        foreach(var s in CurrentGenElite)
        {
            List<Vector2> HighestFitnessUpdatedDNA = new List<Vector2>();
            HighestFitnessUpdatedDNA.AddRange(s.DNA);
            //Fill in the gaps in case there was a bump in gen count from previous to next gen
            while (HighestFitnessUpdatedDNA.Count < GenesPerCreature)
            {
                HighestFitnessUpdatedDNA.Add(RandomVector());
            }
            EliteDna.Add(HighestFitnessUpdatedDNA);
        }

        var NextGen = new List<Square>();
        foreach (var s in GameObjects)
        {
            Destroy(s);
        }
        GameObjects.Clear();

        while (NextGen.Count < InitialPopulation)
        {
            Square ParentA = PickParent();
            Square ParentB = PickParent();
            (Vector2[] Child_A_DNA, Vector2[] Child_B_DNA) childrensDNA = Crossover(ParentA.DNA, ParentB.DNA);
            Vector2[] mutated_child_A_DNA = Mutate(childrensDNA.Child_A_DNA);
            Vector2[] mutated_child_B_DNA = Mutate(childrensDNA.Child_B_DNA);
            var ChildA = InstantiateSquare(mutated_child_A_DNA);
            var ChildB = InstantiateSquare(mutated_child_B_DNA);
            NextGen.Add(ChildA);
            NextGen.Add(ChildB);
        }
        Squares = NextGen;
        foreach(var dna in EliteDna)
        {
            Squares.Add(InstantiateSquare(dna.ToArray(), this.ChadPrefab));
        }
        CurrentGeneration++;
    }

    Square PickParent()
    {
        double TotalFitness = Squares.Sum(s => FitnessFunction(s));
        double Random = UnityEngine.Random.value * TotalFitness;
        double AggregatedFitness = 0;

        for (int i = 0; i < Squares.Count; i++)
        {
            AggregatedFitness += FitnessFunction(Squares[i]);
            if (AggregatedFitness >= Random)
            {
                return Squares[i];
            }
        }
        return Squares[^1]; //This shouldn't happen!
    }
    (Vector2[] Child_A_DNA, Vector2[] Child_B_DNA) Crossover(Vector2[] xDNA, Vector2[] yDNA)
    {
        if (xDNA.Length != yDNA.Length)
        {
            throw new Exception("Different DNA lengths, can't crossover!");
        }

        int CrossPoint = Random.Range(0, xDNA.Length);
        Vector2[] Child_A_DNA = new Vector2[xDNA.Length];
        Vector2[] Child_B_DNA = new Vector2[xDNA.Length];

        for (int i = 0; i < xDNA.Length; i++)
        {
            if (i < CrossPoint)
            {
                Child_A_DNA[i] = xDNA[i];
                Child_B_DNA[i] = yDNA[i];
            }
            else
            {
                Child_A_DNA[i] = yDNA[i];
                Child_B_DNA[i] = xDNA[i];
            }
        }

        return (Child_A_DNA, Child_B_DNA);
    }
    Vector2[] Mutate(Vector2[] DNA)
    {
        Vector2[] MutatedDNA = new Vector2[GenesPerCreature];
        var RandomValue = UnityEngine.Random.value;

        for (int i = 0; i < DNA.Length; i++)
        {
            if (RandomValue < MutationRate)
            {
                MutatedDNA[i] = RandomVector();
            }
            else
            {
                MutatedDNA[i] = DNA[i];
            }
        }

        return MutatedDNA;
    }

}
