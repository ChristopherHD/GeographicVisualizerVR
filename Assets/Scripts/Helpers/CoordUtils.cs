using UnityEngine;

public class CoordUtils {

	public static GameObject text;
	public static GameObject sphere;

	public static Vector3 GetMiddleTilePosition(Tile tile){
		double lon = (tile.BBOX[3] + tile.BBOX[1]) / 2;
		double lat = (tile.BBOX[2] + tile.BBOX[0]) / 2;

		return GetPositionFromLatitudeLongitude((float)lat,(float)lon);
        //return new Vector3( sin1 * cos2, cos1, sin1 * sin2 ) * UVSphereGenerator.radiusStatic;
    }

    public static void CreateText(Vector3 vertexPosition){
		text = GameObject.FindGameObjectWithTag ("clone");
		sphere = GameObject.FindGameObjectWithTag ("Player");
		GameObject.Instantiate (text, sphere.transform.position + vertexPosition, Quaternion.identity);
	}

	public static float GetLatitudeFromPosition(Vector3 pos){
		float x = pos.x;
		float y = pos.y;
		float z = pos.z;
		float theta = Mathf.Acos (y/Mathf.Sqrt(x*x+y*y+z*z));
		return (90 - Mathf.Rad2Deg * theta);
	}
	public static double GetLatitudeFromPosition(Vector3d pos)
	{
		double x = pos.x;
		double y = pos.y;
		double z = pos.z;
		double theta = Mathd.Acos(y / Mathd.Sqrt(x * x + y * y + z * z));
		return (90 - Mathd.Rad2Deg * theta);
	}

	public static double GetLongitudeFromPosition(Vector3d pos)
	{
		double x = pos.x;
		double z = pos.z;
		return ((Mathd.Rad2Deg * (Mathd.Atan2(z, x))));//Mathf.Rad2Deg * Mathf.Atan (x/z); // atan2 determina el cuadrante, atan solo funciona como si fuese un cuadrante
	}

	public static float GetLongitudeFromPosition(Vector3 pos){
		float x = pos.x;
		float z = pos.z;
		return ((Mathf.Rad2Deg * (Mathf.Atan2(z,x))));//Mathf.Rad2Deg * Mathf.Atan (x/z); // atan2 determina el cuadrante, atan solo funciona como si fuese un cuadrante
	}

	public static Vector3 GetPositionFromLatitudeLongitude(float latitude, float longitude){
		float r = UVSphereGenerator.radiusStatic;

		Vector3 result = new Vector3 (Mathf.Cos(Mathf.Deg2Rad * latitude) * Mathf.Cos(Mathf.Deg2Rad * longitude),
			Mathf.Sin(Mathf.Deg2Rad * latitude), 
			Mathf.Cos(Mathf.Deg2Rad * latitude) * Mathf.Sin(Mathf.Deg2Rad * longitude)) * r;  // el par (y,z) está en orden diferente
		//CreateText (result);
		return result;
	}

	public static Vector3d GetPositionFromLatitudeLongitude(double latitude, double longitude)
	{
		double r = UVSphereGenerator.radiusStatic;

		Vector3d result = new Vector3d(Mathd.Cos(Mathd.Deg2Rad * latitude) * Mathd.Cos(Mathd.Deg2Rad * longitude),
            Mathd.Sin(Mathd.Deg2Rad * latitude),
            Mathd.Cos(Mathd.Deg2Rad * latitude) * Mathd.Sin(Mathd.Deg2Rad * longitude)) * r; // el par (y,z) está en orden diferente
		return result;
	}

	/*public static float GetHeightFromPosition(Vector3 pos){
		float longitude = GetLongitudeFromPosition (pos);
		float latitude = GetLatitudeFromPosition (pos);

		return GetPositionFromLatitudeLongitude(longitude, latitude).magnitude;
	}*/
}