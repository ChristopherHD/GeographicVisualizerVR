using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static UnityEngine.Networking.UnityWebRequest;

public abstract class MapProvider : MonoBehaviour {

	public string serviceDomain = "https://ows.terrestris.de/osm/service?";
	protected List<string> pendientRequest;

	void Awake () {
		pendientRequest = new List<string>();
	}

	public abstract IEnumerator Upload(GameObject tile);

	public void SetTexture(GameObject tile){
		StartCoroutine (Upload(tile));
    }

}