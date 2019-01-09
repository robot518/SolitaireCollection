using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioMgr : MonoBehaviour {
	static GameObject go;
	static AudioMgr adMgr;
	//音乐文件
	public AudioSource music;
	public AudioSource sound;

	public static AudioMgr getInstance() {
		if (adMgr == null) {
			adMgr = go.GetComponent<AudioMgr> ();
		}
		return adMgr;
	}

	void Awake() {
		go = gameObject;
        //设置默认音量
        //music.volume = 0.3F;
        //sound.volume = 0.7F;
        if (Global.bVoice == false)
            stop();
        else
            play();
    }

	public void PlaySound(string soundName){
		AudioClip clip = Resources.Load ("audio/" + soundName) as AudioClip;
		sound.PlayOneShot(clip);
	}

    void stop(){
        //music.Stop();
        //sound.Stop ();
        music.volume = 0;
        sound.volume = 0;
    }

    void play()
    {
        music.volume = 1;
        sound.volume = 1;
    }
}
