using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeographicGridEllipsoidTessellator : MonoBehaviour {

	void Start () {
		StartCoroutine (Corrutine ());
	}

	/*private Vector3 coords(float u, float v){
		float r = Mathf.Sin (Mathf.PI * v);
		return new Vector3(r * Mathf.Cos(2 * Mathf.PI * u), Mathf.Sin(r * 2 * Mathf.PI * u), Mathf.Cos(Mathf.PI * v));
	}*/
	IEnumerator Corrutine()
	{
		Mesh mesh = GetComponent<MeshFilter>().mesh;
		List<Vector3> vertex = new List<Vector3>();

		double radio = 1.0f;
		vertex.Add(new Vector3(0, 0, Convert.ToInt32(radio)));

		//int stackPartitions = 30;  // 40
		//int slicePartitions =30;  // 10
		int longitudeLines = 32; // es slice
		int latitudeLines = 16; // position north-south es stack
		Vector2[] uv = new Vector2[(latitudeLines - 1) * (longitudeLines) + 2];

		for (int i = 1; i < latitudeLines; i++)
		{

			double phi = Mathf.PI * ((double)(i + 1) / latitudeLines);
			double cosPhi = Mathd.Cos(phi);
			double sinPhi = Mathd.Sin(phi);

			for (int j = 0; j < longitudeLines; j++)
			{
				if (j == 0)
				{
					double theta = (double)(2.0d * Mathd.PI) * ((double)(j + 1) / longitudeLines);
					double cosTheta = Mathd.Cos(theta);
					double sinTheta = Mathd.Sin(theta);
					vertex.Add(new Vector3(Convert.ToInt32(radio * cosTheta * sinPhi), Convert.ToInt32(radio * sinTheta * sinPhi + 0.1d), Convert.ToInt32(radio * cosPhi)));
					uv[i * longitudeLines + j - (longitudeLines - 1)] = new Vector2((float)j / (longitudeLines - 1), (float)i / (latitudeLines - 1));  // Este parece tener uno de los polos mejor, si el último punto s (0,1)
					Debug.Log(uv[i * longitudeLines + j - (longitudeLines - 1)].x + ":::::" + (double)(0.5d + (double)(Mathd.Atan2(sinTheta * sinPhi, cosTheta * sinPhi) / Mathf.PI / 2)) + ":::::" + (double)(0.5d - (double)(Mathd.Asin(cosPhi) / Mathd.PI)));


				}
				else
				{
					double theta = (double)(2.0 * Mathd.PI) * ((double)(j + 1) / longitudeLines);
					double cosTheta = Mathd.Cos(theta);
					double sinTheta = Mathd.Sin(theta);
					vertex.Add(new Vector3(Convert.ToInt32(radio * cosTheta * sinPhi), Convert.ToInt32(radio * sinTheta * sinPhi), Convert.ToInt32(radio * cosPhi)));

					uv[i * longitudeLines + j - (longitudeLines - 1)] = new Vector2((float)j / (longitudeLines - 1), (float)i / (latitudeLines - 1));  // Este parece tener uno de los polos mejor, si el último punto s (0,1)

				}
			}
		}
		vertex.Add(new Vector3(0, 0, -Convert.ToInt32(radio)));
		uv[0] = new Vector2(0f, 0f);//Vector2.zero;	// 0f, -0.1f
		uv[(latitudeLines - 1) * longitudeLines + 1] = new Vector2(0f, 1f);//Vector2.up;

		int triangleIndex = 0;
		int[] trianglesN = new int[latitudeLines * longitudeLines * 6];

		for (int row = 0; row < (int)Mathf.Sqrt(latitudeLines * longitudeLines); row++)
		{
			for (int column = 0; column < (int)Mathf.Sqrt(latitudeLines * longitudeLines); column++)
			{
				if (row == 0)
				{ // Primer polo
					trianglesN[triangleIndex] = row; //2,1,0
					trianglesN[triangleIndex + 1] = column + 1;
					trianglesN[triangleIndex + 2] = (column + 1) % latitudeLines + 1;
				}
				else if (row != Mathf.Sqrt(latitudeLines * longitudeLines) - 1)
				{ // Zonas intermedias
					trianglesN[triangleIndex + 0] = column == 0 ? row * latitudeLines : (row - 1) * latitudeLines + column;
					trianglesN[triangleIndex + 1] = row * latitudeLines + column + 1;
					trianglesN[triangleIndex + 2] = row * latitudeLines + column - (latitudeLines - 1);

					trianglesN[triangleIndex + 3] = column == 0 ? row * latitudeLines : (row - 1) * latitudeLines + column;
					trianglesN[triangleIndex + 4] = column == 0 ? (row + 1) * latitudeLines : row * latitudeLines + column;
					trianglesN[triangleIndex + 5] = row * latitudeLines + column + 1;
					triangleIndex += 3;
				}
				else
				{ // Segundo polo
					trianglesN[triangleIndex] = (latitudeLines - 1) * latitudeLines + 1;
					trianglesN[triangleIndex + 1] = (row - 1) * latitudeLines + column + 1;
					trianglesN[triangleIndex + 2] = column == 0 ? row * latitudeLines + column : (row - 1) * latitudeLines + column;
				}

				triangleIndex += 3;
				//Debug.Log(vertex.Count+"---"+uv.Count+"---"+trianglesN.Length);
				mesh.vertices = vertex.ToArray();
				mesh.triangles = trianglesN;//trianglesN;
				mesh.uv = uv;
				mesh.RecalculateNormals();
				mesh.RecalculateTangents();
				//yield return new WaitForSeconds (0.5f);
			}
		}
		mesh.vertices = vertex.ToArray();

		mesh.triangles = trianglesN;

		mesh.uv = uv;

		mesh.RecalculateNormals();
		mesh.RecalculateTangents();
		yield return 0;
	}

	IEnumerator CorrutineFloat(){
		Mesh mesh = GetComponent<MeshFilter> ().mesh;
		List<Vector3> vertex = new List<Vector3>();
		
		float radio = 1.0f;
		vertex.Add (new Vector3(0, 0, radio));
		
		//int stackPartitions = 30;  // 40
		//int slicePartitions =30;  // 10
		int longitudeLines = 32; // es slice
		int latitudeLines = 16; // position north-south es stack
		Vector2[] uv = new Vector2[(latitudeLines-1) * (longitudeLines) + 2];

		for(int i = 1; i < latitudeLines; i++){

			float phi = Mathf.PI * ((float)(i+1) / latitudeLines);
			float cosPhi = Mathf.Cos(phi);
			float sinPhi = Mathf.Sin(phi);

			for(int j = 0; j < longitudeLines; j++){
				if (j == 0) {
					float theta = (float) (2.0 * Mathf.PI) * ((float)(j+1) / longitudeLines);
					float cosTheta = Mathf.Cos(theta);
					float sinTheta = Mathf.Sin(theta);
					vertex.Add (new Vector3(radio * cosTheta * sinPhi, radio * sinTheta * sinPhi + 0.1f, radio * cosPhi));
					uv[i*longitudeLines + j - (longitudeLines-1)] = new Vector2( (float)j / (longitudeLines-1), (float)i / (latitudeLines-1));  // Este parece tener uno de los polos mejor, si el último punto s (0,1)
					Debug.Log(uv[i*longitudeLines + j - (longitudeLines-1)].x+":::::"+(float)(0.5f + (float)(Mathf.Atan2(sinTheta * sinPhi, cosTheta * sinPhi)/Mathf.PI/2))+":::::"+(float)(0.5f - (float)(Mathf.Asin(cosPhi)/Mathf.PI)));


				} else {
					float theta = (float) (2.0 * Mathf.PI) * ((float)(j+1) / longitudeLines);
					float cosTheta = Mathf.Cos(theta);
					float sinTheta = Mathf.Sin(theta);
					vertex.Add (new Vector3(radio * cosTheta * sinPhi, radio * sinTheta * sinPhi, radio * cosPhi));
					
					uv[i*longitudeLines + j - (longitudeLines-1)] = new Vector2( (float)j / (longitudeLines-1), (float)i / (latitudeLines-1));  // Este parece tener uno de los polos mejor, si el último punto s (0,1)
				
				}
			}
		}
		vertex.Add (new Vector3(0, 0, -radio));
		uv[0] = new Vector2(0f,0f);//Vector2.zero;	// 0f, -0.1f
		uv[(latitudeLines-1) * longitudeLines + 1] = new Vector2(0f,1f);//Vector2.up;

		int triangleIndex = 0;
		int[] trianglesN = new int[latitudeLines*longitudeLines*6];

		for (int row=0; row < (int) Mathf.Sqrt(latitudeLines * longitudeLines); row++) {
			for (int column=0; column < (int) Mathf.Sqrt(latitudeLines * longitudeLines); column++) {
				if (row == 0) { // Primer polo
					trianglesN [triangleIndex] = row; //2,1,0
					trianglesN [triangleIndex + 1] = column + 1; 
					trianglesN [triangleIndex + 2] = (column + 1) % latitudeLines + 1; 
				} else if(row != Mathf.Sqrt(latitudeLines * longitudeLines) - 1) { // Zonas intermedias
					trianglesN [triangleIndex + 0] = column == 0 ? row * latitudeLines : (row - 1) * latitudeLines + column;
					trianglesN [triangleIndex + 1] = row * latitudeLines + column + 1;
					trianglesN [triangleIndex + 2] = row * latitudeLines + column - (latitudeLines - 1);

					trianglesN [triangleIndex + 3] = column == 0 ? row * latitudeLines : (row - 1) * latitudeLines + column;
					trianglesN [triangleIndex + 4] = column == 0 ? (row + 1) * latitudeLines : row * latitudeLines + column;
					trianglesN [triangleIndex + 5] = row * latitudeLines + column + 1;
					triangleIndex += 3;
				} else { // Segundo polo
					trianglesN [triangleIndex] = (latitudeLines - 1) * latitudeLines + 1; 
					trianglesN [triangleIndex + 1] = (row-1) * latitudeLines + column + 1;
					trianglesN [triangleIndex + 2] = column == 0 ? row * latitudeLines + column: (row-1) * latitudeLines + column; 
				}

				triangleIndex += 3;
				//Debug.Log(vertex.Count+"---"+uv.Count+"---"+trianglesN.Length);
				mesh.vertices = vertex.ToArray();
				mesh.triangles = trianglesN;//trianglesN;
				mesh.uv = uv;
				mesh.RecalculateNormals ();
				mesh.RecalculateTangents ();
				//yield return new WaitForSeconds (0.5f);
			}
		}
		mesh.vertices = vertex.ToArray();

		mesh.triangles = trianglesN;

		mesh.uv = uv;

		mesh.RecalculateNormals ();
		mesh.RecalculateTangents ();
		yield return 0;
	}
}