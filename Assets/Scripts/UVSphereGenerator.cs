using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

public class UVSphereGenerator : MonoBehaviour
{

	public float radius = 63f;
	public int numberLong = 24;
	public int numberLat = 16;
	public static int nbLong;
	public static int nbLat;
	public static float radiusStatic;
	public static int polesLatitudeTilesStatic;
    public static int latitudeTileDivisionsStatic;
    public static int longitudeTileDivisionsStatic;


    void Awake()
	{
		nbLong = numberLong;
		nbLat = numberLat;
		radiusStatic = radius;
		polesLatitudeTilesStatic = polesLatitudeTiles;
        latitudeTileDivisionsStatic = latitudeTileDivisions;
        longitudeTileDivisionsStatic = longitudeTileDivisions;

        GetComponent<SphereCollider>().radius = radius;
	}

	public int longitudeSegments = 32;
	public int latitudeSegments = 27;
	public int polesLatitudeTiles = 1;
	public int latitudeTileDivisions = 4;
	public int longitudeTileDivisions = 4;

    IEnumerator TestCreateUVSphereUV()
	{
		MeshFilter meshFilter = GetComponent<MeshFilter>();
		Mesh mesh = new Mesh();
		meshFilter.mesh = mesh;

		List<Vector3> vertices = new List<Vector3>();
		List<Vector2> uvs = new List<Vector2>();
		List<int> triangles = new List<int>();


		GameObject poles = new GameObject("Poles");
        poles.transform.parent = transform;
        poles.transform.position = transform.position;
        poles.AddComponent<MeshRenderer>();
        poles.AddComponent<MeshFilter>();
        poles.AddComponent<MeshCollider>();
        poles.GetComponent<MeshRenderer>().material = GetComponent<MeshRenderer>().material;
		Mesh polesMesh = poles.GetComponent<MeshFilter>().mesh;
		
		List<Vector3> polesVertices = new();
		List<Vector2> polesUvs = new();
		List<int> polesTriangles = new();
		Dictionary<int, List<Vector3>> tilesVertices = new(latitudeTileDivisions * longitudeTileDivisions);
        Dictionary<int, List<Vector2>> tilesUvs = new(latitudeTileDivisions * longitudeTileDivisions);
		for (int i = 0; i < latitudeTileDivisions * longitudeTileDivisions; i++)
		{
			tilesVertices.Add(i, new());
            tilesUvs.Add(i, new());
		}

		//int count = 0;

        for (int lat = 0; lat <= latitudeSegments; lat++)
		{
			float theta = lat * Mathf.PI / latitudeSegments;
			float sinTheta = Mathf.Sin(theta);
			float cosTheta = Mathf.Cos(theta);
			for (int lon = 0; lon <= longitudeSegments; lon++)
			{
				float phi = lon * 2 * Mathf.PI / longitudeSegments;
				float sinPhi = Mathf.Sin(phi);
				float cosPhi = Mathf.Cos(phi);
				Vector3 position = new Vector3(cosPhi * sinTheta, -cosTheta, sinPhi * sinTheta) * radius;
                Vector2 uv = new Vector2((float)lon / longitudeSegments - 0.5f, (float)lat / latitudeSegments);
                //previous -0.5f just used for old code compatibility, all geographical reference was calculated considering that

                if (lat < polesLatitudeTiles || lat > latitudeSegments - polesLatitudeTiles)
				{
                    polesVertices.Add(position);
                    polesUvs.Add(uv);
                } else
                {
                    if (lat == latitudeSegments - polesLatitudeTiles || lat == polesLatitudeTiles)
                    {
                        polesVertices.Add(position);
                        polesUvs.Add(uv);
                    }

                    int latNoPoles = lat - polesLatitudeTiles;
					int latitudeSegmentsNoPoles = latitudeSegments - 2 * (polesLatitudeTiles + 1) + 2; // we want to ignore polar latitudes for tiles, except for polar the 2 latitude polar borders, that's the reason of the +2 at the end, and each polarLatitudeTile has one additional vertices layer than specified (1 tile are 2 vertices of latitude), and we remove that from both poles
					int latitudesPerTile = (latitudeSegmentsNoPoles + (latitudeTileDivisions - 1)) / latitudeTileDivisions;

                    Assert.IsTrue(longitudeSegments % longitudeTileDivisions == 0);
					Assert.IsTrue((latitudeSegmentsNoPoles + (latitudeTileDivisions - 1)) % latitudeTileDivisions == 0);

					if (latNoPoles > latitudeSegmentsNoPoles) continue;
                    int tileRow = latNoPoles / (latitudeSegmentsNoPoles / latitudeTileDivisions);
                    int tileColumn = lon / (longitudeSegments / longitudeTileDivisions);

                    //count++;

					if (tileColumn == longitudeTileDivisions) tileColumn--;
					if (tileRow == latitudeTileDivisions) tileRow--;

					int tileIndex = tileColumn + tileRow * longitudeTileDivisions;
                    tilesVertices[tileIndex].Add(position);
                    tilesUvs[tileIndex].Add(uv);

                    bool isLongitudeBorder = lon % (longitudeSegments / longitudeTileDivisions) == 0 && lon != longitudeSegments;
					bool isLatitudeBorder = latNoPoles % (latitudesPerTile - 1) == 0 && latNoPoles != latitudeSegmentsNoPoles;

                    // add longitude border vertices to previous column tile
                    if (lon > 0 && isLongitudeBorder){
                        tilesVertices[tileIndex - 1].Add(position);
                        tilesUvs[tileIndex - 1].Add(uv);
                    }

                    // add latitude border vertices to previous row tile
                    if (latNoPoles > 0 && isLatitudeBorder)
                    {
						tilesVertices[tileIndex - longitudeTileDivisions].Add(position);
                        tilesUvs[tileIndex - longitudeTileDivisions].Add(uv);
                    }

					// add latitude and longitude border vertices to previous row and column tile
					if(latNoPoles > 0 && lon > 0 && isLongitudeBorder && isLatitudeBorder)
                    {
                        tilesVertices[tileIndex -1 - longitudeTileDivisions].Add(position);
                        tilesUvs[tileIndex -1 - longitudeTileDivisions].Add(uv);
                    }
                }

                vertices.Add(position);
				uvs.Add(uv);
			}
		}

		/*Debug.Log(count);
		for (int i = 0; i < tilesVertices.Count; i++)
		{
			foreach (Vector3 pos in tilesVertices[i])
            {
                GameObject vertex = GameObject.CreatePrimitive(PrimitiveType.Cube);
                //asd.name = ":: " + tileRow + "::" + tileColumn;
                vertex.transform.localScale *= 60;
                vertex.transform.position = pos;
            }
        }*/

		foreach (int key in tilesVertices.Keys)
		{
            Debug.Log(tilesVertices.GetValueOrDefault(key).Count);
        }
        CreateTriangles(polesTriangles, initialLatitude: 0, finalLatitude: polesLatitudeTiles);
        CreateTriangles(polesTriangles, initialLatitude: polesLatitudeTiles + 1, finalLatitude: 2 * polesLatitudeTiles + 1);
        // +1 to initialLatitude to avoid join north and south pole vertices

        CreateTriangles(triangles, initialLatitude: 0, finalLatitude: latitudeSegments);

        for (int i = 0; i < tilesVertices.Keys.Count; i++)
        {
            GameObject tile = new GameObject("Tile" + i);
            tile.transform.parent = transform;
            tile.transform.position = transform.position;
            tile.AddComponent<MeshRenderer>();
            tile.AddComponent<MeshFilter>();
            tile.AddComponent<MeshCollider>();
            tile.GetComponent<MeshRenderer>().material = GetComponent<MeshRenderer>().material;
            tile.AddComponent<Tile>();
            tile.AddComponent<HeightMapData>();
            tile.GetComponent<Tile>().isSphere = true;
            tile.GetComponent<Tile>().textureLoaded = true;
            tile.GetComponent<Tile>().meshGenerated = true;
            //tile.GetComponent<Tile>().column = (longTile + 1) % 2; // column for WMTS, lat 0 long 0 doesn't match, we need to shift it
                                                                         //middleTile.GetComponent<Tile>().row = (longTile + 2) % 4;
            Mesh tileMesh = tile.GetComponent<MeshFilter>().mesh;
            List<int> tilesTriangles = new(latitudeTileDivisions * longitudeTileDivisions);

            int latitudeSegmentsNoPoles = latitudeSegments - 2 * (polesLatitudeTiles + 1) + 2;
            CreateTriangles(tilesTriangles, initialLatitude:0, finalLatitude: (latitudeSegmentsNoPoles + latitudeTileDivisions - 1) / latitudeTileDivisions - 1, finalLongitude:longitudeSegments/longitudeTileDivisions);

            tileMesh.vertices = tilesVertices.GetValueOrDefault(i).ToArray();
            tileMesh.uv = tilesUvs.GetValueOrDefault(i).ToArray();
            tileMesh.triangles = tilesTriangles.ToArray();
            tileMesh.RecalculateNormals();
            tile.GetComponent<MeshCollider>().sharedMesh = tileMesh;

            float value00 = Mathf.Min(CoordUtils.GetLatitudeFromPosition(tileMesh.vertices[0]), CoordUtils.GetLatitudeFromPosition(tileMesh.vertices[^1]));
            float value01 = Mathf.Min(CoordUtils.GetLongitudeFromPosition(tileMesh.vertices[0]), CoordUtils.GetLongitudeFromPosition(tileMesh.vertices[^1]));
            float value10 = Mathf.Max(CoordUtils.GetLatitudeFromPosition(tileMesh.vertices[0]), CoordUtils.GetLatitudeFromPosition(tileMesh.vertices[^1]));
            float value11 = Mathf.Max(CoordUtils.GetLongitudeFromPosition(tileMesh.vertices[0]), CoordUtils.GetLongitudeFromPosition(tileMesh.vertices[^1]));
            if ((i + 1) % longitudeTileDivisions == 0)
            { // In the limit between longitude 180 and longitude -180, always takes -180, we need a positive value for half cases
                float tempVal = value01;
                value01 = value11;
                value11 = -tempVal;
            }

            //CoordUtils.CreateText (tileMesh.vertices [0]);
            CoordUtils.GetPositionFromLatitudeLongitude(value00, value01);
            tile.GetComponent<Tile>().BBOX = new double[] { value00, value01, value10, value11 };
        }

		mesh.vertices = vertices.ToArray();
		mesh.uv = uvs.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.RecalculateNormals();

        polesMesh.vertices = polesVertices.ToArray();
        polesMesh.uv = polesUvs.ToArray();
        polesMesh.triangles = polesTriangles.ToArray();
        polesMesh.RecalculateNormals();

        poles.GetComponent<MeshCollider>().sharedMesh = polesMesh;
		this.gameObject.GetComponent<MeshCollider>().sharedMesh = mesh;
		yield return 0;
	}

	private void CreateTriangles(List<int> triangles, int initialLatitude, int finalLatitude)
	{
		CreateTriangles(triangles, initialLatitude, finalLatitude, longitudeSegments);
	}
	private void CreateTriangles(List<int> triangles, int initialLatitude, int finalLatitude, int finalLongitude)
	{
        for (int lat = initialLatitude; lat < finalLatitude; lat++)
        {
            for (int lon = 0; lon < finalLongitude; lon++)
            {
                int first = (lat * (finalLongitude + 1)) + lon;
                int second = first + finalLongitude + 1;
                triangles.Add(first);
                triangles.Add(second);
                triangles.Add(first + 1);

                triangles.Add(second);
                triangles.Add(second + 1);
                triangles.Add(first + 1);
            }
        }
    }

	IEnumerator CreateSphere()
	{
		Mesh mesh = gameObject.GetComponent<MeshFilter>().mesh;
		mesh.Clear();

		#region Vertices
		Vector3[] vertices = new Vector3[(nbLong + 1) * (nbLat) + 2 + 2 * nbLong]; //  (+ 2 + 2 * nbLong) para los polos, (nbLong+1) para la franja
		float _pi = Mathf.PI;
		float _2pi = _pi * 2f;

		vertices[0] = Vector3.up * radius;
		for (int lat = 0; lat < nbLat; lat++)
		{
			float a1 = _pi * (float)(lat + 1) / (nbLat + 1);
			float sin1 = Mathf.Sin(a1);
			float cos1 = Mathf.Cos(a1);

			for (int lon = 0; lon <= nbLong; lon++)
			{
				float a2 = _2pi * (float)(lon == nbLong ? 0 : lon) / nbLong;
				float sin2 = Mathf.Sin(a2);
				float cos2 = Mathf.Cos(a2);

				vertices[lon + (lat) * (nbLong + 1) + 1] = new Vector3(sin1 * cos2, cos1, sin1 * sin2) * radius;
			}
		}
		vertices[vertices.Length - 1 - nbLong] = Vector3.up * -radius;

		for (int j = vertices.Length - 2 * nbLong; j < vertices.Length - nbLong; j++) vertices[j] = Vector3.up * radius; // Add north pole vertex
		for (int j = vertices.Length - nbLong; j < vertices.Length; j++) vertices[j] = Vector3.down * radius;  // Add south pole vertex
		#endregion

		#region Normales		
		Vector3[] normales = new Vector3[vertices.Length];
		for (int n = 0; n < vertices.Length; n++) normales[n] = vertices[n].normalized;
		#endregion

		#region UVs
		Vector2[] uvs = new Vector2[vertices.Length];
		uvs[0] = Vector2.up;
		uvs[uvs.Length - 1] = Vector2.zero;
		for (int lat = 0; lat < nbLat; lat++)
			for (int lon = 0; lon <= nbLong; lon++)
				uvs[lon + lat * (nbLong + 1) + 1] = new Vector2((float)lon / nbLong - 0.5f, 1f - (float)(lat + 1) / (nbLat + 1));

		for (int j = vertices.Length - 2 * nbLong; j < vertices.Length - nbLong; j++)
		{
			uvs[j] = Vector2.up + Vector2.right * (j - (vertices.Length - 2 * nbLong)) / nbLong;
			//Debug.Log(":::"+j+":::"+uvs[j]);
		}
		for (int j = vertices.Length - nbLong; j < vertices.Length; j++)
		{
			uvs[j] = Vector2.right * (j - (vertices.Length - nbLong)) / nbLong;
		}
		#endregion

		#region Triangles
		int nbFaces = vertices.Length;
		int nbTriangles = nbFaces * 2;
		int nbIndexes = nbTriangles * 3;
		int[] triangles = new int[nbIndexes];

		//Top Cap
		int i = 0;
		for (int lon = 0; lon < nbLong; lon++)
		{
			triangles[i++] = lon + 2;
			triangles[i++] = lon + 1;
			triangles[i++] = lon + vertices.Length - 2 * nbLong;
		}

		//Middle
		for (int lat = 0; lat < nbLat - 1; lat++)
		{
			for (int lon = 0; lon < nbLong; lon++)
			{
				int current = lon + lat * (nbLong + 1) + 1;
				int next = current + nbLong + 1;
				triangles[i++] = current;
				triangles[i++] = current + 1;
				triangles[i++] = next + 1;

				triangles[i++] = current;
				triangles[i++] = next + 1;
				triangles[i++] = next;
			}
		}

		//Bottom Cap
		for (int lon = 0; lon < nbLong; lon++)
		{
			triangles[i++] = vertices.Length - (lon + 1);
			triangles[i++] = vertices.Length - (lon + 2) - 1 - 2 * nbLong;
			triangles[i++] = vertices.Length - (lon + 1) - 1 - 2 * nbLong;
		}
		#endregion

		Vector3[] tempVertices = new Vector3[vertices.Length];
		for (int n = 0; n < vertices.Length; n++)
		{
			tempVertices[n] = vertices[n];
			mesh.vertices = tempVertices;
			mesh.normals = normales;
			mesh.uv = uvs;
			mesh.triangles = triangles;
			mesh.RecalculateBounds();
			//yield return new WaitForSeconds(0.005f);
		}
		StartCoroutine(CreateMiddleTiles(0, 0, false));
		StartCoroutine(CreateMiddleTiles(0, 1, false));
		StartCoroutine(CreateMiddleTiles(0, 2, false));
		StartCoroutine(CreateMiddleTiles(0, 3, false));
		StartCoroutine(CreateMiddleTiles(1, 0, false));
		StartCoroutine(CreateMiddleTiles(1, 1, false));
		StartCoroutine(CreateMiddleTiles(1, 2, false));
		StartCoroutine(CreateMiddleTiles(1, 3, false));
		GetComponent<MeshRenderer>().enabled = false;
		yield return 0;
	}

	void CreateData()
	{
		Marker marker1 = new(40.416775f, -3.703790f); //posición de ese par lat/Lon correponde en posición de mundo a lat/lon: 40,41894 :: -3,703855
		Debug.Log("CheckCoord: " + CoordUtils.GetPositionFromLatitudeLongitude(40.416775f, -3.703790f)); // 40.41119956237144, -3.7043071211153804
		Debug.Log("CheckCoord: " + CoordUtils.GetPositionFromLatitudeLongitude(40.41119956237144d, -3.7043071211153804d));
		Marker marker2 = new(39.8581f, -4.02263f);
		Marker marker3 = new(40.0201959f, -3.6590717f);
		Vector3 pos = new Vector3(4826.98f, 4152.78f, -281.68f);

		Marker marker4 = new(CoordUtils.GetLatitudeFromPosition(pos), CoordUtils.GetLongitudeFromPosition(pos));
		Debug.Log("CheckCoord: " + CoordUtils.GetPositionFromLatitudeLongitude(CoordUtils.GetLatitudeFromPosition(pos), CoordUtils.GetLongitudeFromPosition(pos)));
		Debug.Log(marker4.GetLat() + " :: " + marker4.GetLon());
		//Marker marker4 = new(CoordUtils.GetLatitudeFromPosition(new Vector3(6041.34131f, 1010.0802f, -2353.47412f)), CoordUtils.GetLongitudeFromPosition(new Vector3(6041.34131f, 1010.0802f, -2353.47412f)));
		//Marker marker5 = new(CoordUtils.GetLatitudeFromPosition(new Vector3(2338.03271f, 986.919739f, 6051.27783f)), CoordUtils.GetLongitudeFromPosition(new Vector3(2338.03271f, 986.919739f, 6051.27783f)));

		marker1.JoinMarker(marker2);
		marker2.JoinMarker(marker3);
		//marker4.JoinMarker(marker5);
	}

	IEnumerator CreateMiddleTiles(int latTile, int longTile, bool debug)
	{
		GameObject middleTile = new GameObject("MiddleTile" + latTile + "_" + longTile);
		Mesh mesh = GetComponent<MeshFilter>().mesh;

		//float latValue = 90 * (numberLat - 1) / (numberLat + 2);
		middleTile.AddComponent<Tile>();
		middleTile.AddComponent<HeightMapData>();
		middleTile.GetComponent<Tile>().isSphere = true;
		middleTile.GetComponent<Tile>().textureLoaded = true;
		middleTile.GetComponent<Tile>().meshGenerated = true;
		middleTile.GetComponent<Tile>().column = (longTile + 1) % 2; // column for WMTS, lat 0 long 0 doesn't match, we need to shift it
																	 //middleTile.GetComponent<Tile>().row = (longTile + 2) % 4;
		middleTile.transform.parent = transform;
		middleTile.transform.position = transform.position;
		middleTile.AddComponent<MeshRenderer>();
		middleTile.AddComponent<MeshFilter>();
		middleTile.GetComponent<MeshRenderer>().material = GetComponent<MeshRenderer>().material;
		Mesh middleMesh = middleTile.GetComponent<MeshFilter>().mesh;

		Vector3[] tempVertex = new Vector3[(nbLong / 4 + 1) * (nbLat / 2 + 1)];
		int initLat = latTile * nbLat / 2;
		int initLon = longTile * nbLong / 4;

		for (int lat = initLat; lat < initLat + nbLat / 2 + 1; lat++)
			for (int lon = initLon; lon <= initLon + nbLong / 4; lon++)
			{
				//if (debug) Debug.Log ((lon-initLon) + (lat-initLat) * (nbLong + 1));
				//tempVertex [(lon-initLon) + (lat-initLat) * (nbLong + 1)] = mesh.vertices [lon + (lat - (latTile == 1 ? 1 : 0)) * (nbLong + 1) + 1];
				if (debug) Debug.Log((lat - initLat) * (nbLat / 2 - 1) + (lon - initLon));
				tempVertex[(lat - initLat) * (nbLat / 2 - 1) + (lon - initLon)] = mesh.vertices[lon + (lat) * (nbLong + 1) + 1];
			}

		int nbFaces = nbLat / 2 * nbLong / 4;
		int nbTriangles = nbFaces * 2;
		int nbIndexes = nbTriangles * 3;
		int[] triangles = new int[nbIndexes];
		int i = 0;

		for (int lat = 0; lat < nbLat / 2 - (latTile == 1 ? 0 : 0); lat++)
		{
			for (int lon = 0; lon < nbLong / 4; lon++)
			{
				int current = lon + lat * (nbLong / 4 + 1);
				int next = current + nbLong / 4 + 1;
				triangles[i++] = current;
				triangles[i++] = current + 1;
				triangles[i++] = next + 1;

				triangles[i++] = current;
				triangles[i++] = next + 1;
				triangles[i++] = next;
			}
		}

		#region UVs
		Vector2[] uvs = new Vector2[tempVertex.Length];
		for (int lat = initLat; lat < initLat + nbLat / 2 + 1; lat++)
			for (int lon = initLon; lon <= initLon + nbLong / 4; lon++)
				//uvs[(lon-initLon) + (lat-initLat) * (nbLong + 1)] = mesh.uv[lon + (lat - (latTile == 1 ? 1 : 0)) * (nbLong + 1) + 1];
				uvs[(lat - initLat) * (nbLat / 2 - 1) + (lon - initLon)] = mesh.uv[lon + lat * (nbLong + 1) + 1];
		#endregion

		#region Normales		
		Vector3[] normales = new Vector3[tempVertex.Length];
		for (int n = 0; n < tempVertex.Length; n++) normales[n] = tempVertex[n].normalized;
		#endregion

		Vector3[] tempVertex2 = new Vector3[tempVertex.Length];
		for (int k = 0; k < tempVertex.Length; k++)
		{
			tempVertex2[k] = tempVertex[k];
			middleMesh.vertices = tempVertex2;
			middleMesh.triangles = triangles;
			middleMesh.uv = uvs;
			middleMesh.normals = normales;
			middleMesh.RecalculateBounds();
			//yield return new WaitForSeconds (0.05f);
		}
		middleTile.AddComponent<MeshCollider>();

		/*Vector3 vertex00 = middleMesh.vertices[Mathf.Min(0, (UVSphereGenerator.nbLong/4 + 1) * (UVSphereGenerator.nbLat/2))];
		Vector3 vertex01 = middleMesh.vertices[Mathf.Min((UVSphereGenerator.nbLong/4), middleMesh.vertices.Length - 1)];
		Vector3 vertex10 = middleMesh.vertices[Mathf.Max(0, (UVSphereGenerator.nbLong/4 + 1) * (UVSphereGenerator.nbLat/2))];
		Vector3 vertex11 = middleMesh.vertices[Mathf.Max((UVSphereGenerator.nbLong/4), middleMesh.vertices.Length - 1)];*/

		float value00 = Mathf.Min(CoordUtils.GetLatitudeFromPosition(middleMesh.vertices[0]), CoordUtils.GetLatitudeFromPosition(middleMesh.vertices[middleMesh.vertices.Length - 1]));
		float value01 = Mathf.Min(CoordUtils.GetLongitudeFromPosition(middleMesh.vertices[0]), CoordUtils.GetLongitudeFromPosition(middleMesh.vertices[middleMesh.vertices.Length - 1]));
		float value10 = Mathf.Max(CoordUtils.GetLatitudeFromPosition(middleMesh.vertices[0]), CoordUtils.GetLatitudeFromPosition(middleMesh.vertices[middleMesh.vertices.Length - 1]));
		float value11 = Mathf.Max(CoordUtils.GetLongitudeFromPosition(middleMesh.vertices[0]), CoordUtils.GetLongitudeFromPosition(middleMesh.vertices[middleMesh.vertices.Length - 1]));
		if (longTile == 1)
		{ // In the limit between longitude 180 and longitude -180, always takes -180, we need a positive value for half cases
			float tempVal = value01;
			value01 = value11;
			value11 = -tempVal;
		}

		//GeoCord.CreateText (middleMesh.vertices [0]);
		CoordUtils.GetPositionFromLatitudeLongitude(value00, value01);
		middleTile.GetComponent<Tile>().BBOX = new double[] { value00, value01, value10, value11 }; // ymin, xmin, ymax, xmax

		yield return 0;
	}

	void Start()
	{
		//StartCoroutine (CreateSphere());
		StartCoroutine(TestCreateUVSphereUV());
		CreateData();
	}
}