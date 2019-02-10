using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class key_checker : MonoBehaviour {

	public Image[] img;
	public Material[] mat;
	camera_move cm;
	public bool alphablend;

	// Use this for initialization
	void Start () {
		cm = Camera.main.GetComponent<camera_move>();
	}
	
	// Update is called once per frame
	void Update () {

		// 終了
		if(Input.GetKeyDown(KeyCode.Escape)){
			Application.Quit();
		}

		// ポッタープログラム
		if(Input.GetKeyDown(KeyCode.LeftShift)){
			img[0].color = Color.cyan;
		}
		if(Input.GetKeyDown(KeyCode.W)){
			img[1].color = Color.yellow;
		}
		if(Input.GetKeyDown(KeyCode.S)){
			img[2].color = Color.yellow;
		}
		if(Input.GetKeyDown(KeyCode.A)){
			img[3].color = Color.yellow;
		}
		if(Input.GetKeyDown(KeyCode.D)){
			img[4].color = Color.yellow;
		}
		if(Input.GetKeyDown(KeyCode.Q)){
			img[5].color = Color.cyan;
		}
		if(Input.GetKeyDown(KeyCode.E)){
			img[6].color = Color.cyan;
		}

		if(Input.GetKeyUp(KeyCode.LeftShift)){
			img[0].color = Color.white;
		}
		if(Input.GetKeyUp(KeyCode.W)){
			img[1].color = Color.white;
		}
		if(Input.GetKeyUp(KeyCode.S)){
			img[2].color = Color.white;
		}
		if(Input.GetKeyUp(KeyCode.A)){
			img[3].color = Color.white;
		}
		if(Input.GetKeyUp(KeyCode.D)){
			img[4].color = Color.white;
		}
		if(Input.GetKeyUp(KeyCode.Q)){
			img[5].color = Color.white;
		}
		if(Input.GetKeyUp(KeyCode.E)){
			img[6].color = Color.white;
		}
	}
}
