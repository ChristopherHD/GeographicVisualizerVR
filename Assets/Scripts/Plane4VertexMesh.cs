using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plane4VertexMesh : MonoBehaviour {

	public static void CreatePlane4VertexMesh(MeshFilter meshFilter, float vertexDistanceX, float vertexDistanceY){
		Vector3[] vertex = new Vector3[4];
		Vector2[] uv = new Vector2[4];

		vertex [0] = Vector3.zero;	
		vertex [1] = Vector3.right * vertexDistanceX;
		vertex [2] = Vector3.down * vertexDistanceY;
		vertex [3] = vertexDistanceX * Vector3.right+ vertexDistanceY * Vector3.down;

		int[] triangles = new int[6];

		triangles [0] = 0;
		triangles [1] = 1;
		triangles [2] = 2;

		triangles [3] = 2;
		triangles [4] = 1;
		triangles [5] = 3;

		uv[0].x = 0;
		uv[0].y = 1;
		uv[1].x = 1;
		uv[1].y = 1;
		uv[2].x = 0;
		uv[2].y = 0;
		uv[3].x = 1;
		uv[3].y = 0;

		Mesh tempMesh = new Mesh ();
		tempMesh.vertices = vertex;
		tempMesh.triangles = triangles;
		tempMesh.uv = uv;
		meshFilter.mesh = tempMesh;	
		Object.Destroy (tempMesh);

		tempMesh.RecalculateNormals ();
		tempMesh.RecalculateBounds ();
	}
}