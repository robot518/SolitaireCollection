using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CBtn : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
//	float _iDown = -1;
//	int _iScale = -1;
//	bool _bScale = false;
//	CGrid _cGrid;
//	Transform _trans;
//	bool _bFlagShow;

	// Use this for initialization
	void Start () {
//		_cGrid = transform.parent.GetComponent<CGrid> ();
//		_trans = transform.parent.GetChild (transform.GetSiblingIndex () + 1);
	}
	
	// Update is called once per frame
	void Update () {
//		if (_iDown > -1) {
//			var dt = Time.deltaTime;
//			_iDown -= dt;
//			if (_iDown <= 0) {
//				_bScale = true;
//				_iScale = 1;
//				_iDown = -1;
//				_bFlagShow = _trans.gameObject.activeSelf;
//				_trans.gameObject.SetActive (true);
//			}
//		}
//		if (_bScale == true) {
//			var dt = Time.deltaTime;
//			if (_iScale == 1) {
//				_trans.localScale += new Vector3 (50 * dt, 50 * dt, 50 * dt);
//				if (_trans.localScale.x >= 5) {
//					_iScale = 2;
//				}
//			} else if (_iScale == 2) {
//				_trans.localScale -= new Vector3 (50 * dt, 50 * dt, 0);
//				if (_trans.localScale.x <= 1) {
//					_trans.localScale = Vector3.one;
//					_iScale = -1;
////					_trans.gameObject.SetActive (_bFlagShow);
//					_cGrid.onFlagEvent (_bFlagShow);
//					_bScale = false;
//				}
//			}
//
//		}
	}

	public void OnPointerDown(PointerEventData eventData){
//		_iDown = 0.5f;
	}

	public void OnPointerUp(PointerEventData eventData){
//		_iDown = -1;
	}
}
