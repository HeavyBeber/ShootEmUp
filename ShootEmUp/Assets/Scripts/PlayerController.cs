using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Rigidbody))]
public class PlayerController : MonoBehaviour
{
    Rigidbody myRigidBody;
    Vector3 velocity;

    void Start()
    {
        myRigidBody = GetComponent<Rigidbody>();
    }

    public void move(Vector3 _velocity) {
        velocity = _velocity;
    }

    public void FixedUpdate() {
        myRigidBody.MovePosition(myRigidBody.position + velocity * Time.fixedDeltaTime);
    }

    public void LookAt(Vector3 _lookPoint) {
        Vector3 pointWithCorrectHeight = new Vector3(_lookPoint.x, transform.position.y, _lookPoint.z);
        transform.LookAt(pointWithCorrectHeight);
    }
}
