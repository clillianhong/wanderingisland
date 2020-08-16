using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoissonDisks
{
    /*
    Generates array of 3D points in a layer given 2D poisson points 
    */
    public static List<Vector3> Generate3DLayer(Vector3 center, int seed, float minRadius, int width, int height, int numGenerationAttempts, float maxHeight){
        List<Vector2> basePoints = Generate2DPoissonPoints(new Vector2(center.x, center.y), seed, minRadius, width, height, numGenerationAttempts); 
        float[,] heightMap = Noise.GenerateNoiseMap(1, basePoints.Count, seed, 1, 5, 0.5f, 1.3f, new Vector2(0,0), Noise.NormalizeMode.Local);
        List<Vector3> layerPoints = new List<Vector3>();

		int idx = 0;
        foreach (Vector2 vec in basePoints){
			int idx_X = (int) Mathf.Min(Mathf.CeilToInt(vec.x), heightMap.GetLength(0)-1);
			int idx_Y = (int) Mathf.Min(Mathf.CeilToInt(vec.y), heightMap.GetLength(1)-1);
            layerPoints.Add(new Vector3(vec.x, maxHeight * heightMap[0, idx], vec.y));
			idx++;
        }

        return layerPoints;
    }
    /*
    Generates array of 2D points sampled from a poisson disk distribution 
    */
   public static List<Vector2> Generate2DPoissonPoints(Vector2 center, int seed, float minRadius, int width, int height, int numGenerationAttempts){

       float cellUnit = minRadius/Mathf.Sqrt(2);
       int widthUnits = Mathf.CeilToInt(width/cellUnit);
       int heightUnits =  Mathf.CeilToInt(height/cellUnit);
       int[,] grid = new int[widthUnits, heightUnits];
       List<Vector2> points = new List<Vector2>();
       List<Vector2> pointGens = new List<Vector2>();

       System.Random rand = new System.Random(seed);
       pointGens.Add(new Vector2(rand.Next(0,widthUnits), rand.Next(0,heightUnits)));
	   int numIslands = 0;
       
       while (pointGens.Count > 0) {
			int spawnIndex = Random.Range(0,pointGens.Count);
			Vector2 spawnCentre = pointGens[spawnIndex];
			bool candidateAccepted = false;

			for (int i = 0; i < numGenerationAttempts; i++)
			{
				float angle = Random.value * Mathf.PI * 2;
				Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
				Vector2 candidate = spawnCentre + dir * Random.Range(minRadius, 2*minRadius);
				if (NoConflict(candidate,new Vector2(width,height), cellUnit, minRadius, points, grid)) {
					points.Add(new Vector2(candidate.x + center.x, candidate.y + center.y));
					numIslands++;
					pointGens.Add(candidate);
					grid[(int)(candidate.x/cellUnit),(int)(candidate.y/cellUnit)] = points.Count;
					candidateAccepted = true;
					break;
				}
			}
			if (!candidateAccepted) {
				pointGens.RemoveAt(spawnIndex);
			}
			if(numIslands > 50){
				break;
			}

		}

		return points;
    
   }


   /*
   Checks whether the spawned point interferes with any other points in the radius of collision cells
   */
   static bool NoConflict(Vector2 candidate, Vector2 sampleRegionSize, float cellSize, float radius, List<Vector2> points, int[,] grid) {
		if (candidate.x >=0 && candidate.x < sampleRegionSize.x && candidate.y >= 0 && candidate.y < sampleRegionSize.y) {
			int cellX = (int)(candidate.x/cellSize);
			int cellY = (int)(candidate.y/cellSize);
			int searchStartX = Mathf.Max(0,cellX -2);
			int searchEndX = Mathf.Min(cellX+2,grid.GetLength(0)-1);
			int searchStartY = Mathf.Max(0,cellY -2);
			int searchEndY = Mathf.Min(cellY+2,grid.GetLength(1)-1);

			for (int x = searchStartX; x <= searchEndX; x++) {
				for (int y = searchStartY; y <= searchEndY; y++) {
					int pointIndex = grid[x,y]-1;
					if (pointIndex != -1) {
						float sqrDst = (candidate - points[pointIndex]).sqrMagnitude;
						if (sqrDst < radius*radius) {
							return false;
						}
					}
				}
			}
			return true;
		}
		return false;
	}




    
}
