using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quiver : MonoBehaviour {

    public GameObject arrowPrefab;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        // listen for click events
        if (GvrController.ClickButtonDown)
        {
            CreateArrow();
        }
		
	}

    private void CreateArrow()
    {
        Instantiate(arrowPrefab);
    }
}
