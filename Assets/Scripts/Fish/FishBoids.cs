using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishBoids : MonoBehaviour {

/*
Each Boid (member of the swarm) will have this script attached to it
The script will be independent for each boid however, i.e every Boid 
needs their own position, direction, velocity, etc.
*/

/*
Settings is a "ScriptableObject" which allows each boid to obtain its 
starting settings without creating 50 new instances of the class.
For more info: https://docs.unity3d.com/Manual/class-ScriptableObject.html
*/
    FishSettings settings;

//These are the values of the boid that update every frame

    public Vector3 velocity;
    public Vector3 acceleration;
    public Vector3 position; //Current position (x,y,z)
    public Vector3 forward; //Current Direction (x,y,z)
    public int howManyPerceived; //How many boids can this boid see 
    public Vector3 avgSwarmDirection; //Of all the boids in the perception radius, what is the average direction  
    public Vector3 avgAvoidanceDirection; //For each perceived boid, there will be an avoidance direction to turn away from that boid, this is the average of avoidance for all perceived boids.
    public Vector3 otherBoidCoordinates; //The centre Coordinate of all perceived boids.
    public Material material; //this boids Material
    public Transform boidTransform; //We "Cache" the transform for optimisation
    public new SphereCollider collider;
    public bool predator;
    private float speeds = 0.8f;
    private float count = 2;
    //Awake happens before anything
    void Awake () {
        material = transform.GetComponentInChildren<MeshRenderer>().material; //Get the material component of this boid
        boidTransform = transform; //Set the Cache
    }

    //We can Initialize the boid with Settings from our Settings Script
    public void Initialize (FishSettings settings) {
        this.settings = settings; //Get a new reference of the settings just for this boid
        position = boidTransform.position; //Initialize the start position
        forward = boidTransform.forward; //Initialize the start direction
        float startSpeed = (settings.minimumSpeed + settings.maximumSpeed) / 4; //Initialize the start speed
        velocity = transform.forward * startSpeed; //Initialize the velocity
    }


    //Update is called every frame
    void Update()
    {
        //If this boid is the target then change its colour to red
        if (ThirdPersonCamera.Target == this.gameObject)
        {
            this.material.color = Color.red;
        }
        else
        {
            this.material.color = Color.blue;
        }
        if (predator) this.material.color = Color.yellow;
        
    }

    public void MoveBoid () {

       acceleration = Vector3.zero; //Start with an acceleration of 0

        if (howManyPerceived != 0) {

            //Divide the sum of the other boids coordinates with how many there are to get the average,
            //This will give back the centre of all perceived boids
            //用其他boid坐标的和除以有多少得到平均值，//这将使一切可感知的男孩回到中心
            otherBoidCoordinates /= howManyPerceived;

            //We need to get the difference between this boids location and the swarms centre location
            //我们需要弄清楚这个鸟群位置和蜂群中心位置的区别
            Vector3 offsetToSwarmCentre = (otherBoidCoordinates - position);

            //Cohesion - Move towards the Centre of the swarm, based on the weight
            //凝聚力-根据重量向蜂群的中心移动
            var cohesion = MoveTowards (offsetToSwarmCentre) * settings.cohesionWeight * count;

            //Alignment - Move towards the average swarms direction, based on the weight
            //对齐-根据重量向群的平均方向移动
            var alignment = MoveTowards (avgSwarmDirection) * settings.alignmentWeight;

            //Seperation - Move towards the avoidance direction, based on the weight
            //分离-根据重量向回避方向移动
            var seperation = MoveTowards (avgAvoidanceDirection) * settings.seperateWeight;

            //Add these new values to the acceleration of the boid
            //将这些新值添加到boid的加速中
            acceleration += cohesion;
            acceleration += alignment;
            acceleration += seperation;
        }

        //Check whether the boid is heading for an obstacle
        //检查boid是否正向障碍物前进
        if (HeadingForObstacle ()) {
            //if it is then get the closest direction that doesnt intersect with an obstacle
            //如果是，那就向最近的不与障碍物相交的方向走
            Vector3 avoidCollisionRay = ObstacleDirections ();
            //And move towards that direction
            //朝那个方向走
            Vector3 avoidCollisionForce = MoveTowards (avoidCollisionRay) * settings.avoidCollisionWeight;
            acceleration += avoidCollisionForce;
        }


        velocity += acceleration * Time.deltaTime;
        float speed = velocity.magnitude;
        Vector3 direction = velocity / speed;
        //Make sure the speed doesn't exceed the min or max speeds
        //确保速度不超过最小或最大速度
        speed = Mathf.Clamp (speed, settings.minimumSpeed, settings.maximumSpeed);
        velocity = direction * speed;


        //Update Boid variables
        // 更新Boid变量
        if (boidTransform.position.y > 5f)
        {
            boidTransform.position -= new Vector3(0, 1, 0);
            boidTransform.forward = direction;
            position = boidTransform.position;
            forward = direction;

        }
        else if (boidTransform.position.x > 10f)
        {
            boidTransform.position -= new Vector3(1, 0, 0);
            boidTransform.forward = direction;
            position = boidTransform.position;
            forward = direction;
        }
        else if (boidTransform.position.z > 10f)
        {
            boidTransform.position -= new Vector3(0, 0, 1);
            boidTransform.forward = direction;
            position = boidTransform.position;
            forward = direction;
        }
        else if (boidTransform.position.x < -10f)
        {
            boidTransform.position += new Vector3(1, 0, 0);
            boidTransform.forward = direction;
            position = boidTransform.position;
            forward = direction;
        }
        else if (boidTransform.position.z < -10f)
        {
            boidTransform.position += new Vector3(0, 0, 1);
            boidTransform.forward = direction;
            position = boidTransform.position;
            forward = direction;
        }
        else
        {
            boidTransform.position += velocity * Time.deltaTime * speeds;
            boidTransform.forward = direction;
            position = boidTransform.position;
            forward = direction;
        }
        
    }

    Vector3 MoveTowards (Vector3 vector) {
        //Normalizing the vector just gives the direction towards it, not the length, 
        //so we can make sure it can only travels the max speed towards it
        //对向量进行归一化只给出它的方向，而不是长度，//这样我们可以确保它只能以最大的速度接近它
        Vector3 v = vector.normalized * settings.maximumSpeed - velocity;
        //https://docs.unity3d.com/ScriptReference/Vector3.ClampMagnitude.html
        return Vector3.ClampMagnitude (v, settings.maxSteerForce);
    }

    bool HeadingForObstacle() {
        //What did the raycast hit?
        RaycastHit hit;
        //If it does hit something, return true
        if (Physics.SphereCast (position, settings.raycastDisplacementRadius, forward, out hit, settings.collisionAvoidDst, settings.obstacleMask)) {
            return true;
        } else 
            return false;
    }

    Vector3 ObstacleDirections() {
        //An array of all the directions the boids can move (300)
        // boids可以移动的所有方向的数组(300)
        Vector3[] rayDirections = Helper.directions;
        //Iterate through all the directions
        for (int i = 0; i < rayDirections.Length; i++) {
            Vector3 dir = boidTransform.TransformDirection (rayDirections[i]);
            //Shoot a ray from this direction
            Ray ray = new Ray (position, dir);
            //If it Doesn't hit anything (an obstacle), then return that directions
            if (!Physics.SphereCast (ray, settings.raycastDisplacementRadius, settings.collisionAvoidDst, settings.obstacleMask)) {
                return dir;
            }
        }
        //If somehow everything around it is an obstacle then just move forward
        //We just need this so that this function always returns something
        return forward;
    }

    
   
    private void OnTriggerEnter(Collider other)
    {

        this.gameObject.SetActive(false);
    }



}