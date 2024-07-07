using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class TestAsync : MonoBehaviour {

	bool threadRunning;
	Thread hilo;
	Vector3 pos;
	public static List<Tile> list;
	public static Dictionary<Tile, float[]> map;
	//public static Dictionary<Tile, Vector3[]> map;
	public static bool finishedWork;
	/*Vector3[] vertex = new Vector3[4];
	int[] triangles = new int[6];
	Vector2[] uv = new Vector2[4];*/
	Tile currentTile;
	float vertexDistance = -1;
	float[] vertexDistanceN;

	Vector3[] vertexN;
	int[] trianglesN;
	Vector2[] uvN;

	void Awake(){
		list = new List<Tile>();
		map = new Dictionary<Tile,float[]> ();
		vertexDistanceN = new float[3];
	}

	IEnumerator UpdateMesh(){
		yield return new WaitUntil(() => finishedWork);
		if(list.Count > 0 && list[0] != null){

			list[0].GetComponent<MeshFilter> ().mesh.vertices = vertexN;
			list[0].GetComponent<MeshFilter> ().mesh.triangles = trianglesN;
			list[0].GetComponent<MeshFilter> ().mesh.uv = uvN;
			list[0].GetComponent<MeshFilter> ().mesh.RecalculateNormals ();
			list[0].GetComponent<MeshFilter> ().mesh.RecalculateBounds ();

			map.Remove (list[0]);
			list.Remove (list[0]);
		} // Quizás desapareció el tile padre que intentábamos dividir de la 
		if(list.Count > 0  && list[0] == null){
			map.Remove (list[0]);
			list.Remove (list[0]);
		}

		vertexDistance = -1;
		finishedWork = false;

		StartCoroutine (UpdateMesh());
	}

	void Start () {
		int n = 15;//GameObject.FindGameObjectWithTag ("GameController").GetComponent<Mapper> ().meshVertexResolution;
		vertexN = new Vector3[n * n];
		trianglesN = new int[(n-1)*(n-1)*6];
		uvN = new Vector2[n * n];
		pos = transform.position;

		//hilo = new Thread (new ParameterizedThreadStart(TareaHilo));
		hilo = new Thread (new ParameterizedThreadStart(TareaHiloMultiple));
		//hilo = new Thread (new ParameterizedThreadStart(MultithreadSphere));
		StartCoroutine (UpdateMesh ());
		StartCoroutine (testTimer ());
		//hilo.IsBackground = true;
		hilo.Start (pos);

	}

	IEnumerator testTimer(){
		yield return new WaitForSeconds (3f);
		Debug.Log ("Hilo vivo? -> "+hilo.IsAlive);
		StartCoroutine (testTimer());
	}

	/*void TareaHilo(object post){
		threadRunning = true;

		while (threadRunning) {
			//	Debug.Log ("haciendo algo o no?: " + !finishedWork + "---"+list.Count ); // Por alguna razón, si quito esto, no se muestran los nuevos tiles

			while (!finishedWork && list.Count > 0) { // La idea sería que el threadRunning sea la variable stopped realmente, el for usado tendría que ser ese while, agrupando términos o de otra manera
				Debug.Log ("haciendo algo"); // Por alguna razón, si quito esto, no se muestran los nuevos tiles
				if(list.Count == 0) break;
				map.TryGetValue (list [0], out vertexDistance); // Posiblemente se detuvo el hilo aquí




				vertex [1] = Vector3.right * vertexDistance;
				vertex [2] = Vector3.down * vertexDistance;
				vertex [3] = vertexDistance * (Vector3.right + Vector3.down);

				triangles [0] = 0;
				triangles [1] = 1;
				triangles [2] = 2;

				triangles [3] = 2;
				triangles [4] = 1;
				triangles [5] = 3;

				uv [0].x = 0;
				uv [0].y = 1;
				uv [1].x = 1;
				uv [1].y = 1;
				uv [2].x = 0;
				uv [2].y = 0;
				uv [3].x = 1;
				uv [3].y = 0;

				list [0].meshGenerated = true;
				finishedWork = true;
			}
		}

	}*/

	public Mesh CreateHorizontalPlane( Vector2 meshSize, int meshVertexResolution )
	{
		//int N_VERTICES = meshVertexResolution * meshVertexResolution;
		Vector2 DISTANCE_BETWEEN_VERTICES = new Vector2( meshSize.x / (float)(meshVertexResolution - 1.0f), meshSize.y / (float)(meshVertexResolution - 1.0f) );
		float DISTANCE_BETWEEN_UV = 1.0f / (float)(meshVertexResolution - 1.0f);


		// Generate vertices and UV.
		for (int row=0; row<meshVertexResolution; row++) {
			for (int column=0; column<meshVertexResolution; column++) {
				int VERTEX_INDEX = row * meshVertexResolution + column;

				vertexN[VERTEX_INDEX].x =  column * DISTANCE_BETWEEN_VERTICES.x;
				vertexN[VERTEX_INDEX].z = 0.0f;
				vertexN[VERTEX_INDEX].y = - row * DISTANCE_BETWEEN_VERTICES.y;

				uvN[VERTEX_INDEX].x = DISTANCE_BETWEEN_UV * column;
				uvN[VERTEX_INDEX].y = 1.0f - DISTANCE_BETWEEN_UV * row;
			}
		}

		// Generate triangles
		int triangleIndex = 0;
		for (int row=0; row<meshVertexResolution - 1; row++) {
			for (int column=0; column<meshVertexResolution - 1; column++) {
				trianglesN[triangleIndex] = GetVertexIndex( row, column, meshVertexResolution );
				trianglesN[triangleIndex + 1] = GetVertexIndex( row, column+1, meshVertexResolution ); 
				trianglesN[triangleIndex + 2] = GetVertexIndex( row+1, column, meshVertexResolution ); 

				trianglesN[triangleIndex + 3] = GetVertexIndex( row, column+1, meshVertexResolution );
				trianglesN[triangleIndex + 4] = GetVertexIndex( row+1, column+1, meshVertexResolution ); 
				trianglesN[triangleIndex + 5] = GetVertexIndex( row+1, column, meshVertexResolution ); 

				triangleIndex += 6;
			}
		}

		Mesh mesh = new Mesh ();
		mesh.vertices = vertexN;
		mesh.triangles = trianglesN;
		mesh.uv = uvN;
		mesh.RecalculateNormals ();
		mesh.RecalculateBounds ();
		return mesh;
	}

	private int GetVertexIndex( int row, int column, int verticesPerRow )
	{
		return row * verticesPerRow + column;
	}


	void TareaHiloMultiple(object post){
		threadRunning = true;
		while (threadRunning) {
			
			while (!finishedWork && list.Count > 0) { // La idea sería que el threadRunning sea la variable stopped realmente, el for usado tendría que ser ese while, agrupando términos o de otra manera
				if(list.Count == 0) break;
				map.TryGetValue (list [0], out vertexDistanceN);

				int meshVertexResolution = (int) vertexDistanceN [2];

				//float distanceX = (list [0].GetComponent<MeshFilter> ().mesh.vertices [0] - list [0].GetComponent<MeshFilter> ().mesh.vertices [1]).magnitude;
				//float distanceY = (list [0].GetComponent<MeshFilter> ().mesh.vertices [0] - list [0].GetComponent<MeshFilter> ().mesh.vertices [meshVertexResolution]).magnitude;

				Vector2 meshSize = new Vector2(vertexDistanceN[0] * Vector2.right.magnitude , vertexDistanceN[1] * Vector2.down.magnitude);

				Vector2 DISTANCE_BETWEEN_VERTICES = new Vector2( meshSize.x / (float)(meshVertexResolution - 1.0f), meshSize.y / (float)(meshVertexResolution - 1.0f) );
				float DISTANCE_BETWEEN_UV = 1.0f / (float)(meshVertexResolution - 1.0f);


				// Generate vertices and UV.
				for (int row=0; row<meshVertexResolution; row++) {
					for (int column=0; column<meshVertexResolution; column++) {
						int VERTEX_INDEX = row * meshVertexResolution + column;

						vertexN[VERTEX_INDEX].x =  column * DISTANCE_BETWEEN_VERTICES.x;
						vertexN[VERTEX_INDEX].z = 0.0f;
						vertexN[VERTEX_INDEX].y = - row * DISTANCE_BETWEEN_VERTICES.y;

						uvN[VERTEX_INDEX].x = DISTANCE_BETWEEN_UV * column;
						uvN[VERTEX_INDEX].y = 1.0f - DISTANCE_BETWEEN_UV * row;
					}
				}

				// Generate triangles
				int triangleIndex = 0;
				for (int row=0; row<meshVertexResolution - 1; row++) {
					for (int column=0; column<meshVertexResolution - 1; column++) {
						trianglesN[triangleIndex] = GetVertexIndex( row, column, meshVertexResolution );
						trianglesN[triangleIndex + 1] = GetVertexIndex( row, column+1, meshVertexResolution ); 
						trianglesN[triangleIndex + 2] = GetVertexIndex( row+1, column, meshVertexResolution ); 

						trianglesN[triangleIndex + 3] = GetVertexIndex( row, column+1, meshVertexResolution );
						trianglesN[triangleIndex + 4] = GetVertexIndex( row+1, column+1, meshVertexResolution ); 
						trianglesN[triangleIndex + 5] = GetVertexIndex( row+1, column, meshVertexResolution ); 

						triangleIndex += 6;
					}
				}

				/*Mesh mesh = new Mesh ();
				mesh.vertices = vertexN;
				mesh.triangles = trianglesN;
				mesh.uv = uvN;
				mesh.RecalculateNormals ();
				mesh.RecalculateBounds ();
				return mesh;*/

				list [0].meshGenerated = true;
				finishedWork = true;
			}
		}
	}

	void OnDisable(){
		Debug.Log ("deshabilitado");
		if(threadRunning){
			threadRunning = false;
			if(!finishedWork) finishedWork = true;
			hilo.Join ();
		}
	}
}