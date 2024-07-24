using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardManager : MonoBehaviour
{
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.N))
		{
			Debug.Log("Modo: "+ Screen.fullScreenMode);
			Screen.fullScreen = !Screen.fullScreen;
		}
	}
}
