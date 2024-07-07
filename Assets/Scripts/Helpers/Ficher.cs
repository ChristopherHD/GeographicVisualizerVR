using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Ficher : MonoBehaviour {

	private int latitudes;
	private int longitudes;
	private int divitions;

	private static GameObject text;
	private static GameObject sphere;

	void Start(){
		latitudes = UVSphereGenerator.nbLat;
		longitudes = UVSphereGenerator.nbLong;
		text = GameObject.FindGameObjectWithTag ("clone");
		sphere = GameObject.FindGameObjectWithTag ("Player");
		//GetPositionsFromBilForVertex ();
		//Debug.Log(GetBilPositionFromVertexIndex (1,0));
	}

	IEnumerator WaitBytes(){
		yield return new WaitUntil(() => HeightMapProvider.finished);
		for(int i = 0; i < HeightMapProvider.bytes2.Length; i++){
			//Debug.Log ("--"+i+": "+HeightMapProvider.bytes2[i]);
		}
	}

	void Update () {
		if (Input.GetKeyDown (KeyCode.M)){
			/*foreach (GameObject selection in Selection.gameObjects) {
				StartCoroutine(HeightMapProvider.Upload(selection, selection.GetComponent<Tile>().BBOX));
				//CreateHeight (selection);
			}*/

			callingItself(GameObject.FindGameObjectWithTag("Player"));
			/*foreach (GameObject selection in Selection.gameObjects) {
				if (selection.transform.childCount == 0) {
					StartCoroutine (HeightMapProvider.Upload (selection, selection.GetComponent<Tile> ().BBOX));

					//GeoCord.GetNextVertexLatitude (selection.GetComponent<MeshFilter> ().mesh.vertices [0]);
					//GeoCord.GetNextVertexLatitude (selection.GetComponent<MeshFilter> ().mesh.vertices [1]);
				}
			}*/
			//transform.position += transform.forward * (transform.position - meshObject.transform.position + transform.forward * 5).magnitude * 0.01f;
		}
	}

	void callingItself(GameObject objeto)
	{
		if (objeto.name.Equals("SouthPoleTile") || objeto.name.Equals("NorthPoleTile")) return;
		if (objeto.transform.childCount == 0 && objeto.GetComponent<Tile>() != null) StartCoroutine(HeightMapProvider.Upload(objeto, objeto.GetComponent<Tile>().BBOX));
		else
		{
			for (int objetos = 0; objetos < objeto.transform.childCount; objetos++)
			{
				callingItself(objeto.transform.GetChild(objetos).gameObject);
			}
		}
	}


	private void GetPositionsFromBilForVertex(){
		int columns = UVSphereGenerator.nbLong / 4 + 1; // número de columnas respecto a la consideración de los 4 tiles en que se divide inicialmente y que queda como ancho de cada tile
		int rows = UVSphereGenerator.nbLat / 2 + 1;
		int heightSize = rows + (rows - 1) * HeightMapProvider.kRowsFactorStatic;
		int widthSize = columns + (columns - 1) * HeightMapProvider.kColumnsFactorStatic;
		for(int i = 0; i < rows; i++){
			for(int j = 0; j < columns; j++){
				//Debug.Log ((i * (HeightMapProvider.kRowsFactorStatic + 1) * (widthSize)));
				//Debug.Log ("Valor["+(i*(columns)+j)+"]: "+((HeightMapProvider.kColumnsFactorStatic + 1) * j + i * (HeightMapProvider.kRowsFactorStatic + 1) * (widthSize)));
			}
		}
	}

	public static int GetBilHeightFromVertexIndex(int row, int column){
		int columns = UVSphereGenerator.nbLong / 4 + 1; 
		int widthSize = columns + (columns - 1) * HeightMapProvider.kColumnsFactorStatic;

		/*Debug.Log(HeightMapProvider.kColumnsFactorStatic);
		Debug.Log(column);
		Debug.Log(row);
		Debug.Log(HeightMapProvider.kRowsFactorStatic);*/
		return HeightMapProvider.bytes2[(HeightMapProvider.kColumnsFactorStatic + 1) * column + row * (HeightMapProvider.kRowsFactorStatic + 1) * (widthSize)];
	}

	public static void CreateHeight(GameObject selection){ // emitimos vector desde el medio de la esfera hasta cada vértice y sumamos en dirección al exterior, vector unitario por el valor de altura por variación
		Mesh tileMesh = selection.GetComponent<MeshFilter> ().mesh;
		Vector3[] vertices = tileMesh.vertices;

		Vector3[] normals = tileMesh.normals;

		float factorModified = UVSphereGenerator.radiusStatic / 6371; // Valores en kilómetros

		for (int i = 0; i < UVSphereGenerator.nbLat / 2 + 1; i++){
			for(int j = 0; j < UVSphereGenerator.nbLong / 4 + 1; j++){
				//Debug.Log (i * (UVSphereGenerator.nbLong/4+1) + j);
				if(GetBilHeightFromVertexIndex(i, j) > 0) vertices [i * (UVSphereGenerator.nbLong/4+1) + j] += normals[i * (UVSphereGenerator.nbLong/4+1) + j] * GetBilHeightFromVertexIndex(i, j) * factorModified; 
				//vertices [i] += normals[i] * GetBilPositionFromVertexIndex(i, j); 
			}
		}
		tileMesh.vertices = vertices;
	}

	private void getHeights(){

	}

	private void CreateText(Vector3 vertexPosition){
		GameObject.Instantiate (text, sphere.transform.position + vertexPosition, Quaternion.identity);
	}
}