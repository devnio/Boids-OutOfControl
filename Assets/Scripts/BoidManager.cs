using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidManager : Singleton<BoidManager>
{
    Vector3 worldExtents;

    private List<Boid> boids;
    private List<Boid> neighbors;

    private bool separateMode;

    public GameObject BoidPrefab;
    public HealingZone HealingCircle;

    [Header("Initial Values")]    
    public int initialClustersOfBoids;
    public int amountOfBoidsPerClusterMin;
    public int amountOfBoidsPerClusterMax;

    [Header("Normal State")]    
    public float nearThreshold;
    public float rayCastLength;
    public float maxSpeed;
    public float maxForce;

    public float wAlignment;
    public float wSeparation;
    public float wCohesion;
    public float wAvoidObst;

    [Header("Separation State")]
    public float newWSeparation;
    private float currentSeparationWeight;
    private float separatorNeighborsPercent;

    [Header("Sickness")]
    public float sicknessSpeed;
    public float timeForRecovery;

    private void Start() {
        this.HealingCircle.gameObject.SetActive(false);

        worldExtents = Camera.main.ViewportToWorldPoint(Vector2.zero);

        this.boids = new List<Boid>();
        this.neighbors = new List<Boid>();

        for (int i = 0; i < initialClustersOfBoids; i++)
        {
            int amountOfBoids = Random.Range(amountOfBoidsPerClusterMin, amountOfBoidsPerClusterMax);
            Vector3 randomPosition = GetRandomPosition(6f, 4.5f);
            InstantiateBoids(randomPosition, amountOfBoids);
        }

        this.currentSeparationWeight = this.wSeparation;
        this.separatorNeighborsPercent = 0.4f;
    }

    private void Update() {
        UpdateBoidsState();
        UpdateBoidsPosition();
    }

    public void InstantiateBoids(Vector3 randomPosition, int amountOfBoidsAtPosition)
    {
        for (int i = 0; i < amountOfBoidsAtPosition; i++)
        {
            Vector3 offset = GetRandomPosition(0.5f, 0.5f);
            GameObject boidGO = Instantiate(BoidPrefab, randomPosition + offset, Quaternion.identity);
            Boid boid = boidGO.GetComponent<Boid>();
            boid.InitBoid(maxSpeed, this.sicknessSpeed, this.timeForRecovery);
            this.boids.Add(boid);
        }
    }

    private Vector3 GetRandomPosition(float maxX, float maxY)
    {
        Vector3 randomPosition = Vector3.zero;
        randomPosition.x = Random.Range(-maxX, maxX);
        randomPosition.y = Random.Range(-maxY, maxY);
        return randomPosition;
    }

    void UpdateBoidsPosition()
    {
        foreach(Boid b in boids)
        {
            b.transform.position = b.transform.position + b.velocity * Time.deltaTime;
        }
    }

    // Update is called once per frame
    void UpdateBoidsState()
    {
        foreach(Boid b in boids)
        {
            if (CheckInsideHealingCircle(b)) continue;
            UpdateBoid(b, boids, nearThreshold);
        }
    }

    /// <summary>
    /// Update boid using the 3 rules:
    /// - Separation
    /// - Alignment
    /// - Cohesion
    /// </summary>
    /// <param name="boids"> List of all boids in the scene.</param>
    /// <param name="T"> Threshold determining neighborhood distance. </param>
    public void UpdateBoid(Boid boid, List<Boid> boids, float T)
    {
        // =====================
        // Check bounds
        // =====================
        boid.CheckBounds(worldExtents);

        // =====================
        // Check sickness
        // =====================
        bool justSick = boid.justGotSick;
        if (justSick)
        {
            boid.ResetSicknesProbability();
        } 

        // =====================
        // Get neighboors
        // =====================
        foreach(Boid b in boids)
        {
            float dist = Vector3.Magnitude(b.transform.position - boid.transform.position);
            if (dist < T && dist > Mathf.Epsilon)
            {
                neighbors.Add(b);
            }

            if(justSick)
            {
                b.ResetSicknesProbability();
            }

        }

        // ==============
        // Obstacles
        // ==============
        Vector3 alignment = Vector3.zero;
        Vector3 separation = Vector3.zero;
        Vector3 cohesion = Vector3.zero;

        // Vector3 avoidObstaclesVel = boid.AvoidObstaclesDir(rayCastLength);
        Vector3 avoidObstaclesVel = Vector3.zero;
        if (this.HealingCircle.gameObject.activeSelf)
        {
            avoidObstaclesVel = boid.AvoidObstaclesDir(this.HealingCircle.transform.position);
            avoidObstaclesVel = boid.Steer(avoidObstaclesVel.normalized * maxSpeed, maxForce);
        }

        if (neighbors.Count == 0) 
        {
            // boid.ApplyAcceleration(alignment, wAlignment, separation, wSeparation, cohesion, wCohesion, avoidObstaclesVel, wAvoidObst, maxSpeed);
            // Clean up neighbors
            this.neighbors.Clear();
            return;
        }

        // ============================
        // 3 RULES
        // ============================
        // =====================
        // Alignment - direction
        // =====================
        foreach(Boid b in neighbors)
        {
            Vector3 rb_vel = b.velocity;
            alignment += rb_vel;
        }
        alignment /= boids.Count;
        alignment = boid.Steer(alignment.normalized * maxSpeed, maxForce);

        // =====================
        // Separation
        // ===================== 
        float half = T*this.separatorNeighborsPercent;
        foreach(Boid b in neighbors)
        {   
            Vector3 dir = boid.transform.position - b.transform.position;
            float dist = Vector3.Magnitude(dir);
            if (dist < half)
            {
                separation += dir.normalized / dist; 

                // Push sickness prob if Im sick
                if (justSick)
                {
                    b.PushSicknessProbability(T*2 - dist);
                }  
            }
        } 
        separation = boid.Steer(separation.normalized * maxSpeed, maxForce);

        // =====================
        // Cohesion 
        // =====================
        Vector3 centroid = Vector3.zero;
        float halfCohesion = T*(0.5f);
        int count = 0;
        foreach(Boid b in neighbors)
        {
            Vector3 dir = boid.transform.position - b.transform.position;
            float dist = Vector3.Magnitude(dir);
            if (dist < halfCohesion)
            {
                centroid += b.transform.position;
                count++;
            }
        }
        if (count == 0) cohesion = Vector3.zero;
        else
        {
            centroid /= count;//neighbors.Count;
            cohesion = boid.Steer((centroid - boid.transform.position).normalized * maxSpeed, maxForce);
        }

        // =====================
        // Apply
        // =====================
        boid.ApplyAcceleration(alignment, wAlignment, separation, currentSeparationWeight, cohesion, wCohesion, avoidObstaclesVel, wAvoidObst, maxSpeed);
        // boid.ApplyAcceleration(alignment, wAlignment, separation, wSeparation, cohesion, wCohesion, Vector3.zero, wAvoidObst, maxSpeed);
        // Clean up neighbors
        this.neighbors.Clear();

        // ==================
        // Pay if using separation
        // ==================
        if(separateMode)
        {
            GameManager.Instance.UseSeparator();
        }
    }

    public bool CheckInsideHealingCircle(Boid b)
    {
        if (this.HealingCircle.gameObject.activeSelf && (Vector3.Magnitude(this.HealingCircle.transform.position - b.transform.position) < 1.25f))
        {
            b.ResetVelocity();
            return true;
        }
        return false;
    }


    public void PlaceHealingCircle(Vector3 pos)
    {
        // this.HealingCircle.gameObject.SetActive(true);
        this.HealingCircle.transform.position = pos;
        this.HealingCircle.StartHealingZone();
    }
    
    public void ActivateSeparation()
    {
        if (!separateMode)
        {
            separateMode = true;
            this.currentSeparationWeight = this.newWSeparation;
            this.separatorNeighborsPercent = 0.8f;
            // Debug.Log("START - Current sep w: " + this.currentSeparationWeight);
        }
        
    }

    public void DeactivateSeparation()
    {
        if (separateMode)
        {
            separateMode = false;
            this.currentSeparationWeight = this.wSeparation;
            this.separatorNeighborsPercent = 0.4f;
            // Debug.Log("END - Current sep w: " + this.currentSeparationWeight);
        }
    }

}
