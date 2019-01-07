using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPMine {
	int _iTotal;
	int _iRow;

	public SPMine(int iTotal, int iRow){
		_iTotal = iTotal;
		_iRow = iRow;
	}

	public int[] getSPMine(string str){
		if (str == "sa")
			return getMine1 ();
		else if (str == "sb")
			return getMine2 ();
		else 
			return getMine3 ();
	}

	//旋转90度的艹
	int[] getMine1(){
		var iTotal = _iTotal;
		var iRow = _iRow;
		var tMineNum = new int[iTotal];
		for (var i = 0; i < iTotal; i++) {
			var iLine = (int)Mathf.Floor (i / iRow) + 1;
			var iR = i % iRow;
			if (iR == 4)
				tMineNum [i] = 1;
			else if (iLine == 4 || iLine == 9)
				tMineNum [i] = 1;
			else
				tMineNum [i] = 0;
		};
		return tMineNum;
	}

	//日
	int[] getMine2(){
		var iTotal = _iTotal;
		var iRow = _iRow;
		var tMineNum = new int[iTotal];
		for (var i = 0; i < iTotal; i++) {
			var iLine = (int)Mathf.Floor (i / iRow) + 1;
			var iR = i % iRow;
			if (iR == 0 || iLine == 1 || iR == 8 || iLine == 12 || iLine == 6)
				tMineNum [i] = 1;
			else
				tMineNum[i] = 0;
		};
		return tMineNum;
	}

	//月
	int[] getMine3(){
		var iTotal = _iTotal;
		var iRow = _iRow;
		var tMineNum = new int[iTotal];
		for (var i = 0; i < iTotal; i++) {
			var iLine = (int)Mathf.Floor (i / iRow) + 1;
			var iR = i % iRow;
			if (iR > 2 && iLine < 9) {
				if (iR == 3 || iR == 8 || iLine == 1 || iLine == 8 || iLine == 4)
					tMineNum [i] = 1;
				else
					tMineNum [i] = 0;
			} else {
				if (iR + iLine == 12 || iR == 8 || (iR > 8 && iLine == 12))
					tMineNum [i] = 1;
				else 
					tMineNum[i] = 0;
			}
		};
		return tMineNum;
	}
}
