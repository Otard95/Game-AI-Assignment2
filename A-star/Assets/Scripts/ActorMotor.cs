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
	[SerializeField]
	float moveForce = 60;

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
		if (_path != null)
		{
			Vector3 translation = _path.First() - transform.position;
			translation.y = 0;
			if (translation.magnitude < arriveThreshold)
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

		move_dir.y = 0;

		_rb.AddForce(move_dir.normalized * moveForce, ForceMode.Force);

		_forces.Clear();

	}

	public void Seek (Vector3 point)
	{

		Vector3 pos = transform.position;
		pos.y = 0;
		point.y = 0;

		Vector3 desired_dir = point - pos;
		Vector3 steering = desired_dir - _rb.velocity;
		_forces.Add(steering.normalized);	
	}

	public void FollowPath (Vector3[] path) {
		_path = new List<Vector3>(path);
		_path.Reverse();
	}

}
