using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishBoidController : MonoBehaviour {

    const int threadGroupSize = 1024;

    public FishSettings settings;
    public ComputeShader compute;
    FishBoids[] boids;

    void Start() {
        boids = FindObjectsOfType<FishBoids>();
        foreach (FishBoids b in boids) {
            b.Initialize(settings);
        }

    }

    void FixedUpdate () {
        if (boids != null) {

            int numBoids = boids.Length;
            var boidData = new BoidData[numBoids];

            for (int i = 0; i < boids.Length; i++) {
                boidData[i].position = boids[i].position;
                boidData[i].direction = boids[i].forward;
            }

            var boidBuffer = new ComputeBuffer(numBoids, BoidData.Size);
            boidBuffer.SetData(boidData);

            compute.SetBuffer(0, "boids", boidBuffer);
            compute.SetInt("numBoids", boids.Length);
            compute.SetFloat("viewRadius", settings.perceptionRadius);
            compute.SetFloat("avoidRadius", settings.avoidanceRadius);

            int threadGroups = Mathf.CeilToInt(numBoids / (float) threadGroupSize);
            compute.Dispatch(0, threadGroups, 1, 1);

            boidBuffer.GetData(boidData);

            for (int i = 0; i < boids.Length; i++) {
                boids[i].avgSwarmDirection = boidData[i].SwarmDirection;
                boids[i].otherBoidCoordinates = boidData[i].SwarmCentre;
                boids[i].avgAvoidanceDirection = boidData[i].avoidanceDirection;
                boids[i].howManyPerceived = boidData[i].numSwarm;

                boids[i].MoveBoid();
            }

            boidBuffer.Release();
        }
    }

    public struct BoidData {
        public Vector3 position;
        public Vector3 direction;

        public Vector3 SwarmDirection;
        public Vector3 SwarmCentre;
        public Vector3 avoidanceDirection;
        public int numSwarm;

        

        public static int Size {
            get {
                return sizeof(float) * 3 * 5 + sizeof(int);
            }
        }
    }
}