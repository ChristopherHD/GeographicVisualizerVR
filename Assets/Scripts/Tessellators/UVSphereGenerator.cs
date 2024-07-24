using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class UVSphereGenerator : MonoBehaviour
{

	public float radius = 63f;
	public int numberLong = 24;
	public int numberLat = 16;
	public static int nbLong;
	public static int nbLat;
	public static float radiusStatic;


    void Awake()
	{
		nbLong = numberLong;
		nbLat = numberLat;
		radiusStatic = radius;
        GetComponent<SphereCollider>().radius = radius;
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

        mesh.vertices = vertices;
        mesh.normals = normales;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        CreateNorthPoleTile();
        CreateSouthPoleTile();
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
		
		marker1.JoinMarker(marker2);
		marker2.JoinMarker(marker3);
	}
	IEnumerator CreateMiddleTiles(int latTile, int longTile, bool debug)
	{
		GameObject middleTile = new GameObject("MiddleTile" + latTile + "_" + longTile);
		Mesh mesh = GetComponent<MeshFilter>().mesh;
		mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
		Vector3[] meshVertices = mesh.vertices;

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

		Vector3[] tempVertex;
		int initLat = latTile * nbLat / 2;
		int initLon = longTile * nbLong / 4;

		List<Vector3> tempVertexList = new();

		for (int lat = initLat; lat < initLat + nbLat / 2 + 1; lat++)
			for (int lon = initLon; lon <= initLon + nbLong / 4; lon++)
                tempVertexList.Add(meshVertices[lon + (lat) * (nbLong + 1) + 1]);
            
		tempVertex = tempVertexList.ToArray();

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
        List<Vector2> tempUvsList = new();
		Vector2[] meshUvs = mesh.uv;
		for (int lat = initLat; lat < initLat + nbLat / 2 + 1; lat++)
			for (int lon = initLon; lon <= initLon + nbLong / 4; lon++)
				tempUvsList.Add(meshUvs[lon + lat * (nbLong + 1) + 1]);
        Vector2[] uvs = tempUvsList.ToArray();
        #endregion

        #region Normales		
        Vector3[] normales = new Vector3[tempVertex.Length];
		for (int n = 0; n < tempVertex.Length; n++) normales[n] = tempVertex[n].normalized;
		#endregion

        middleMesh.vertices = tempVertex;
        middleMesh.triangles = triangles;
        middleMesh.uv = uvs;
        middleMesh.normals = normales;
        middleMesh.RecalculateBounds();
        middleTile.AddComponent<MeshCollider>();

		float value00 = Mathf.Min(CoordUtils.GetLatitudeFromPosition(middleMesh.vertices[0]), CoordUtils.GetLatitudeFromPosition(middleMesh.vertices[^1]));
		float value01 = Mathf.Min(CoordUtils.GetLongitudeFromPosition(middleMesh.vertices[0]), CoordUtils.GetLongitudeFromPosition(middleMesh.vertices[^1]));
		float value10 = Mathf.Max(CoordUtils.GetLatitudeFromPosition(middleMesh.vertices[0]), CoordUtils.GetLatitudeFromPosition(middleMesh.vertices[^1]));
		float value11 = Mathf.Max(CoordUtils.GetLongitudeFromPosition(middleMesh.vertices[0]), CoordUtils.GetLongitudeFromPosition(middleMesh.vertices[^1]));
		if (longTile == 1)
		{ // In the limit between longitude 180 and longitude -180, always takes -180, we need a positive value for half cases
			float tempVal = value01;
			value01 = value11;
			value11 = -tempVal;
		}

		CoordUtils.GetPositionFromLatitudeLongitude(value00, value01);
		middleTile.GetComponent<Tile>().BBOX = new double[] { value00, value01, value10, value11 }; // ymin, xmin, ymax, xmax

		yield return 0;
	}
    void CreateNorthPoleTile()
    {
        GameObject northPole = new("NorthPoleTile");
        northPole.transform.parent = transform;
        northPole.transform.position = transform.position;
        northPole.AddComponent<MeshRenderer>();
        northPole.AddComponent<MeshFilter>();
        northPole.GetComponent<MeshRenderer>().material = GetComponent<MeshRenderer>().material;
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Mesh northMesh = northPole.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;

        #region Vertices
        List<Vector3> poleVertices = new();
        for (int lon = 0; lon <= nbLong; lon++) poleVertices.Add(vertices[lon + 1]);
        for (int j = 0; j <= nbLong; j++) poleVertices.Add(vertices[vertices.Length - 2 * nbLong + j]);
        #endregion

        #region UVs
        Vector2[] uvs = mesh.uv;
        List<Vector2> poleUvs = new();
        for (int lon = 0; lon <= nbLong; lon++) poleUvs.Add(uvs[lon + 1] + Vector2.right * 0.5f);
        for (int j = 0; j <= nbLong; j++) poleUvs.Add(uvs[vertices.Length - 2 * nbLong + j]);
        #endregion

        int[] triangles = new int[poleVertices.Count * 6];
        int i = 0;
        for (int lon = 0; lon < nbLong; lon++)
        {
            triangles[i++] = lon + 1;
            triangles[i++] = lon;
            triangles[i++] = lon + poleVertices.Count - nbLong - 1;
        }

        #region Normales		
        Vector3[] normales = new Vector3[poleVertices.Count];
        for (int n = 0; n < poleVertices.Count; n++) normales[n] = poleVertices[n].normalized;
        #endregion

        northMesh.vertices = poleVertices.ToArray();
        northMesh.uv = poleUvs.ToArray();
        northMesh.normals = normales;
        northMesh.triangles = triangles;
        northMesh.RecalculateBounds();
        northPole.AddComponent<MeshCollider>();
    }

    void CreateSouthPoleTile()
    {
        GameObject southPole = new("SouthPoleTile");
        southPole.transform.parent = transform;
        southPole.transform.position = transform.position;
        southPole.AddComponent<MeshRenderer>();
        southPole.AddComponent<MeshFilter>();
        southPole.GetComponent<MeshRenderer>().material = GetComponent<MeshRenderer>().material;
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Mesh southMesh = southPole.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;

        #region Vertices
        List<Vector3> poleVertices = new();
        for (int lon = 0; lon <= nbLong; lon++) poleVertices.Add(vertices[vertices.Length - 3 * nbLong + (lon - 2)]);
        for (int j = 0; j <= nbLong; j++) poleVertices.Add(vertices[vertices.Length - nbLong + j - 1]);
        #endregion

        #region UVs
        Vector2[] uvs = mesh.uv;
        List<Vector2> poleUvs = new();
        for (int lon = 0; lon <= nbLong; lon++) poleUvs.Add(uvs[uvs.Length - 3 * nbLong + (lon - 2)] + Vector2.right * 0.5f);
        for (int j = 0; j <= nbLong; j++) poleUvs.Add(uvs[uvs.Length - nbLong + j - 1]);
        #endregion

        #region Normales		
        Vector3[] normales = new Vector3[poleVertices.Count];
        for (int n = 0; n < poleVertices.Count; n++) normales[n] = poleVertices[n].normalized;
        #endregion

        int[] triangles = new int[poleVertices.Count * 6];
        int i = 0;
        for (int lon = 0; lon < nbLong; lon++)
        {
            triangles[i++] = lon;
            triangles[i++] = lon + 1;
            triangles[i++] = lon + poleVertices.Count - nbLong;
        }

        southMesh.vertices = poleVertices.ToArray();
        southMesh.uv = poleUvs.ToArray();
        southMesh.normals = normales;
        southMesh.triangles = triangles;
        southMesh.RecalculateBounds();
        southPole.AddComponent<MeshCollider>();
    }

    void Start()
	{
		StartCoroutine (CreateSphere());
		//StartCoroutine(TestCreateUVSphereUV());
		CreateData();
	}
}