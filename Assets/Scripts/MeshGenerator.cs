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

		Debug.Log ("baseCenterPosition " + baseCenterPosition.x + " " + baseCenterPosition.y + " " + baseCenterPosition.z);
		int divisions = (int)(12f * jaggedDensity);
		int numVertices = 2 * divisions * meshRings - (divisions - 1);
		
		IslandMeshData meshData = new IslandMeshData(divisions, meshRings); 

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

		float[,] topNoiseMapType2 = Noise.GenerateNoiseMap(meshRings, divisions,
			seed,
			contourNoiseParams.noiseScale,
			contourNoiseParams.octaves,
			contourNoiseParams.persistance,
			contourNoiseParams.lacunarity,
			contourNoiseParams.offset,
			contourNoiseParams.normalizeMode);

		float theta = 0;
		int noiseIdx = 0;

		int halfway = numVertices / 2; 

		int topVertexIndex = halfway + 1;
		int botVertexIndex = halfway - 1;

		int topIdxBegin =  halfway + 1;
		int botIdxBegin = halfway - 1;

		meshData.vertices[halfway] = new Vector3(baseCenterPosition.x, maxTopHeight/2f, baseCenterPosition.z);
		meshData.vertices[0] = new Vector3(baseCenterPosition.x, -maxBotHeight/2f, baseCenterPosition.z);

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

				float curve1 = 1 - ((float)j/(float)meshRings);
				float curve2 = 1 - (float)Math.Sqrt((float)j/(float)meshRings);
				float curve3 = 1 - (float)Math.Pow((float)j/(float)meshRings, 2);
				 

				meshData.vertices[topVertexIndex] = new Vector3(
					xPart + baseCenterPosition.x, 
					baseCenterPosition.y + topNoiseMap[i+j,0] * (maxTopHeight * curve3), 
					zPart + baseCenterPosition.z);

				meshData.vertices[botVertexIndex] = new Vector3(
					xPart + baseCenterPosition.x,
					baseCenterPosition.y - topNoiseMap[i + j, 0] * (maxBotHeight * curve3),
					zPart + baseCenterPosition.z);

				// meshData.vertices[topVertexIndex] = new Vector3(
				// 	xPart + baseCenterPosition.x, 
				// 	baseCenterPosition.y + topNoiseMapType2[j, noiseIdx] * (maxTopHeight * (1f -(float)j/(float)meshRings)), 
				// 	zPart + baseCenterPosition.z);

				// meshData.vertices[botVertexIndex] = new Vector3(
				// 	xPart + baseCenterPosition.x,
				// 	baseCenterPosition.y - topNoiseMapType2[j, noiseIdx] * (maxBotHeight *  ( 1f - (float)j / (float)meshRings)),
				// 	zPart + baseCenterPosition.z);


				// seal center disc 
				if(j == 1 && noiseIdx < divisions-1){
						//add island top triangles
						// meshData.AddTriangle(topVertexIndex, topVertexIndex + (meshRings + 1), topVertexIndex + 1);
						meshData.AddTriangle(topVertexIndex,  topVertexIndex + meshRings, topVertexIndex + (meshRings + 1), true);
						meshData.AddTriangle(topVertexIndex, topVertexIndex + (meshRings - 1), topVertexIndex + meshRings, true);

						meshData.AddTriangle( botVertexIndex,  botVertexIndex - meshRings, botVertexIndex - (meshRings - 1), true);

						//connect to center
						meshData.AddTriangle(topVertexIndex+meshRings-1, topVertexIndex, halfway);
						meshData.AddTriangle(botVertexIndex, botVertexIndex-meshRings + 1, halfway);
				}				

				//add middle triangles
				if(noiseIdx < divisions-1 && j < meshRings - 2){
						//add island top triangles
						meshData.AddTriangle(topVertexIndex, topVertexIndex + (meshRings + 1), topVertexIndex + 1);
						meshData.AddTriangle(topVertexIndex,  topVertexIndex + meshRings, topVertexIndex + (meshRings + 1));
						// add island bottom triangles 
						meshData.AddTriangle(botVertexIndex, botVertexIndex - 1, botVertexIndex - (meshRings + 1));
						meshData.AddTriangle(botVertexIndex, botVertexIndex - (meshRings + 1),  botVertexIndex - meshRings);
				}

				// seal the edges 
				if(noiseIdx < divisions-1 && j == meshRings-2){
					//extra
					meshData.AddTriangle(topVertexIndex, topVertexIndex + (meshRings), topVertexIndex + 1);
					meshData.AddTriangle(botVertexIndex - (meshRings), botVertexIndex, botVertexIndex - 1);

					// //stitch gap
					meshData.AddTriangle(topVertexIndex + 1, topVertexIndex + (meshRings),  botVertexIndex - 1);
					meshData.AddTriangle(botVertexIndex - 1, topVertexIndex + (meshRings),  (botVertexIndex - (meshRings)));
				}

				// seal the end of the disc 
				if(noiseIdx == divisions - 1 && j < meshRings - 1){
					
					//seal the edges
					if( j == meshRings-2){
						//extra
						meshData.AddTriangle(topVertexIndex + 1, topVertexIndex, topIdxBegin);
						meshData.AddTriangle(botVertexIndex - 1, botIdxBegin, botVertexIndex);
						//stitch gap 
						meshData.AddTriangle(topVertexIndex + 1, topIdxBegin, botVertexIndex-1);
						meshData.AddTriangle(botVertexIndex-1, topIdxBegin, botIdxBegin);
					}else{ //normal 
						meshData.AddTriangle(topVertexIndex + 1, topVertexIndex, topIdxBegin);
						meshData.AddTriangle(topVertexIndex + 1, topIdxBegin, topIdxBegin+1);

						meshData.AddTriangle(botVertexIndex - 1, botIdxBegin, botVertexIndex);
						meshData.AddTriangle(botVertexIndex - 1, botIdxBegin-1, botIdxBegin);
					}

					// seal the center
					if(j==1){
						meshData.AddTriangle(botVertexIndex, botIdxBegin, halfway);
						meshData.AddTriangle(topIdxBegin, topVertexIndex, halfway);
					}

					
					topIdxBegin++;
					botIdxBegin--;
				}

				topVertexIndex++;
				botVertexIndex--;
			}
			
			theta += deltaTheta;
			noiseIdx++;
		}

		
		topIdxBegin =  halfway + 1;
		botIdxBegin = halfway - 1;

		topVertexIndex = numVertices - meshRings;
		botVertexIndex = meshRings;

		Debug.Log ( "Divisions " + divisions);
		Debug.Log ( "MeshRings " + meshRings);
		
		// topVertexIndex -= meshRings;
		// botVertexIndex += meshRings;

		//patch the beginnning and end triangles 
		for (int i = 0; i < meshRings; i++){
			
			// if(i == meshRings - 1 ){
			// 	meshData.AddTriangle(topIdxBegin, botVertexIndex-i+1, topVertexIndex+i-1); 
			// 	meshData.AddTriangle(topIdxBegin, botIdxBegin, botVertexIndex-i+1);
			
			// }else{
			// 	//top 
			// 	meshData.AddTriangle(topIdxBegin, topIdxBegin+1, topVertexIndex + i); 
			// 	meshData.AddTriangle(topVertexIndex+i, topVertexIndex+i-1, topIdxBegin); 

			// 	// //bottom 
			// 	meshData.AddTriangle(botIdxBegin, botVertexIndex-i, botIdxBegin-1); 
			// 	meshData.AddTriangle(botVertexIndex-i, botIdxBegin, botVertexIndex-i+1); 
		
			// }  

			//top 
				// meshData.AddTriangle(topIdxBegin, topIdxBegin+1, topVertexIndex + i); 
				// meshData.AddTriangle(topVertexIndex+i, topVertexIndex+i-1, topIdxBegin); 

				// meshData.AddTriangle(botIdxBegin, botVertexIndex-i, botIdxBegin-1, true); 
				// meshData.AddTriangle(botVertexIndex-i, botIdxBegin, botVertexIndex-i+1, true); 

				// // //bottom 
			
				if(i !=  0 && i < meshRings - 1){
					// meshData.AddTriangle(botIdxBegin, botVertexIndex-i, botIdxBegin-1, true); 
					// meshData.AddTriangle(botVertexIndex-i, botIdxBegin, botVertexIndex-i+1, true); 
				}
				
			topIdxBegin++; 
			botIdxBegin--;
		}


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

		Debug.Log ("original size  " + (3 * 4 * meshRings * divisions));
		Debug.Log("old size " + (3 * 2 * (divisions * (1 + 2*(meshRings-2)))));
		triangles = new int[3 * 4 * meshRings * divisions]; //FIGURE OUT TRIANGLES TODO
	}

		public void AddTriangle(int a, int b, int c, bool debug=false) {
		triangles [triangleIndex] = a;
		triangles [triangleIndex + 1] = b;
		triangles [triangleIndex + 2] = c;
		triangleIndex += 3;
		if(debug){
			Debug.Log("ADD TRIANGLE DEBUG ------------");

			try{
				Debug.Log ("COORD A: (" + vertices[triangleIndex]);
			}
			catch{
				Debug.Log("COORD A OUT OF BOUNDS " + triangleIndex);
			}
			try{
				Debug.Log ("COORD B: (" + vertices[triangleIndex + 1]);
			}
			catch{
				Debug.Log("COORD B OUT OF BOUNDS " + (triangleIndex + 1));
			}
			try{
				Debug.Log ("COORD C: (" + vertices[triangleIndex + 2]);
			}
			catch{
				Debug.Log("COORD C OUT OF BOUNDS " + (triangleIndex + 2));
			}
		}
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