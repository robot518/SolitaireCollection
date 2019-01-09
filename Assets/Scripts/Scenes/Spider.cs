using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class Spider : MonoBehaviour,IMain 
{
	public Text textStep;
	public Text textTime;
	public GameObject goCard;
	public GameObject goFind;
	public GameObject goMove;
	Transform goChoice;
	List<CardSpider> lCards = new List<CardSpider>();
	List<CardSpider> lFindCards = new List<CardSpider>();
	List<int> lCardDatas = new List<int>();
	List<int> lMediumClickStep = new List<int>();
	List<int> lDesStep = new List<int>();
	List<int> lDesRow = new List<int>();
	List<bool> lDesBShow = new List<bool>();
//	List<int> lRedoMediumClickStep = new List<int>();
	Transform[] tTrans;
	int _iStep;
	int _iTime;
	Coroutine coPlayTime;
	AudioMgr adMgr;
	MoveMgrSpider undoMgr;
//	MoveMgrSpider redoMgr;
	LangMgr langMgr;
	int _iDiff;
	float _px;
	float _py;
	float _ply;
	float _plx;
	float _prx;
	const int IROWCOUNT = 10;
	const int IROWDIS = 65;
	int _iWinCount;
	Transform _transWin;
	int _iMediumCount = 5;
	int _iPrompt;

	// Use this for initialization
	void Start () {
		initParas ();
		initShow ();
		Invoke("onClickStart", 1.0f);
	}

	// Update is called once per frame
	void Update () {
		if ( Application.platform == RuntimePlatform.Android &&(Input.GetKeyDown(KeyCode.Escape)))
		{
			Application.Quit ();
		}
	}

	void initParas(){
		var goC = transform.Find ("ndH/ndV/ndCard1").gameObject;
		goC.transform.SetParent (goMove.transform);
		var pos = goC.GetComponent<RectTransform>().anchoredPosition;
		_px = pos.x;
		_py = pos.y;
		Destroy (goC);
		var goL = transform.Find ("goLeft/ndCard/ndCard3").gameObject;
		goL.transform.SetParent (goMove.transform);
		var posL = goL.GetComponent<RectTransform> ().anchoredPosition;
		_ply = posL.y;
		_plx = posL.x;
		Destroy (goL);
		var goR = transform.Find ("goRight/ndCard/ndCard2").gameObject;
		goR.transform.SetParent (goMove.transform);
		var posR = goR.GetComponent<RectTransform> ().anchoredPosition;
		_prx = posR.x;
		Destroy (goR);

		undoMgr = new MoveMgrSpider (this);
//		redoMgr = new MoveMgrSpider (this);
		adMgr = AudioMgr.getInstance ();
		langMgr = LangMgr.getInstance ();
		tTrans = new Transform[]{transform.Find ("ndH"), transform.Find ("goRight"), transform.Find ("goLeft")};
		goChoice = transform.Find ("goToggles");
		reset ();
	}

	void initShow(){
		initBtnEvents ();
		initGoCards ();
		initFinds ();
		showTexts ();
	}

	public float getPLX(){
		return _plx;
	}

	void reset(){
		_iPrompt = 0;
		_iWinCount = 0;
		_iMediumCount = 5;
		_iStep = 0;
		showTextStep (_iStep);
		_iTime = 0;
		textTime.text = "00:00";
		undoMgr.removeAll ();
//		redoMgr.removeAll ();
		lMediumClickStep.Clear();
//		lRedoMediumClickStep.RemoveRange(0, lRedoMediumClickStep.Count);
		lDesStep.RemoveRange(0, lDesStep.Count);
		lDesRow.RemoveRange(0, lDesRow.Count);
		lDesBShow.RemoveRange (0, lDesBShow.Count);
	}

	void initBtnEvents(){
        transform.Find("goTop/back").GetComponent<Button>().onClick.AddListener(delegate
        {
            SceneManager.LoadScene("Login");
        });

        var btns = transform.Find("goBtns");
        UnityAction[] tFunc = {onClickStart, onClickPrompt, onClickUndo};
		for (var i = 0; i < tFunc.Length; i++){
			var btn = btns.GetChild(i).gameObject.GetComponent<Button>();
			btn.onClick.AddListener (tFunc[i]);
		}

		_iDiff = 1;
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
						lLab[idx].color = new Color(0, 1, 1, 1);
					}else
						lLab[idx].color = Color.white;
				}
			);
		}

		var btnMedium = transform.Find ("goRight/btn").gameObject.GetComponent<Button>();
		btnMedium.onClick.AddListener (onClickMedium);
	}

	void initFinds(){
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

	void onClickMedium(){
		if (_iMediumCount > 0){
			setTouchable (false);
			adMgr.PlaySound ("click");
			_iMediumCount--;
			showTextStep (++_iStep);
			lMediumClickStep.Add (_iStep);
			var trans = getTransP (1, _iMediumCount);
			int len = trans.childCount;
			for (int i = len - 1; i >= 0; i--) {
				var idx = len - i - 1;
				var card = trans.GetChild (i).GetComponent<CardSpider> ();
				card.setPos (0);
				card.setRow (idx);
				card.transform.SetParent (goMove.transform);
				var iPosX = _px + 60 * idx;
				var iLine = getTransP (0, idx).childCount;
				var iPosY = _py - 30 * iLine;
				var pos = card.gameObject.GetComponent<RectTransform> ().anchoredPosition;
				card.showMove (iPosX - pos.x, iPosY - pos.y, iPosX);
			}
			Invoke ("setBMove", 0.5f);
		}
	}

	void setBMove(){
		var transP = tTrans [0];
		for (int i = 0; i < IROWCOUNT; i++) {
			var trans = transP.GetChild (i);
			var len = trans.childCount;
			if (len > 0) {
				CardSpider preCard = null;
				for (int j = len - 1; j >= 0; j--) {
					var card = trans.GetChild (j).GetComponent<CardSpider> ();
					if (card.getBShowBg() == false && (j == len - 1 
						|| (preCard.getBMove() == true && preCard.getCardType () == card.getCardType () && preCard.getCardNum () + 1 == card.getCardNum ())))
						card.setBMove (true);
					else {
						card.setBMove(false);
					}
					preCard = card;
				}
			}
		}
		setTouchable (true);
	}

	void onClickStart () {
//		StartCoroutine (playWinCards());
		setTouchable (false);
		foreach (var card in lCards) {
			card.gameObject.SetActive (false);
			card.transform.SetParent (goMove.transform);
			card.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2 (0, -100);
		}
		if (_iDiff == 3)
			initDataCardsDiff ();
		else
			initDataCards ();
		reset ();
		if (coPlayTime != null)
			StopCoroutine (coPlayTime);
		StartCoroutine (playCards());
	}

	void onClickUndo(){
		if (_iStep == 0 || _iWinCount >= 8)
			return;
		var bMove = lMediumClickStep.Remove (_iStep);
		if (bMove == true) {
			setTouchable (false);
			_iMediumCount++;
			var trans = getTransP (1, _iMediumCount - 1);
			int len = trans.childCount;
			for (int i = IROWCOUNT - 1; i >= 0; i--) {
				var idx = IROWCOUNT - i - 1;
				var transP = getTransP (0, idx);
				var card = transP.GetChild (transP.childCount - 1).GetComponent<CardSpider> ();
				card.setPos (1);
				card.setRow (_iMediumCount - 1);
				card.transform.SetParent (goMove.transform);
				var iPosX = _prx - 30 * (5 - _iMediumCount);
				var pos = card.gameObject.GetComponent<RectTransform> ().anchoredPosition;
				card.showMove (iPosX - pos.x, _ply - pos.y, iPosX);
			}
//			lRedoMediumClickStep.Add (_iStep);
			Invoke ("setBMove", 0.5f);
		} else {
//			redoMgr.addCard (undoMgr);
			bMove = lDesStep.Remove (_iStep);
			if (bMove == true) {
				setTouchable (false);
				_iWinCount--;
				var idx = lDesRow.Count - 1;
				var iRow = lDesRow [idx];
				lDesRow.RemoveAt (idx);
				var transP = tTrans [2].GetChild (_iWinCount);
				var iLenTemp = transP.childCount;
				var card = undoMgr.getPreCard ();
				var iCardNum = card.getCardNum ();
				var cCard = transP.GetChild (iLenTemp - 1).GetComponent<CardSpider> ();
				for (int i = iLenTemp - 2; i >= 1; i--) {
					var cardTemp = transP.GetChild (i).GetComponent<CardSpider> ();
					if (cardTemp.getCardNum () > iCardNum) {
						cardTemp.transform.SetParent (cCard.transform);
						cardTemp.setPos (0);
						cardTemp.setRow (iRow);
					} else if (cardTemp.getCardNum () < iCardNum) {
						cardTemp.transform.SetParent (card.transform);
						cardTemp.setPos (0);
						cardTemp.setRow (undoMgr.getPreRow ());
					}
				}
				moveCards(cCard, 0, iRow);
				var transPre = getTransP (0, iRow);
				if (transPre.childCount > 0) {
					var preCard = transPre.GetChild (transPre.childCount - 1).GetComponent<CardSpider> ();
					var idxBShow = lDesBShow.Count - 1;
					preCard.showBg (lDesBShow[idxBShow]);
					lDesBShow.RemoveAt (idxBShow);
					preCard.setBMove (false);
				}
				Invoke("playUndoMgr", 0.2f);
			} else
				undoMgr.onMoveCard();
		}
		showTextStep (--_iStep);
	}

	void playUndoMgr(){
		if (_iWinCount >= 8)
			return;
		undoMgr.onMoveCard ();
		setTouchable (true);
	}

//	void onClickRedo(){
//		var bMove = lRedoMediumClickStep.Remove (_iStep + 1);
//		if (bMove == true) {
//			onClickMedium ();
//		} else {
//			if (redoMgr.getCountCards () > 0) {
//				showTextStep (++_iStep);
//				undoMgr.addCard (redoMgr);
//				redoMgr.onMoveCard ();
//			}
//		}
//	}

	void onClickPrompt(){
		if (_iWinCount >= 8)
			return;
		var idx = _iPrompt;
		var preIdx = _iPrompt;
		var transP = tTrans [0];
		for (int i = 0; i < IROWCOUNT; i++) {
			var trans = transP.GetChild (i);
			var iLen = trans.childCount;
			if (iLen > 0) {
				for (int j = 0; j < iLen; j++) {
					var card = trans.GetChild (j).GetComponent<CardSpider> ();
					if (card.getBMove() == true && getBMoveToCard (card, false) == true) {
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

	IEnumerator playTextTime(){
		while (true) {
			textTime.text = getStrTime (_iTime);
			_iTime++;
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

	void showTextStep(int iStep){
		textStep.text = iStep.ToString();
	}

	void initGoCards () {
		goCard.SetActive(false);
		for (int i = 0; i < 104; i++){
			GameObject item;
			if (i == 0)
				item = goCard;
			else {
				GameObject goCardTemp = Instantiate(goCard);
				goCardTemp.transform.SetParent(goMove.transform);
				goCardTemp.transform.localScale = Vector3.one;
				item = goCardTemp;
			}
			var card = item.GetComponent<CardSpider> ();
			lCards.Add(card);
		}
	}

	void initDataCards () {
		List<int> CARDS = new List<int> ();
		if (_iDiff == 1) {
			int iCardType = 4;
			for (int i = 0; i < 8; i++) {
				for (int j = 0; j < 13; j++) {
					CARDS.Add (100 * iCardType + j + 1);
				}
			}
		} else if (_iDiff == 2){
			int iCardType = 4;
			for (int i = 0; i < 8; i++) {
				if (i == 4) {
					iCardType = 3;
				}
				for (int j = 0; j < 13; j++) {
					CARDS.Add (100 * iCardType + j + 1);
				}
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
		StartCoroutine(initMediumShow());
	}

	void initDataCardsDiff () {
		List<int> CARDS = new List<int> {
			101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113,
			201, 202, 203, 204, 205, 206, 207, 208, 209, 210, 211, 212, 213,
			301, 302, 303, 304, 305, 306, 307, 308, 309, 310, 311, 312, 313,
			401, 402, 403, 404, 405, 406, 407, 408, 409, 410, 411, 412, 413,
			101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113,
			201, 202, 203, 204, 205, 206, 207, 208, 209, 210, 211, 212, 213,
			301, 302, 303, 304, 305, 306, 307, 308, 309, 310, 311, 312, 313,
			401, 402, 403, 404, 405, 406, 407, 408, 409, 410, 411, 412, 413,
		};
		lCardDatas.Clear ();
		while (true) {
			int iRandom = Random.Range (0, CARDS.Count - 1);
			int iCard = CARDS [iRandom];
			CARDS.RemoveAt(iRandom);
			lCardDatas.Add(iCard);
			if (CARDS.Count == 0) break;
		};
		StartCoroutine(initMediumShow());
	}

	IEnumerator initMediumShow(){
		var iLen = lCardDatas.Count;
		for (int i = 0; i < 5; i++) {
			for (int j = 0; j < 10; j++) {
				int idx = iLen - 1 - (j + 10 * i);
				int iCard = lCardDatas [idx];
				int iCardNum = iCard % 100;
				int iCardType = (int)Mathf.Floor (iCard / 100);
				int iRow = i;
				CardSpider card = lCards [idx];
				card.init (iCardNum, iCardType, 1, iRow, this);
				var iPosX = _prx - 30 * (4 - i);
				card.transform.localPosition = new Vector3(iPosX - 30, _ply, 0);
				var pos = card.gameObject.GetComponent<RectTransform> ().anchoredPosition;
				card.showMove (iPosX - pos.x, 0, iPosX);
			}
			yield return new WaitForSeconds (0.2f);
		}
	}

	IEnumerator playCards () {
		yield return new WaitForSeconds (1.2f);
		adMgr.PlaySound ("start");
		int len = lCardDatas.Count - 50;
		for (var i = 0; i < 6; i++) {
			for (int j = 0; j < IROWCOUNT; j++) {
				int idx = j + IROWCOUNT * i;
				if (idx >= len)
					break;
				int iCard = lCardDatas [idx];
				int iCardNum = iCard % 100;
				int iCardType = (int)Mathf.Floor (iCard / 100);
				CardSpider card = lCards [idx];
				card.init (iCardNum, iCardType, 0, j, this);

				var iPosX = _px + 60 * j;
				var iPosY = _py - 30 * i;
				var pos = card.gameObject.GetComponent<RectTransform> ().anchoredPosition;
				card.showMove (iPosX - pos.x, iPosY - pos.y, iPosX);
			}
			yield return new WaitForSeconds (0.2f);
		}

		showInitCards ();
		coPlayTime = StartCoroutine (playTextTime());
		yield return new WaitForSeconds (0.5f);
		setTouchable (true);
	}

	void showInitCards(){
		Transform trans;
		int len;
		for (var i = 0; i < IROWCOUNT; i++){
			trans = getTransP(0, i).transform;
			len = trans.childCount - 1;
			for (var j = len; j >= 0; j--) {
				CardSpider card = trans.GetChild (j).GetComponent<CardSpider> ();
				if (j == len)
					card.setBMove (true);
				else
					card.showBg (true);
			}
		}
	}

	public bool getBDropped(CardSpider card, Vector2 pos){
		int i = 0;
		var trans = tTrans [i];
		for (var j = 0; j < trans.childCount; j++) {
			var rect = trans.GetChild (j);
			if (i == 0 && rect.childCount > 0)
				rect = rect.GetChild (rect.childCount - 1);
			bool bIn = RectTransformUtility.RectangleContainsScreenPoint ((RectTransform)rect, pos);
			if (bIn == true) {
//				if (card.getPos() == i && card.getRow() == j) return false;
				if (i == 0) 
					return getBSuitCard (card, j);
			}
		}

		return false;
	}

	void onDrop(CardSpider card){
		showTextStep (++_iStep);
		undoMgr.addCard(card);
//		redoMgr.removeAll ();
	}

	void onCardMove(CardSpider card, int iPos, int iRow){
		onDrop (card);
		moveCards (card, iPos, iRow);
	}

	public void moveCards(CardSpider card, int iPos, int iRow){
		adMgr.PlaySound ("move");
		bool bAutoMove = true;
		if (card.transform.parent.gameObject == goMove) {
			bAutoMove = false;
		}
		if (bAutoMove == true && iPos == 0)
			card.addItems ();
		var items = card.getItems ();
		Transform upTrans;
		if (bAutoMove == true) {
			var cIdx = card.transform.GetSiblingIndex ();
			if (cIdx > 0 && card.getPos() == 0) {
				upTrans = card.transform.parent;
				var upCard = upTrans.GetChild (cIdx - 1).GetComponent<CardSpider> ();
				if (upCard.getBShowBg () == true) {
					upCard.showBg (false);
				}
			}
		} else {
			upTrans = getTransP (card.getPos(), card.getRow());
			var iLen = upTrans.childCount;
			if (iLen > 0) {
				var upCard = upTrans.GetChild (iLen - 1).GetComponent<CardSpider> ();
				if (upCard.getBShowBg () == true) {
					upCard.showBg (false);
				}
			}
		}
		var trans = getTransP(iPos, iRow);
		card.setPos (iPos);
		card.setRow (iRow);
		card.transform.SetParent (trans);
		if (iPos == 0) {
			if (items.Count > 0) {
				foreach (Transform item in items) {
					var itemCard = item.GetComponent<CardSpider>();
					itemCard.setPos (iPos);
					itemCard.setRow (iRow);
					item.SetParent (trans);
				}
			}
			_transWin = trans;
			setBMove ();
			showWin ();
			if (bAutoMove == false) {
				card.transform.localPosition = new Vector3 (0, 0 - 30 * (trans.childCount - 1 - items.Count), 0);
				if (items.Count > 0) {
					var idx = 0;
					foreach (Transform item in items) {
						idx++;
						item.localPosition = new Vector3(0, 0 - 30 * (trans.childCount - 1 - items.Count + idx), 0);
					}
				}
			} else {
				card.transform.SetParent (goMove.transform);
				var iPosX = _px + IROWDIS * iRow;
				var iPosY = _py - 30 * (trans.childCount - items.Count);
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

	bool getBSuitCard(CardSpider cardDown, int iRow){
		var iPos = 0;
		var rect = getTransP(iPos, iRow);
		if (rect.childCount == 0) {
			onCardMove (cardDown, iPos, iRow);
			return true;
		} else {
			var cardUp = rect.GetChild (rect.childCount - 1).gameObject.GetComponent<CardSpider> ();
			if (cardUp.getCardNum () - cardDown.getCardNum () == 1) {
				onCardMove (cardDown, iPos, iRow);
				return true;
			}
		}
		return false;
	}

	public bool getBMoveToCard (CardSpider card, bool bMove) {
		var iPos = 0;
		var transP = tTrans [iPos];
		for (var i = 0; i < transP.childCount; i++) {
			if (card.getPos() == 0 && card.getRow() == i) continue;
			var rect = transP.GetChild (i);
			if (rect.childCount > 0) {
				var cardUp = rect.GetChild (rect.childCount - 1).gameObject.GetComponent<CardSpider> ();
				if (cardUp.getCardNum () - card.getCardNum () == 1) {
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
		return tTrans [iPos].GetChild (iRow);
	}

	void showWin(){
		var trans = _transWin;
		var iLen = trans.childCount;
		if (iLen >= 13) {
			int iCount = 0;
			bool bWin = true;
			CardSpider preCard = null;
			for (var i = iLen - 1; i >= 0; i--) {
				var card = trans.GetChild (i).GetComponent<CardSpider> ();
				if (card.getBShowBg () == true)
					break;
				if (i == iLen - 1) {
					if (card.getCardNum () == 1)
						iCount++;
					else
						bWin = false;
				} else {
					if (preCard.getCardType () == card.getCardType () && preCard.getCardNum () + 1 == card.getCardNum ())
						iCount++;
					else
						bWin = false;
				}
				if (bWin == false)
					break;
				preCard = card;
			}
			if (iCount == 13) {
				adMgr.PlaySound ("score");
				_transWin = trans;
				lDesStep.Add (_iStep);
				lDesRow.Add (trans.GetSiblingIndex ());
                if (Global.bWinPlay)
                {
                    setTouchable(false);
                    Invoke("playWin", 0.5f);
                }
            }
		}
	}

	void playWin(){
		StartCoroutine (playTransCards());
	}

	IEnumerator playTransCards(){
		var trans = _transWin;
		int idx = 0;
		for (var i = trans.childCount - 1; i >= 0; i--) {
			adMgr.PlaySound ("move");
			idx++;
			var card = trans.GetChild (i).GetComponent<CardSpider> ();
			card.setPos (2);
			card.setRow (_iWinCount);
//			card.setBMove (false);
			card.transform.SetParent(goMove.transform);
			var px = _plx + (IROWDIS - 5) * _iWinCount;
			var pos = card.gameObject.GetComponent<RectTransform> ().anchoredPosition;
			card.showMove (px - pos.x, _ply - pos.y, px);
			yield return new WaitForSeconds (0.1f);
			if (idx >= 13)
				break;
		}
		if (trans.childCount > 0) {
			var card = trans.GetChild (trans.childCount - 1).GetComponent<CardSpider> ();
			var bShow = card.getBShowBg ();
			lDesBShow.Add (bShow);
			if (bShow == true)
				card.showBg (false);
			card.setBMove (true);
		}
		_iWinCount++;
		if (_iWinCount < 8)
			setTouchable (true);
		else {
			adMgr.PlaySound ("win");
			StartCoroutine (playWinCards());
		}
	}

	IEnumerator playWinCards(){
//		for (int i = 0; i < 104; i++) {
//			var idx = (int)Mathf.Floor (i / 13);
////			print ("idx = " + idx);
//			lCards [i].transform.SetParent (transP.GetChild(idx));
//			lCards [i].gameObject.SetActive (true);
////			lCards [i].transform.localPosition = new Vector3 (_plx + 60 * idx, _ply, 0);
//			lCards [i].transform.localPosition = Vector3.zero;
//		}
		yield return new WaitForSeconds (0.2f);
		var transP = tTrans [2];
		for (int iTemp = 0; iTemp < 13; iTemp++) {
			adMgr.PlaySound ("move");
			for (int i = 0; i < 8; i++) {
				var trans = transP.GetChild (i);
				var card = trans.GetChild (trans.childCount - 1).GetComponent<CardSpider> ();
				card.transform.SetParent (goMove.transform);
				
				card.setPos (-1);
				var pos = card.transform.GetComponent<RectTransform> ().anchoredPosition;
				card.showMove (-pos.x, -100 - pos.y, 0);
				yield return new WaitForSeconds (0.1f);
			}
			yield return new WaitForSeconds (0.2f);
		}
		yield return new WaitForSeconds (21.0f);
		setTouchable (true);
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

	void showTexts(){
		transform.Find ("goTop/labTime/lab").gameObject.GetComponent<Text> ().text = langMgr.getValue ("time") + ": ";
		transform.Find ("goTop/labStep/lab").gameObject.GetComponent<Text> ().text = langMgr.getValue ("step") + ": ";
		transform.Find ("goBtns/btnStart/Text").gameObject.GetComponent<Text> ().text = langMgr.getValue ("start");
		transform.Find ("goBtns/btnAuto/Text").gameObject.GetComponent<Text> ().text = langMgr.getValue ("prompt");
		transform.Find ("goBtns/btnUndo/Text").gameObject.GetComponent<Text> ().text = langMgr.getValue ("undo");
		transform.Find ("goBtns/btnRedo/Text").gameObject.GetComponent<Text> ().text = langMgr.getValue ("redo");
		transform.Find ("goToggles/tog1/Label").gameObject.GetComponent<Text> ().text = langMgr.getValue ("easy");
		transform.Find ("goToggles/tog2/Label").gameObject.GetComponent<Text> ().text = langMgr.getValue ("normal");
		transform.Find ("goToggles/tog3/Label").gameObject.GetComponent<Text> ().text = langMgr.getValue ("difficult");
	}
}