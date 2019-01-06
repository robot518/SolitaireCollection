using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler {
	public Text textCardNum;
	public Image imgBigCardType;
	public Image imgSmCardType;
	public Transform transMove;
	int _iCardType;			// 1-4方块、梅花、红桃、黑桃
	int _iCardNum;			// 1-13
	int _iColor;			// 0红、1黑
	bool _bMove;
	int _iPos;				// 0牌组、1中转、2终点
	int _iRow;				// 0-7
	Kdjl _delegate;
	List<Transform> items = new List<Transform>();
	AudioMgr adMgr;
	bool _bAutoMove = false;
	bool _bAutoRotate = false;
	bool _bAutoMoveBack = false;
	int _iRotate;
	int _iMove;
	float _px;
	float _py;
	float _pxDes;
	int iSpeed = 10;
	bool _bPlayWin;

	// Use this for initialization
	void Start () {
		adMgr = AudioMgr.getInstance ();
	}
	
	// Update is called once per frame
	void Update () {
		var dt = Time.deltaTime;
		var rect = GetComponent<RectTransform> ();
		if (_bAutoMove == true) {
			transform.Translate (iSpeed * _px * dt, iSpeed * _py * dt, 0);
			var pxTemp = rect.anchoredPosition.x;
			if (_iPos != 0) {
				if ((_px <= 0 && pxTemp <= _pxDes)
					|| (_px > 0 && pxTemp >= _pxDes)) {
					_bAutoMove = false;
					transform.SetParent (_delegate.getTransP (_iPos, _iRow));
					transform.localPosition = Vector3.zero;
				}
			} else {
				if ((_px <= 0 && pxTemp <= _pxDes)
					|| (_px > 0 && pxTemp >= _pxDes)) {
					_bAutoMove = false;
					var transP = _delegate.getTransP (_iPos, _iRow);
					transform.SetParent (transP);
					transform.localPosition = new Vector2 (0, 0 - 30 * (transP.childCount - 1));
					if (transform.childCount > 3) {
						for (int i = 3, len = transform.childCount; i < len; i++) {
							var item = transform.GetChild (3);
							item.SetParent (transP);
							item.localPosition = new Vector3 (0, 0 - 30 * (transP.childCount - 1), 0);
						}
					}
				}
			}
		} else if (_bPlayWin == true) {
			var iMove = 200;
			transform.Translate (-iMove * dt, 0, 0);
			var iL = _delegate.getPLX ();
			if (rect.anchoredPosition.x <= iL) {
				rect.anchoredPosition = new Vector2 (iL, rect.anchoredPosition.y);
				_bPlayWin = false;
			}
		}
		if (_bAutoRotate == true) {
			switch (_iRotate) {
			case 1:
				transform.Rotate (0, 0, -500 * dt);
				if (transform.rotation.z <= Quaternion.AngleAxis(-30, Vector3.forward).z) {
					_iRotate = 2;
				}
				break;
			case 2:
				transform.Rotate (0, 0, 400 * dt);
				if (transform.rotation.z >= Quaternion.AngleAxis(25, Vector3.forward).z) {
					_iRotate = 3;
				}
				break;
			case 3:
				transform.Rotate (0, 0, -300 * dt);
				if (transform.rotation.z <= Quaternion.AngleAxis(-10, Vector3.forward).z) {
					transform.rotation = Quaternion.identity;
					_bAutoRotate = false;
				}
				break;
			default:
				break;
			}
		}
		if (_bAutoMoveBack == true) {
			switch (_iMove) {
			case 1:
				transform.Translate (400 * dt, 0, 0);
				if (rect.anchoredPosition.x >= 34) {
					_iMove = 2;
				}
				break;
			case 2:
				transform.Translate (-300 * dt, 0, 0);
				if (rect.anchoredPosition.x <= 23) {
					_iMove = 3;
				}
				break;
			case 3:
				transform.Translate (200 * dt, 0, 0);
				if (rect.anchoredPosition.x >= 31) {
					var pyTemp = rect.anchoredPosition.y;
					rect.anchoredPosition = new Vector2 (30, pyTemp);
					_bAutoMoveBack = false;
				}
				break;
			default:
				break;
			}
		}
	}

	public void showMove(float px, float py, float pxDes){
		_bAutoMove = true;
		_px = px;
		_py = py;
		_pxDes = pxDes;
	}

	public void showRotate(){
		_bAutoRotate = true;
		_iRotate = 1;
	}

	void showMoveBack(){
		_bAutoMoveBack = true;
		_iMove = 1;
	}

	public void playWinMove(bool bPlay){
		_bPlayWin = bPlay;
	}

	public void init(int iCardNum, int iCardType, int iPos, int iRow, Kdjl delt){
		gameObject.SetActive (true);
		_iPos = iPos;
		_iRow = iRow;
		_bMove = false;
		_delegate = delt;

		_iCardType = iCardType;
		_iCardNum = iCardNum;
		_iColor = iCardType % 2 == 1 ? 0 : 1;

		textCardNum.text = FuncMgr.getInstance ().getCardNum (iCardNum, _iColor);
		string[] CardType = {"fangkuai", "meihua", "hongtao", "heitao"};
		var sprite = Resources.Load<Sprite> ("res/" + CardType [iCardType - 1]);
		imgBigCardType.GetComponent<Image> ().sprite = sprite;
		imgSmCardType.GetComponent<Image> ().sprite = sprite;
	}

	public void setBMove(bool bMove){
		_bMove = bMove;
	}

	public bool getBMove(){
		return _bMove;
	}

	public void OnBeginDrag(PointerEventData data){		
		if (_bMove == false)
			return;
		addItems ();
		transform.SetParent (transMove);
		if (items.Count > 0)
			foreach (Transform item in items) {
				item.SetParent(transform);
			}
	}

	public void OnDrag(PointerEventData data){
		if (_bMove == false)
			return;
		Vector3 globalMousePos;
		var transP = _delegate.getTransP (_iPos, _iRow);
		if (RectTransformUtility.ScreenPointToWorldPointInRectangle (transP as RectTransform, data.position, data.pressEventCamera, out globalMousePos)) {
			transform.position = globalMousePos;
			if (transform.childCount > 3) {
				for (int i = 3; i < transform.childCount; i++) {
					transform.GetChild(i).localPosition = new Vector3 (0, 0 - 30 * (i - 2), 0);
				}
			}
		}
	}

	public void OnEndDrag(PointerEventData data){
		if (_bMove == false)
			return;
		var bDrop = _delegate.getBDropped (this, data.position);
		var transP = _delegate.getTransP (_iPos, _iRow);
		if (bDrop == false) {
			transform.SetParent (transP);
			transform.localPosition = new Vector3 (0, 0 - 30 * (transP.childCount - 1), 0);
			if (transform.childCount > 3) {
				for (int i = 3, len = transform.childCount; i < len; i++) {
					var item = transform.GetChild (3);
					item.SetParent (transP);
					item.localPosition = new Vector3 (0, 0 - 30 * (transP.childCount - 1), 0);
				}
			}
		}
		if (_iPos != 0) {
			var rect = gameObject.GetComponent<RectTransform>();
			rect.anchoredPosition = new Vector2 (rect.rect.width / 2, -rect.rect.height / 2);
		}
	}

	public void OnPointerClick(PointerEventData pointerEventData){
		string sSound = "click";
		if (_bMove == false || transform.parent == transMove) {
			adMgr.PlaySound (sSound);
			return;
		}
		addItems ();
		if (items.Count == 0 && (_delegate.getBMoveToDes (this) == true || _delegate.getBMoveToCard (this, true) == true
		    || _delegate.getBMoveToMedium (this) == true)) {
		} else if (_delegate.getBMoveToCard (this, true) == false) {
			sSound = "cMove";
			showMoveBack ();
		}
		adMgr.PlaySound (sSound);
	}

	public int getPos(){
		return _iPos;
	}

	public int getRow(){
		return _iRow;
	}

	public int getColor(){
		return _iColor;
	}

	public int getCardType(){
		return _iCardType;
	}

	public int getCardNum(){
		return _iCardNum;
	}

	public void setPos(int pos){
		_iPos = pos;
	}

	public void setRow(int iRow){
		_iRow = iRow;
	}

	public List<Transform> getItems(){
		return items;
	}

	public void addItems(){
		items.Clear ();
		var trans = _delegate.getTransP (0, _iRow);
		int len = trans.childCount;
		int iLine = 100;
		if (len > 1)
			for (var i = 0; i < len; i++) {
				if (trans.GetChild (i).transform == transform)
					iLine = i;
				if (i > iLine)
					items.Add (trans.GetChild (i));
			}
	}

	public void showFindColor(){
		GetComponent<Image> ().color = Color.gray;
	}

	public void resetColor(){
		GetComponent<Image> ().color = Color.white;
	}
}
