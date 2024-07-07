using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UVSphereGenerator : MonoBehaviour {

	public float radius = 63f;
	public int numberLong = 24;
	public int numberLat = 16;
	public static int nbLong;
	public static int nbLat;
	public static float radiusStatic;

	void Awake(){
		nbLong = numberLong;
		nbLat = numberLat;
		radiusStatic = radius;
        GetComponent<SphereCollider>().radius = radius;
	}

    public int longitudeSegments = 80;
    public int latitudeSegments = 80;

    IEnumerator TestCreateUVSphereUV()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        meshFilter.mesh = mesh;

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();

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

                vertices.Add(new Vector3(cosPhi * sinTheta, -cosTheta, sinPhi * sinTheta) * radius);
                uvs.Add(new Vector2((float)lon / longitudeSegments -0.5f, (float)lat / latitudeSegments));
				//previous -0.5f just used for old code compatibility, all geographical reference was calculated considering that
            }
        }

        for (int lat = 0; lat < latitudeSegments; lat++)
        {
            for (int lon = 0; lon < longitudeSegments; lon++)
            {
                int first = (lat * (longitudeSegments + 1)) + lon;
                int second = first + longitudeSegments + 1;

                triangles.Add(first);
                triangles.Add(second);
                triangles.Add(first + 1);

                triangles.Add(second);
                triangles.Add(second + 1);
                triangles.Add(first + 1);
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
		yield return 0;
    }

    IEnumerator CreateSphere(){
		Mesh mesh = gameObject.GetComponent< MeshFilter >().mesh;
		mesh.Clear();

		#region Vertices
		Vector3[] vertices = new Vector3[(nbLong+1) * (nbLat) + 2 + 2 * nbLong]; //  (+ 2 + 2 * nbLong) para los polos, (nbLong+1) para la franja
		float _pi = Mathf.PI;
		float _2pi = _pi * 2f;

		vertices[0] = Vector3.up * radius;
		for( int lat = 0; lat < nbLat; lat++ )
		{
			float a1 = _pi * (float)(lat+1) / (nbLat+1);
			float sin1 = Mathf.Sin(a1);
			float cos1 = Mathf.Cos(a1);

			for( int lon = 0; lon <= nbLong; lon++ )
			{
				float a2 = _2pi * (float)(lon == nbLong ? 0 : lon) / nbLong;
				float sin2 = Mathf.Sin(a2);
				float cos2 = Mathf.Cos(a2);

				vertices[ lon + (lat) * (nbLong + 1) + 1] = new Vector3( sin1 * cos2, cos1, sin1 * sin2 ) * radius;
			}
		}
		vertices[vertices.Length-1-nbLong] = Vector3.up * -radius;

		for(int j = vertices.Length-2*nbLong; j < vertices.Length-nbLong; j++) vertices[j] = Vector3.up * radius; // Add north pole vertex
		for(int j = vertices.Length-nbLong; j < vertices.Length; j++) vertices[j] = Vector3.down * radius;  // Add south pole vertex
		#endregion

		#region Normales		
		Vector3[] normales = new Vector3[vertices.Length];
		for( int n = 0; n < vertices.Length; n++ ) normales[n] = vertices[n].normalized;
		#endregion

		#region UVs
		Vector2[] uvs = new Vector2[vertices.Length];
		uvs[0] = Vector2.up;
		uvs[uvs.Length-1] = Vector2.zero;
		for( int lat = 0; lat < nbLat; lat++ )
			for( int lon = 0; lon <= nbLong; lon++ )
					uvs[lon + lat * (nbLong + 1) + 1] = new Vector2((float)lon / nbLong -0.5f, 1f - (float)(lat+1) / (nbLat+1) );
		
		for(int j = vertices.Length-2*nbLong; j < vertices.Length-nbLong; j++){
			uvs[j] = Vector2.up + Vector2.right * (j-( vertices.Length-2*nbLong)) / nbLong;
			//Debug.Log(":::"+j+":::"+uvs[j]);
		}
		for(int j = vertices.Length-nbLong; j < vertices.Length; j++){
			uvs[j] = Vector2.right * (j-( vertices.Length-nbLong)) / nbLong;
		}
		#endregion

		#region Triangles
		int nbFaces = vertices.Length;
		int nbTriangles = nbFaces * 2;
		int nbIndexes = nbTriangles * 3;
		int[] triangles = new int[ nbIndexes ];

		//Top Cap
		int i = 0;
		for( int lon = 0; lon < nbLong; lon++ )
		{
			triangles[i++] = lon+2;
			triangles[i++] = lon+1;
			triangles[i++] = lon+vertices.Length-2*nbLong;
		}

		//Middle
		for( int lat = 0; lat < nbLat - 1; lat++ )
		{
			for( int lon = 0; lon < nbLong; lon++ )
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
		for( int lon = 0; lon < nbLong; lon++ )
		{
			triangles[i++] = vertices.Length - (lon+1);
			triangles[i++] = vertices.Length - (lon+2) - 1 -2*nbLong;
			triangles[i++] = vertices.Length - (lon+1) - 1 -2*nbLong;
		}
		#endregion

		Vector3[] tempVertices = new Vector3[vertices.Length];
		for(int n = 0; n < vertices.Length; n++){
			tempVertices [n] = vertices [n];
			mesh.vertices = tempVertices;
			mesh.normals = normales;
			mesh.uv = uvs;
			mesh.triangles = triangles;
			mesh.RecalculateBounds();
			//yield return new WaitForSeconds(0.005f);
		}
		CreateNorthPoleTile ();
		StartCoroutine(CreateSouthPoleTile ());
		StartCoroutine(CreateMiddleTiles(0,0,false));
		StartCoroutine(CreateMiddleTiles(0,1,false));
		StartCoroutine(CreateMiddleTiles(0,2,false));
		StartCoroutine(CreateMiddleTiles(0,3,false));
		StartCoroutine(CreateMiddleTiles(1,0,false));
		StartCoroutine(CreateMiddleTiles(1,1,false));
		StartCoroutine(CreateMiddleTiles(1,2,false));
		StartCoroutine(CreateMiddleTiles(1,3,false));
		GetComponent<MeshRenderer>().enabled = false;
		yield return 0;
	}

	void CreateData()
	{
		Marker marker1 = new(40.416775f, -3.703790f);
        Marker marker2 = new(39.8581f, -4.02263f);
		Marker marker3 = new(40.0201959f, -3.6590717f);
        marker1.JoinMarker(marker2);
		marker2.JoinMarker(marker3);
    }

	void CreateNorthPoleTile(){
		GameObject northPole = new GameObject ("NorthPoleTile");
		northPole.transform.parent = transform;
		northPole.transform.position = transform.position;
		northPole.AddComponent<MeshRenderer> ();
		northPole.AddComponent<MeshFilter> ();
		northPole.GetComponent<MeshRenderer> ().material = GetComponent<MeshRenderer>().material;
		Mesh mesh = GetComponent<MeshFilter> ().mesh;
		Mesh northMesh = northPole.GetComponent<MeshFilter> ().mesh;

		Vector3[] vertices = mesh.vertices;
		Vector3[] tempVertex = new Vector3[2*(nbLong+1)];
		Vector2[] tempUvs = new Vector2[2*(nbLong+1)];

		#region Vertices
		for(int lon = 0; lon <= nbLong; lon++ ) tempVertex[lon] = vertices[ lon + 1];
		for(int j = 0; j < nbLong; j++)	tempVertex[j+nbLong+1] = vertices[vertices.Length-2*nbLong + j ];
		#endregion

		#region UVs
		Vector2[] uvs = mesh.uv;
		for( int lon = 0; lon <= nbLong; lon++ ) {
			tempUvs[lon] = uvs[lon + 1] + Vector2.right * 0.5f;
			//Debug.Log(tempUvs[lon]);
		}
		for(int j = 0; j <= nbLong; j++) tempUvs[j+nbLong+1] = uvs[vertices.Length-2*nbLong+j];
		#endregion

		int[] triangles = new int[tempVertex.Length * 6];
		int i = 0;
		for(int lon = 0; lon < nbLong; lon++ ) {
			triangles[i++] = lon+1;
			triangles[i++] = lon;
			triangles[i++] = lon+tempVertex.Length-nbLong-1;
		}
			
		#region Normales		
		Vector3[] normales = new Vector3[tempVertex.Length];
		for( int n = 0; n < tempVertex.Length; n++ ) normales[n] = tempVertex[n].normalized;
		#endregion

		northMesh.vertices = tempVertex;
		northMesh.uv = tempUvs;
		northMesh.normals = normales;
		northMesh.triangles = triangles;
		northMesh.RecalculateBounds();
		northPole.AddComponent<MeshCollider>();
	}

	IEnumerator CreateSouthPoleTile(){
		GameObject southPole = new GameObject ("SouthPoleTile");
		southPole.transform.parent = transform;
		southPole.transform.position = transform.position;
		southPole.AddComponent<MeshRenderer> ();
		southPole.AddComponent<MeshFilter> ();
		southPole.GetComponent<MeshRenderer> ().material = GetComponent<MeshRenderer>().material;
		Mesh mesh = GetComponent<MeshFilter> ().mesh;
		Mesh southMesh = southPole.GetComponent<MeshFilter> ().mesh;

		Vector3[] tempVertex = new Vector3[2 * (nbLong + 1)];
		Vector2[] tempUvs = new Vector2[2 * (nbLong + 1)];

		#region Vertices
		for(int lon = 0; lon <= nbLong; lon++ ) tempVertex[lon] = mesh.vertices[mesh.vertices.Length - 3 * nbLong + (lon - 2)];
		for(int j = 0; j <= nbLong; j++) tempVertex[j+nbLong+1] = mesh.vertices[mesh.vertices.Length - nbLong + j - 1];
		#endregion

		#region UVs
		for( int lon = 0; lon <= nbLong; lon++ ) {
			tempUvs[lon] = mesh.uv[mesh.uv.Length - 3 * nbLong + (lon-2)] + Vector2.right * 0.5f;
			//Debug.Log(tempUvs[lon]);
		}
		for(int j = 0; j <= nbLong; j++) tempUvs[j+nbLong+1] = mesh.uv[mesh.uv.Length - nbLong + j - 1];
		#endregion

		#region Normales		
		Vector3[] normales = new Vector3[tempVertex.Length];
		for( int n = 0; n < tempVertex.Length; n++ ) normales[n] = tempVertex[n].normalized;
		#endregion

		int[] triangles = new int[tempVertex.Length * 6];
		int i = 0;
		for(int lon = 0; lon < nbLong; lon++ ) {
			triangles[i++] = lon;
			triangles[i++] = lon+1;
			triangles[i++] = lon+tempVertex.Length-nbLong;
		}

		southMesh.vertices = tempVertex;
		southMesh.uv = tempUvs;
		southMesh.normals = normales;
		southMesh.triangles = triangles;
		southMesh.RecalculateBounds();
		southPole.AddComponent<MeshCollider>();
		yield return 0;
	}

	IEnumerator CreateMiddleTiles(int latTile, int longTile, bool debug){
		GameObject middleTile = new GameObject ("MiddleTile"+latTile+"_"+longTile);
		Mesh mesh = GetComponent<MeshFilter> ().mesh;

		//float latValue = 90 * (numberLat - 1) / (numberLat + 2);
		middleTile.AddComponent<Tile> ();
		middleTile.AddComponent<HeightMapData> ();
		middleTile.GetComponent<Tile>().isSphere = true;
		middleTile.GetComponent<Tile>().textureLoaded = true;
		middleTile.GetComponent<Tile>().meshGenerated = true;
		middleTile.GetComponent<Tile>().column = (longTile + 1) % 2; // column for WMTS, lat 0 long 0 doesn't match, we need to shift it
		//middleTile.GetComponent<Tile>().row = (longTile + 2) % 4;
		middleTile.transform.parent = transform;
		middleTile.transform.position = transform.position;
		middleTile.AddComponent<MeshRenderer> ();
		middleTile.AddComponent<MeshFilter> ();
		middleTile.GetComponent<MeshRenderer> ().material = GetComponent<MeshRenderer>().material;
		Mesh middleMesh = middleTile.GetComponent<MeshFilter> ().mesh;

		Vector3[] tempVertex = new Vector3[(nbLong/4+1) * (nbLat/2+1)];
		int initLat = latTile * nbLat / 2;
		int initLon = longTile * nbLong / 4;

		for (int lat = initLat; lat < initLat + nbLat/2 + 1; lat++)
			for (int lon = initLon; lon <= initLon + nbLong/4; lon++) 
			{
				//if (debug) Debug.Log ((lon-initLon) + (lat-initLat) * (nbLong + 1));
				//tempVertex [(lon-initLon) + (lat-initLat) * (nbLong + 1)] = mesh.vertices [lon + (lat - (latTile == 1 ? 1 : 0)) * (nbLong + 1) + 1];
				if (debug) Debug.Log ((lat-initLat) * (nbLat/2-1) + (lon-initLon));
				tempVertex [(lat-initLat) * (nbLat/2-1) + (lon-initLon)] = mesh.vertices [lon + (lat) * (nbLong + 1) + 1];
			}

		int nbFaces = nbLat/2 * nbLong/4 ;
		int nbTriangles = nbFaces * 2;
		int nbIndexes = nbTriangles * 3;
		int[] triangles = new int[ nbIndexes ];
		int i = 0;

		for( int lat = 0; lat < nbLat/2 - (latTile == 1 ? 0 : 0); lat++ ) {
			for( int lon = 0; lon < nbLong/4; lon++ )
			{
				int current = lon + lat * (nbLong/4 + 1);
				int next = current + nbLong/4 + 1;
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
		for( int lat = initLat; lat < initLat + nbLat/2 + 1; lat++ )
			for( int lon = initLon; lon <= initLon + nbLong/4; lon++ )
				//uvs[(lon-initLon) + (lat-initLat) * (nbLong + 1)] = mesh.uv[lon + (lat - (latTile == 1 ? 1 : 0)) * (nbLong + 1) + 1];
				uvs[(lat-initLat) * (nbLat/2-1) + (lon-initLon)] = mesh.uv[lon + lat * (nbLong + 1) + 1];
		#endregion

		#region Normales		
		Vector3[] normales = new Vector3[tempVertex.Length];
		for( int n = 0; n < tempVertex.Length; n++ ) normales[n] = tempVertex[n].normalized;
		#endregion

		Vector3[] tempVertex2 = new Vector3[tempVertex.Length];
		for(int k = 0; k < tempVertex.Length; k++){
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

		float value00 = Mathf.Min(GeoCord.GetLatitudeFromPosition (middleMesh.vertices[0]), GeoCord.GetLatitudeFromPosition (middleMesh.vertices[middleMesh.vertices.Length - 1]));
		float value01 = Mathf.Min(GeoCord.GetLongitudeFromPosition (middleMesh.vertices[0]), GeoCord.GetLongitudeFromPosition (middleMesh.vertices[middleMesh.vertices.Length - 1]));
		float value10 = Mathf.Max(GeoCord.GetLatitudeFromPosition (middleMesh.vertices[0]), GeoCord.GetLatitudeFromPosition (middleMesh.vertices[middleMesh.vertices.Length - 1]));
		float value11 = Mathf.Max(GeoCord.GetLongitudeFromPosition (middleMesh.vertices[0]), GeoCord.GetLongitudeFromPosition (middleMesh.vertices[middleMesh.vertices.Length - 1]));
		if (longTile == 1) { // In the limit between longitude 180 and longitude -180, always takes -180, we need a positive value for half cases
			float tempVal = value01;
			value01 = value11;
			value11 = -tempVal;
		}

		//GeoCord.CreateText (middleMesh.vertices [0]);
		GeoCord.GetPositionFromLatitudeLongitude(value00, value01);
		middleTile.GetComponent<Tile> ().BBOX = new double[]{value00, value01, value10, value11}; // ymin, xmin, ymax, xmax

		yield return 0;
	}

	void Start () {
		StartCoroutine (CreateSphere());
		//StartCoroutine(TestCreateUVSphereUV());
        CreateData();
    }
}