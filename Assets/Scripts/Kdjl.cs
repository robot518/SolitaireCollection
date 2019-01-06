using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class Kdjl : MonoBehaviour {
	public Text textStep;
	public Text textTime;
	public GameObject goCard;
	public GameObject goFind;
	public GameObject goMove;
	Transform goChoice;
	List<Card> lCards = new List<Card>();
	List<Card> lFindCards = new List<Card>();
	List<int> lCardDatas = new List<int>();
	Transform[] tTrans;
	int _iStep;
	int _iTime;
	Coroutine coPlayTime;
	int _gameCount;
	AudioMgr adMgr;
	MoveMgr undoMgr;
	MoveMgr redoMgr;
	LangMgr langMgr;
	int _iDiff;
	float _px;
	float _py;
	float _ply;
	float _plx;
	float _prx;
	bool _bTouch;
	bool _bShowBtns;
	bool _bWin;
	int _iPrompt;
	const int IROWCOUNT = 8;
	const int IROWDIS = 65;

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
		var goL = transform.Find ("goMedium/ndcard/ndCard2").gameObject;
		goL.transform.SetParent (goMove.transform);
		var posL = goL.GetComponent<RectTransform> ().anchoredPosition;
		_ply = posL.y;
		_plx = posL.x;
		Destroy (goL);
		var goR = transform.Find ("goDes/ndcard/ndCard3").gameObject;
		goR.transform.SetParent (goMove.transform);
		var posR = goR.GetComponent<RectTransform> ().anchoredPosition;
		_prx = posR.x;
		Destroy (goR);

		_gameCount = 0;
		_bTouch = true;
		_bShowBtns = true;
		undoMgr = new MoveMgr (this);
		redoMgr = new MoveMgr (this);
		adMgr = AudioMgr.getInstance ();
		langMgr = LangMgr.getInstance ();
		tTrans = new Transform[]{transform.Find ("ndH"), transform.Find ("goMedium"), transform.Find ("goDes")};
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
		_bWin = false;
		_iStep = 0;
		showTextStep (_iStep);
		_iTime = 0;
		textTime.text = "00:00";
		undoMgr.removeAll ();
		redoMgr.removeAll ();
	}

	void initBtnEvents(){
		var btnShow = transform.Find ("btnShow").gameObject.GetComponent<Button> ();
		btnShow.onClick.AddListener (delegate {
			_bShowBtns = !_bShowBtns;
			transform.Find("goFinds").gameObject.SetActive(_bShowBtns);
			transform.Find("goBtns").gameObject.SetActive(_bShowBtns);
			transform.Find("goToggles").gameObject.SetActive(_bShowBtns);
			transform.Find("goTogLang").gameObject.SetActive(_bShowBtns);
			transform.Find("soundSld").gameObject.SetActive(_bShowBtns);
			transform.Find("musicSld").gameObject.SetActive(_bShowBtns);
			var text = btnShow.transform.GetChild(0).gameObject.GetComponent<Text>();
			var str = _bShowBtns == true ? "hide" : "show";
			text.text = langMgr.getValue(str);
		});

		UnityAction[] tFunc = {onClickStart, onClickAuto, onClickPrompt, onClickUndo, onClickRedo};
		for (var i = 0; i < tFunc.Length; i++){
			var btn = transform.Find ("goBtns").GetChild(i).gameObject.GetComponent<Button>();
			btn.onClick.AddListener (tFunc[i]);
		}

		_iDiff = 1;
		var lLab = new List<Text> ();
		for (var i = 0; i < 3; i++){
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

		string[] tStr = { "Chinese", "Englise", "SPChinese" };
		var lLab2 = new List<Text> ();
		var transP = transform.Find ("goTogLang");
		for (var i = 0; i < 3; i++){
			var idx = i;
			var trans = transP.GetChild (i);
			var tog = trans.GetComponent<Toggle>();
			lLab2.Add (trans.GetChild (1).gameObject.GetComponent<Text> ());
			tog.onValueChanged.AddListener (
				delegate(bool isOn)
				{
					if (isOn == true){
						adMgr.PlaySound ("click");
						lLab2[idx].color = new Color(0, 1, 1, 1);
						langMgr.setLang(tStr[idx]);
						showTexts();
					}else
						lLab2[idx].color = Color.white;
				}
			);
		}
	}

	void initFinds(){
		for (int i = 0; i < 13; i++) {
			GameObject item;
			if (i == 0)
				item = goFind;
			else {
				item = GameObject.Instantiate(goFind);
				item.transform.SetParent (goFind.transform.parent);
				item.transform.localScale = Vector3.one;
			}
			item.GetComponent<Find>().init (i, this);
		}
	}

	void onClickStart () {
		setTouchable (false);
		foreach (var card in lCards) {
			//card.playWinMove (false);
			card.gameObject.SetActive (false);
			card.transform.SetParent (goMove.transform);
		}
		if (_iDiff == 3)
			initDataCardsDiff ();
		else
			initDataCards ();
		reset ();
		if (coPlayTime != null)
			StopCoroutine (coPlayTime);
		StartCoroutine (playCards());

		_gameCount++;
		adMgr.PlaySound ("start");
	}

	void onClickUndo(){
		if (_iStep == 0 || _bWin == true)
			return;
		showTextStep (--_iStep);
		redoMgr.addCard (undoMgr);
		undoMgr.onMoveCard();
	}

	void onClickRedo(){
		if (redoMgr.getCountCards() == 0 || _bWin == true)
			return;
		showTextStep (++_iStep);
		undoMgr.addCard (redoMgr);
		redoMgr.onMoveCard ();
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
					var card = trans.GetChild (j).GetComponent<Card> ();
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

	IEnumerator playAutoMoveCards(){
		setTouchable (false);
		var transP = tTrans [1];
		var iTime = 0.25f;
		while (true) {
			var bMove = false;
			for (var i = 0; i < IROWCOUNT; i++) {
				var trans = getTransP (0, i);
				if (trans.childCount > 0) {
					if (getBMoveToDes (trans.GetChild (trans.childCount - 1).gameObject.GetComponent<Card> ()) == true) {						
						bMove = true;
						yield return new WaitForSeconds(iTime);
					}
				}
			}
			for (var i = 0; i < transP.childCount; i++) {
				var transTemp = transP.GetChild (i);
				if (transTemp.childCount > 0 && getBMoveToDes (transTemp.GetChild(0).gameObject.GetComponent<Card> ()) == true) {
					bMove = true;
					yield return new WaitForSeconds(iTime);
				}
			}
			if (bMove == false) {
				if (_bWin == false)
					setTouchable (true);
				yield break;
			}
		}
	}

	IEnumerator playTextTime(){
		while (true) {
			textTime.text = getStrTime (_iTime);
			_iTime++;
			if (_iTime > 3600){
				adMgr.PlaySound ("lose");
				Invoke ("onClickStart", 2.0f);
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

	void showTextStep(int iStep){
		textStep.text = iStep.ToString();
	}

	void initGoCards () {
		goCard.SetActive(false);
		for (int i = 0; i < 52; i++){
			GameObject item;
			if (i == 0)
				item = goCard;
			else {
				GameObject goCardTemp = GameObject.Instantiate(goCard);
				goCardTemp.transform.SetParent(goMove.transform);
				goCardTemp.transform.localScale = Vector3.one;
				item = goCardTemp;
			}
			var card = item.GetComponent<Card> ();
			lCards.Add(card);
		}
	}

	void initDataCards () {
		var cardSp1 = new List<int> {
			101, 102, 103,
			201, 202, 203,
			301, 302, 303,
			401, 402, 403,
		};
		var cardSp2 = new List<int> {
			101,
			201,
			301,
			401,
		};
		var cardInit = _iDiff == 1 ? cardSp1 : cardSp2;
		var len = 52;
		var iCount = cardInit.Count / 4;
		for (var i = 0; i < 4; i++) {
			for (var j = 0; j < iCount; j++) {
				int idx = j + iCount * i;
				int iCard = cardInit [idx];
				int iCardNum = iCard % 100;
				int iCardType = (int)Mathf.Floor (iCard / 100);
				Card card = lCards [len - idx - 1];
				card.init (iCardNum, iCardType, 2, i, this);
				card.transform.SetParent (getTransP (2, i));
				var rect = card.gameObject.GetComponent<RectTransform>();
				rect.anchoredPosition = new Vector2 (rect.rect.width / 2, -rect.rect.height / 2);
				rect.anchorMax = new Vector2 (0, 1);
				rect.anchorMin = new Vector2 (0, 1);
			}
		}
		List<int> CARDS1 = new List<int> {
			104, 105, 106, 107, 108, 109, 110, 111, 112, 113,
			204, 205, 206, 207, 208, 209, 210, 211, 212, 213,
			304, 305, 306, 307, 308, 309, 310, 311, 312, 313,
			404, 405, 406, 407, 408, 409, 410, 411, 412, 413,
		};
		List<int> CARDS2 = new List<int> {
			102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113,
			202, 203, 204, 205, 206, 207, 208, 209, 210, 211, 212, 213,
			302, 303, 304, 305, 306, 307, 308, 309, 310, 311, 312, 313,
			402, 403, 404, 405, 406, 407, 408, 409, 410, 411, 412, 413,
		};
		var CARDS = _iDiff == 1 ? CARDS1 : CARDS2;
		lCardDatas.Clear ();
		while (true) {
			int iRandom = Random.Range (0, CARDS.Count - 1);
			int iCard = CARDS [iRandom];
			CARDS.RemoveAt(iRandom);
			lCardDatas.Add(iCard);
			if (CARDS.Count == 0) break;
		};
	}

	void initDataCardsDiff () {
		List<int> CARDS = new List<int> {
			101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113,
			201, 202, 203, 204, 205, 206, 207, 208, 209, 210, 211, 212, 213,
			301, 302, 303, 304, 305, 306, 307, 308, 309, 310, 311, 312, 313,
			401, 402, 403, 404, 405, 406, 407, 408, 409, 410, 411, 412, 413,
		};
		lCardDatas.Clear ();
		while (true) {
			int iRandom = Random.Range (0, CARDS.Count - 1);
//			if (_gameCount % 3 == 1)
//				iRandom = CARDS.Count - 1;
			int iCard = CARDS [iRandom];
			CARDS.RemoveAt(iRandom);
			lCardDatas.Add(iCard);
			if (CARDS.Count == 0) break;
		};
	}

	IEnumerator playCards () {
		int len = lCardDatas.Count;
		for (var i = 0; i < 7; i++) {
			for (int j = 0; j < IROWCOUNT; j++) {
				int idx = j + IROWCOUNT * i;
				if (idx >= len)
					break;
				int iCard = lCardDatas [idx];
				int iCardNum = iCard % 100;
				int iCardType = (int)Mathf.Floor (iCard / 100);
				Card card = lCards [idx];
				card.init (iCardNum, iCardType, 0, j, this);

				card.transform.SetParent (goMove.transform);
				card.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2 (_plx, _ply);
				var iPosX = _px + 65 * j;
				var iPosY = _py - 30 * (i - 1 + 1);
				var pos = card.gameObject.GetComponent<RectTransform> ().anchoredPosition;
				card.showMove (iPosX - pos.x, iPosY - pos.y, iPosX);
			}
			yield return new WaitForSeconds (0.2f);
		}

		setBMove ();
		coPlayTime = StartCoroutine (playTextTime());
		yield return new WaitForSeconds (0.5f);
		setTouchable (true);
	}

	public void setBMove(){
		Transform trans;
		int len;
		Card preCard = null;
		for (var i = 0; i < IROWCOUNT; i++){
			trans = getTransP(0, i).transform;
			len = trans.childCount - 1;
			if (len < 0)
				continue;
			for (var j = len; j >= 0; j--) {
				Card card = trans.GetChild (j).GetComponent<Card> ();
				if ((j == len) || (preCard.getBMove () == true && card.getColor () != preCard.getColor () && card.getCardNum () - 1 == preCard.getCardNum ()))
					card.setBMove (true);
				else
					card.setBMove (false);
				preCard = card;
			}
		}
	}

	public bool getBDropped(Card card, Vector2 pos){
		for (var i = 0; i < tTrans.Length; i++) {
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

	void onDrop(Card card){
		showTextStep (++_iStep);
		undoMgr.addCard(card);
		redoMgr.removeAll ();
	}

	void onCardMove(Card card, int iPos, int iRow){
		onDrop (card);
		moveCards (card, iPos, iRow);
	}

	public void moveCards(Card card, int iPos, int iRow){
		adMgr.PlaySound ("move");
		bool bAutoMove = true;
		if (card.transform.parent.gameObject == goMove) {
			bAutoMove = false;
		}
		if (bAutoMove == true && iPos == 0)
			card.addItems ();
		var items = card.getItems ();
		var trans = getTransP(iPos, iRow);
		card.setPos (iPos);
		card.setRow (iRow);
		card.transform.SetParent (trans);
		if (iPos != 0) {
			setBMove ();
			showWin ();
			if (bAutoMove == false) {
				var rect = card.gameObject.GetComponent<RectTransform> ();
				rect.anchoredPosition = new Vector2 (rect.rect.width / 2, -rect.rect.height / 2);
			} else {
				card.transform.SetParent (goMove.transform);
				var px = iPos == 1 ? (_plx + 65 * iRow) : (_prx - 65 * iRow);
				var pos = card.gameObject.GetComponent<RectTransform> ().anchoredPosition;
				card.showMove (px - pos.x, _ply - pos.y, px);
			}
		} else {
			if (items.Count > 0) {
				foreach (Transform item in items) {
					var cCard = item.gameObject.GetComponent<Card> ();
					cCard.setPos (iPos);
					cCard.setRow (iRow);
					item.SetParent (trans);
				}
			}
			setBMove ();
			if (bAutoMove == false) {
				card.transform.localPosition = new Vector3 (0, 0 - 30 * (trans.childCount - 1 - items.Count), 0);
				if (items.Count > 0) {
					var idx = 0;
					foreach (Transform item in items) {
						idx++;
						item.localPosition = new Vector3(0, 0 - 30 * (trans.childCount - 1 + idx - items.Count), 0);
					}
				}
			} else {
				card.transform.SetParent (goMove.transform);
				var iPosX = _px + 65 * iRow;
				var iPosY = _py - 30 * (trans.childCount - items.Count - 1 + 1);
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

	bool getBSuitCard(Card cardDown, int iRow){
		var iPos = 0;
		var rect = getTransP(iPos, iRow);
		if (rect.childCount == 0) {
			onCardMove (cardDown, iPos, iRow);
			return true;
		} else {
			var cardUp = rect.GetChild (rect.childCount - 1).gameObject.GetComponent<Card> ();
			if (cardDown.getColor () != cardUp.getColor () && cardUp.getCardNum () - cardDown.getCardNum () == 1) {
				onCardMove (cardDown, iPos, iRow);
				return true;
			}
		}
		return false;
	}

	public bool getBMoveToMedium(Card card){
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

	public bool getBMoveToDes(Card card){
		for (var i = 0; i < tTrans[2].childCount; i++) {
			if (getBMoveToDes (card, i) == true) {
				return true;
			}
		}
		return false;
	}

	bool getBMoveToDes (Card card, int iRow) {
		var transTemp = getTransP(2, iRow);
		if (card.getCardType () - 1 == iRow) {
			if ((transTemp.childCount == 1 && card.getCardNum () == 1) ||
			    (transTemp.childCount > 1 && card.getCardNum () - 1 == transTemp.GetChild (transTemp.childCount - 1).gameObject.GetComponent<Card> ().getCardNum ())) {
				onCardMove (card, 2, iRow);
				return true;
			}
		}
		return false;
	}

	public bool getBMoveToCard (Card card, bool bMove) {
		var iPos = 0;
		var transP = tTrans [iPos];
		for (var i = 0; i < transP.childCount; i++) {
			if (card.getPos() == 0 && card.getRow() == i) continue;
			var rect = transP.GetChild (i);
			if (rect.childCount > 0) {
				var cardUp = rect.GetChild (rect.childCount - 1).gameObject.GetComponent<Card> ();
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

	public Transform getTransP(int iPos, int iRow){
		return tTrans [iPos].GetChild (iRow);
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
			Invoke ("playWin", 2.0f);
		}
	}

	void playWin(){
		StartCoroutine (playWinCards());
	}

	IEnumerator playWinCards(){
		var transP = tTrans [2];
		for (int i = 0; i < transP.childCount; i++) {
			var trans = transP.GetChild (i);
			for (int j = 1, len = trans.childCount; j < len; j++) {
				var item = trans.GetChild (len - j);
				item.SetParent (goMove.transform);
				var card = item.gameObject.GetComponent<Card> ();
				card.playWinMove (true);
				yield return new WaitForSeconds (0.5f);
			}
			yield return new WaitForSeconds (0.5f);
		}
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
		_bTouch = bTouch;
		var control = GetComponent<CanvasGroup> ();
		control.blocksRaycasts = _bTouch;
	} 

	void showTexts(){
		transform.Find ("goOthers/textTime").gameObject.GetComponent<Text> ().text = langMgr.getValue ("time") + ": ";
		transform.Find ("goOthers/textStep").gameObject.GetComponent<Text> ().text = langMgr.getValue ("step") + ": ";
		transform.Find ("goBtns/btnStart/Text").gameObject.GetComponent<Text> ().text = langMgr.getValue ("start");
		transform.Find ("goBtns/btnAuto/Text").gameObject.GetComponent<Text> ().text = langMgr.getValue ("auto");
		transform.Find ("goBtns/btnUndo/Text").gameObject.GetComponent<Text> ().text = langMgr.getValue ("undo");
		transform.Find ("goBtns/btnRedo/Text").gameObject.GetComponent<Text> ().text = langMgr.getValue ("redo");
		transform.Find ("goBtns/btnPrompt/Text").gameObject.GetComponent<Text> ().text = langMgr.getValue ("tips");
		transform.Find ("goToggles/tog1/Label").gameObject.GetComponent<Text> ().text = langMgr.getValue ("easy");
		transform.Find ("goToggles/tog2/Label").gameObject.GetComponent<Text> ().text = langMgr.getValue ("normal");
		transform.Find ("goToggles/tog3/Label").gameObject.GetComponent<Text> ().text = langMgr.getValue ("difficult");
		transform.Find ("soundSld/Text").gameObject.GetComponent<Text> ().text = langMgr.getValue ("sound") + ": ";
		transform.Find ("musicSld/Text").gameObject.GetComponent<Text> ().text = langMgr.getValue ("music") + ": ";
		var str = _bShowBtns == true ? "hide" : "show";
		transform.Find ("btnShow/Text").gameObject.GetComponent<Text> ().text = langMgr.getValue (str);
	}
}