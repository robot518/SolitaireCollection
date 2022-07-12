using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Find : MonoBehaviour, IPointerDownHandler, IPointerUpHandler{
    IMain _delegate;
	int _iCardNum;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void init(int iCardNum, IMain delt){
		_delegate = delt;
		_iCardNum = iCardNum + 1;
		string str = "finds__" + _iCardNum;
		gameObject.GetComponent<Image>().sprite = AtlasMgr.getInstance().getSpt("res", str);
	}

	public void OnPointerDown(PointerEventData data){
		_delegate.onFindDown (_iCardNum);
	}

	public void OnPointerUp(PointerEventData data){
		_delegate.onFindUp ();
	}
}
