﻿using UnityEngine;
using System.Collections;
using System;
using UnityEditor;
using System.Collections.Generic;
using System.Collections.Specialized;


public static class MeshGenerator {

	public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve _heightCurve, int levelOfDetail) {
		AnimationCurve heightCurve = new AnimationCurve (_heightCurve.keys);

		int width = heightMap.GetLength (0);
		int height = heightMap.GetLength (1);
		float topLeftX = (width - 1) / -2f;
		float topLeftZ = (height - 1) / 2f;

		int meshSimplificationIncrement = (levelOfDetail == 0)?1:levelOfDetail * 2;
		int verticesPerLine = (width - 1) / meshSimplificationIncrement + 1;

		MeshData meshData = new MeshData (verticesPerLine, verticesPerLine);
		int vertexIndex = 0;

		for (int y = 0; y < height; y += meshSimplificationIncrement) {
			for (int x = 0; x < width; x += meshSimplificationIncrement) {
				meshData.vertices [vertexIndex] = new Vector3 (topLeftX + x, heightCurve.Evaluate (heightMap [x, y]) * heightMultiplier, topLeftZ - y);
				meshData.uvs [vertexIndex] = new Vector2 (x / (float)width, y / (float)height);

				if (x < width - 1 && y < height - 1) {
					meshData.AddTriangle (vertexIndex, vertexIndex + verticesPerLine + 1, vertexIndex + verticesPerLine);
					meshData.AddTriangle (vertexIndex + verticesPerLine + 1, vertexIndex, vertexIndex + 1);
				}

				vertexIndex++;
			}
		}

		return meshData;

	}  
	public static IslandMeshData GenerateFloatingIslandMesh(
		Vector3 baseCenterPosition,
		float maxTopHeight,
		float maxBotHeight,
		float jaggedScale,
		float islandScale,
		float minorRadius,
		float jaggedDensity, 
		int meshRings,
		int seed,
		Noise.NoiseParams edgeNoiseParams,
		Noise.NoiseParams contourNoiseParams) 
    {
		int divisions = (int)(12f * jaggedDensity);
		int numVertices = 2 * divisions * meshRings - (divisions - 1);
		
		IslandMeshData meshData = new IslandMeshData(divisions, meshRings); 

		Vector3[] topVertices = new Vector3[numVertices / 2];
		Vector3[] botVertices = new Vector3[numVertices / 2];

		float deltaTheta = 360f / (float)divisions;
		float[,] noiseDivisionMap = Noise.GenerateNoiseMap(divisions, 1,
			seed,
			edgeNoiseParams.noiseScale,
			edgeNoiseParams.octaves,
			edgeNoiseParams.persistance,
			edgeNoiseParams.lacunarity,
			edgeNoiseParams.offset,
			edgeNoiseParams.normalizeMode);

		float[,] topNoiseMap = Noise.GenerateNoiseMap(numVertices / 2, 1,
			seed,
			contourNoiseParams.noiseScale,
			contourNoiseParams.octaves,
			contourNoiseParams.persistance,
			contourNoiseParams.lacunarity,
			contourNoiseParams.offset,
			contourNoiseParams.normalizeMode);

		float theta = 0;
		int noiseIdx = 0;

		int topVertexIndex = numVertices / 2;
		int botVertexIndex = numVertices / 2 - 1;

		int sanityCheck = 0; 

		//generate vertices 
		for (int i = 0; i < divisions * (meshRings - 1); i += meshRings - 1)
		{
			
			float rayLength = noiseDivisionMap[noiseIdx, 0] * jaggedScale + minorRadius * islandScale;
			float offset = rayLength;
			float offsetIncr = rayLength / (float)meshRings;

			for (int j = 0; j < meshRings; j++)
			{

				float modRayLength = rayLength - offset;
				float xPart = modRayLength * (float)Math.Cos((double)theta * Math.PI / 180f);
				float zPart = modRayLength * (float)Math.Sin((double)theta * Math.PI / 180f);

				offset -= offsetIncr;
				if(xPart == 0 && zPart == 0)
				{
					continue;
				}

				meshData.vertices[topVertexIndex] = new Vector3(
					xPart + baseCenterPosition.x, 
					baseCenterPosition.y + topNoiseMap[i+j,0] * (maxTopHeight * (1 -(float)j/(float)meshRings)), 
					zPart + baseCenterPosition.z);

				meshData.vertices[botVertexIndex] = new Vector3(
					xPart + baseCenterPosition.x,
					baseCenterPosition.y - topNoiseMap[i + j, 0] * (maxBotHeight * (1 - (float)j / (float)meshRings)),
					zPart + baseCenterPosition.z);

				
				if(noiseIdx < divisions && j < meshRings - 2){
					if(topVertexIndex + (meshRings + 1) < numVertices  && botVertexIndex  - (meshRings - 2) > 0){
						if(topVertexIndex + (meshRings - 1) >= numVertices){
							Debug.Log ("TOP INDEX OUT: " + topVertexIndex);
						}
						if(botVertexIndex  - (meshRings - 2) < 0){
							Debug.Log ("BOTT INDEX OUT: " + botVertexIndex);
						}
						//add island top triangles
						meshData.AddTriangle(topVertexIndex, topVertexIndex + (meshRings + 1),topVertexIndex + 1);
						meshData.AddTriangle(topVertexIndex,  topVertexIndex + meshRings, topVertexIndex + (meshRings + 1));
						//add island bottom triangles 
						meshData.AddTriangle(botVertexIndex, botVertexIndex - 1, botVertexIndex - (meshRings + 1));
						meshData.AddTriangle(botVertexIndex, botVertexIndex - (meshRings + 1),  botVertexIndex - meshRings);
					}
				}
				
				

				topVertexIndex++;
				botVertexIndex--;
			}
			
			theta += deltaTheta;
			noiseIdx++;
		}

		if( sanityCheck == divisions-1) {
			Debug.Log ("SANITY CHECK PASSED");
		}else{
			Debug.Log ("SANITY CHECK FAILED" + sanityCheck);
		}

		//raise central vertex
		topVertices[0] = new Vector3(baseCenterPosition.x, topNoiseMap[numVertices / 2-1, 0] * maxTopHeight, baseCenterPosition.z);
		botVertices[0] = topVertices[0];

		return meshData;

    }

}

public class IslandMeshData {
	public Vector3[] vertices;
	public int[] triangles;
	public Vector2[] uvs;

	int triangleIndex;

	public IslandMeshData(int divisions, int meshRings) {
		int numVerts = 2 * divisions * meshRings - (divisions - 1);
		vertices = new Vector3[numVerts];
		uvs = new Vector2[numVerts];
		// triangles = new int[ 2 * (divisions * (1 + 2*(meshRings-2)))];
		triangles = new int[3 * 4 * meshRings * divisions]; //FIGURE OUT TRIANGLES TODO
	}

		public void AddTriangle(int a, int b, int c) {
		triangles [triangleIndex] = a;
		triangles [triangleIndex + 1] = b;
		triangles [triangleIndex + 2] = c;
		triangleIndex += 3;
	}

	public Mesh CreateMesh() {
		Mesh mesh = new Mesh ();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uvs;
		mesh.RecalculateNormals ();
		return mesh;
	}
}
public class MeshData {
	public Vector3[] vertices;
	public int[] triangles;
	public Vector2[] uvs;

	int triangleIndex;

	public MeshData(int meshWidth, int meshHeight) {
		vertices = new Vector3[meshWidth * meshHeight];
		uvs = new Vector2[meshWidth * meshHeight];
		triangles = new int[(meshWidth-1)*(meshHeight-1)*6];
	}

	public void AddTriangle(int a, int b, int c) {
		triangles [triangleIndex] = a;
		triangles [triangleIndex + 1] = b;
		triangles [triangleIndex + 2] = c;
		triangleIndex += 3;
	}

	public Mesh CreateMesh() {
		Mesh mesh = new Mesh ();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uvs;
		mesh.RecalculateNormals ();
		return mesh;
	}

}