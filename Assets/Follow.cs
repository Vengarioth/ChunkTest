using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow : MonoBehaviour {

    public Transform _target;
    public float _distance = 10f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        var position = (Vector2)_target.position;
        transform.position = new Vector3(position.x, position.y, -_distance);
	}
}
