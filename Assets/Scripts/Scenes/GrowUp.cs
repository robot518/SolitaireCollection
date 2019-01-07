using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GrowUp : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
	const int HEIGHT = (1280 - 200)/2;
	const int WIDTH = 720/2;
	const int CARDCOUNT = 17;
	const int DX = 5;
	const int PX = 40;
	const int MAXSIZE = 10;
	int _iTime;
	int _iSize = 1;
	int _iCardCount = 0;
	int _iEnemyCount = 5;
	int _iStory = 0;
	int _iMeng;
	int _iType = 0;
	int _iCount = 5;
	float _iMoreY;
	float iW;
	float iWEnemy;
	float iHEnemy;
	float _iMoveDis = 0;
	float _px;
	float _py;
	float _iPx;
	float _iPy;
	float _pxTemp;
	float _pyTemp;
	float _pxCard;
	float _pyCard;
	bool _bStop = false;
	bool _bBeginDrag = false;
	bool _bMove = false;
	bool _bOver = false;
	bool _bNext = false;
	bool _bSkip = false;
	string _sStory = "";
	string _sMeng = "abc"; //abc对应艹日月
	string _sIntro = "手指滑动/鼠标移动的方式，控制主角在屏幕移动，主角碰到牌吃牌，吃到的牌移动到展示区，展示区中相同的牌会消除，消除能让主角变大";
	Vector2 _vPre;
	List<Transform> lCards = new List<Transform> ();
	List<Transform> lEnemy = new List<Transform>();
	List<int> lCardData = new List<int> ();
	List<string> lStory = new List<string>();
	List<Transform> lMeng = new List<Transform>();
	List<List<string>> lStoryCond = new List<List<string>> ();
	Text labTime;
	Text labSize;
	Text labCount;
	Transform goTips;
	Transform _player;
	Transform _enemy;
	Transform goMove;
	Transform goEnemy;
	Transform goStorySP;
	Transform goStory;
	Transform goIntro;
	Transform goMine;
	Transform skipTips;
	Transform goBtns;
	Coroutine coPlayTime;
	Coroutine coPlayWin;
	Coroutine coPlayStory;
	AudioMgr adMgr;

	// Use this for initialization
	void Start () {
		initParas ();
		initEvent ();
		initShow ();
//		Invoke ("onStart", 0.5f);
		StartCoroutine(playStorySP());
//		StartCoroutine (playSkipTips ());
	}
	
	// Update is called once per frame
	void Update () {
		if ( Application.platform == RuntimePlatform.Android &&(Input.GetKeyDown(KeyCode.Escape)))  
		{
			Application.Quit ();
		}
		if (_bMove == true && _bOver == false) {
			var dt = Time.deltaTime;
			_iMoveDis += dt;
			var bNew = false;
			for (int i = 0, iL = lEnemy.Count; i < iL; i++) {
				var idx = iL - 1 - i;
				var item = lEnemy [idx];
				item.Translate (0, -100 * dt, 0);
				if (item.localPosition.y < -650)
					bNew = true;
			}
			if (bNew == true) {
				onNextEvent ();
			}
			onCollision ();
		}
	}

	void initParas(){
		_iTime = 0;
		var bg = transform.Find ("bg");
		_iMoreY = bg.GetChild (0).position.y - bg.GetChild (1).position.y;
		labTime = transform.Find ("goTop/time").GetComponent<Text> ();
		labSize = transform.Find ("goTop/size").GetComponent<Text> ();
		labCount = transform.Find ("goTop/count").GetComponent<Text> ();
		goTips = transform.Find ("goTips");
		_player = transform.Find ("player");
		goMove = transform.Find ("goMove");
		goStorySP = transform.Find ("storySP");
		goStory = transform.Find ("story");
		goIntro = transform.Find ("intro");
		goMine = transform.Find ("goMine");
		goBtns = transform.Find ("btns");
		var rect = _player.GetComponent<RectTransform> ().rect;
		iW = rect.width/2;
		var cards = transform.Find ("cards");
		for (int i = 1; i < cards.childCount - 1; i++) {
			lCards.Add (cards.GetChild (i));
		}
		goEnemy = transform.Find ("goEnemy");
		_enemy = goEnemy.GetChild (0);
		for (int i = 0; i < goEnemy.childCount; i++) {
			lEnemy.Add (goEnemy.GetChild (i));
		}
		var pos = _enemy.localPosition;
		_px = pos.x;
		_py = pos.y;
		var vP = _player.localPosition;
		_iPx = vP.x;
		_iPy = vP.y;
		var vPCard = lCards [0].localPosition;
		_pxCard = vPCard.x;
		_pyCard = vPCard.y;
		var rectEnemy = _enemy.GetComponent<RectTransform> ().rect;
		iWEnemy = rectEnemy.width/2;
		iHEnemy = rectEnemy.height / 2;
		adMgr = AudioMgr.getInstance ();
		for (int i = 0; i < _player.childCount; i++) {
			lMeng.Add (_player.GetChild (i));
		}
		skipTips = goStory.GetChild (1);

		var goTemp = transform.Find ("cards/cardTemp").gameObject;
		goTemp.transform.SetParent (goMove);
		var posTemp = goTemp.GetComponent<RectTransform>().anchoredPosition;
		_pxTemp = posTemp.x;
		_pyTemp = posTemp.y;
		Destroy (goTemp);

		lStoryCond.Add (new List<string> ());
		lStoryCond.Add (new List<string> ());
		lStoryCond [0].Add ("顺子");
		lStoryCond [0].Add ("5张连续");
		lStoryCond [1].Add ("炸弹");
		lStoryCond [1].Add ("4张相同");

		lStory.Add ("怀疑");
		lStory.Add ("怀疑如草木之芽");
		lStory.Add ("从真理之根萌生");
		lStory.Add ("Wind生活在扑克星球，这里到处是巨大的扑克，传言5张连续的扑克能召唤出“顺子”，顺子拥有无与伦比的智慧，但是，顺子处于沉睡之中。。。");
		lStory.Add ("Wind每天的生活就是打扑克，打扑克，打扑克，如此日复一日，年复一年。。。");
		lStory.Add ("Wind三岁了，他对每日重复的生活产生了怀疑（这不是我想要的生活。。。生活应该是怎么样的呢？谁能给我答案）。。。");
		lStory.Add ("虽然怀疑，但是，迫于生活压力（扑克如此巨大，而我如此渺小），他选择了屈服。。。");
		lStory.Add ("两年后，Wind5岁了，大了一圈的体型给了他一点信心，他决心去寻求智者洞房不败的帮助。。。");
		lStory.Add ("洞房不败告诉Wind去寻找“顺子”，通过集齐5张连续的扑克可召唤顺子，不过顺子处于沉睡之中，要唤醒顺子需借助神器“萌”的力量。。。");
		lStory.Add(	"9岁后，每当你消除一次困难，就能得到一次获得神器碎片的机会，集齐碎片“艹日月”，获得神器“萌”，就拥有唤醒顺子之力。。。顺子能给你想要的答案。。。");
	}

	void initShow(){
		labTime.text = "00:00";
		showSize ();
		showCount ();
		goTips.gameObject.SetActive(false);
		goMine.gameObject.SetActive (false);
		for (int i = 0; i < lCards.Count; i++) {
			var item = lCards [i];
			item.gameObject.SetActive (false);
			if (i != 0) {
				var px = _pxCard + PX * i;
				item.localPosition = new Vector2 (px, _pyCard);
			}
		}
		for (int i = 0; i < lEnemy.Count; i++) {
			lEnemy [i].gameObject.SetActive (false);
		}
		for (int i = 0; i < lMeng.Count; i++) {
			lMeng [i].gameObject.SetActive (false);
		}
		_player.gameObject.SetActive (false);
		goBtns.gameObject.SetActive (false);
	}

	void initEvent(){
        transform.Find("goTop/back").GetComponent<Button>().onClick.AddListener(delegate
        {
            SceneManager.LoadScene("Login");
        });

        UnityAction[] tFunc = {onClickStart, onNext, onIntro, onStop};
		for (int i = 0; i < tFunc.Length; i++) {
			var btn = goBtns.GetChild(i).gameObject.GetComponent<Button> ();
			btn.onClick.AddListener (tFunc[i]);
		}
		goStory.gameObject.GetComponent<Button>().onClick.AddListener (onSkip);
	}

	void initStory(){
		var str1 = lStoryCond [_iType] [0];
		var str2 = lStoryCond [_iType] [1];
		lStory[3] = "Wind生活在扑克星球，这里到处是巨大的扑克，传言"+str2+"的扑克能召唤出“"+str1+"”，"+str1+"拥有无与伦比的智慧，但是，"+str1+"处于沉睡之中。。。";
		lStory[8] = "洞房不败告诉Wind去寻找“"+str1+"”，通过集齐"+str2+"的扑克可召唤"+str1+"，不过"+str1+"处于沉睡之中，要唤醒"+str1+"需借助神器“萌”的力量。。。";
		lStory[9] = "9岁后，每当你消除一次困难，就能得到一次获得神器碎片的机会，集齐碎片“艹日月”，获得神器“萌”，就拥有唤醒"+str1+"之力。。。"+str1+"能给你想要的答案。。。";
	}

	void onClickStart(){
//		_iType = Random.Range (0, 2);
//		_iCount = _iType == 0 ? 5 : 4;
//		initStory ();
		_iStory = 3;
		showTips ("start");
		lCardData.Clear ();
		_sStory = "";
		_sMeng = "abc";
		_bBeginDrag = false;
		_bMove = false;
		_bOver = false;
		_bNext = false;
		_bSkip = false;
		_iTime = 0;
		_iSize = 1;
		_iCardCount = 0;
		_iMoveDis = 0;
		showSize ();
		showCount ();
		for (int i = 0; i < lCards.Count; i++) {
			var item = lCards [i];
			item.gameObject.SetActive (false);
			item.GetComponent<Image> ().color = Color.white;
		}
		for (int i = 0; i < lEnemy.Count; i++) {
			lEnemy [i].gameObject.SetActive (false);
		}
		for (int i = 0; i < lMeng.Count; i++) {
			var goMeng = lMeng [i].gameObject;
			goMeng.GetComponent<Text> ().color = Color.white;
			goMeng.SetActive (false);
		}
		goTips.gameObject.SetActive (false);
		_player.localPosition = new Vector2(_iPx, _iPy);
		_player.GetComponent<RectTransform> ().sizeDelta = new Vector2 (10 * _iSize, 10 * _iSize);
		_player.gameObject.SetActive (false);
		if (coPlayTime != null)
			StopCoroutine (coPlayTime);
		if (coPlayWin != null)
			StopCoroutine (coPlayWin);

		goIntro.gameObject.SetActive (false);
		coPlayStory = StartCoroutine (playStory ());
	}

	void onStart(){
		_bMove = true;
		onProduceEnemy ();
		coPlayTime = StartCoroutine (playTextTime());
	}

	void onStop(){
		if (_bOver == true || _iStory < 5)
			return;
		var str = _bStop == false ? "stop" : "continue";
		_bStop = !_bStop;
		_bMove = !_bMove;
		showTips (str);
		if (_bStop == true)
			StopCoroutine (coPlayTime);
		else
			coPlayTime = StartCoroutine (playTextTime());
	}

	void onNext(){
		if (_bOver == true || _iStory < 5)
			return;
		if (_bNext == true) {
			StartCoroutine (playNext ());
		}
	}

	void onIntro(){
		goIntro.gameObject.SetActive (!goIntro.gameObject.activeSelf);
	}

	void onNextEvent(){
		if (_iStory < 5)
			return;
		onProduceEnemy ();
		_iMoveDis = 0;
	}

	void onProduceEnemy(){
		var lCard = new List<int> ();
		for (int i = 0; i < 13; i++) {
			lCard.Add (400 + i + 1);
		}
		var lCardTemp = new List<int> ();
		for (int i = 0; i < 5; i++) {
			var idx = Random.Range (0, 13);
			lCardTemp.Add (lCard [idx]);
		}
		for (int i = 0; i < lEnemy.Count; i++) {
			var item = lEnemy [i];
			item.localScale = new Vector2 (1, 1);
			item.SetParent (goEnemy);
			item.gameObject.SetActive (true);
			var px = _px + (2 * iWEnemy + DX) * i;
			item.localPosition = new Vector2 (px, _py);
			item.GetComponent<CardGrowUp> ().init (lCardTemp [i], this);
		}
		_iEnemyCount = 5;
		_bNext = false;
	}

	void onCollision(){
		var vP = _player.localPosition;
		var iDis = 10 * _iSize / 2;
		var iDisX = iWEnemy + iDis;
		var iDisY = iHEnemy + iDis;
		for (int i = 0, iL = lEnemy.Count; i < iL; i++) {
			var idx = iL - i - 1;
			var item = lEnemy [idx];
			var vE = item.localPosition;
			if (item.gameObject.activeSelf == true && Mathf.Abs (vP.x - vE.x) < iDisX && Mathf.Abs (vP.y - vE.y) < iDisY) {
				_bNext = true;
				item.SetParent (goMove);
				adMgr.PlaySound ("move");
				var iPosX = _pxTemp + 40 * _iCardCount;
				var pos = item.gameObject.GetComponent<RectTransform> ().anchoredPosition;
				item.GetComponent<CardGrowUp>().showMove (iPosX - pos.x, _pyTemp - pos.y, iPosX);
			}
		}
	}

	public void onReachCall(int iCard){
		showCard (iCard);
		_iEnemyCount--;
		if (_iEnemyCount == 0) {
			onNextEvent ();
		}
	}

	void showCard(int iCard){
		if (_iCardCount < CARDCOUNT) {
			lCardData.Add (iCard);
			var item = lCards [_iCardCount];
			item.gameObject.SetActive (true);
			item.GetComponent<CardGrowUp> ().init (iCard, null);
			_iCardCount++;
			showCount ();
			if (_iCardCount > 1 && iCard == lCardData [_iCardCount - 2]) {
				StartCoroutine (playDel ());
			}else if (_iSize == MAXSIZE && _sMeng.Length == 0 && _iCardCount > _iCount - 1) {
				onCheckWin ();
			}
		} else {
			onGameOver (false);
			for (int i = 0; i < lCards.Count; i++) {
				lCards[i].GetComponent<Image> ().color = Color.green;
			}
		}
	}

	void onCheckWin(){
		var bWin = true;
		var iNum = lCardData[_iCardCount - 1];
		switch (_iType) {
		case 0:
			iNum = iNum % 100 == 1 ? iNum + 13 : iNum;
			for (int i = 0; i < _iCount - 1; i++) {
				var iNumTemp = lCardData [_iCardCount - 2 - i];
				if (iNumTemp - iNum != -1) {
					bWin = false;
					break;
				}
				iNum = iNumTemp;
			}
			if (bWin == false) {
				bWin = true;
				iNum = lCardData[_iCardCount - 1];
				for (int i = 0; i < _iCount - 1; i++) {
					var iNumTemp = lCardData [_iCardCount - 2 - i];
					iNumTemp = iNumTemp % 100 == 1 ? iNumTemp + 13 : iNumTemp;
					if (iNumTemp - iNum != 1) {
						bWin = false;
						break;
					}
					iNum = iNumTemp;
				}
			}
			break;
		case 1:
			for (int i = 0; i < _iCount - 1; i++) {
				var iNumTemp = lCardData [_iCardCount - 2 - i];
				if (iNumTemp - iNum != 0) {
					bWin = false;
					break;
				}
				iNum = iNumTemp;
			}
			break;
		}

		if (bWin == true) {
			_bOver = true;
			_bMove = false;
			coPlayWin = StartCoroutine (playWin ());
		}
	}

	void onGameOver(bool bWin){
		var str = bWin == true ? "win" : "lose";
		showTipsSP (str);
		adMgr.PlaySound (str);
		_bOver = true;
		_bMove = false;
		StopCoroutine (coPlayTime);
	}

	public void OnBeginDrag(PointerEventData data){
		if (_bBeginDrag == true || _bMove == false || _bOver == true)
			return;
		_bBeginDrag = true;
		_vPre = _player.localPosition;
	}

	public void OnDrag(PointerEventData data){
		if (_bMove == false || _bOver == true || _bBeginDrag == false)
			return;
		var vPre = data.pressPosition;
		var pos = data.position;
		var iPx = _vPre.x + pos.x - vPre.x;
		var iPy = _vPre.y + pos.y - vPre.y;
		var iL = iW + 10 * (_iSize - 10) / 2;
		if (iPx > WIDTH - iL)
			iPx = WIDTH - iL;
		if (iPx < -WIDTH + iL)
			iPx = -WIDTH + iL;
		if (iPy > HEIGHT - iL - _iMoreY)
			iPy = HEIGHT - iL - _iMoreY;
		if (iPy < -HEIGHT + iL)
			iPy = -HEIGHT + iL;
		_player.localPosition = new Vector2 (iPx, iPy);
	}

	public void OnEndDrag(PointerEventData data){
		if (_bMove == false || _bOver == true)
			return;
		_bBeginDrag = false;
	}

	void showSize(){
		labSize.text = _iSize.ToString ();
		if (_iSize == 10)
			labSize.GetComponent<Text> ().color = Color.red;
		else
			labSize.GetComponent<Text> ().color = Color.black;
	}

	void showCount(){
		labCount.text = _iCardCount.ToString ();
	}

	IEnumerator playTextTime(){
		while (true) {
			labTime.text = getStrTime (_iTime);
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

	void showTips(string str){
		goTips.GetChild (0).GetComponent<Text> ().text = str;
		StartCoroutine(playTips());
	}

	void showTipsSP(string str){
		goTips.GetChild (0).GetComponent<Text> ().text = str;
		goTips.gameObject.SetActive (true);
	}

	IEnumerator playTips(){
		setTouchable (false);
		goTips.gameObject.SetActive (true);
		yield return new WaitForSeconds (1.0f);
		goTips.gameObject.SetActive (false);
		setTouchable (true);
	}

	void setTouchable(bool bTouch){
		var control = GetComponent<CanvasGroup> ();
		control.blocksRaycasts = bTouch;
	}

	IEnumerator playWin(){
		var idx = 0;
		while (true) {
			idx++;
			yield return new WaitForSeconds (1.0f);
			for (int i = 0; i < _iCount; i++) {
				var item = lCards [_iCardCount - 1 - i];
				item.gameObject.SetActive (false);
			}
			yield return new WaitForSeconds (0.2f);
			for (int i = 0; i < _iCount; i++) {
				var item = lCards [_iCardCount - 1 - i];
				item.gameObject.SetActive (true);
			}
			if (idx == 5) {
				onClickStart ();
//				onGameOver (true);
			}

		}
	}

	IEnumerator playDel(){
		_bBeginDrag = false;
		_bMove = false;
		setTouchable (false);
		for (int j = 0; j < 1; j++) {
			yield return new WaitForSeconds (0.2f);
			for (int i = 0; i < 2; i++) {
				var item = lCards [_iCardCount - 1 - i];
				item.gameObject.SetActive (false);
			}
			yield return new WaitForSeconds (0.1f);
			for (int i = 0; i < 2; i++) {
				var item = lCards [_iCardCount - 1 - i];
				item.gameObject.SetActive (true);
			}
		}
		yield return new WaitForSeconds (0.3f);
		for (int i = 0; i < 2; i++) {
			var item = lCards [_iCardCount - 1 - i];
			item.gameObject.SetActive (false);
		}
		var iS = 12 / _iSize;
		for (int j = 0; j < 2; j++) {
			yield return new WaitForSeconds (0.2f);
			_player.localScale = new Vector2 (iS, iS);
			yield return new WaitForSeconds (0.1f);
			_player.localScale = new Vector2 (1, 1);
		}
		lCardData.RemoveAt (_iCardCount - 1);
		lCardData.RemoveAt (_iCardCount - 2);
		_iCardCount -= 2;
		showCount ();
		if (_iSize < MAXSIZE) {
			_iSize++;
			showSize ();
			_player.GetComponent<RectTransform> ().sizeDelta = new Vector2 (10 * _iSize, 10 * _iSize);
			if (_iSize == 3 || _iSize == 5) {
				coPlayStory = StartCoroutine (playStory ());
			}
		}
		if (_iSize != 3 && _iSize != 5) {
			_bMove = true;
		}
		if (_iSize == MAXSIZE && _sMeng.Length != 0) {
			showMine ();
			_bMove = false;
		}
		setTouchable (true);
	}

	IEnumerator playNext(){
		setTouchable (false);
		var iS = 0.1f;
		yield return new WaitForSeconds (0.05f);
		for (int i = 0; i < lEnemy.Count; i++) {
			var item = lEnemy [i];
			if (item.gameObject.activeSelf == true)
				item.localScale = new Vector2 (iS, iS);
		}
		yield return new WaitForSeconds (0.2f);
		for (int i = 0; i < lEnemy.Count; i++) {
			var item = lEnemy [i];
			if (item.gameObject.activeSelf == true)
				item.localScale = new Vector2 (1, 1);
		}
		yield return new WaitForSeconds (0.1f);
		for (int i = 0; i < lEnemy.Count; i++) {
			var item = lEnemy [i];
			if (item.gameObject.activeSelf == true)
				item.localScale = new Vector2 (0.01f, 0.01f);
		}
		yield return new WaitForSeconds (0.3f);
		setTouchable (true);
		onNextEvent ();
	}

	IEnumerator playStorySP(){
		setTouchable (false);
		goStorySP.gameObject.SetActive (true);
		var lab1 = goStorySP.GetChild (0).GetComponent<Text> ();
		var lab2 = goStorySP.GetChild (1).GetComponent<Text> ();
		var lab3 = goStorySP.GetChild (2).GetComponent<Text> ();
		lab1.text = "";
		lab2.text = "";
		lab3.text = "";
		var str1 = lStory [_iStory];
		var str2 = lStory [_iStory + 1];
		var str3 = lStory [_iStory + 2];
		var idx1 = 1;
		var idx2 = 1;
		var idx3 = 1;
		_iStory = 3;
		yield return new WaitForSeconds (0.5f);
		_bSkip = true;
		while (true) {
			if (lab2.text == str2) {
				yield return new WaitForSeconds (0.2f);
				lab3.text = str3.Substring (0, idx3);
				idx3++;
			} else if (lab1.text == str1) {
				yield return new WaitForSeconds (0.2f);
				lab2.text = str2.Substring (0, idx2);
				idx2++;
			} else {
				lab1.text = str1.Substring (0, idx1);
				idx1++;
				yield return new WaitForSeconds (0.5f);
			}
			if (lab3.text == str3)
				break;
		}
		yield return new WaitForSeconds (1.0f);
		goStorySP.gameObject.SetActive (false);
		setTouchable (true);
		onIntro ();
		StartCoroutine(playIntro());
	}

	IEnumerator playIntro(){
		var lab = goIntro.GetChild (1).GetComponent<Text> ();
		lab.text = "";
		var str = _sIntro;
		var idx = 1;
		yield return new WaitForSeconds (0.3f);
		while (true) {
			yield return new WaitForSeconds (0.1f);
			lab.text = str.Substring (0, idx);
			idx++;
			if (lab.text == str)
				break;
		}
		yield return new WaitForSeconds (0.5f);
		transform.Find("btns").gameObject.SetActive(true);
	}

	IEnumerator playStory(){
		goStory.gameObject.SetActive (true);
		skipTips.gameObject.SetActive(false);
		var lab = goStory.GetChild (0).GetComponent<Text> ();
		lab.text = "";
		var str = lStory [_iStory];
		var idx = 1;
		_iStory++;
		yield return new WaitForSeconds (0.3f);
		while (true) {
			if (idx > 10) {
				_bSkip = true;
				skipTips.gameObject.SetActive(true);
			}
			yield return new WaitForSeconds (0.1f);
			lab.text = str.Substring (0, idx);
			idx++;
			if (lab.text == str)
				break;
		}
		yield return new WaitForSeconds (0.5f);
	}

	void showMine(){
		var iLen = _sMeng.Length;
		if (iLen > 0) {
			goMine.gameObject.SetActive (true);
			var iRandom = Random.Range (0, iLen);
			_sStory = "s" + _sMeng [iRandom];
			goMine.GetComponent<Mine> ().init (this, _sStory);
		}
	}

	public void showMeng(string str){
		var idx = 0;
		if (str == "sb")
			idx = 1;
		else if (str == "sc")
			idx = 2;
		var idx2 = _sMeng.IndexOf (str [1]);
		if (idx2 != -1)
			_sMeng = _sMeng.Remove (idx2, 1);
		_iMeng = idx;
		StartCoroutine (playShowMeng ());
	}

	public void setBMove(bool bMove){
		_bMove = bMove;
	}

	void onSkip(){
		if (_bSkip == true) {
			if (coPlayStory != null)
				StopCoroutine (coPlayStory);
			_bSkip = false;
			goStory.gameObject.SetActive (false);
			if (_iStory == 4)
				coPlayStory = StartCoroutine (playStory ());
			else if (_iStory == 5)
				StartCoroutine (playStart ());
			else if (_iStory == 6)
				coPlayStory = StartCoroutine (playStory ());
			else if (_iStory == 7 || _iStory == 10)
				_bMove = true;
			else if (_iStory == 8 || _iStory == 9)
				coPlayStory = StartCoroutine(playStory());
		}
	}

	IEnumerator playStart(){
		setTouchable (false);
		yield return new WaitForSeconds (0.5f);
		_player.gameObject.SetActive (true);
		var iS = 12 / _iSize;
		for (int j = 0; j < 3; j++) {
			yield return new WaitForSeconds (0.2f);
			_player.localScale = new Vector2 (iS, iS);
			yield return new WaitForSeconds (0.1f);
			_player.localScale = new Vector2 (1, 1);
		}
		yield return new WaitForSeconds (0.5f);
		setTouchable (true);
		onStart ();
	}

	IEnumerator playShowMeng(){
		setTouchable (false);
		var go = lMeng [_iMeng].gameObject;
		for (int j = 0; j < 2; j++) {
			yield return new WaitForSeconds (0.2f);
			go.SetActive (true);
			yield return new WaitForSeconds (0.1f);
			go.SetActive (false);
		}
		yield return new WaitForSeconds (0.3f);
		go.SetActive (true);
		if (_sMeng.Length == 0)
			StartCoroutine (playShowAllMeng ());
		else {
			setTouchable (true);
			_bMove = true;
			if (_sMeng.Length == 0) {
				onCheckWin ();
			}
		}
	}

	IEnumerator playShowAllMeng(){
		yield return new WaitForSeconds (0.2f);
		for (int j = 0; j < 3; j++) {
			yield return new WaitForSeconds (0.2f);
			for (int i = 0; i < lMeng.Count; i++) {
				lMeng [i].GetComponent<Text> ().color = Color.red;
			}
			yield return new WaitForSeconds (0.1f);
			for (int i = 0; i < lMeng.Count; i++) {
				lMeng [i].GetComponent<Text> ().color = Color.white;
			}
		}
		yield return new WaitForSeconds (0.3f);
		for (int i = 0; i < lMeng.Count; i++) {
			lMeng [i].GetComponent<Text> ().color = Color.red;
		}
		setTouchable (true);
		_bMove = true;
		if (_sMeng.Length == 0) {
			onCheckWin ();
		}
	}
}
