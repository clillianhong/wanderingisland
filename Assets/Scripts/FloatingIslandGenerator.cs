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
    bool islandCreated; 

    FloatingIsland island;


    // Start is called before the first frame update
    void Start()
    {
        island = new FloatingIsland(new Vector3(0,0,0), minRadius, seed, islandScale, jaggedDensity, jaggedScale);
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
        island = new FloatingIsland(new Vector3(0, 0, 0), minRadius, seed, islandScale, jaggedDensity, jaggedScale);
        islandCreated = true;
    }

    public void DrawMapInEditor()
    {
        FloatingIsland testIsland = new FloatingIsland(new Vector3(0, 0, 0), minRadius, seed, islandScale, jaggedDensity, jaggedScale);
        print("floating island created");
        //DrawDebugGizmo(testIsland);
    }

    [DrawGizmo(GizmoType.Selected | GizmoType.Active)]
    static void DrawDebugGizmo(FloatingIsland isl)
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
       
        foreach (Vector3 vec in isl.islandOutline)
        {
            Gizmos.DrawCube(vec, new Vector3(1, 1, 1));
        }
    }


    void OnDrawGizmos()
    {
        if (islandCreated == true)
        {

            Gizmos.color = new Color(1, 0, 0, 0.5f);

            foreach (Vector3 vec in island.islandOutline)
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
        int seed; 
        public Vector3[] islandOutline;
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

        public FloatingIsland(Vector3 center, float minRadius, int seed, float islandScale, float jaggedDensity, float jaggedScale)
        {
            islandGenerator = FindObjectOfType<FloatingIslandGenerator>();
            this.baseCenterPosition = center;
            this.minorRadius = minRadius;
            this.islandScale = islandScale;
            this.jaggedDensity= jaggedDensity;
            this.jaggedScale = jaggedScale; 
            this.seed = seed;
            this.islandOutline = CreateIslandOutline();
        }

        public Vector3[] CreateIslandOutline()
        {
            int divisions = (int) (12f * this.jaggedDensity);
            Vector3[] outline = new Vector3[divisions];
            float deltaTheta = 360f / (float) divisions;
            float[,] noiseDivisionMap = Noise.GenerateNoiseMap(divisions, 1, 
                this.seed, 
                islandGenerator.noiseScale,
                islandGenerator.octaves, 
                islandGenerator.persistance, 
                islandGenerator.lacunarity, 
                islandGenerator.offset, 
                islandGenerator.normalizeMode);

            float theta = 0;  

            for (int i = 0; i<divisions; i++)
            {
                float rayLength = noiseDivisionMap[i, 0] * this.jaggedScale + this.minorRadius;
                float xPart =  rayLength * (float) Math.Cos((double) theta * Math.PI / 180f);
                float zPart =  rayLength * (float) Math.Sin((double) theta * Math.PI / 180f);
                outline[i] = new Vector3(xPart + this.baseCenterPosition.x, this.baseCenterPosition.y, zPart + baseCenterPosition.z);

                theta += deltaTheta;
            }

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
}
