using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveMgrZpjl {
	List<CardZpjl> lPreCard = new List<CardZpjl> ();
	List<int> lPrePos = new List<int> ();
	List<int> lPreRow = new List<int> ();
	List<CardZpjl> lPreUpCard = new List<CardZpjl> ();
	Zpjl _delegate;

	public MoveMgrZpjl(Zpjl delt){
		_delegate = delt;
	}

	public void removeAll(){
		lPreCard.RemoveRange(0, lPreCard.Count);
		lPrePos.RemoveRange (0, lPrePos.Count);
		lPreRow.RemoveRange (0, lPreRow.Count);
		lPreUpCard.RemoveRange(0, lPreCard.Count);
	}

	public void onMoveCard(){
		int[] tIdx = {lPreCard.Count - 1, lPrePos.Count - 1, lPreRow.Count - 1, lPreUpCard.Count - 1};
		var preCard = lPreCard [tIdx [0]];
		var prePos = lPrePos [tIdx [1]];
		var preRow = lPreRow [tIdx [2]];
		var preUpCard = lPreUpCard [tIdx [3]];
		if (preUpCard != null) {
			preUpCard.showBg (true);
		}
		_delegate.moveCards (preCard, prePos, preRow);
		lPreCard.RemoveAt (tIdx [0]);
		lPrePos.RemoveAt (tIdx [1]);
		lPreRow.RemoveAt (tIdx [2]);
		lPreUpCard.RemoveAt (tIdx [3]);
	}

	public void addCard(CardZpjl card){
		var iPos = card.getPos ();
		var iRow = card.getRow ();
		CardZpjl upCardTemp = null;
		if (card.transform.parent == _delegate.goMove.transform) {
			var upTrans = _delegate.getTransP (iPos, iRow);
			var iLen = upTrans.childCount;
			if (iLen > 0) {
				var upCard = upTrans.GetChild (iLen - 1).GetComponent<CardZpjl> ();
				if (upCard.getBShowBg () == true) {
					upCardTemp = upCard;
				}
			}
		} else {
			var idx = card.transform.GetSiblingIndex ();
			if (idx > 0) {
				var upTrans = card.transform.parent;
				var upCard = upTrans.GetChild (idx - 1).GetComponent<CardZpjl> ();
				if (upCard.getBShowBg () == true) {
					upCardTemp = upCard;
				}
			}
		}
		lPreCard.Add(card);
		lPrePos.Add(iPos);
		lPreRow.Add(iRow);
		lPreUpCard.Add (upCardTemp);
	}

	public void addCard(MoveMgrZpjl moveMgr){
		var card = moveMgr.getPreCard ();
		var upCard = moveMgr.getPreUpcard ();
		lPreCard.Add(card);
		lPrePos.Add(card.getPos ());
		lPreRow.Add(card.getRow ());
		lPreUpCard.Add (upCard);
	}

	public CardZpjl getPreCard(){
		return lPreCard [lPreCard.Count - 1];
	}

	public int getCountCards(){
		return lPreCard.Count;
	}

	public CardZpjl getPreUpcard(){
		return lPreUpCard [lPreCard.Count - 1];
	}

	public int getPreRow(){
		return lPreRow [lPreRow.Count - 1];
	}
}
