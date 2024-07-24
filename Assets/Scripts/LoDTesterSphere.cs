using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static Tile;

public class LoDTesterSphere : MonoBehaviour {

	private static Vector3[] currentTileChildsPosition;
    private CameraManager camera;

    void Start(){
		currentTileChildsPosition = new Vector3[4]{Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero};
        camera = Camera.main.GetComponent<CameraManager>();
    }

	IEnumerator WaitingChildTexturized(Tile tile){
		yield return 0;
		if(tile != null && tile.transform.childCount == 4){
			if (tile.transform.GetChild (0).GetComponent<Tile> ().textureLoaded &&
				tile.transform.GetChild (1).GetComponent<Tile> ().textureLoaded &&
				tile.transform.GetChild (2).GetComponent<Tile> ().textureLoaded &&
				tile.transform.GetChild (3).GetComponent<Tile> ().textureLoaded) {

				tile.transform.GetChild (0).GetComponent<MeshRenderer> ().enabled = true;
				tile.transform.GetChild (1).GetComponent<MeshRenderer> ().enabled = true;
				tile.transform.GetChild (2).GetComponent<MeshRenderer> ().enabled = true;
				tile.transform.GetChild (3).GetComponent<MeshRenderer> ().enabled = true;
				tile.GetComponent<MeshRenderer> ().enabled = false;
				tile.GetComponent<MeshCollider>().enabled = false;

			} else StartCoroutine (WaitingChildTexturized (tile));
		}
	}

	public bool ShouldDivide(Tile tile, bool debug)
	{
		if (tile.isDivided) return false;
		if (tile.centerPoint == null) return false;
		if (tile.LoD >= 18) return false;

        double tileLongDif = tile.BBOX[3] - tile.BBOX[1];
		double tileLatDif = tile.BBOX[2] - tile.BBOX[0];
		//double tileLatDif = Vector3d.Distance(tile.GetVertexPositiond(Tile.VertexType.DownLeft), tile.GetVertexPositiond(Tile.VertexType.UpperLeft));
		double longitudeBonus = tileLongDif * 1.5d;
		double latitudeBonus = tileLatDif * 1.5d;

        double cameraLongDistanceToCenter = Mathd.Abs(tile.centerPoint.longitude - camera.cameraHitPosition.longitude);
		double cameraLatDistanceToCenter = Mathd.Abs(tile.centerPoint.latitude - camera.cameraHitPosition.latitude);

		double tileLonDifPosition = Vector3d.Distance(tile.GetVertexPositiond(Tile.VertexType.DownLeft), tile.GetVertexPositiond(Tile.VertexType.DownRight));

        if (debug)
        {
            Debug.Log("_1_ " + camera.cameraHitPosition.longitude + " -- " + camera.cameraHitPosition.latitude);
            Debug.Log("_2_ " + cameraLongDistanceToCenter + " -- " + longitudeBonus);
            Debug.Log("_2_1 " + tile.centerPoint.longitude + " -- " + camera.cameraHitPosition.longitude);
            Debug.Log("_3_ " + cameraLatDistanceToCenter + " -- " + latitudeBonus);
            Debug.Log("_4_ " + tile.centerPoint.longitude + " -- " + tile.centerPoint.latitude);
            Debug.Log("5____ :: " + tile.GetVertexPosition(VertexType.DownLeft));
        }
        if (cameraLongDistanceToCenter <= longitudeBonus && cameraLatDistanceToCenter <= latitudeBonus)
		{;
			if (Vector3d.Distance(new Vector3d(camera.gameObject.transform.position), Vector3d.zero) - (double)UVSphereGenerator.radiusStatic < tileLonDifPosition)
			{
                return true;
			}
		}
		return false;
	}

	public bool ShouldDestroy(Tile tile)
	{
		if (tile.centerPoint == null) return false;
		CameraManager camera = Camera.main.GetComponent<CameraManager>();
		double tileLongDif = tile.BBOX[3] - tile.BBOX[1];
		double tileLatDif = tile.BBOX[2] - tile.BBOX[0];
		
		double longitudeBonus = 2 * tileLongDif;
		double latitudeBonus = 2 * tileLatDif;

		double cameraLongDistanceToCenter = Mathd.Abs(tile.centerPoint.longitude - camera.cameraHitPosition.longitude);
		double cameraLatDistanceToCenter = Mathd.Abs(tile.centerPoint.latitude - camera.cameraHitPosition.latitude);

		double tileLonDifPosition = Vector3d.Distance(tile.GetVertexPositiond(Tile.VertexType.DownLeft), tile.GetVertexPositiond(Tile.VertexType.DownRight));
		if (cameraLongDistanceToCenter > longitudeBonus && cameraLatDistanceToCenter > latitudeBonus) return true;
		if (Vector3d.Distance(new Vector3d(camera.gameObject.transform.position), Vector3d.zero) - (double)UVSphereGenerator.radiusStatic > tileLonDifPosition * 2d) return true;
		
		return false;
	}

		public void CheckLoD(Transform tileObject)
	{
		if (!HasNeededElementsForDivide(tileObject)) return;
		Tile tile = tileObject.GetComponent<Tile>();

        if (ShouldDivide(tile, false) || tile.LoD < 1)
        {
            Debug.Log("Dividing tile ::: " + tile.name);
            //if (ShouldDestroy(tile)) return;
            if (tileObject.childCount == 0)
			{
				//Mesh tempMesh = new Mesh();	
				//tempMesh.vertices = new Vector3[]{Vector3.zero, Vector3.right * vertexDistance}; // Dado que tenemos que esperar a que se forme la malla, pero se hacen los cálculos respecto al doble de la distancia entre 2 de los vértices de dichas mallas, para evitar problemas mientras espera a que se le asigne el valor final, le damos valor nulo
				for (int i = 0; i < 4; ++i)
				{
					GameObject child = GameObject.Instantiate(tileObject.gameObject, tileObject.position + currentTileChildsPosition[i], Quaternion.identity);
					Tile childTile = child.GetComponent<Tile>();

                    child.GetComponent<MeshRenderer>().enabled = false;
                    child.GetComponent<MeshRenderer>().material.mainTexture = null;
                    child.name = "Tile" + tileObject.name.Substring(tileObject.name.Length - 1) + (child.GetComponent<Tile>().LoD + 1) + "_" + i;

					child.GetComponent<MeshFilter>().mesh = DivideTile(child, i % 2, i < 2 ? 0 : 1);

					switch (i)
					{
						case 0:
                            child.GetComponent<Tile>().BBOX[0] += (child.GetComponent<Tile>().BBOX[2] - child.GetComponent<Tile>().BBOX[0]) / 2;
                            child.GetComponent<Tile>().BBOX[3] -= (child.GetComponent<Tile>().BBOX[3] - child.GetComponent<Tile>().BBOX[1]) / 2;
							break;
						case 1:
                            child.GetComponent<Tile>().BBOX[0] += (child.GetComponent<Tile>().BBOX[2] - child.GetComponent<Tile>().BBOX[0]) / 2;
                            child.GetComponent<Tile>().BBOX[1] += (child.GetComponent<Tile>().BBOX[3] - child.GetComponent<Tile>().BBOX[1]) / 2;
							break;
						case 2:
                            child.GetComponent<Tile>().BBOX[2] -= (child.GetComponent<Tile>().BBOX[2] - child.GetComponent<Tile>().BBOX[0]) / 2;
                            child.GetComponent<Tile>().BBOX[3] -= (child.GetComponent<Tile>().BBOX[3] - child.GetComponent<Tile>().BBOX[1]) / 2;
							break;
						case 3:
                            child.GetComponent<Tile>().BBOX[1] += (child.GetComponent<Tile>().BBOX[3] - child.GetComponent<Tile>().BBOX[1]) / 2;
                            child.GetComponent<Tile>().BBOX[2] -= (child.GetComponent<Tile>().BBOX[2] - child.GetComponent<Tile>().BBOX[0]) / 2;
							break;
					}
					GameObject.FindGameObjectWithTag("GameController").GetComponent<MapProvider>().SetTexture(child);

                    //test1.GetComponent<MeshFilter>().mesh = GameObject.FindGameObjectWithTag("GameController").GetComponent<TileTesellator>().Create4VertexMesh (test1.GetComponent<MeshFilter> (), vertexDistance).Result;// Mapper.vertexDistance/2);

                    childTile.LoD++;
					childTile.column = tile.column * 2 + i % 2;
                    childTile.row = tile.row * 2 + (i < 2 ? 0 : 1);
                    tileObject.GetComponent<Tile>().children[i] = childTile;
				}
				for (int i = 0; i < 4; ++i)
				{
					tileObject.GetComponent<Tile>().children[i].transform.parent = tileObject;
                    tile.GetComponent<Tile>().children[i].transform.localRotation = Quaternion.identity;
                }
				StartCoroutine(WaitingChildTexturized(tileObject.GetComponent<Tile>()));
			} else
			{
				Debug.Log("Tile no dividido con hijos: " + tile.name);
			}
		} else if (tile.gameObject.transform.childCount == 0 && tile.gameObject.transform.parent != null)
		{
			if (tile.LoD <= 1) return;
			if (ShouldDestroy(tile) && !ShouldDivide(tile.gameObject.transform.parent.GetComponent<Tile>(), false)) tile.GetComponent<Tile>().mustBeDestroyed = true;	
		}
	}

	private bool HasNeededElementsForDivide(Transform tile)
	{
		if (tile.childCount != 0 || !tile.GetComponent<MeshRenderer>().enabled || !tile.GetComponent<Tile>().meshGenerated) return false;
		return true;
	}

    private Mesh DivideTile(GameObject tile, int xTile, int yTile)
    {
        Mesh resultMesh = new Mesh();
        Mesh tileMesh = tile.GetComponent<MeshFilter>().mesh;
        Vector3[] tileMeshVertices = tileMesh.vertices;
        Vector3[] vertices = new Vector3[tileMeshVertices.Length];
        for (int i = 0; i <= UVSphereGenerator.nbLat / 4; i++)
        {
            int kLong = i * (UVSphereGenerator.nbLat / 2 - 1) + xTile * (UVSphereGenerator.nbLat / 4 - 1) + yTile * (UVSphereGenerator.nbLong / 4 + 1) * (UVSphereGenerator.nbLat / 4);
            int val = i * (UVSphereGenerator.nbLat / 2 - 1);

            for (int j = 0; j <= UVSphereGenerator.nbLong / 8; j++)
            {
                int kLat = j;
                vertices[2 * (val + j)] = tileMeshVertices[kLong + kLat];

                if (i != UVSphereGenerator.nbLat / 4) vertices[2 * (val + j) + (UVSphereGenerator.nbLat / 2 - 1)] = (tileMeshVertices[(UVSphereGenerator.nbLat / 2 - 1) + kLong + kLat] + tileMeshVertices[kLong + kLat]) / 2;
                if (j != UVSphereGenerator.nbLong / 8) vertices[2 * (val + j) + 1] = (tileMeshVertices[kLong + kLat + 1] + tileMeshVertices[kLong + kLat]) / 2;
                if (i != UVSphereGenerator.nbLat / 4 && j != UVSphereGenerator.nbLong / 8) vertices[2 * (val + j) + (UVSphereGenerator.nbLat / 2)] = ((tileMeshVertices[(UVSphereGenerator.nbLat / 2 - 1) + kLong + kLat + 1] + tileMeshVertices[kLong + j]) / 2 + (tileMeshVertices[(UVSphereGenerator.nbLat / 2 - 1) + kLong + kLat] + tileMeshVertices[kLong + kLat + 1]) / 2) / 2;
            }
        }

        // vertices should recalculate its position to adapt to the sphere and really increase level of detail
        /*for (int i = 0; i < vertices.Length; i++)
        {
            float lat = CoordUtils.GetLatitudeFromPosition(vertices[i]);
            float lon = CoordUtils.GetLongitudeFromPosition(vertices[i]);
            Vector3d pos3d= CoordUtils.GetPositionFromLatitudeLongitude(Double.Parse(lat.ToString()), Double.Parse(lon.ToString()));
            vertices[i] = new Vector3((float)pos3d.x, (float)pos3d.y, (float)pos3d.z);
        }*/
        //vertices = verticesTest.ToArray();

        List<Vector2> uvs = new();
        for (int vertical = 0; vertical < UVSphereGenerator.nbLat / 2 + 1; vertical++)
        {
            for (int horizontal = 0; horizontal < UVSphereGenerator.nbLong / 4 + 1; horizontal++)
            {
                uvs.Add(new Vector2((float)horizontal / (UVSphereGenerator.nbLong / 4), 1f - (float)(vertical) / (UVSphereGenerator.nbLat / 2)));
            }
        }

        int[] triangles = new int[tileMesh.triangles.Length];
        int idx = 0;
        for (int lat = 0; lat < UVSphereGenerator.nbLat / 2; lat++)
        {
            for (int lon = 0; lon <= UVSphereGenerator.nbLong / 4 - 1; lon++)
            {
                int current = lon + lat * (UVSphereGenerator.nbLong / 4 + 1);
                int next = current + UVSphereGenerator.nbLat / 2 - 1;
                triangles[idx++] = current;
                triangles[idx++] = current + 1;
                triangles[idx++] = next + 1;

                triangles[idx++] = current;
                triangles[idx++] = next + 1;
                triangles[idx++] = next;
            }
        }

        #region Normales		
        Vector3[] normales = new Vector3[vertices.Length];
        for (int n = 0; n < vertices.Length; n++) normales[n] = vertices[n].normalized;
        #endregion

        resultMesh.vertices = vertices;
        resultMesh.uv = uvs.ToArray();
        resultMesh.triangles = triangles;
        resultMesh.normals = normales;
        return resultMesh;
    }

}