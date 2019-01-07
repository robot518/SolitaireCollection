using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Mine : MonoBehaviour {
//	const int DX = 70;
//	const int DY = 70;
	int _iLine = 12;
	int _iRow = 9;
	int _iTotal;
	int _iMineCount;
//	int _iMode = 0;
	int _iMineIdx = -1;
//	float px;
//	float py;
	bool _bInit = false;
	bool _bWin = false;
	string _str;
 	List<CGrid> _tGrid = new List<CGrid>();
	List<int> _tMine = new List<int>();
	List<int> _tSearch = new List<int>();
	GrowUp _delt;
//	AudioMgr adMgr;
	SPMine spMine;

	// Use this for initialization
	void Start () {
//		initParas ();
//		initEvent ();
//		initShow ();
//		_bInit = true;
//		print (111);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void initParas(){
		_iRow = 9;
		_iLine = 12;
		_iTotal = _iRow * _iLine;
//		adMgr = AudioMgr.getInstance ();
		var tGrid = transform.Find ("goGrids");
		var iLen = tGrid.childCount;
		for (int i = 0; i < iLen; i++) {
			var cGrid = tGrid.GetChild(i).GetComponent<CGrid> ();
			cGrid.init (i, this);
			_tGrid.Add (cGrid); 
		}
//		spMine = new SPMine (_iTotal, _iRow);
	}

	void initEvent(){
		
	}

	void initShow(){

	}

	public void init(GrowUp delt, string str){
		if (_bInit == false) {
			initParas ();
			_bInit = true;
		}
		_delt = delt;
		_str = str;
		_tMine.Clear ();
		initSPMines();
		initLabs();
		for (var i = 0; i < _iTotal; i++) {
			var grid = _tGrid[i];
			_tGrid[i].showLab(grid.getNum ());
			grid.showBtn (true);
//			grid.showBtn (false);
//			grid.showFlag (false);
		};
//		showWin ();
	}

	public void showWin(){
		var bWin = true;
		for (int i = 0; i < _iTotal; i++) {
			var grid = _tGrid [i];
			bool bMine = grid.getMine (), bBtnShow = grid.getBBtnShow ();
			if (bMine == false && bBtnShow == true) {
				bWin = false;
				break;
			}
		}
		if (bWin == true) {
			showMines (0);
			_bWin = true;
			StartCoroutine (playResult ());
		}
	}

	void initSPMines(){
		if (spMine == null)
			spMine = new SPMine (_iTotal, _iRow);
		var tMineNum = spMine.getSPMine(_str);
		var idx = 0;
		for (var i = 0; i < _iTotal; i++) {
			if (tMineNum [i] == 1) {
				idx++;
				_tMine.Add(i);
				_tGrid [i].showMine (true);
				_tGrid [i].setNum (-1);
			} else {
				_tGrid [i].showMine (false);
				_tGrid [i].setNum (0);
			}
		};
		_iMineCount = idx;
//		showMineCount (_iMineCount);
	}

	public int getMineCount(){
		return _iMineCount;
	}

	void initLabs(){
		for (var i = 0; i < _iMineCount; i++) {
			var iMine = _tMine[i];
			var iCurLine = Mathf.Floor(iMine/_iRow);
			var iCurRow = iMine % _iRow;
			for (var iLine = 0; iLine < 3; iLine++) {
				for (var iRow = 0; iRow < 3; iRow++) {
					var iRowTemp = iCurRow - 1 + iRow;
					var iLineTemp = iCurLine - 1 + iLine;
					if (iRowTemp > -1 && iRowTemp < _iRow && iLineTemp > -1 && iLineTemp < _iLine){
						var idx = iMine + (iRow - 1) + (iLine * _iRow - _iRow);
						var grid = _tGrid [idx];
						if (grid.getMine() == false)
							grid.setNum (grid.getNum () + 1);
					}
				};
			};
		};
	}

	public void showMines(int idx){
		_iMineIdx = idx;
		for (int i = 0, iLen = _tMine.Count; i < iLen; i++) {
			_tGrid [_tMine [i]].showBtn (false);
		}
		_bWin = false;
		StartCoroutine (playResult ());
	}

	public void setTSearch(){
		_tSearch.Clear ();
	}

	public void showGrids(int idxNum){
		var iLabNum = _tGrid [idxNum].getNum ();
		if (iLabNum >= 0) {
			_tGrid [idxNum].showBtn (false);
			if (iLabNum == 0) {
				var iMine = idxNum;
				var iCurLine = Mathf.Floor (iMine / _iRow);
				var iCurRow = iMine % _iRow;
				for (var iLine = 0; iLine < 3; iLine++) {
					for (var iRow = 0; iRow < 3; iRow++) {
						var iRowTemp = iCurRow - 1 + iRow;
						var iLineTemp = iCurLine - 1 + iLine;
						if (iRowTemp > -1 && iRowTemp < _iRow && iLineTemp > -1 && iLineTemp < _iLine) {
							var idx = iMine + (iRow - 1) + (iLine * _iRow - _iRow);
							if (_tGrid [idx].getBBtnShow () == true && _tSearch.Contains (idx) == false) {
								_tSearch.Add (idx);
								showGrids (idx);
							}
						}
					}
				}
			}
		}
	}

	IEnumerator playResult(){
		if (_bWin == true) {
			for (int j = 0; j < 3; j++) {
				yield return new WaitForSeconds (0.3f);
				for (int i = 0; i < _iMineCount; i++) {
					_tGrid [_tMine [i]].showRedMine ();
				}
				yield return new WaitForSeconds (0.2f);
				for (int i = 0; i < _iMineCount; i++) {
					_tGrid [_tMine [i]].resetMine ();
				}
			}
			if (_iMineIdx != -1) {
				_tGrid [_iMineIdx].resetMine ();
				_iMineIdx = -1;
			}
			gameObject.SetActive (false);
			_delt.showMeng (_str);
		} else {
			yield return new WaitForSeconds (2.0f);
			if (_iMineIdx != -1) {
				_tGrid [_iMineIdx].resetMine ();
				_iMineIdx = -1;
			}
			gameObject.SetActive (false);
			_delt.setBMove (true);
		}
	}
}
