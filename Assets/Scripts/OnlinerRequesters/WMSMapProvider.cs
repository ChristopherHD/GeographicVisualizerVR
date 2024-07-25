using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using static UnityEngine.Networking.UnityWebRequest;

public class WMSMapProvider : MapProvider {

    public TMP_Dropdown mainLayerOption;
    public TMP_Dropdown secondaryLayerOption;

    public override IEnumerator Upload(GameObject tile)
    {
        double[] BBOX = tile.GetComponent<Tile>().BBOX;
        string mainLayer = mainLayerOption == null ? "OSM-WMS" : mainLayerOption.options[mainLayerOption.value].text;
        string secondaryLayer = secondaryLayerOption == null ? "None" : secondaryLayerOption.options[secondaryLayerOption.value].text;
        secondaryLayer = secondaryLayer.Equals("None") ? "" : "," + secondaryLayer;

        yield return new WaitUntil(() => pendientRequest.Count < 20);
        string requestText = serviceDomain +
            "version=1.3.0&" +
            "request=GetMap&" +
            "CRS=EPSG:4326&" +
            "bbox=" + BBOX[0].ToString().Replace(",", ".") + "," + BBOX[1].ToString().Replace(",", ".") + "," + BBOX[2].ToString().Replace(",", ".") + "," + BBOX[3].ToString().Replace(",", ".") + "&" +
            "width=320&" +
            "height=320&" +
            "layers=" + mainLayer + secondaryLayer + "&" +
            "styles=default,default&" +
            "format=image/png";
        //Debug.Log(requestText);

        if (!pendientRequest.Contains(requestText))
        {
            pendientRequest.Add(requestText);
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(requestText); //UnityWebRequest request = UnityWebRequest.GetTexture(requestText); // en el wgs:84 o epsg:4326, se pasa el bbox en orden miny, minx, many, manx //request = UnityWebRequestTexture.GetTexture("http://ows.mundialis.de/services/service?version=1.3.0&request=GetMap&CRS=EPSG:4326&bbox=26.183923,-18.201495,29.817334,-13.137273&width=360&height=360&layers=OSM-WMS&styles=default&format=image/png");

            yield return request.SendWebRequest();
            pendientRequest.Remove(requestText);

            if (request.result == Result.ConnectionError || request.result == Result.ProtocolError)
            {
                Debug.Log("------------------ERROR--------------------------------1");
                Debug.Log("Petición: " + requestText);
                Debug.Log("Error: " + request.error);
                StartCoroutine(Upload(tile)); // Intenta actualizar de nuevo el tile
            } else {
                if (tile != null)
                {
                    tile.GetComponent<MeshRenderer>().material.mainTexture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                    tile.GetComponent<MeshRenderer>().material.mainTexture.wrapMode = TextureWrapMode.Clamp;
                    tile.GetComponent<Tile>().textureLoaded = true;
                } else {
                    Object.Destroy(((DownloadHandlerTexture) request.downloadHandler).texture);
                }
            }
        }
    }
}