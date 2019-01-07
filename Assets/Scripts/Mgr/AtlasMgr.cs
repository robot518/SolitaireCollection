using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtlasMgr : MonoBehaviour {
	static GameObject go;
	static AtlasMgr atMgr;
	Dictionary<string, Object[]> atDic = new Dictionary<string, Object[]> ();

	public static AtlasMgr getInstance() {
		if (atMgr == null)
			atMgr = go.GetComponent<AtlasMgr> ();
		return atMgr;
	}

	void Awake(){
		AtlasMgr.go = gameObject;
	}
	public Sprite getSpt(string sptAtlasPath, string sptName) {
		Object[] atlas;
		if (atDic.ContainsKey (sptAtlasPath))
			atlas = atDic [sptAtlasPath];
		else {
			atlas = Resources.LoadAll (sptAtlasPath);
			atDic.Add (sptAtlasPath, atlas);
		}
		for (int i = 0, len = atlas.Length; i < len; i++) {
			if (atlas [i].name == sptName)
				return (Sprite) atlas [i];
		}
		return null;
	}
	public void delAtals(string sptAtlasPath){
		if (atDic.ContainsKey (sptAtlasPath))
			atDic.Remove (sptAtlasPath);
	}
}
