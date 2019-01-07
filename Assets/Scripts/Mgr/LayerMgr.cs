using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerMgr : MonoBehaviour {
	static GameObject go;
	static LayerMgr layerMgr;

	public static LayerMgr getInstance() {
		if (layerMgr == null)
			layerMgr = go.GetComponent<LayerMgr> ();
		return layerMgr;
	}

	// Use this for initialization
	void Awake () {
		go = gameObject;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void openLayer(string sName){

	}
}
