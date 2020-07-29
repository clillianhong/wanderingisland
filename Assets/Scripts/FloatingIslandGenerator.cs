using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Collections.Specialized;

public class FloatingIslandGenerator : MonoBehaviour
{
    public int octaves;
    public float persistance;
    public float lacunarity;
    public Noise.NormalizeMode normalizeMode;
    public Vector2 offset;
    public bool autoUpdate;

    public float minRadius;
    public int seed;
    public float islandScale;
    public float noiseScale;
    public float jaggedDensity;
    public float jaggedScale;
    public int meshDensity;
    bool islandCreated; 

    FloatingIsland island;


    // Start is called before the first frame update
    void Start()
    {
        island = new FloatingIsland(new Vector3(0,0,0), minRadius, seed, islandScale, jaggedDensity, jaggedScale, meshDensity);
        islandCreated = true;
    }

    // Update is called once per frame
    void Update()
    {
       
    }
    public FloatingIsland getIsland()
    {
        return island;

    }
    public void UpdateIsland()
    {
        island = new FloatingIsland(new Vector3(0, 0, 0), minRadius, seed, islandScale, jaggedDensity, jaggedScale, meshDensity);
        islandCreated = true;
    }

    public void DrawMapInEditor()
    {
        FloatingIsland testIsland = new FloatingIsland(new Vector3(0, 0, 0), minRadius, seed, islandScale, jaggedDensity, jaggedScale, meshDensity);
        print("floating island created");
        //DrawDebugGizmo(testIsland);
    }

    void OnDrawGizmos()
    {
        if (islandCreated == true)
        {

            Gizmos.color = new Color(1, 0, 0, 0.5f);

            foreach (Vector3 vec in island.baseVertices)
            {
                Gizmos.DrawCube(vec, new Vector3(1, 1, 1));
            }

        }
    }
        public class FloatingIsland
    {
        Vector3 baseCenterPosition;
        float minorRadius;
        float islandScale;
        float jaggedScale;
        float jaggedDensity;
        int meshDensity;
        int seed;
        public Vector3[] baseVertices;
        public FloatingIslandGenerator islandGenerator;

        GameObject meshObject;
        Vector2 position;
        Bounds bounds;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;

        LODInfo[] detailLevels;
        FloatingIslandMesh[] lodMeshes;

        MapData mapData;
        bool mapDataReceived;
        int previousLODIndex = -1;

        public FloatingIsland(Vector3 center, float minRadius, int seed, float islandScale, float jaggedDensity, float jaggedScale, int meshDensity)
        {
            islandGenerator = FindObjectOfType<FloatingIslandGenerator>();
            this.baseCenterPosition = center;
            this.minorRadius = minRadius;
            this.islandScale = islandScale;
            this.jaggedDensity = jaggedDensity;
            this.jaggedScale = jaggedScale;
            this.seed = seed;
            this.meshDensity = meshDensity;
            this.baseVertices = CreateBaseVertices();
        }

        public Vector3[] CreateBaseVertices()
        {
            int divisions = (int)(12f * this.jaggedDensity);
            print("Instantiate outline");
            Vector3[] outline = new Vector3[divisions * meshDensity - (divisions -1)];
            float deltaTheta = 360f / (float)divisions;
            float[,] noiseDivisionMap = Noise.GenerateNoiseMap(divisions, 1,
                this.seed,
                islandGenerator.noiseScale,
                islandGenerator.octaves,
                islandGenerator.persistance,
                islandGenerator.lacunarity,
                islandGenerator.offset,
                islandGenerator.normalizeMode);

            float theta = 0;
            bool firstOriginVecAssigned = false; //avoid duplicates 
            int noiseIdx = 0;

            int increment = meshDensity;
            //generate vertices 
            for (int i = 0; i < divisions * meshDensity - (divisions-1) - meshDensity + 1; i += increment - 1)
            {
                float rayLength = noiseDivisionMap[noiseIdx, 0] * this.jaggedScale + this.minorRadius;
                noiseIdx++;
                float offset = 0;

                increment = 0;
                for (int j = 0; j < meshDensity; j++)
                {
                    float modRayLength = rayLength - offset;
                    float xPart = modRayLength * (float)Math.Cos((double)theta * Math.PI / 180f);
                    float zPart = modRayLength * (float)Math.Sin((double)theta * Math.PI / 180f);
                    if (xPart == 0 && zPart == 0 && !firstOriginVecAssigned)
                    {
                        firstOriginVecAssigned = true;
                    }
                    if (xPart == 0 && zPart == 0 && firstOriginVecAssigned)
                    {
                        break;
                    }
                    offset += rayLength / (float)meshDensity;
                    increment++;
                    outline[i + j] = new Vector3(xPart + this.baseCenterPosition.x, this.baseCenterPosition.y, zPart + baseCenterPosition.z);
                }
                
                theta += deltaTheta;
                print("THETA " + theta);
            }

            //fill in vertices 

            return outline;

        }
    }

    class FloatingIslandMesh
    {

        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        int lod;
        System.Action updateCallback;

        public FloatingIslandMesh(int lod, System.Action updateCallback)
        {
            this.lod = lod;
            this.updateCallback = updateCallback;
        }

        void OnMeshDataReceived(MeshData meshData)
        {
            mesh = meshData.CreateMesh();
            hasMesh = true;

            updateCallback();
        }

        public void RequestMesh(MapData mapData)
        {
            hasRequestedMesh = true;
            //singletonFIGenerator.RequestMeshData(mapData, lod, OnMeshDataReceived);
        }

    }

    [System.Serializable]
    public struct LODInfo
    {
        public int lod;
        public float visibleDstThreshold;
    }




    void OnValidate()
    {
        if (lacunarity < 1)
        {
            lacunarity = 1;
        }
        if (octaves < 0)
        {
            octaves = 0;
        }

        if(meshDensity < 5)
        {
            meshDensity = 5;
        }
        if(islandScale < 1)
        {
            islandScale = 1;
        }
        if (jaggedDensity < 1)
        {
            jaggedDensity = 1;
        }
        if (jaggedScale < 1)
        {
            jaggedScale = 1;
        }
        if (noiseScale < 1)
        {
            noiseScale = 1;
        }


    }


}
