using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardGrowUp : MonoBehaviour {
	Text labNum1;
	Image img1;
//	Text labNum2;
//	Image img2;
//	Text labNum3;
	int _iCard;
//	AtlasMgr atMgr;
	bool _bAutoMove = false;
	float _px;
	float _py;
	float _pxDes;
	GrowUp _delt;

	// Use this for initialization
	void Start () {
//		initParas ();
	}
	
	// Update is called once per frame
	void Update () {
		var dt = Time.deltaTime;
		var rect = GetComponent<RectTransform> ();
		if (_bAutoMove == true) {
			var iSpeed = 10.0f;
			transform.Translate (iSpeed * _px * dt, iSpeed * _py * dt, 0);
			var pxTemp = rect.anchoredPosition.x;
			if ((_px <= 0 && pxTemp <= _pxDes)
				|| (_px > 0 && pxTemp >= _pxDes)) {
				_bAutoMove = false;
				gameObject.SetActive (false);
				_delt.onReachCall (_iCard);
			}
		}
	}

	void initParas(){
		labNum1 = transform.GetChild (1).GetComponent<Text> ();
		img1 = transform.GetChild (0).GetComponent<Image> ();
	}

	void initShow(bool bShow){
		labNum1.gameObject.SetActive (bShow);
		img1.gameObject.SetActive (bShow);
	}

	public void showMove(float px, float py, float pxDes){
		_bAutoMove = true;
		_px = px;
		_py = py;
		_pxDes = pxDes;
	}

	public int getICard(){
		return _iCard;
	}

	public void init(int iCard, GrowUp delt){
		initParas ();
		_iCard = iCard;
		_delt = delt;
		var iCardNum = iCard % 100;
		var iCardType = (int)Mathf.Floor (iCard / 100);
		if (iCardNum > 13) {
			initShow (false);
		} else {
			initShow (true);
			var iColor = iCardType % 2 == 1 ? 0 : 1;
			labNum1.text = getCardNum (iCardNum, iColor);
		}
	}

	string getCardNum(int iCard, int iColor){
		string[] CardNum = { "A", "J", "Q", "K" };
		string str;
		if (iCard == 1)
			str = CardNum [0];
		else if (iCard > 10)
			str = CardNum [iCard - 10];
		else
			str = iCard.ToString ();
		string strColor = iColor == 0 ? "D94432FF" : "191919FF"; // 0红1黑
		str = "<color=#" + strColor + ">" + str + "</color>";
		return str;
	}
}
