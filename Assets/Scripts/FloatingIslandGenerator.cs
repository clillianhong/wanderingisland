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

    public IslandTerrianType[] topRegions;
    public IslandTerrianType[] botRegions;

    public Material islandMaterial;

    bool drawGizmos;


    // Start is called before the first frame update
    void Start()
    {
        CreateIsland();
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
            drawGizmos = false;
            // display.DrawOnlyMesh (island.islandMesh);
            //Texture2D islandTexture = TextureGenerator.TextureFromColourMap(island.islandData.colorMap,  (int) meshDensity-1, (int) (jaggedDensity * 2f) + 1);
            
            //display.DrawIslandMesh(island.islandData.meshData, islandTexture);
        }
    }

    public void CreateIsland()
    {
        island = new FloatingIsland(new Vector3(0, 0, 0), minRadius, seed, islandScale, jaggedDensity, jaggedScale, meshDensity);
        islandCreated = true;
        island.CreateIsland();
    }


    void OnDrawGizmos()
    {
        if (islandCreated == true && drawGizmos)
        {

            Gizmos.color = new Color(1, 0, 0, 0.5f);
            foreach (Vector3 vec in island.islandData.meshData.vertices)
            {             
                Gizmos.DrawCube(vec, new Vector3(1, 1, 1));   
            }


        }
        // Gizmos.color = new Color(0, 0, 1, 1f);
        // Gizmos.DrawCube(new Vector3(0,0,0), new Vector3(1, 1, 1));   
    }
        public class FloatingIsland
    {

        GameObject meshObject;
		Bounds bounds;

		MeshRenderer meshRenderer;
		MeshFilter meshFilter;

        Vector3 baseCenterPosition;
        
        public IslandMapData islandData;
        
        public FloatingIslandGenerator islandGenerator;

        //island attributes
        float minorRadius;
        float islandScale;
        float jaggedScale;
        float jaggedDensity;
        int meshDensity;
        int seed;

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

            meshObject = new GameObject("Terrain Chunk");
			meshRenderer = meshObject.AddComponent<MeshRenderer>();
			meshFilter = meshObject.AddComponent<MeshFilter>();
			meshRenderer.material = islandGenerator.islandMaterial;

            meshObject.transform.position = center * islandScale;
			meshObject.transform.parent = islandGenerator.transform;
			meshObject.transform.localScale = Vector3.one * islandScale;
			SetVisible(false);


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

            this.islandData = MeshGenerator.GenerateFloatingIslandMesh(
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
                contourNoiseParams,
                islandGenerator.topRegions,
                islandGenerator.botRegions);
        }

		public void SetVisible(bool visible) {
			meshObject.SetActive (visible);
		}

		public bool IsVisible() {
			return meshObject.activeSelf;
		}

        public void CreateIsland(){
            Texture2D islandTexture = TextureGenerator.TextureFromColourMap(this.islandData.colorMap,  (int) this.meshDensity-1, (int) (this.jaggedDensity * 2f) + 1);
            meshRenderer.material.mainTexture = islandTexture;
            this.meshFilter.mesh = this.islandData.meshData.CreateMesh();
            SetVisible(true);
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
        if(islandScale < 0){
            islandScale = 0.1f;
        }


    }


}


[System.Serializable]
public struct IslandTerrianType {
	public string name;
	public float height;
	public Color colour;
}

public struct IslandMapData {
    public IslandMeshData meshData;
    public Color[] colorMap;
}
