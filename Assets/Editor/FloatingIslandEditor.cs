using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Specialized;

[CustomEditor(typeof(FloatingIslandGenerator))]
public class FloatingIslandEditor : Editor
{
    FloatingIslandGenerator mapGen;

    public override void OnInspectorGUI()
	{
		mapGen = (FloatingIslandGenerator)target;

		if (DrawDefaultInspector())
		{
			if (mapGen.autoUpdate)
			{
				mapGen.DrawMapInEditor();
			}
		}

		if (GUILayout.Button("Generate"))
		{
			mapGen.DrawMapInEditor();

		}
	}

   

}
