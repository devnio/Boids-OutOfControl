using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    private Rigidbody2D rb;
    private float velMagnitude;

    private float rayCastLength;

    // This list gets filled at every UpdateBoid call
    private List<Boid> neighbors;

    public void InitBoid() {
        rb = this.GetComponent<Rigidbody2D>();
        this.velMagnitude = 1.0f;
        this.rayCastLength = 2.0f;

        this.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
        this.rb.velocity = this.transform.rotation * Vector3.right * velMagnitude;

        this.neighbors = new List<Boid>();
    }

    /// <summary>
    /// Update boid using the 3 rules:
    /// - Separation
    /// - Alignment
    /// - Cohesion
    /// </summary>
    /// <param name="boids"> List of all boids in the scene.</param>
    /// <param name="T"> Threshold determining neighborhood distance. </param>
    public void UpdateBoid(List<Boid> boids, float T)
    {
        // Get neighboors
        foreach(Boid b in boids)
        {
            float dist = Vector3.Magnitude(b.transform.position - this.transform.position);
            if (dist < T && dist > Mathf.Epsilon)
            {
                neighbors.Add(b);
            }
        }

        // ==============
        // Obstacles
        // ==============
        Vector3 acceleration = Vector3.zero;
        Vector3 avoidObstaclesVel = AvoidObstaclesDir() * 2.5f;
        acceleration += avoidObstaclesVel;

        // TODO: this makes the boids stick to the edges
        // If no neighbors then no need to go through rules, just avoid obstacles
        if (neighbors.Count == 0) //|| Vector3.Magnitude(obstaclesForce) > 3f 
        {
            Steer(acceleration);
            return;
        }

        // ============================
        // 3 RULES
        // ============================
        // =====================
        // Alignment - direction
        // =====================
        Vector3 tempAccumulator = Vector3.zero;
        foreach(Boid b in neighbors)
        {
            float dist = Vector3.Magnitude(this.transform.position - b.transform.position);
            Vector3 rb_vel = b.rb.velocity.normalized / dist;
            tempAccumulator += rb_vel;

            // Vector3 rb_vel = b.rb.velocity;
            // tempAccumulator += rb_vel;
        }
        acceleration += tempAccumulator / boids.Count * 1.0f;

        // =====================
        // Separation
        // ===================== 
        foreach(Boid b in neighbors)
        {
            Vector3 dir = this.transform.position - b.transform.position;
            float dist = Vector3.Magnitude(dir); 
            // Weight the normalized dir by distance (effectively not using sqrt when normalizing -> can be optimized)
            tempAccumulator += dir.normalized / dist;   
        }
        acceleration += tempAccumulator * 0.75f;

        // =====================
        // Cohesion 
        // =====================
        Vector3 centroid = Vector3.zero;
        foreach(Boid b in neighbors)
        {
            centroid += b.transform.position;
        }
        centroid /= neighbors.Count;
        acceleration += (centroid - this.transform.position) * 0.4f;

        // =====================
        // Apply
        // =====================
        Steer(acceleration);
        this.neighbors.Clear();
    }

    public Vector3 AvoidObstaclesDir()
    {
        float angleStep = 180f/5;
        float rotateAngle = 0f;
        Vector3 prevDir = Vector3.zero;
        
        // define state [collided first or not]
        int firstIteration = 0;

        float minCollDist = Mathf.Infinity;

        for (int i = 0; i < 6; i++)
        { 
            Vector3 rayDir = this.transform.TransformDirection(Quaternion.Euler(0, 0, rotateAngle) * Vector3.up);
            RaycastHit2D hit = Physics2D.Raycast(this.transform.position, rayDir, rayCastLength);

            if (hit.collider != null && !hit.collider.CompareTag("Boid"))
            {
                Debug.DrawRay(this.transform.position, rayDir * hit.distance, Color.red, Time.deltaTime);
             
                minCollDist = Mathf.Min(minCollDist, hit.distance);
             
                if (firstIteration == 0) firstIteration = 1;
                if (firstIteration == 2)
                {
                    Debug.Log("Return left");
                    return prevDir / (rayCastLength - minCollDist);
                }
            }
            else
            {
                Debug.DrawRay(this.transform.position, rayDir * rayCastLength, Color.green, Time.deltaTime);
                if (firstIteration == 0) firstIteration = 2;
                if (firstIteration == 1)
                {
                    Debug.Log("Return right");
                    return rayDir / (rayCastLength - minCollDist);
                }
            }

            prevDir = rayDir;
            rotateAngle -= angleStep; 
        }

        // if all collided (no return until this point & firstIteration=1) go in opposite direction
        if (firstIteration == 1)
        {
            Debug.Log("All collided return");
            Debug.DrawRay(this.transform.position, Quaternion.Euler(0, 0, 180f) * this.rb.velocity, Color.blue, 1f);
            return Quaternion.Euler(0, 0, 180f) * this.rb.velocity / (rayCastLength - minCollDist);
        }
        
        // if none collided then return (0,0,0)
        return Vector3.zero;
    }

    /// <summary>
    /// Change angle based on input. Angle 0 points right.
    /// </summary>
    /// <param name="angle"></param>
    public void Steer(Vector3 vel) {
        Vector3 rb_vel = this.rb.velocity;
        this.rb.velocity = Vector3.ClampMagnitude(rb_vel + vel, 3f);

        this.transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * Mathf.Atan2(this.rb.velocity.y, this.rb.velocity.x));
    }

}
