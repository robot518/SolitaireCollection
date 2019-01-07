using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LangMgr {
	static LangMgr mgr;
	string _sLang = "Chinese";
	Dictionary<string, string> dic1 = new Dictionary<string, string> ();
	Dictionary<string, string> dic2 = new Dictionary<string, string> ();

	public static LangMgr getInstance() {
		if (mgr == null)
			mgr = new LangMgr ();
		return mgr;
	}
	public LangMgr()
	{    
		string[] tSLang = { "Chinese", "SPChinese" };
		Dictionary<string, string>[] tDic = { dic1, dic2 };
		for (int i = 0; i < tSLang.Length; i++) {
			var sLang = tSLang [i];
			var dic = tDic [i];
			TextAsset ta = Resources.Load<TextAsset>("lang/" + sLang);
			string text = ta.text;
			string[] lines = text.Split('\n');
			foreach (string line in lines)
			{    
				if (line == null)    
				{    
					continue;    
				}
				string[] keyAndValue = line.Split('=');    
				dic.Add(keyAndValue[0], keyAndValue[1]);    
			} 
		}   
	}
	public string getValue(string key)    
	{    
		if (_sLang == "Englise")
			return key;
		var dic = _sLang == "Chinese" ? dic1 : dic2;
		if (dic.ContainsKey(key) == false)
		{    
			return null;    
		}    
		string value = null;    
		dic.TryGetValue(key, out value);    
		return value;    
	} 
	public void setLang(string sLang){
		_sLang = sLang;
	}
}
