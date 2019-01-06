using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveMgr {
	List<Card> lPreCard = new List<Card> ();
	List<int> lPrePos = new List<int> ();
	List<int> lPreRow = new List<int> ();
	Kdjl _delegate;

	public MoveMgr(Kdjl delt){
		_delegate = delt;
	}

	public void removeAll(){
		lPreCard.RemoveRange(0, lPreCard.Count);
		lPrePos.RemoveRange (0, lPrePos.Count);
		lPreRow.RemoveRange (0, lPreRow.Count);
	}

	public void onMoveCard(){
		int[] tIdx = {lPreCard.Count - 1, lPrePos.Count - 1, lPreRow.Count - 1};
		var preCard = lPreCard [tIdx [0]];
		var prePos = lPrePos [tIdx [1]];
		var preRow = lPreRow [tIdx [2]];
		_delegate.moveCards (preCard, prePos, preRow);
		lPreCard.RemoveAt (tIdx [0]);
		lPrePos.RemoveAt (tIdx [1]);
		lPreRow.RemoveAt (tIdx [2]);
	}

	public void addCard(Card card){
		lPreCard.Add(card);
		lPrePos.Add(card.getPos ());
		lPreRow.Add(card.getRow ());
	}

	public void addCard(MoveMgr moveMgr){
		var card = moveMgr.getPreCard ();
		lPreCard.Add(card);
		lPrePos.Add(card.getPos ());
		lPreRow.Add(card.getRow ());
	}

	public Card getPreCard(){
		return lPreCard [lPreCard.Count - 1];
	}

	public int getCountCards(){
		return lPreCard.Count;
	}
}
