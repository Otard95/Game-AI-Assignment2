using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ActorMotor : MonoBehaviour {
	/**
	 * # Unity Proporties
	*/
	[Header("General")]
	[SerializeField] float moveForce = 60;

	[Header("Path Following")]
	[SerializeField]
	float arriveThreshold = .1f;

	/**
	 * # Components
	*/
	Rigidbody _rb;

	/**
	 * # Private Fields
	*/
	List<Vector3> _forces;
	List<Vector3> _path = null;

	[UsedImplicitly]
	void Start () {
		_rb = GetComponent<Rigidbody>();
		_forces = new List<Vector3>();
	}

	[UsedImplicitly]
	void FixedUpdate () {

		// Path following
		if (_path != null) {
			if (Vector3.Distance(transform.position, _path.First()) < arriveThreshold)
				_path.RemoveAt(0);

			if (_path.Count == 0) {
				_path = null;
			} else {
				Seek(_path.First());
			}
		}

		// Movement
		Vector3 move_dir = Vector3.zero;

		foreach (var force in _forces) {
			move_dir += force;
		}

		_rb.AddForce(move_dir.normalized * moveForce, ForceMode.Force);

	}

	public void Seek (Vector3 point) {
		Vector3 desired_dir = point - transform.position;
		_forces.Add((_rb.velocity - desired_dir).normalized);
	}

	public void FollowPath (Vector3[] path) {
		_path = new List<Vector3>(path);
	}

}
