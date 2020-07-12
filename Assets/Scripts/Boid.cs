using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class Boid : MonoBehaviour
{
    public Light2D light2D;

    private float sicknessSpeed;
    private float sickProbability;
    private bool sick;
    public bool justGotSick {get; private set;}

    private float timeForRecovery;
    private float timePassedForRecovery;

    public Vector3 velocity {get; private set;}


    private void Update() {
        if (!sick)
        {
            this.sickProbability += Time.deltaTime * sicknessSpeed;

            float r = Random.Range(0f, 1f);
            if (r < this.sickProbability)
            {
                this.sick = true;
            }
            this.light2D.color = Color.Lerp(Color.blue, Color.yellow, this.sickProbability);
        }
        else
        {
            timePassedForRecovery += Time.deltaTime;
            if (timePassedForRecovery > timeForRecovery)
            {
                this.sick = false; 
                ResetSicknesProbability();
            }
            this.light2D.color = Color.red;
        }

    }

    // Used by nearby boids
    public void PushSicknessProbability(float value)
    {
        this.sickProbability += value * Time.deltaTime;
    }

    public void ResetSicknesProbability()
    {
        this.sickProbability = 0f;
        this.justGotSick = false;
    } 

    public void ResetVelocity()
    {
        this.velocity = Vector3.zero;
    }

    public void InitBoid(float maxSpeed, float sicknessSpeed, float recoveryTime) {
        this.timeForRecovery = recoveryTime;
        this.sicknessSpeed = sicknessSpeed;
        this.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
        this.velocity = this.transform.rotation * Vector3.right * maxSpeed;
    }

    public void CheckBounds(Vector3 worldExtents)
    {
        Vector3 pos = this.transform.position;

        if (this.transform.position.x < worldExtents.x)
        {
            pos.x = -worldExtents.x-0.1f;
        }
        if (this.transform.position.x > -worldExtents.x)
        {
            pos.x = worldExtents.x+0.1f;
        }
        if (this.transform.position.y < worldExtents.y)
        {
            pos.y = -worldExtents.y-0.1f;
        }
        if (this.transform.position.y > -worldExtents.y)
        {
            pos.y = worldExtents.y+0.1f;
        }
        this.transform.position = pos;
    }

    public Vector3 AvoidObstaclesDir(Vector3 healingCirclePos)
    {
        Vector3 dir = this.transform.position - healingCirclePos;
        float dist = Vector3.Magnitude(dir);
        if (dist < 3f && dist > 1.25f)
        {
            return dir;
        }
        
        // if none collided then return same velocity vector
        return this.velocity;
    }

    public Vector3 Steer(Vector3 desired, float maxForce)
    {
        Vector3 steer = desired - this.velocity;
        steer = Vector3.ClampMagnitude(steer, maxForce);

        return steer;
    }

    public void ApplyAcceleration(Vector3 alignment, float wAlignment, Vector3 separation, float wSeparation, 
                                  Vector3 cohesion, float wCohesion, Vector3 avoidObstacle, float wAvoidObst, float maxSpeed)
    {
        // Accelerate
        Vector3 acceleration = alignment * wAlignment + separation * wSeparation + cohesion * wCohesion + avoidObstacle * wAvoidObst;
        this.velocity = Vector3.ClampMagnitude(this.velocity + acceleration, maxSpeed);

        // Update rotation
        this.transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * Mathf.Atan2(this.velocity.y, this.velocity.x));
    }


    // public Vector3 AvoidObstaclesDir(float rayCastLength)
    // {
    //     float angleStep = 180f/5;
    //     float rotateAngle = 0f;
    //     Vector3 prevDir = Vector3.zero;
        
    //     // define state [collided first or not]
    //     int firstIteration = 0;

    //     float minCollDist = Mathf.Infinity;

    //     for (int i = 0; i < 6; i++)
    //     { 
    //         Vector3 rayDir = this.transform.TransformDirection(Quaternion.Euler(0, 0, rotateAngle) * Vector3.up);
    //         RaycastHit2D hit = Physics2D.Raycast(this.transform.position, rayDir, rayCastLength);

    //         if (hit.collider != null && !hit.collider.CompareTag("Boid"))
    //         {
    //             Debug.DrawRay(this.transform.position, rayDir * hit.distance, Color.red, Time.deltaTime);
             
    //             minCollDist = Mathf.Min(minCollDist, hit.distance);
             
    //             if (firstIteration == 0) firstIteration = 1;
    //             if (firstIteration == 2)
    //             {
    //                 Debug.Log("Return left");
    //                 return prevDir; // / (rayCastLength - minCollDist);
    //             }
    //         }
    //         else
    //         {
    //             Debug.DrawRay(this.transform.position, rayDir * rayCastLength, Color.green, Time.deltaTime);
    //             if (firstIteration == 0) firstIteration = 2;
    //             if (firstIteration == 1)
    //             {
    //                 Debug.Log("Return right");
    //                 return rayDir; // / (rayCastLength - minCollDist);
    //             }
    //         }

    //         prevDir = rayDir;
    //         rotateAngle -= angleStep; 
    //     }

    //     // if all collided (no return until this point & firstIteration=1) go in opposite direction
    //     if (firstIteration == 1)
    //     {
    //         Debug.Log("All collided return");
    //         Debug.DrawRay(this.transform.position, Quaternion.Euler(0, 0, 180f) * this.velocity, Color.blue, 1f);
    //         return Quaternion.Euler(0, 0, 180f) * this.velocity; // / (rayCastLength - minCollDist);
    //     }
        
    //     // if none collided then return same velocity vector
    //     return this.velocity;
    // }
}
