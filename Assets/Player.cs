using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        var horizontal = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");

        var moveBy = new Vector2(
            horizontal * Time.deltaTime * 50f,
            vertical * Time.deltaTime * 50f
        );

        transform.position = (Vector2)transform.position + moveBy;
	}
}
