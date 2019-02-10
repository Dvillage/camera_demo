using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class outline : MonoBehaviour {
	
	Material mat;

	// Use this for initialization
	void Start () {
		mat = GetComponent<Renderer>().material;
	}
	
	// Update is called once per frame
	void Update () {
		mat.SetFloat("_OutlineWidth", 0.00f);
	}
}
