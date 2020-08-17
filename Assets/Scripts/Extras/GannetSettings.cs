using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]

/*
BoidSettings is a "ScriptableObject" which allows each boid to obtain its 
starting settings without creating 50 new instances of the class.
For more info: https://docs.unity3d.com/Manual/class-ScriptableObject.html
*/

public class GannetSettings : ScriptableObject {
    // FishBoids Settings
    public float minimumSpeed = 3f; 
    public float maximumSpeed = 8f;
    public float perceptionRadius = 2.5f; //How far boids can see other boids
    public float avoidanceRadius = 1f; //H
    public float maxSteerForce = 3f; //How quickly a boid can change direction

    //How much the boids prioritise each behaviour rule
    public float alignmentWeight = 1f; //How much they want to go in the same direction
    public float cohesionWeight = 1f; //How much they want to stay in the centre of the pack
    public float seperateWeight = 1f; //How much they want to seperate from other boids

    public float targetWeight = 1;

    //These Variables are to do with collision between a boid and an obstacle
    //Namely in FishBoids.cs IsHeadingForCollision() and ObstacleRays()

    //We give any obstacle that the boid wants to avoid a "LayerMask" which tells it that it is an obstacle
    public LayerMask obstacleMask;

    //Hard to explain and not too important -- See the "Radius" parameter in Physics.SphereCast
    //https://docs.unity3d.com/ScriptReference/Physics.SphereCast.html
    public float raycastDisplacementRadius = .27f;

    //How much the boids want to avoid a collision with an obstacle
    public float avoidCollisionWeight = 10;

    //How far the ray casts from the boid -- Basically how far the boid can see in regards to obstacles
    public float collisionAvoidDst = 5;

}