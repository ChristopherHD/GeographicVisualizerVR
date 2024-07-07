using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Net;
using System.IO;

public class HeightMapProvider : MonoBehaviour {

	public Dictionary<Tile, Texture2D> tileToTexturize; 
	public static int contador = 0;
	public static int contador2 = 0;
	public List<string> pendientRequest;
	public LinkedList<Tile> pendientRequests2;
	public static bool finished = false;
	public static int[] bytes2;

	public static int widthSize;
	public static int heightSize;
	private int rows;
	private int columns;
	private int rowsIncrement;
	private int columnsIncrement;
	public int kRowsFactor = 1;
	public int kColumnsFactor = 1;
	public static int kRowsFactorStatic=20;
	public static int kColumnsFactorStatic=20;

	void ScaleHeight(int[] numbers){
		int min = 0, max = 0;
		for(int i = 0; i < numbers.Length; i++){
			if (numbers [i] < min) min = numbers [i];
			if (numbers [i] > max) max = numbers [i];
		}
		for(int i = 0; i < numbers.Length; i++){
			//Debug.Log ("Scale ("+i+"): "+(float)(numbers [i] - min) / (max - min));
		}
	}

	void Awake () {
		tileToTexturize = new Dictionary<Tile,Texture2D> ();
		pendientRequest = new List<string> ();
		pendientRequests2 = new LinkedList<Tile> ();
		kRowsFactorStatic = kRowsFactor;
		kColumnsFactorStatic = kColumnsFactor;
		//StartCoroutine (SetTexture());
		//StartCoroutine(TypeObjectCount<Mesh>("Meshes: "));
		//StartCoroutine(TypeObjectCount<Texture2D>("Texturas2D: "));
	}



	void Start(){
		columns = UVSphereGenerator.nbLong / 4 + 1; // número de columnas respecto a la consideración de los 4 tiles en que se divide inicialmente y que queda como ancho de cada tile
		rows = UVSphereGenerator.nbLat / 2 + 1;

		rowsIncrement = rows;//+ 2;
		columnsIncrement = columns;// + 2;

		//heightSize = rows + (rows - 1) * kRowsFactor;
		heightSize = rowsIncrement + (rowsIncrement - 1) * kRowsFactor;
		//widthSize = columns + (columns - 1) * kColumnsFactor;
		widthSize = columnsIncrement + (columnsIncrement - 1) * kColumnsFactor;
		//StartCoroutine(Upload (this.gameObject));

		Debug.Log (widthSize+"x"+heightSize+"::::"+heightSize * widthSize);
	}



	IEnumerator TypeObjectCount<T>(string message){
		yield return new WaitForSeconds (1);
		Debug.Log(message + FindObjectsOfType (typeof(T)).GetLength(0));
		StartCoroutine(TypeObjectCount<T>(message));
	}

	public static IEnumerator Upload(GameObject tile){
		string requestText = "https://data.worldwind.arc.nasa.gov/elev?"+
			"REQUEST=GetMap"+
			"&SERVICE=WMS"+
			"&VERSION=1.3.0"+
			"&LAYERS=srtm30"+
			"&STYLES="+
			"&FORMAT=image/bil"+
			"&CRS=EPSG:4326" +
			"&BBOX=-45,-20.0000045736216,-22.5,-0.000000355503061655327" +
			"&WIDTH=" + widthSize +
			"&HEIGHT=" + heightSize;

		Debug.Log(requestText);
		UnityWebRequest request = UnityWebRequest.Get (requestText); //UnityWebRequest request = UnityWebRequest.GetTexture(requestText); // en el wgs:84 o epsg:4326, se pasa el bbox en orden miny, minx, many, manx //request = UnityWebRequestTexture.GetTexture("http://ows.mundialis.de/services/service?version=1.3.0&request=GetMap&CRS=EPSG:4326&bbox=26.183923,-18.201495,29.817334,-13.137273&width=360&height=360&layers=OSM-WMS&styles=default&format=image/png");
		yield return request.Send();
		Debug.Log ("algo");
		if (!(request.isNetworkError || request.isHttpError) && tile != null) {
			//tile.GetComponent<MeshRenderer> ().material.mainTexture = ((DownloadHandlerTexture)request.downloadHandler).texture;
			byte[] bytes = request.downloadHandler.data;
			int[] numbers = new int[bytes.Length / 2];

			if (!System.BitConverter.IsLittleEndian) {
				Debug.Log ("BigEndian machine, it's needed to invert result");
			}
			for (int i = 0; i < bytes.Length / 2; i++) { // Los valores están almacenados como bytes de 16 bits, lo que son 2 bytes por valor, y tenemos 200 valores, así que hay 400 bytes de los que solo necesitamos la mitad por número
				numbers[i] = System.BitConverter.ToInt16 (bytes, i * 2);
				//yield return new WaitForSeconds (1);
			}
			bytes2 = numbers;
			//ScaleHeight (numbers);
			Debug.Log("aplicado-old");
			bytes2 = numbers;
			finished = true;
		} else if ((!(request.isNetworkError || request.isHttpError) && tile == null)) { 
			Debug.Log("raro");
		}
		if(request.isNetworkError || request.isHttpError){
			Debug.Log (request.error);
			Debug.Log("------------------ERROR---------------------3-----------1092978888888888845343");
		}
		Ficher.CreateHeight(tile);
	}

	public static IEnumerator Upload(GameObject tile, double[] vector){
		string requestText = "https://data.worldwind.arc.nasa.gov/elev?"+
			"REQUEST=GetMap"+
			"&SERVICE=WMS"+
			"&VERSION=1.3.0"+
			"&LAYERS=srtm30"+
			"&STYLES="+
			"&FORMAT=image/bil"+
			"&CRS=EPSG:4326" +
			"&BBOX="+vector[1].ToString().Replace(",",".")+","+vector[0].ToString().Replace(",",".")+","+vector[3].ToString().Replace(",",".")+","+vector[2].ToString().Replace(",",".")+//-45,-20.0000045736216,-22.5,-0.000000355503061655327" +
			"&WIDTH=" + widthSize +
			"&HEIGHT=" + heightSize;
		
		Debug.Log(requestText);
		UnityWebRequest request = UnityWebRequest.Get (requestText); //UnityWebRequest request = UnityWebRequest.GetTexture(requestText); // en el wgs:84 o epsg:4326, se pasa el bbox en orden miny, minx, many, manx //request = UnityWebRequestTexture.GetTexture("http://ows.mundialis.de/services/service?version=1.3.0&request=GetMap&CRS=EPSG:4326&bbox=26.183923,-18.201495,29.817334,-13.137273&width=360&height=360&layers=OSM-WMS&styles=default&format=image/png");

		yield return request.Send();
		Debug.Log ("Enviando petición de descarga de mapa de alturas");
		if (!(request.isNetworkError || request.isHttpError) && tile != null) {
			//tile.GetComponent<MeshRenderer> ().material.mainTexture = ((DownloadHandlerTexture)request.downloadHandler).texture;
			byte[] bytes = request.downloadHandler.data;
			int[] numbers = new int[bytes.Length / 2];

			if (!System.BitConverter.IsLittleEndian) {
				Debug.Log ("BigEndian machine, it's needed to invert result");
			}
			for (int i = 0; i < bytes.Length / 2; i++) { // Los valores están almacenados como bytes de 16 bits, lo que son 2 bytes por valor, y tenemos 200 valores, así que hay 400 bytes de los que solo necesitamos la mitad por número
				//Debug.Log (i+": "+System.BitConverter.ToInt16 (bytes, i * 2));
				numbers[i] = System.BitConverter.ToInt16 (bytes, i * 2);
			}
			tile.GetComponent<HeightMapData> ().heightMapData = numbers;
			tile.GetComponent<HeightMapData> ().CreateHeight();
			//ScaleHeight (numbers);
			Debug.Log("aplicado");
			bytes2 = numbers;
			finished = true;
		} else if ((!(request.isNetworkError || request.isHttpError) && tile == null)) { 
			Debug.Log("raro");
		}
		if(request.isNetworkError || request.isHttpError){
			Debug.Log (request.error);
			Debug.Log("------------------ERROR---------------------3-----------10929745343");
		}
		//Ficher.CreateHeight(tile);
	}

	IEnumerator LimitRequest(GameObject tile){
		if (pendientRequests2.Contains (tile.GetComponent<Tile>())) pendientRequests2.Remove (tile.GetComponent<Tile> ());
		if (pendientRequests2.Count == 0) {
			pendientRequests2.AddFirst (tile.GetComponent<Tile> ());
		} else {
			foreach (Tile tempTile in pendientRequests2) {
				if (tempTile.LoD >= tile.GetComponent<Tile> ().LoD) {
					pendientRequests2.AddBefore (pendientRequests2.Find (tempTile), tile.GetComponent<Tile> ());
					break;
				}
			}
			string val = "";
			foreach (Tile tempTile in pendientRequests2) {
				val += "--"+tempTile.name;
			}
			Debug.Log (val);
			if (!pendientRequests2.Contains (tile.GetComponent<Tile> ())) pendientRequests2.AddLast (tile.GetComponent<Tile>());
		}
		yield return 0;
	}
}