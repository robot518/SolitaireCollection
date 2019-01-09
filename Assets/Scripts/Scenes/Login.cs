using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Login : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        initEvent();
        initShow();
    }

    // Update is called once per frame
    void Update()
    {
        if (Application.platform == RuntimePlatform.Android && (Input.GetKeyDown(KeyCode.Escape)))
        {
            Application.Quit();
        }
    }

    void initEvent()
    {
        var btns = transform.Find("btns");
        string[] tMenuItem = { "Kdjl", "Spider", "Zpjl", "Growup" };
        for (int i = 0, iL = btns.childCount; i < iL; i++)
        {
            var item = btns.GetChild(i).GetComponent<Button>();
            var idx = i;
            item.onClick.AddListener(delegate {
                SceneManager.LoadScene(tMenuItem[idx]);
            });
        }

        var sets = transform.Find("sets");
        for (int i = 0, iL = sets.childCount; i < iL; i++)
        {
            var item = sets.GetChild(i).GetComponent<Button>();
            var idx = i;
            item.onClick.AddListener(delegate {
                if (idx == 0)
                {
                    Global.bVoice = !Global.bVoice;
                    if (Global.bVoice == false)
                        item.GetComponent<Image>().color = Color.gray;
                    else if (Global.bVoice == true)
                        item.GetComponent<Image>().color = Color.white;
                }
                else if (idx == 1)
                {
                    Global.bWinPlay = !Global.bWinPlay;
                    if (Global.bWinPlay == false)
                        item.GetComponent<Image>().color = Color.gray;
                    else if (Global.bVoice == true)
                        item.GetComponent<Image>().color = Color.white;
                }

            });
        }
    }

    void initShow()
    {
        var sets = transform.Find("sets");
        var voice = sets.GetChild(0).GetComponent<Image>();
        var winPlay = sets.GetChild(1).GetComponent<Image>();
        if (Global.bVoice == false)
            voice.color = Color.gray;
        else if (Global.bVoice == true)
            voice.color = Color.white;
        if (Global.bWinPlay == false)
            winPlay.color = Color.gray;
        else if (Global.bVoice == true)
            winPlay.color = Color.white;
    }
}
