using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Collections.Specialized;

public class FloatingIslandGenerator : MonoBehaviour
{
    public enum DrawMode {Gizmos, Mesh};
    public DrawMode drawMode;
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

    public float noiseScale_top;
    public int octaves_top;
    public float persistance_top;
    public float lacunarity_top;
    public Vector2 offset_top;

    public float maxTopHeight;
    public float maxBotHeight;

    bool drawGizmos;



    // Start is called before the first frame update
    void Start()
    {
   
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    public FloatingIsland getIsland()
    {
        return island;

    }
   
    public void DrawMapInEditor()
    {
        MapDisplay display = FindObjectOfType<MapDisplay> ();

        CreateIsland();

        if(drawMode == DrawMode.Gizmos){
            drawGizmos = true;
        }else if(drawMode == DrawMode.Mesh){
            display.DrawOnlyMesh (island.islandMesh);
        }
       

    }

    public void CreateIsland()
    {
        island = new FloatingIsland(new Vector3(0, 0, 0), minRadius, seed, islandScale, jaggedDensity, jaggedScale, meshDensity);
        islandCreated = true;
    }


    void OnDrawGizmos()
    {
        if (islandCreated == true && drawGizmos)
        {

            Gizmos.color = new Color(1, 0, 0, 0.5f);
            foreach (Vector3 vec in island.topVertices)
            {             
                Gizmos.DrawCube(vec, new Vector3(1, 1, 1));   
            }

            Gizmos.color = new Color(0, 1, 0, 0.5f);
            foreach (Vector3 vec in island.botVertices)
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
        public Vector3[] topVertices;
        public Vector3[] botVertices;
        public Tuple<Vector3[], Vector3[]> islandVertices;
        public FloatingIslandGenerator islandGenerator;

        public IslandMeshData islandMesh;

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
            if(islandScale <= 0)
            {
                this.islandScale = 1f; 
            }
            this.jaggedDensity = jaggedDensity;
            this.jaggedScale = jaggedScale;
            this.seed = seed;
            this.meshDensity = meshDensity;

            Noise.NoiseParams edgeNoiseParams = new Noise.NoiseParams(islandGenerator.noiseScale,
                islandGenerator.octaves,
                islandGenerator.persistance,
                islandGenerator.lacunarity,
                islandGenerator.offset,
                islandGenerator.normalizeMode);

            Noise.NoiseParams contourNoiseParams = new Noise.NoiseParams(islandGenerator.noiseScale_top,
              islandGenerator.octaves_top,
              islandGenerator.persistance_top,
              islandGenerator.lacunarity_top,
              islandGenerator.offset_top,
              islandGenerator.normalizeMode);

            this.islandMesh = MeshGenerator.GenerateFloatingIslandMesh(
                this.baseCenterPosition,
                islandGenerator.maxTopHeight,
                islandGenerator.maxBotHeight,
                islandGenerator.jaggedScale,
                islandGenerator.islandScale,
                this.minorRadius,
                islandGenerator.jaggedDensity, 
                this.meshDensity,
                this.seed,
                edgeNoiseParams,
                contourNoiseParams);

            this.islandVertices = CreateBaseVertices();
            this.topVertices = this.islandVertices.Item1;
            this.botVertices = this.islandVertices.Item2;
        }
        public Tuple<Vector3[], Vector3[]> CreateBaseVertices()
        {
            int divisions = (int)(12f * this.jaggedDensity);
            int numVertices = divisions * meshDensity - (divisions - 1);

            Vector3[] topVertices = new Vector3[numVertices];
            Vector3[] botVertices = new Vector3[numVertices];

            float deltaTheta = 360f / (float)divisions;
            float[,] noiseDivisionMap = Noise.GenerateNoiseMap(divisions, 1,
                this.seed,
                islandGenerator.noiseScale,
                islandGenerator.octaves,
                islandGenerator.persistance,
                islandGenerator.lacunarity,
                islandGenerator.offset,
                islandGenerator.normalizeMode);

            float[,] topNoiseMap = Noise.GenerateNoiseMap(numVertices, 1,
              this.seed,
              islandGenerator.noiseScale_top,
              islandGenerator.octaves_top,
              islandGenerator.persistance_top,
              islandGenerator.lacunarity_top,
              islandGenerator.offset_top,
              islandGenerator.normalizeMode);

            float theta = 0;
            int noiseIdx = 0;

            //generate vertices 
            for (int i = 0; i < divisions * (meshDensity - 1); i += meshDensity - 1)
            {
                float rayLength = noiseDivisionMap[noiseIdx, 0] * this.jaggedScale + this.minorRadius * this.islandScale;
                float offset = rayLength;
                float offsetIncr = rayLength / (float)meshDensity;
                for (int j = 0; j < meshDensity; j++)
                {
                    float modRayLength = rayLength - offset;
                    float xPart = modRayLength * (float)Math.Cos((double)theta * Math.PI / 180f);
                    float zPart = modRayLength * (float)Math.Sin((double)theta * Math.PI / 180f);

                    offset -= offsetIncr;
                    if(xPart == 0 && zPart == 0)
                    {
                        continue;
                    }
                    topVertices[i + j] = new Vector3(
                        xPart + this.baseCenterPosition.x, 
                        this.baseCenterPosition.y + topNoiseMap[i+j,0] * (islandGenerator.maxTopHeight * (1 -(float)j/(float)meshDensity)), 
                        zPart + baseCenterPosition.z);

                    botVertices[i + j] = new Vector3(
                        xPart + this.baseCenterPosition.x,
                        this.baseCenterPosition.y - topNoiseMap[i + j, 0] * (islandGenerator.maxBotHeight * ( (float) Math.Sqrt(1f - (float)j / (float)meshDensity)) ),
                        zPart + baseCenterPosition.z);
                }
                
                theta += deltaTheta;
                noiseIdx++;
            }

            //raise central vertex
            topVertices[0] = new Vector3(this.baseCenterPosition.x, topNoiseMap[numVertices-1, 0] * islandGenerator.maxTopHeight, baseCenterPosition.z);
            botVertices[0] = topVertices[0];

            print("Finished up generating vertices");
            return new Tuple<Vector3[], Vector3[]>(topVertices, botVertices);

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
