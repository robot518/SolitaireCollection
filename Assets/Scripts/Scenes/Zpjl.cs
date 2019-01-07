using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class Zpjl : MonoBehaviour, IMain
{
	Text labStep;
	Text labTime;
	Text labLeftCount;
	int _iStep;
	int _iTime;
	int _iLeftCount;
	Transform[] tTrans;
	int _iPrompt;
	GameObject goCard;
	List<CardZpjl> lCards = new List<CardZpjl> ();
	public GameObject goMove;
	List<int> lCardDatas = new List<int> ();
	List<int> lInitCardDatas = new List<int> ();
	Coroutine coPlayTime;
	float _px;
	float _py;
	float _plx;
	float _ply;
	float _prx;
	bool _bWin;
	const int IROWCOUNT = 7;
	const int ILINEDIS = 40;
	const int IROWDIS = 95;
    MoveMgrZpjl undoMgr;
	AudioMgr adMgr;
	List<CardZpjl> lFindCards = new List<CardZpjl>();
	List<int> lLeftClickStep = new List<int>();
	int _iDiff;

	// Use this for initialization
	void Start () {
		intParas ();
		initShow ();
		Invoke("onClickStart", 1.0f);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void intParas(){
		goMove = transform.Find ("goMove").gameObject;
		var goC = transform.Find ("goMain/img/ndCard1").gameObject;
		goC.transform.SetParent (goMove.transform);
		var pos = goC.GetComponent<RectTransform>().anchoredPosition;
		_px = pos.x;
		_py = pos.y;
		Destroy (goC);
		var goL = transform.Find ("goLeft/img/ndCard2").gameObject;
		goL.transform.SetParent (goMove.transform);
		var posL = goL.GetComponent<RectTransform> ().anchoredPosition;
		_ply = posL.y;
		_plx = posL.x;
		Destroy (goL);
		var goR = transform.Find ("goRight/img/ndCard3").gameObject;
		goR.transform.SetParent (goMove.transform);
		var posR = goR.GetComponent<RectTransform> ().anchoredPosition;
		_prx = posR.x;
		Destroy (goR);

		var goTop = transform.Find ("goTop");
		labTime = goTop.GetChild (1).GetComponent<Text> ();
		labStep = goTop.GetChild (2).GetComponent<Text> ();
		_iStep = 0;
		_iTime = 0;
		tTrans = new Transform[]{transform.Find ("goMain"), transform.Find ("goLeft"), transform.Find ("goRight")};
		_iPrompt = 0;
		goCard = transform.Find ("goMain/img/Card").gameObject;
		labLeftCount = transform.Find ("goLeft/lab").gameObject.GetComponent<Text>();
		_iLeftCount = 0;
		undoMgr = new MoveMgrZpjl(this);
		adMgr = AudioMgr.getInstance ();
	}

	void initShow(){
		initBtnEvents ();
		initGoCards ();
		initFinds ();
		showLabStep (_iStep);
		labTime.text = "00:00";
		labLeftCount.text = _iLeftCount.ToString();
	}

	void initBtnEvents(){
        transform.Find("goTop/back").GetComponent<Button>().onClick.AddListener(delegate
        {
            SceneManager.LoadScene("Login");
        });

        var btnMedium = transform.Find ("goLeft/btn").gameObject.GetComponent<Button>();
		btnMedium.onClick.AddListener (onClickLeft);

        var btns = transform.Find("goBtns");
        UnityAction[] tFunc = {onClickStart, onClickAuto, onClickPrompt, onClickUndo};
		for (var i = 0; i < tFunc.Length; i++){
			var btn = btns.GetChild(i).gameObject.GetComponent<Button>();
			btn.onClick.AddListener (tFunc[i]);
		}

		_iDiff = 1;
		var goChoice = transform.Find ("goTogs");
		var lLab = new List<Text> ();
		for (var i = 0; i < goChoice.childCount; i++){
			var idx = i;
			var trans = goChoice.GetChild (i);
			var tog = trans.GetComponent<Toggle>();
			lLab.Add (trans.GetChild (1).gameObject.GetComponent<Text> ());
			tog.onValueChanged.AddListener (
				delegate(bool isOn)
				{
					if (isOn == true){
						adMgr.PlaySound ("click");
						_iDiff = idx + 1;
//					}
						lLab[idx].color = new Color(0, 1, 1, 1);
					}else
						lLab[idx].color = Color.white;
				}
			);
		}
	}

	void initFinds(){
		var goFind = transform.Find ("goFinds/goFind").gameObject;
		for (int i = 0; i < 13; i++) {
			GameObject item;
			if (i == 0)
				item = goFind;
			else {
				item = Instantiate(goFind);
				item.transform.SetParent (goFind.transform.parent);
				item.transform.localScale = Vector3.one;
			}
			item.GetComponent<Find>().init (i, this);
		}
	}

	void initGoCards(){
		goCard.SetActive(false);
		for (int i = 0; i < 52; i++){
			GameObject item;
			if (i == 0)
				item = goCard;
			else {
				GameObject goCardTemp = Instantiate(goCard);
				goCardTemp.transform.SetParent(goMove.transform);
				goCardTemp.transform.localScale = Vector3.one;
				item = goCardTemp;
			}
			var card = item.GetComponent<CardZpjl> ();
			lCards.Add(card);
		}
	}

	void reset(){
		_bWin = false;
		_iPrompt = 0;
		_iStep = 0;
		showLabStep (_iStep);
		_iTime = 0;
		labTime.text = "00:00";
		_iLeftCount = 0;
		showLabLeftCount (0);
		undoMgr.removeAll ();
	}

	void onClickLeft(){
		var trans = getTransP (1, 0);
		if (trans.childCount > 0) {
			adMgr.PlaySound ("move");
			_iLeftCount--;
			showLabLeftCount (_iLeftCount);
			var card = trans.GetChild (trans.childCount - 1).GetComponent<CardZpjl> ();
			onDrop (card);
			card.setRow (1);
			card.transform.SetParent (goMove.transform);
			var iPosX = _plx + IROWDIS;
			var pos = card.transform.GetComponent<RectTransform> ().anchoredPosition;
			card.showMove (iPosX - pos.x, 0, iPosX);
		} else {
			lLeftClickStep.Add (_iStep);
			showLabStep (++_iStep);
			StartCoroutine (playLeft ());
		}
	}

	IEnumerator playLeft(){
		setTouchable (false);
		var idx = getTransP (1, 0).childCount > 0 ? 0 : 1;
		var trans = getTransP (1, idx);
		var iLen = trans.childCount;
		if (iLen > 0) {
			var iRow = 1 - idx;
			for (int i = 0; i < iLen; i++) {
				var card = trans.GetChild (trans.childCount - 1).GetComponent<CardZpjl> ();
				card.setRow (iRow);
				card.transform.SetParent (goMove.transform);
				var iPosX = _plx + IROWCOUNT * iRow;
				var pos = card.transform.GetComponent<RectTransform> ().anchoredPosition;
				card.showMove (iPosX - pos.x, 0, iPosX);
				adMgr.PlaySound ("move");
				yield return new WaitForSeconds (0.02f);
			}
		}
		_iLeftCount = iLen * idx;
		showLabLeftCount (_iLeftCount);
		setTouchable (true);
	}

	void onClickStart(){
		setTouchable (false);
		foreach (var card in lCards) {
			card.gameObject.SetActive (false);
			card.transform.SetParent (goMove.transform);
			card.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2 (_plx + 100, -100);
		}
//		for (int i = 0; i < 4; i++) {
//			for (int j = 0; j < 13; j++) {
//				var idx = j + 13 * i;
//				var card = lCards [idx];
//				card.init(j + 1, i + 1, 2, i, this);
//				card.transform.SetParent (getTransP (2, i));
//				card.transform.localPosition = Vector3.zero;
//			}
//		}
//		playWin ();
		initDataCards ();
		reset ();
		if (coPlayTime != null)
			StopCoroutine (coPlayTime);
		StartCoroutine (initRightShow());
	}

	void onClickAuto(){
		if (_bWin == true)
			return;
		StartCoroutine (playAutoMoveCards());
	}

	void onClickPrompt(){
		if (_bWin == true)
			return;
		var idx = _iPrompt;
		var preIdx = _iPrompt;
		var transP = tTrans [0];
		for (int i = 0; i < IROWCOUNT; i++) {
			var trans = transP.GetChild (i);
			var iLen = trans.childCount;
			if (iLen > 0) {
				for (int j = 0; j < iLen; j++) {
					var card = trans.GetChild (j).GetComponent<CardZpjl> ();
					if (card.getBShowBg() == false && getBMoveToCard (card, false) == true) {
						if (idx > 0) {
							idx--;
							continue;
						} else {
							if (_iPrompt > preIdx)
								return;
							_iPrompt++;
							card.showRotate ();
						}
					}
				}
			}
		}
		_iPrompt = 0;
	}

	void onClickUndo(){
		if (_iStep == 0 || _bWin == true)
			return;
		showLabStep (--_iStep);
		var bMove = lLeftClickStep.Remove (_iStep);
		if (bMove == true) {
			StartCoroutine (playLeft ());
		}else
			undoMgr.onMoveCard();
	}

	void initDataCards () {
		lInitCardDatas.Clear ();
		var iNum = 0;
		if (_iDiff < 3) {
			iNum = _iDiff == 1 ? 3 : 1;
			for (int i = 1; i < iNum + 1; i++) {
				for (int j = 1; j < 5; j++) {
					lInitCardDatas.Add (100 * j + i);
				}
			}
		}
		List<int> CARDS = new List<int> ();
		for (int i = 1; i < 5; i++) {
			for (int j = iNum + 1; j < 14; j++) {
				CARDS.Add (100 * i + j);
			}
		}
		lCardDatas.Clear ();
		while (true) {
			int iRandom = Random.Range (0, CARDS.Count - 1);
			int iCard = CARDS [iRandom];
			CARDS.RemoveAt(iRandom);
			lCardDatas.Add(iCard);
			if (CARDS.Count == 0) break;
		};
	}

	IEnumerator initRightShow(){
		yield return new WaitForSeconds (0.1f);
		var iLen = lInitCardDatas.Count;
		for (var i = 0; i < iLen; i++) {
			var iCard = lInitCardDatas [i];
			var iCardNum = iCard % 100;
			int iCardType = (int)Mathf.Floor (iCard / 100);
			CardZpjl card = lCards [lCards.Count - i - 1];
			card.init (iCardNum, iCardType, 2, iCardType - 1, this);
			var pos = card.gameObject.GetComponent<RectTransform> ().anchoredPosition;
			var iPosX = _prx + IROWDIS * iCardType - IROWDIS;
			card.showMove (iPosX - pos.x, _ply - pos.y, iPosX);
			adMgr.PlaySound ("move");
			yield return new WaitForSeconds (0.02f);
			if (i % 4 == 3)
				yield return new WaitForSeconds (0.3f);
		}
		StartCoroutine(initLeftShow());
	}

	IEnumerator initLeftShow(){
		yield return new WaitForSeconds (0.3f);
		var iLen = lCardDatas.Count;
		var iCount = 24 - lInitCardDatas.Count;
		for (var i = 0; i < iCount; i++) {
			var idx = iLen - 1 - i;
			var iCard = lCardDatas [idx];
			var iCardNum = iCard % 100;
			var iCardType = (int)Mathf.Floor (iCard / 100);
			CardZpjl card = lCards [idx];
			card.init (iCardNum, iCardType, 1, 0, this);
			var pos = card.gameObject.GetComponent<RectTransform> ().anchoredPosition;
			card.showMove (_plx - pos.x, _ply - pos.y, _plx);
			adMgr.PlaySound ("move");
			yield return new WaitForSeconds (0.02f);
			if (i % 4 == 3) {
				_iLeftCount+= 4;
				showLabLeftCount (_iLeftCount);
				yield return new WaitForSeconds (0.2f);
			}
		}
		StartCoroutine(playCards());
	}

	IEnumerator playAutoMoveCards(){
		setTouchable (false);
		var iTime = 0.25f;
		while (true) {
			var bMove = false;
			for (var i = 0; i < IROWCOUNT; i++) {
				var trans = getTransP (0, i);
				if (trans.childCount > 0) {
					if (getBMoveToDes (trans.GetChild (trans.childCount - 1).gameObject.GetComponent<CardZpjl> ()) == true) {						
						bMove = true;
						yield return new WaitForSeconds(iTime);
					}
				}
			}
			var transTemp = getTransP (1, 1);
			if (transTemp.childCount > 0 && getBMoveToDes (transTemp.GetChild(transTemp.childCount - 1).gameObject.GetComponent<CardZpjl> ()) == true) {
				bMove = true;
				yield return new WaitForSeconds(iTime);
			}
			if (bMove == false) {
				if (_bWin == false)
					setTouchable (true);
				yield break;
			}
		}
	}

	IEnumerator playCards () {
		yield return new WaitForSeconds (0.2f);
		adMgr.PlaySound ("start");
		var len = lCards.Count - 24;
		var idx = 0;
		for (var i = 0; i < IROWCOUNT; i++) {
			for (int j = i; j < IROWCOUNT; j++) {
				if (idx >= len)
					break;
				int iCard = lCardDatas [idx];
				int iCardNum = iCard % 100;
				int iCardType = (int)Mathf.Floor (iCard / 100);
				CardZpjl card = lCards [idx];
				card.init (iCardNum, iCardType, 0, j, this);

				var iPosX = _px + IROWDIS * j;
				var iPosY = _py - ILINEDIS * i;
				var pos = card.gameObject.GetComponent<RectTransform> ().anchoredPosition;
				card.showMove (iPosX - pos.x, iPosY - pos.y, iPosX);
				idx++;
			}
			yield return new WaitForSeconds (0.2f);
		}

		showInitCards ();
		yield return new WaitForSeconds (0.6f);
		onClickLeft ();
		coPlayTime = StartCoroutine (playTextTime());
		yield return new WaitForSeconds (0.3f);
		setTouchable (true);
	}

	void showInitCards(){
		Transform trans;
		int len;
		for (var i = 0; i < IROWCOUNT; i++){
			trans = getTransP(0, i).transform;
			len = trans.childCount - 1;
			for (var j = len; j >= 0; j--) {
				CardZpjl card = trans.GetChild (j).GetComponent<CardZpjl> ();
				if (j != len)
					card.showBg (true);
			}
		}
	}

	IEnumerator playTextTime(){
		while (true) {
			labTime.text = getStrTime (_iTime);
			_iTime++;
			if (_iTime > 3600){
				adMgr.PlaySound ("lose");
				Invoke ("onClickStart", 1.5f);
				yield break;
			}
			yield return new WaitForSeconds (1);
		}
	}

	string getStrTime(int iTime){
		string str = "";
		int[] time = new int[2];
		time[0] = (int)(iTime / 60);
		time[1] = iTime % 60;
		int v;
		for (int i = 0; i < time.Length; i++) {
			v = time [i];
			if (v < 10)
				str += "0" + v.ToString ();
			else
				str += v.ToString ();
			if (i == 0)
				str += ":";
		}

		return str;
	}

	void showLabStep(int iStep){
		labStep.text = iStep.ToString();
	}

	void showLabLeftCount(int iCount){
		labLeftCount.text = iCount.ToString();
	}

	public void onFindDown(int iCardNum){
		foreach (var card in lCards) {
			if (card.getCardNum() == iCardNum){
				lFindCards.Add (card);
				card.showFindColor ();
			}
		}
	}

	public void onFindUp(){
		foreach (var card in lFindCards) {
			card.resetColor ();
		}
		lFindCards.Clear ();
	}

	void setTouchable(bool bTouch){
		var control = GetComponent<CanvasGroup> ();
		control.blocksRaycasts = bTouch;
	}

	public bool getBDropped(CardZpjl card, Vector2 pos){
		for (var i = 0; i < tTrans.Length; i++) {
			if (i == 1)
				continue;
			var trans = tTrans [i];
			for (var j = 0; j < trans.childCount; j++) {
				var rect = trans.GetChild (j);
				if (i == 0 && rect.childCount > 0)
					rect = rect.GetChild (rect.childCount - 1);
				bool bIn = RectTransformUtility.RectangleContainsScreenPoint ((RectTransform)rect, pos);
				if (bIn == true) {
					if (card.getPos() == i && card.getRow() == j) return false;
					if (i == 0) 
						return getBSuitCard (card, j);
					else if (card.getItems().Count == 0) {
						if (i == 1 && rect.childCount == 0) {
							onCardMove (card, i, j);
							return true;
						}
						if (i == 2 && getBMoveToDes (card, j) == true) {
							return true;
						}
						return false;
					}
					else 
						return false;
				}
			}
		}

		return false;
	}

	void onDrop(CardZpjl card){
		showLabStep (++_iStep);
		undoMgr.addCard(card);
	}

	void onCardMove(CardZpjl card, int iPos, int iRow){
		onDrop (card);
		moveCards (card, iPos, iRow);
	}

	public void moveCards(CardZpjl card, int iPos, int iRow){
		adMgr.PlaySound ("move");
		bool bAutoMove = true;
		var preTrans = card.transform.parent;
		if (preTrans.gameObject == goMove) {
			bAutoMove = false;
		}
		if (bAutoMove == true && iPos == 0)
			card.addItems ();
		var items = card.getItems ();
		var prePos = card.getPos ();
		if (prePos == 0) {
			if (bAutoMove == true) {
				if (preTrans.childCount - items.Count > 1)
					preTrans.GetChild (preTrans.childCount - 2 - items.Count).GetComponent<CardZpjl> ().showBg (false);
			} else {
				preTrans = getTransP (card.getPos (), card.getRow ());
				if (preTrans.childCount > 0)
					preTrans.GetChild (preTrans.childCount - 1).GetComponent<CardZpjl> ().showBg (false);
			}
		}
		var trans = getTransP(iPos, iRow);
		card.setPos (iPos);
		card.setRow (iRow);
		card.transform.SetParent (trans);
		if (iPos != 0) {
			if (iPos == 1 && iRow == 0) {
				_iLeftCount++;
				showLabLeftCount (_iLeftCount);
			}
//			setBMove ();
			showWin ();
			if (bAutoMove == false) {
				var rect = card.gameObject.GetComponent<RectTransform> ();
				rect.anchoredPosition = new Vector2 (rect.rect.width / 2, -rect.rect.height / 2);
			} else {
				card.transform.SetParent (goMove.transform);
				var px = iPos == 1 ? (_plx + IROWDIS * iRow) : (_prx + IROWDIS * iRow);
				var pos = card.gameObject.GetComponent<RectTransform> ().anchoredPosition;
				card.showMove (px - pos.x, _ply - pos.y, px);
			}
		} else {
			if (items.Count > 0) {
				foreach (Transform item in items) {
					var cCard = item.gameObject.GetComponent<CardZpjl> ();
					cCard.setPos (iPos);
					cCard.setRow (iRow);
					item.SetParent (trans);
				}
			}
//			setBMove ();
			if (bAutoMove == false) {
				card.transform.localPosition = new Vector3 (0, 0 - ILINEDIS * (trans.childCount - 1 - items.Count), 0);
				if (items.Count > 0) {
					var idx = 0;
					foreach (Transform item in items) {
						idx++;
						item.localPosition = new Vector3(0, 0 - ILINEDIS * (trans.childCount - 1 + idx - items.Count), 0);
					}
				}
			} else {
				card.transform.SetParent (goMove.transform);
				var iPosX = _px + IROWDIS * iRow;
				var iPosY = _py - ILINEDIS * (trans.childCount - items.Count - 1 + 1);
				var pos = card.gameObject.GetComponent<RectTransform> ().anchoredPosition;
				if (items.Count > 0) {
					foreach (Transform item in items) {
						item.SetParent (card.transform);
					}
				}
				card.showMove (iPosX - pos.x, iPosY - pos.y, iPosX);
			}
		}
	}

	bool getBSuitCard(CardZpjl cardDown, int iRow){
		var iPos = 0;
		var rect = getTransP(iPos, iRow);
		if (rect.childCount == 0) {
			onCardMove (cardDown, iPos, iRow);
			return true;
		} else {
			var cardUp = rect.GetChild (rect.childCount - 1).gameObject.GetComponent<CardZpjl> ();
			if (cardDown.getColor () != cardUp.getColor () && cardUp.getCardNum () - cardDown.getCardNum () == 1) {
				onCardMove (cardDown, iPos, iRow);
				return true;
			}
		}
		return false;
	}

	public bool getBMoveToMedium(CardZpjl card){
		var iPos = 1;
		var trans = tTrans [iPos];
		for (var i = 0; i < trans.childCount; i++) {
			var transTemp = trans.GetChild (i);
			if (transTemp.childCount == 0) {
				onCardMove (card, iPos, i);
				return true;
			}
		}
		return false;
	}

	public bool getBMoveToDes(CardZpjl card){
		for (var i = 0; i < tTrans[2].childCount; i++) {
			if (getBMoveToDes (card, i) == true) {
				return true;
			}
		}
		return false;
	}

	bool getBMoveToDes (CardZpjl card, int iRow) {
		var transTemp = getTransP(2, iRow);
		if (card.getCardType () - 1 == iRow) {
			if ((transTemp.childCount == 1 && card.getCardNum () == 1) ||
				(transTemp.childCount > 1 && card.getCardNum () - 1 == transTemp.GetChild (transTemp.childCount - 1).gameObject.GetComponent<CardZpjl> ().getCardNum ())) {
				onCardMove (card, 2, iRow);
				return true;
			}
		}
		return false;
	}

	public bool getBMoveToCard (CardZpjl card, bool bMove) {
		var iPos = 0;
		var transP = tTrans [iPos];
		for (var i = 0; i < transP.childCount; i++) {
			if (card.getPos() == 0 && card.getRow() == i) continue;
			var rect = transP.GetChild (i);
			if (rect.childCount > 0) {
				var cardUp = rect.GetChild (rect.childCount - 1).gameObject.GetComponent<CardZpjl> ();
				if (card.getColor () != cardUp.getColor () && cardUp.getCardNum () - card.getCardNum () == 1) {
					if (bMove == true)
						onCardMove (card, iPos, i);
					return true;
				}
			}
		}
		for (var i = 0; i < transP.childCount; i++) {
			if (card.getPos() == 0 && card.getRow() == i) continue;
			if (transP.GetChild (i).childCount == 0) {
				if (bMove == true)
					onCardMove (card, iPos, i);
				return true;
			}
		}
		return false;
	}

    public Transform getTransMove()
    {
        return goMove.transform;
    }

    public Transform getTransP(int iPos, int iRow)
    {
        return tTrans[iPos].GetChild(iRow);
    }

    void showWin(){
		var iPos = 2;
		var trans = tTrans [iPos];
		var bWin = true;
		for (var i = 0; i < trans.childCount; i++) {
			var transTemp = trans.GetChild (i);
			if (transTemp.childCount != 14) {
				bWin = false;
				break;
			}
		}
		if (bWin == true) {
			_bWin = true;
			setTouchable (false);
			adMgr.PlaySound ("win");
			StopCoroutine (coPlayTime);
			Invoke ("playWin", 1.0f);
		}
	}

	void playWin(){
		StartCoroutine (playWinCards());
	}

	IEnumerator playWinCards(){
		var transP = tTrans [2];
		for (int j = 1, len = 14; j < len; j++) {
//			adMgr.PlaySound ("move");
			for (int i = 0; i < transP.childCount; i++) {
				var trans = transP.GetChild (i);
				var item = trans.GetChild (len - j);
				item.SetParent (goMove.transform);
				item.SetSiblingIndex (0);
				var card = item.gameObject.GetComponent<CardZpjl> ();
				var iPosX = _plx + 100;
				var pos = card.transform.GetComponent<RectTransform> ().anchoredPosition;
				card.playWinMove (iPosX - pos.x, -100 - pos.y, iPosX);
//				yield return new WaitForSeconds (0.05f);
			}
			yield return new WaitForSeconds (0.3f);
		}
		yield return new WaitForSeconds (5.0f);
		setTouchable (true);
	}
}
