using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    private Rigidbody2D rb;
    private float velMagnitude;

    // This list gets filled at every UpdateBoid call
    private List<Boid> neighbors;

    public void InitBoid() {
        rb = this.GetComponent<Rigidbody2D>();
        this.velMagnitude = 1.0f;

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

        if (neighbors.Count == 0) return;

        // 3 RULES
        // Alignment
        Vector3 totVel = Vector3.zero;
        foreach(Boid b in neighbors)
        {
            // totVel += b.transform.rotation.eulerAngles.z; 
            Vector3 rb_vel = b.rb.velocity.normalized;
            totVel += rb_vel;
        }

        // Separation 
        foreach(Boid b in neighbors)
        {
            Vector3 dir = this.transform.position - b.transform.position;
            float dist = Vector3.Magnitude(dir); 
            // Weight the normalized dir by distance (effectively not using sqrt when normalizing -> can be optimized)
            totVel += dir.normalized / dist;   
        }

        // Cohesion 
        Vector3 centroid = Vector3.zero;
        foreach(Boid b in neighbors)
        {
            centroid += b.transform.position;
        }
        centroid /= neighbors.Count;
        totVel += centroid - this.transform.position;

        // Average
        totVel = totVel / 3*neighbors.Count;

        // Clamp speed
        totVel = Vector3.ClampMagnitude(totVel, 3f);

        Steer(totVel);
        
        this.neighbors.Clear();
    }

    /// <summary>
    /// Change angle based on input. Angle 0 points right.
    /// </summary>
    /// <param name="angle"></param>
    public void Steer(Vector3 vel) {
        this.rb.velocity = vel;
        this.transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * Mathf.Atan2(vel.y, vel.x));
    }

}
