using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidManager : Singleton<BoidManager>
{
    public GameObject BoidPrefab;
    public float nearThreshold;
    private List<Boid> boids;

    private void Start() {
        this.boids = new List<Boid>();
    }

    private void Update() {
        UpdateAllBoids();
    }

    public void InstantiateBoid()
    {
        Vector3 randomPosition = Vector3.zero;
        randomPosition.x = Random.Range(0f, 2f);
        randomPosition.y = Random.Range(0f, 2f);
        GameObject boidGO = Instantiate(BoidPrefab, randomPosition, Quaternion.identity);
        Boid boid = boidGO.GetComponent<Boid>();
        boid.InitBoid();
        this.boids.Add(boid);
    }

    // Update is called once per frame
    void UpdateAllBoids()
    {
        foreach(Boid b in boids)
        {
            b.UpdateBoid(boids, nearThreshold);
        }
    }
}
