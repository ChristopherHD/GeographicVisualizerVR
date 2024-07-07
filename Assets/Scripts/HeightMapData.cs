using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeightMapData : MonoBehaviour {

	public int[] heightMapData;

	public void CreateHeight(){ // emitimos vector desde el medio de la esfera hasta cada vértice y sumamos en dirección al exterior, vector unitario por el valor de altura por variación
		StartCoroutine(UpdateHeightSmooth());
	}

	private int GetBilHeightFromVertexIndex(int row, int column){
		int columns = UVSphereGenerator.nbLong / 4 + 1; 
		int widthSize = columns + (columns - 1) * HeightMapProvider.kColumnsFactorStatic;

		return heightMapData[(HeightMapProvider.kColumnsFactorStatic + 1) * column + row * (HeightMapProvider.kRowsFactorStatic + 1) * (widthSize)];
	}

	IEnumerator UpdateHeightSmooth(){
		Mesh tileMesh = GetComponent<MeshFilter> ().mesh;
		Vector3[] vertices = tileMesh.vertices;
		Vector3[] vertices2 = tileMesh.vertices;
		Vector2[] uv = tileMesh.uv;
		Vector3[] normals = tileMesh.normals;

		float factorModified = UVSphereGenerator.radiusStatic / (6371 * 5); // Valores en kilómetros
		Debug.Log("empieza");
		Vector3[] skirtVertices = new Vector3[4 * (UVSphereGenerator.nbLat / 2 + 1 + UVSphereGenerator.nbLong / 4)];
		int[] skirtTriangles = new int[skirtVertices.Length * 6];
		Vector2[] skirtUV = new Vector2[skirtVertices.Length];

		for(int k = 0; k < 15; k++){
			for (int i = 0; i < UVSphereGenerator.nbLat / 2 + 1; i++){
				for(int j = 0; j < UVSphereGenerator.nbLong / 4 + 1; j++){
					if(GetBilHeightFromVertexIndex(i, j) > 0) vertices [i * (UVSphereGenerator.nbLong/4+1) + j] += normals[i * (UVSphereGenerator.nbLong/4+1) + j] * GetBilHeightFromVertexIndex(i, j) * factorModified / 15; 				
				}
			}
			
			yield return new WaitForSeconds(0.01f);
			tileMesh.vertices = vertices;
		}

		int count = 0;
		for (int j = 0; j < UVSphereGenerator.nbLong / 4 + 1; j++) {
			skirtUV [count] = uv [j];
			skirtUV [count+1] = uv [j];
			skirtVertices[count] = vertices [j];
			skirtVertices[count+1] = vertices2 [j];
			count += 2;
		}
		for (int i = 0; i < UVSphereGenerator.nbLat / 2 + 1; i++) {
			skirtUV [count] = uv [i * (UVSphereGenerator.nbLong/4+1) + UVSphereGenerator.nbLong / 4];
			skirtVertices[count] = vertices [i * (UVSphereGenerator.nbLong/4+1) + UVSphereGenerator.nbLong / 4];
			count++;
			if (i != UVSphereGenerator.nbLat / 2 && i != 0) {
				skirtUV [count] = uv [i * (UVSphereGenerator.nbLong/4+1) + UVSphereGenerator.nbLong / 4];
				skirtVertices[count] = vertices2 [i * (UVSphereGenerator.nbLong/4+1) + UVSphereGenerator.nbLong / 4];
				count++;
			}
		}
		for (int j = UVSphereGenerator.nbLong / 4; j >= 0; j--) {
			skirtUV [count] = uv [j + UVSphereGenerator.nbLat / 2 * (UVSphereGenerator.nbLong/4+1)];
			skirtVertices[count] = vertices2 [j + UVSphereGenerator.nbLat / 2 * (UVSphereGenerator.nbLong/4+1)];
			skirtUV [count+1] = uv [j + UVSphereGenerator.nbLat / 2 * (UVSphereGenerator.nbLong/4+1)];
			skirtVertices[count+1] = vertices [j + UVSphereGenerator.nbLat / 2 * (UVSphereGenerator.nbLong/4+1)];
			count += 2;
		}
		for (int i = UVSphereGenerator.nbLat / 2; i >= 0 ; i--) {
			skirtUV [count] = uv [i * (UVSphereGenerator.nbLong / 4 + 1)];
			skirtVertices [count] = vertices2 [i * (UVSphereGenerator.nbLong / 4 + 1)];
			count++;
			if (i != UVSphereGenerator.nbLat / 2 && i != 0) {
				skirtUV [count] = uv [i * (UVSphereGenerator.nbLong / 4 + 1)];
				skirtVertices [count] = vertices [i * (UVSphereGenerator.nbLong / 4 + 1)];
				count++;
			}
		}

		int sum = 0;
		for(int i = 0; i < skirtVertices.Length / 2 ; i++){
			bool invert = true;//(2 * i / 4) < skirtVertices.Length / 4;
			bool firstLayer = 2 * i + 1 < skirtVertices.Length / 4;
			bool thirdLayer = 2 * i + 1 < 4*skirtVertices.Length / 8;
			bool fourthLayer = 2 * i + 1 < 6*skirtVertices.Length / 8;
			if (!thirdLayer) firstLayer = thirdLayer;
			if (!fourthLayer) firstLayer = !fourthLayer;
			bool end = (i == skirtVertices.Length / 2 - 1);
			skirtTriangles  [sum+0] = 2*i +  (firstLayer ? 0:1);
			skirtTriangles[sum+1] = 2*i+  (firstLayer ? 1:0);
			skirtTriangles[sum+2] = end ? 0 : 2*i+ 2;

			skirtTriangles[sum+3] = end ? 0 : 2*i+ (firstLayer ? 2:3);
			skirtTriangles[sum+4] = 2*i+ 1;
			skirtTriangles[sum+5] = end ? 1 : 2*i+(firstLayer ? 3:2);
			sum += 6;
		}

		// Set corner between first and second layer
		skirtTriangles[sum++] = 2 * (UVSphereGenerator.nbLong / 4 + 1)-2;
		skirtTriangles[sum++] = 2 * (UVSphereGenerator.nbLong / 4 + 1) - 1;
		skirtTriangles[sum++] = 2 * (UVSphereGenerator.nbLong / 4 + 1) + 2;

		// Set corner between third and fourth layer
		skirtTriangles[sum++] = 4 * (UVSphereGenerator.nbLong / 4 + 1)  + 2 * (UVSphereGenerator.nbLat / 2)-2;
		skirtTriangles[sum++] = 4 * (UVSphereGenerator.nbLong / 4 + 1)  + 2 * (UVSphereGenerator.nbLat / 2)+2;
		skirtTriangles [sum++] = 4 * (UVSphereGenerator.nbLong / 4 + 1)  + 2 * (UVSphereGenerator.nbLat / 2)-1;
		                        
		// Set corner between fourth and first layer
		skirtTriangles[sum++] = 0;
		skirtTriangles [sum++] = skirtVertices.Length - 2;
		skirtTriangles[sum++] = 1;

		CreateSkirt (skirtVertices, skirtTriangles, skirtUV);
		Debug.Log("aplicado malla");
	}

	void CreateSkirt(Vector3[] skirtVertices, int[] skirtTriangles, Vector2[] skirtUV){
		GameObject skirt = new GameObject (gameObject.name + "_skirt");
		skirt.transform.parent = gameObject.transform;
		skirt.transform.localRotation = Quaternion.Euler(0,0,0);
		skirt.transform.localPosition = Vector3.zero;
		skirt.AddComponent <MeshRenderer>();
		skirt.AddComponent <MeshFilter>();
		Mesh mesh = skirt.GetComponent<MeshFilter> ().mesh;
		mesh.vertices = skirtVertices;
		mesh.triangles = skirtTriangles;
		mesh.uv = skirtUV;
		skirt.GetComponent<MeshRenderer> ().material.mainTexture = GetComponent<MeshRenderer> ().material.mainTexture;
		skirt.GetComponent<MeshRenderer> ().material.shader = GetComponent<MeshRenderer> ().material.shader;
		skirt.GetComponent<MeshRenderer> ().material.mainTexture.wrapMode = TextureWrapMode.Clamp;
	}
}