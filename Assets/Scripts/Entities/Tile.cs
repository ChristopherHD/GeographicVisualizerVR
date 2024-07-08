using System.Collections;
using UnityEngine;

public class Tile : MonoBehaviour {

	public int LoD = 0;
	public int row = 0;
	public int column = 0;
	private Mesh meshCached;
	public Tile[] children { get; set;}
	public double[] BBOX;
	public bool mustBeDestroyed{ get; set;}
	public bool isDivided = false;

	public bool textureLoaded = false;
	public bool meshGenerated = false;
	public bool isSphere = false;

	public float DistanceCamera;
	public float DistanceVertex;

	private static LoDTesterSphere _LODTesterSphere; 
	private float vertexDistance;

	public double latitudeDiff;

	private int attempts = 0;
	private Vector3[] cachedMeshCornerVertices;

    void Awake(){
		textureLoaded = false;
		if (_LODTesterSphere == null) _LODTesterSphere = GameObject.FindGameObjectWithTag ("GameController").GetComponent<LoDTesterSphere> ();
	}

	void Start () {
		children = new Tile[4];
		meshCached = GetComponent<MeshFilter>().mesh;
        cachedMeshCornerVertices = new Vector3[4];
		cachedMeshCornerVertices[0] = meshCached.vertices[0];
		cachedMeshCornerVertices[1] = meshCached.vertices[UVSphereGenerator.nbLong / 4];
        cachedMeshCornerVertices[2] = meshCached.vertices[meshCached.vertices.Length - 1];
        cachedMeshCornerVertices[3] = meshCached.vertices[meshCached.vertices.Length - UVSphereGenerator.nbLong / 4 - 1];

		vertexDistance = (meshCached.vertices[0] - meshCached.vertices[1]).magnitude / 2;
		centerPoint = new MapPosition();
		//centerPoint.longitude = (BBOX[3] + BBOX[1]) / 2d;
		//centerPoint.latitude = (BBOX[2] + BBOX[0]) / 2d;
		//Debug.Log(centerPoint.longitude + " : " + centerPoint.latitude + " : " + new Vector3d((GetVertexPosition(VertexType.DownLeft) + GetVertexPosition(VertexType.DownRight) +
		//	GetVertexPosition(VertexType.UpperLeft) + GetVertexPosition(VertexType.UpperRight)) / 4));
		centerPoint.worldPosition = new Vector3d((GetVertexPosition(VertexType.DownLeft) + GetVertexPosition(VertexType.DownRight) +
			GetVertexPosition(VertexType.UpperLeft) + GetVertexPosition(VertexType.UpperRight)) / 4);
        centerPoint.longitude = CoordUtils.GetLongitudeFromPosition((GetVertexPosition(VertexType.DownLeft) + GetVertexPosition(VertexType.DownRight) +
			GetVertexPosition(VertexType.UpperLeft) + GetVertexPosition(VertexType.UpperRight)) / 4);
		centerPoint.latitude = CoordUtils.GetLatitudeFromPosition((GetVertexPosition(VertexType.DownLeft) + GetVertexPosition(VertexType.DownRight) +
			GetVertexPosition(VertexType.UpperLeft) + GetVertexPosition(VertexType.UpperRight)) / 4);

		/*centerPoint.longitude = (BBOX[2] - BBOX[0]) / 2f;
		centerPoint.latitude = (BBOX[3] - BBOX[1]) / 2f;*/

		//centerPoint.worldPosition = GeoCord.GetPositionFromLatitudeLongitude(centerPoint.longitude, centerPoint.latitude);
		StartCoroutine (CheckLoD ());
		//BBOX = new float[4];
		//GameObject.FindGameObjectWithTag("GameController").GetComponent<MapProvider>().SetTexture(gameObject, BBOX);
	}

	void Update()
	{
		RecalcCenterPosition();
	}

	// needed in case the globe rotates, will probably make the camera rotates instead to remove this
	private void RecalcCenterPosition()
	{
		// GC high usage 
		centerPoint.worldPosition = new Vector3d((GetVertexPosition(VertexType.DownLeft) + GetVertexPosition(VertexType.DownRight) +
            GetVertexPosition(VertexType.UpperLeft) + GetVertexPosition(VertexType.UpperRight)) / 4);
        GameObject worldObject = GameObject.FindWithTag("Player");
        Matrix4x4 localToWorld = worldObject.transform.localToWorldMatrix;
		Vector3 result = (GetVertexPosition(VertexType.DownLeft) + GetVertexPosition(VertexType.DownRight) + GetVertexPosition(VertexType.UpperLeft) + GetVertexPosition(VertexType.UpperRight)) / 4;
        result = localToWorld.MultiplyPoint3x4(result);
		centerPoint.longitude = CoordUtils.GetLongitudeFromPosition(result);
		centerPoint.latitude = CoordUtils.GetLatitudeFromPosition(result);
        /*centerPoint.longitude = GeoCord.GetLongitudeFromPosition((GetVertexPosition(VertexType.DownLeft) + GetVertexPosition(VertexType.DownRight) +
            GetVertexPosition(VertexType.UpperLeft) + GetVertexPosition(VertexType.UpperRight)) / 4);
        centerPoint.latitude = GeoCord.GetLatitudeFromPosition((GetVertexPosition(VertexType.DownLeft) + GetVertexPosition(VertexType.DownRight) +
            GetVertexPosition(VertexType.UpperLeft) + GetVertexPosition(VertexType.UpperRight)) / 4);*/
        //Vector3[] test = meshCached.vertices;

        //GetVertexPosition(VertexType.DownRight);GetVertexPosition(VertexType.UpperLeft); GetVertexPosition(VertexType.UpperRight);
        /*centerPoint.worldPosition.x = vertexPosition.x;
        centerPoint.worldPosition.y = vertexPosition.y;
        centerPoint.worldPosition.z = vertexPosition.z;*/
    }

    public void UpdateTexture(){
		GameObject.FindGameObjectWithTag("GameController").GetComponent<MapProvider>().SetTexture(gameObject);
	}

	public enum VertexType{
		UpperLeft,
		UpperRight,
		DownLeft,
		DownRight,
		Center
	}
    
    public MapPosition centerPoint = null;
	public Vector3 GetVertexPosition(VertexType type)
    {
		Vector3 result = Vector3.down;
		switch (type)
        {
			case VertexType.UpperLeft: result = cachedMeshCornerVertices[0]; break;
			case VertexType.UpperRight: result = cachedMeshCornerVertices[1]; break;
            case VertexType.DownRight: result = cachedMeshCornerVertices[2]; break;
            case VertexType.DownLeft: result = cachedMeshCornerVertices[3]; break;
                //case VertexType.Center: return centerPoint.worldPosition; // center could have a vertex or not
        }
        GameObject worldObject = GameObject.FindWithTag("Player");
        Matrix4x4 localToWorld = worldObject.transform.localToWorldMatrix;
        result = localToWorld.MultiplyPoint3x4(result);
        return result;
	}

	public Vector3d GetVertexPositiond(VertexType type)
    {
        Vector3d result = Vector3d.down;
        switch (type)
        {
            case VertexType.UpperLeft: result = new Vector3d(cachedMeshCornerVertices[0]); break;
            case VertexType.UpperRight: result = new Vector3d(cachedMeshCornerVertices[1]); break;
            case VertexType.DownRight: result = new Vector3d(cachedMeshCornerVertices[2]); break;
            case VertexType.DownLeft: result = new Vector3d(cachedMeshCornerVertices[3]); break;
            case VertexType.Center: result = centerPoint.worldPosition; break; // center could have a vertex or not
        }
        GameObject worldObject = GameObject.FindWithTag("Player");
        Matrix4x4 localToWorld = worldObject.transform.localToWorldMatrix;
		Vector3 worldRotationPosition = new ((float)result.x, (float)result.y, (float)result.z);
        result = new Vector3d(localToWorld.MultiplyPoint3x4(worldRotationPosition));
        return result;
	}

#if UNITY_EDITOR
	private void OnDrawGizmosSelected()
	{
		GUIStyle style = new GUIStyle();
		Color color = Color.black;
		style.normal.textColor = color;
		UnityEditor.Handles.color = color;
        //string x = "Distance Camera " + DistanceCamera + " Distance Vertex " + DistanceVertex;
        //UnityEditor.Handles.Label(GetComponent<MeshFilter>().mesh.vertices[0]  , x, style);
        UnityEditor.Handles.Label(GetComponent<MeshFilter>().mesh.vertices[0], GetComponent<MeshFilter>().mesh.vertices[0].ToString(), style);

        //double lon = (this.BBOX[3] + this.BBOX[1]) / 2;
		//double lat = (this.BBOX[2] + this.BBOX[0]) / 2;

        Mesh tileMesh = GetComponent<MeshFilter>().mesh;
        Vector3[] tileMeshVertices = tileMesh.vertices;
        
        GameObject worldObject = GameObject.FindWithTag("Player");
        Matrix4x4 localToWorld = worldObject.transform.localToWorldMatrix;
        for (int i = 0; i < tileMeshVertices.Length; ++i)
        {
            tileMeshVertices[i] = localToWorld.MultiplyPoint3x4(tileMeshVertices[i]);
        }
        style.normal.textColor = Color.red;
        UnityEditor.Handles.Label(tileMeshVertices[0], tileMeshVertices[0].ToString(), style);

		Matrix4x4 worldToLocal = worldObject.transform.worldToLocalMatrix;
        for (int i = 0; i < tileMeshVertices.Length; ++i)
        {
            tileMeshVertices[i] = worldToLocal.MultiplyPoint3x4(tileMeshVertices[i]);
        }
        style.normal.textColor = Color.green;
        UnityEditor.Handles.Label(tileMeshVertices[0], tileMeshVertices[0].ToString(), style);

		//Debug.Log ("TransformTile: "  + transform.position);
		//Gizmos.DrawSphere (GeoCord.GetMiddleTilePosition (this), 0.5f);

		if (transform.childCount == 0)
        {
            Gizmos.color = Color.yellow;
            Vector3 centerVector = new Vector3((float)centerPoint.worldPosition.x, (float)centerPoint.worldPosition.y, (float)centerPoint.worldPosition.z);
            //Gizmos.DrawSphere(centerVector, 100f / (LoD+2));
            Gizmos.DrawSphere(centerVector, vertexDistance * 10);
        }
        Gizmos.color = Color.red;
		/*Gizmos.DrawSphere(this.GetVertexPosition(Tile.VertexType.UpperLeft), 0.5f);
		Gizmos.DrawSphere(this.GetVertexPosition(Tile.VertexType.UpperRight), 0.5f);
		Gizmos.DrawSphere(this.GetVertexPosition(Tile.VertexType.DownLeft), 0.5f);
		Gizmos.DrawSphere(this.GetVertexPosition(Tile.VertexType.DownRight), 0.5f);*/
        Gizmos.color = Color.green;
		Vector3 vector = new Vector3((float)Camera.main.GetComponent<CameraManager>().cameraHitPosition.worldPosition.x, (float)Camera.main.GetComponent<CameraManager>().cameraHitPosition.worldPosition.y, (float)Camera.main.GetComponent<CameraManager>().cameraHitPosition.worldPosition.z);
		Gizmos.DrawSphere(vector, 0.5f);
		//Gizmos.DrawSphere(this.GetVertexPosition(Tile.VertexType.UpperLeft), 0.5f);
		//Gizmos.DrawSphere(this.GetVertexPosition(Tile.VertexType.UpperLeft), 0.5f);
		//Gizmos.DrawSphere (GeoCord.GetPositionFromLatitudeLongitude ((float)lon, (float)lat), 1);
		//Debug.Log("ShouldDivide: " + _LODTesterSphere.ShouldDivide(this, true));
	}
	#endif


	IEnumerator CheckLoD(){
		yield return new WaitUntil(() => (textureLoaded || (transform.parent != null && transform.parent.childCount != 4 && transform.parent.childCount != 0)));
		if (isSphere) {
			if (transform.parent.parent.parent != null && transform.parent.childCount != 4 && transform.parent.childCount != 0) { 
				mustBeDestroyed = true;
			}
		} else {
			if (transform.parent != null && transform.parent.childCount != 4 && transform.parent.childCount != 0)
				mustBeDestroyed = true;
		}

		if (!mustBeDestroyed) {
			if (!isSphere){
				//_LODTester.checkLoD (transform);
			} else {
				/*if (GameObject.FindWithTag("GameController").GetComponent<LoDTesterSphere>().ShouldDivide(this)) { 
					attempts++;
					Debug.Log("First try: " + this.name);
                    if (attempts > 1 && transform.childCount == 0) Debug.Log("Intentos: " + attempts);
                }*/
				_LODTesterSphere.CheckLoD (transform);
			}
			
			yield return new WaitForSeconds (0.5f);
			if (!mustBeDestroyed) {
				StartCoroutine (CheckLoD ());
			} else {
				if (LoD != 0 && transform.childCount == 0)
                {
                    transform.parent.GetComponent<Tile>().isDivided = false;
                    Object.Destroy(gameObject);
                }
            }
		} else {
			if (LoD != 0) 
			{
				transform.parent.GetComponent<Tile>().isDivided = false;
                Object.Destroy(gameObject);
            }
		}
	}

	void OnDestroy(){
		FreeMemory ();
	}

	private void FreeMemory(){
		Object.Destroy (GetComponent<MeshFilter> ().mesh);
        if(transform.parent != null)
        {
            if(transform.parent.childCount == 0) GetComponent<MeshRenderer>().enabled = true;
			if (LoD != 0) Object.Destroy(GetComponent<MeshRenderer>().material.mainTexture);
            if (!transform.parent.GetComponent<MeshRenderer>().enabled) transform.parent.GetComponent<MeshRenderer>().enabled = true;
			if (!transform.parent.GetComponent<MeshCollider>().enabled) transform.parent.GetComponent<MeshCollider>().enabled = true;
		}
	}
}