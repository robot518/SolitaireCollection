using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
        
    }

    void initEvent()
    {
        var btns = transform.Find("btns");
        for (int i = 0, iL = btns.childCount; i < iL; i++)
        {
            var item = btns.GetChild(i).GetComponent<Button>();
            item.onClick.AddListener(delegate {
                SceneManager.LoadScene("Kdjl");
            });
        }   
    }
}
