using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class UVSphereGeneratorWithDivisions : MonoBehaviour
{
    public float radius = 63f;
    public int longitudeSegments = 32;
    public int latitudeSegments = 27;
    public int polesLatitudeTiles = 1;
    public int latitudeTileDivisions = 4;
    public int longitudeTileDivisions = 4;
    public static float radiusStatic;

    public static int polesLatitudeTilesStatic;
    public static int latitudeTileDivisionsStatic;
    public static int longitudeTileDivisionsStatic;

    private void Awake()
    {
        polesLatitudeTilesStatic = polesLatitudeTiles;
        latitudeTileDivisionsStatic = latitudeTileDivisions;
        longitudeTileDivisionsStatic = longitudeTileDivisions;
		radiusStatic = radius;
    }
    IEnumerator TestCreateUVSphereUV()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        meshFilter.mesh = mesh;
        //mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

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
                }
                else
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
                    if (lon > 0 && isLongitudeBorder)
                    {
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
                    if (latNoPoles > 0 && lon > 0 && isLongitudeBorder && isLatitudeBorder)
                    {
                        tilesVertices[tileIndex - 1 - longitudeTileDivisions].Add(position);
                        tilesUvs[tileIndex - 1 - longitudeTileDivisions].Add(uv);
                    }
                }

                vertices.Add(position);
                uvs.Add(uv);
            }
        }

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
            CreateTriangles(tilesTriangles, initialLatitude: 0, finalLatitude: (latitudeSegmentsNoPoles + latitudeTileDivisions - 1) / latitudeTileDivisions - 1, finalLongitude: longitudeSegments / longitudeTileDivisions);

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

    void Start()
    {
        StartCoroutine(TestCreateUVSphereUV());
        CreateData();
    }
}
