using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

public class FuncMgr : MonoBehaviour {
	static GameObject go;
	static FuncMgr funcMgr;

	public static FuncMgr getInstance() {
		if (funcMgr == null)
			funcMgr = go.GetComponent<FuncMgr> ();
		return funcMgr;
	}

	void Awake(){
		go = gameObject;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public string getCardNum(int iCard, int iColor){
		string[] CardNum = { "A", "J", "Q", "K" };
		string str;
		if (iCard == 1)
			str = CardNum [0];
		else if (iCard > 10)
			str = CardNum [iCard - 10];
		else
			str = iCard.ToString ();
		string strColor = iColor == 0 ? "A03228FF" : "191919FF"; // 0红1黑
		str = "<color=#" + strColor + ">" + str + "</color>";
		return str;
	}

	public void showTimeCost(UnityAction func){
		TimeSpan ts1 = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
		var t1 = ts1.Seconds * 1000 + ts1.Milliseconds;
		if (func != null)
			func ();
		TimeSpan ts2 = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
		var t2 = ts2.Seconds * 1000 + ts2.Milliseconds;
		print (Convert.ToInt64 (t2 - t1));
	}
}
