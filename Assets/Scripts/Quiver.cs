using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quiver : MonoBehaviour {

    public GameObject arrowPrefab;
    private GameObject heldArrow;

    private Vector3 throwVelocity;
    private Vector3 previousPosition;

    public Vector3 maxFireVelocity = new Vector3(0, 30, 0);
    private Vector3 fireVelocity;

    private float pullBackAmount = 0.0f;
    private LineRenderer trajectoryLineRenderer;

    private bool IsFiring
    {
        get { return pullBackAmount > 0.5f; }
    }

    private bool ArmIsInPosition
    {
        get
        {
            // The rotation to test against
            const float compareAgainstRotation = 90f;

            // wiggle room afforded to the rotation check
            const float compareEpsilon = 65f;

            // get roatation from gObj's xform which is set by arm controller
            float observedRotation = transform.localEulerAngles.x;

            return Mathf.Abs(compareAgainstRotation - observedRotation) < compareEpsilon;
        }
    }

    // Use this for initialization
    void Start() {
        // get line renderer for trajectory simulation
        trajectoryLineRenderer = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update() {

        // listen for click events
        if (GvrController.ClickButtonDown && ArmIsInPosition)
        {
            CreateArrow();
        }

        if (heldArrow == null) return;

        PollTouchpad();
        SimulateTrajectory();
        CalculateThrowVelocity();
        CalculateFireVelocity();

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

        if (IsFiring)
        {
            // fire the object when releasing while aiming
            arrowRigidBody.AddRelativeForce(fireVelocity, ForceMode.VelocityChange);
        }
        else
        {
            // throw the object when releasing while held
            arrowRigidBody.AddForce(throwVelocity, ForceMode.VelocityChange);
        }

        TrailRenderer trailRenderer = heldArrow.GetComponent<TrailRenderer>();
        trailRenderer.enabled = true;

        trajectoryLineRenderer.enabled = false;
        heldArrow = null;
    }

    private void CalculateThrowVelocity()
    {
        // the velocity based on the previous position
        throwVelocity = (heldArrow.transform.position - previousPosition) / Time.deltaTime;

        // update previous position
        previousPosition = heldArrow.transform.position;
    }
    private void PollTouchpad()
    {
        pullBackAmount = GvrController.TouchPos.y;
        PositionArrow();
    }

    private void PositionArrow()
    {
        // update the position of the arrow locally based on the pullback amount
        // since touchpad ranges from 0 (top).. 1(bottom) we must invert the amount coming in
        const float initialOffset = 0.25f;
        Vector3 tranformLocalPosition = heldArrow.transform.localPosition;
        tranformLocalPosition.z = initialOffset + 1f - pullBackAmount;
        heldArrow.transform.localPosition = tranformLocalPosition;
    }

    private void CalculateFireVelocity()
    {
        fireVelocity = maxFireVelocity * pullBackAmount;
    }

    private void SimulateTrajectory()
    {
        // only show if the arrow is being fired
        trajectoryLineRenderer.enabled = IsFiring;

        Vector3 initialPosition = heldArrow.transform.position;
        Vector3 initialVelocity = heldArrow.transform.rotation * fireVelocity;

        const int numberOfPositionsToSimulate = 50;
        const float timeStepBetweenPositions = 0.2f;

        // setup initial conditions
        Vector3 simulatedPosition = initialPosition;
        Vector3 simulatedVelocity = initialVelocity;

        // update position count
        trajectoryLineRenderer.positionCount = numberOfPositionsToSimulate;

        for (int i = 0; i < numberOfPositionsToSimulate; i++)
        {
            // set each position of the line renderer
            trajectoryLineRenderer.SetPosition(i, simulatedPosition);

            // change velocity based on gravity and the time step
            simulatedVelocity += Physics.gravity * timeStepBetweenPositions;

            // change position based on gravity and time step
            simulatedPosition += simulatedVelocity * timeStepBetweenPositions;
        }
    }
}
