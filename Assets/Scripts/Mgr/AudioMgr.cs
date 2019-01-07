using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
		music.volume = 0.3F;
		sound.volume = 0.7F;
	}

	public void PlaySound(string soundName){
		AudioClip clip = Resources.Load ("audio/" + soundName) as AudioClip;
		sound.PlayOneShot(clip);
	}

	void stopSound(){
		sound.Stop ();
	}
}
