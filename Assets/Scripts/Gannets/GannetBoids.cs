using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GannetBoids : MonoBehaviour
{

    /*
    Each Predator (member of the swarm) will have this script attached to it
    The script will be independent for each predator however, i.e every Predator 
    needs their own position, direction, velocity, etc.
    */

    /*
    Settings is a "ScriptableObject" which allows each predator to obtain its 
    starting psettings without creating 50 new instances of the class.
    For more info: https://docs.unity3d.com/Manual/class-ScriptableObject.html
    */
    GannetSettings psettings;

    //These are the values of the predator that update every frame

    public Vector3 velocity;
    public Vector3 acceleration;
    public Vector3 position; //Current position (x,y,z)
    public Vector3 forward; //Current Direction (x,y,z)
    public int howManyPerceived; //How many predators can this predator see 
    public Vector3 avgSwarmDirection; //Of all the predators in the perception radius, what is the average direction  
    public Vector3 avgAvoidanceDirection; //For each perceived predator, there will be an avoidance direction to turn away from that predator, this is the average of avoidance for all perceived predators.
    public Vector3 otherPredatorCoordinates; //The centre Coordinate of all perceived predators.
    public Material material; //this predators Material
    public Transform predatorTransform; //We "Cache" the transform for optimisation
    public Transform target;
    public new SphereCollider collider;
    public bool predator;
    public float timer;//计时器
    private float noBreath = 5;//最大屏息时常
    private float maxView = 50;//最大视距
    private float speeds;//计速器
    private float flySpeed = 1.1f;//飞行速度系数
    private float waterSpeed = 0.9f;//游泳速度系数
    private float toSwimSpeed = 1.2f;//俯冲速度系数
    private float toFlySpeed = 1f;//出水速度系数
    private int fishCount = 10;
    private int nowFishCount = 0;
    private float[] outWater=new float[4];
    private float[] inWater=new float[4];
    private int ran;
    private int ranIn;
    
    //Awake happens before anything
    void Awake()
    {
        material = transform.GetComponentInChildren<MeshRenderer>().material; //Get the material component of this predator
        predatorTransform = transform; //Set the Cache
        if (predator) this.material.color = Color.yellow;
    }


    //We can Initialize the predator with Settings from our Settings Script
    public void Initialize(GannetSettings psettings)
    {
        this.psettings = psettings; //Get a new reference of the psettings just for this predator
        position = predatorTransform.position; //Initialize the start position
        forward = predatorTransform.forward; //Initialize the start direction
        float startSpeed = (psettings.minimumSpeed + psettings.maximumSpeed) / 2; //Initialize the start speed
        velocity = transform.forward * startSpeed; //Initialize the velocity
                                                   //body.velocity = velocity;
    }


    //Update is called every frame
    void Update()
    {

        if (timer < noBreath)
        {
            outWater[0] = Random.Range(Mathf.Sqrt(3)/3, 1)/2 ;
            outWater[1] = Random.Range(Mathf.Sqrt(3)/3, 1) /2;
            outWater[2] = Random.Range(Mathf.Sqrt(3)/3, 1) /2;
            outWater[3] = Random.Range(Mathf.Sqrt(3)/3, 1) /2;
            ran = Random.Range(0, 4);
        }
        else
        {
            inWater[0] = Random.Range(1, Mathf.Sqrt(3))/2;
            inWater[1] = Random.Range(1, Mathf.Sqrt(3))/2;
            inWater[2] = Random.Range(1, Mathf.Sqrt(3))/2 ;
            inWater[3] = Random.Range(1, Mathf.Sqrt(3))/2;
            ranIn = Random.Range(0, 4);
        }
        //If this predator is the target then change its colour to red
        if (ThirdPersonCamera.Target == this.gameObject)
        {
            this.material.color = Color.red;
        }
        if (predatorTransform.position.y < 5.5f)
        {
            collider.isTrigger = true;
            timer += Time.deltaTime;
            if (timer < noBreath)
            {
                speeds = waterSpeed;
            }
            else
            {
                speeds = toFlySpeed;
            }

        }
        else
        {
            timer -= Time.deltaTime * 2.5f;
            if (timer < 0) timer = 0;
            if (target == null)
            {
                speeds = flySpeed;
                collider.isTrigger = false;
            }
            else
            {
                speeds = toSwimSpeed;
                collider.isTrigger = true;
            }

        }

        if (nowFishCount < fishCount)
        {
            if (timer >= noBreath) target = null;
            else if (ClosestPrey() != null) target = ClosestPrey().transform;
        }
        else
        {
            target = null;
        }

    }

    public void MovePredator()
    {


        acceleration = Vector3.one; //Start with an acceleration of 0

        if (target != null)
        {
            Vector3 offsetToTarget = (target.position - position);
            acceleration = MoveTowards(offsetToTarget) * psettings.targetWeight;
        }

        if (howManyPerceived != 0)
        {

            //Divide the sum of the other predators coordinates with how many there are to get the average,
            //This will give back the centre of all perceived predators
            otherPredatorCoordinates /= howManyPerceived;

            //We need to get the difference between this predators location and the swarms centre location
            Vector3 offsetToSwarmCentre = (otherPredatorCoordinates - position);

            //Cohesion - Move towards the Centre of the swarm, based on the weight
            var cohesion = MoveTowards(offsetToSwarmCentre) * psettings.cohesionWeight;

            //Alignment - Move towards the average swarms direction, based on the weight
            var alignment = MoveTowards(avgSwarmDirection) * psettings.alignmentWeight;

            //Seperation - Move towards the avoidance direction, based on the weight
            var seperation = MoveTowards(avgAvoidanceDirection) * psettings.seperateWeight;

            //Add these new values to the acceleration of the predator
            acceleration += cohesion;
            acceleration += alignment;
            acceleration += seperation;
        }

        //Check whether the predator is heading for an obstacle
        if (HeadingForObstacle())
        {

            //if it is then get the closest direction that doesnt intersect with an obstacle
            Vector3 avoidCollisionRay = ObstacleDirections();
            //And move towards that direction
            Vector3 avoidCollisionForce = MoveTowards(avoidCollisionRay) * psettings.avoidCollisionWeight;
            acceleration += avoidCollisionForce;
        }

        //Some physics equations
        // V - m/s,  a - m/s^2, T - s
        velocity += acceleration * Time.deltaTime;

        //Speed is the scalar of the velocity vector
        float speed = velocity.magnitude;
        // 
        Vector3 direction = velocity / speed;

        //Make sure the speed doesn't exceed the min or max speeds
        speed = Mathf.Clamp(speed, psettings.minimumSpeed, psettings.maximumSpeed);
        //Update the velocity to effectively clamp the velocity as well
        
        if (timer >= noBreath)
        {
            direction.y = outWater[ran];
            velocity = direction * speed * speeds;
            predatorTransform.position += velocity * Time.deltaTime;
            //Set the new direction of the predator
            predatorTransform.forward = direction;
            //Set the new position of the predator
            position = predatorTransform.position;
            forward = direction;
        }
        else
        {
            
            //Set the new direction of the predator
            if (target != null&& predatorTransform.position.y > 5.5f && predatorTransform.position.y <5.5f)
            {
                direction.y = inWater[ranIn];
                velocity = direction * speed * speeds;
                predatorTransform.position += velocity * Time.deltaTime;
                predatorTransform.forward = direction;
                //Set the new position of the predator
                position = predatorTransform.position;
                forward = direction;

            }
            else
            {
                velocity = direction * speed * speeds;
                predatorTransform.position += velocity * Time.deltaTime;
                predatorTransform.forward = direction;
                //Set the new position of the predator
                position = predatorTransform.position;
                forward = direction;
            }
            
        }

    }

    Vector3 MoveTowards(Vector3 vector)
    {
        //Normalizing the vector just gives the direction towards it, not the length, 
        //so we can make sure it can only travels the max speed towards it
        Vector3 v = vector.normalized * psettings.maximumSpeed - velocity;
        //https://docs.unity3d.com/ScriptReference/Vector3.ClampMagnitude.html
        return Vector3.ClampMagnitude(v, psettings.maxSteerForce);
    }

    bool HeadingForObstacle()
    {
        //What did the raycast hit?
        RaycastHit hit;
        //If it does hit something, return true
        if (Physics.SphereCast(position, psettings.raycastDisplacementRadius, forward, out hit, psettings.collisionAvoidDst, psettings.obstacleMask))
        {
            return true;
        }
        else
            return false;
    }

    Vector3 ObstacleDirections()
    {
        //An array of all the directions the predators can move (300)
        Vector3[] rayDirections = Helper.directions;
        //Iterate through all the directions
        for (int i = 0; i < rayDirections.Length; i++)
        {
            Vector3 dir = predatorTransform.TransformDirection(rayDirections[i]);
            //Shoot a ray from this direction
            Ray ray = new Ray(position, dir);
            //If it Doesn't hit anything (an obstacle), then return that directions
            if (!Physics.SphereCast(ray, psettings.raycastDisplacementRadius, psettings.collisionAvoidDst, psettings.obstacleMask))
            {
                return dir;
            }
        }
        //If somehow everything around it is an obstacle then just move forward
        //We just need this so that this function always returns something
        return forward;
    }

    GameObject ClosestPrey()
    {
        GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag("Boid");
        GameObject closest = null;
        float distance = maxView;
        Vector3 position = transform.position;
        foreach (GameObject go in gos)
        {
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;
            }
        }

        return closest;

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Boid")
        {
            nowFishCount += 1;
            if (nowFishCount > fishCount)
            {
                Debug.Log("hunting finished");
            }
        }

    }

}