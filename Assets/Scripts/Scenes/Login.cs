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
        string[] tStr = { "Kdjl", "Spider", "Zpjl", "Growup" };
        for (int i = 0, iL = btns.childCount; i < iL; i++)
        {
            var item = btns.GetChild(i).GetComponent<Button>();
            var idx = i;
            item.onClick.AddListener(delegate {
                SceneManager.LoadScene(tStr[idx]);
            });
        }
    }
}
