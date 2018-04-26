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
		if (Input.GetButtonDown("Fire2")) {
			
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(ray, out hit)) {
				NavSurface target = hit.transform.GetComponent<NavSurface>();
				if (!target) return;

				_motor.Stop();
				PathFinder.Instance.GetPath(transform.position, hit.point, target, HandleReturnedPath);
			}

		}
	}

	void HandleReturnedPath (Vector3[] path) {
		if (path == null) {
			Debug.Log("Path not found.");
			return;
		}
		Debug.Log("Path Found!");
		_motor.FollowPath(path);
	}

}
