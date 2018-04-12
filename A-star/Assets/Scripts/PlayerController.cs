using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

[RequireComponent(typeof(ActorMotor))]
public class PlayerController : MonoBehaviour {

	ActorMotor _motor;

	[UsedImplicitly]
	void Start () {
		_motor = GetComponent<ActorMotor>();
	}

	[UsedImplicitly]
	void Update () {
		if (Input.GetMouseButtonDown(0)) {

			NavSurface target;
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(ray, out hit)) {
				target = hit.transform.GetComponent<NavSurface>();
				if (!target) return;

				PathFinder.Instance.GetPath(transform.position, hit.point, target, HandleReturnedPath);
			}

		}
	}

	void HandleReturnedPath (Vector3[] path) {
		if (path == null) {
			Debug.Log("Path not found.");
			return;
		}

		_motor.FollowPath(path);
	}

}
