using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quiver : MonoBehaviour {

    public GameObject arrowPrefab;
    private GameObject heldArrow;

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

        if (heldArrow == null) return;

        if (GvrController.ClickButtonUp)
            ReleaseArrow();
	}

    private void CreateArrow()
    {
        // create the arrow
        GameObject arrow = Instantiate(arrowPrefab);

        // position and orient the arrow near the arm
        HoldArrow(arrow);
    }

    private void HoldArrow(GameObject arrow)
    {
        heldArrow = arrow;

        // make a child of this object
        heldArrow.transform.SetParent(transform, false);
        heldArrow.transform.localPosition = new Vector3(0, 0, 1);
        heldArrow.transform.localEulerAngles = new Vector3(90, 0, 0);
    }

    private void ReleaseArrow()
    {
        // change the parent to the world
        heldArrow.transform.SetParent(null, true);

        // nullify the current velocity
        Rigidbody arrowRigidBody = heldArrow.GetComponent<Rigidbody>();
        arrowRigidBody.velocity = Vector3.zero;
        arrowRigidBody.isKinematic = false;

        heldArrow = null;
    }
}
