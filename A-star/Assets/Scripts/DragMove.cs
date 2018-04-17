using JetBrains.Annotations;
using UnityEngine;

public class DragMove : MonoBehaviour {

	[SerializeField] LayerMask movableLayers = 1;
	[SerializeField] LayerMask placeableLayer = 512;

	Vector3 _offset;
	Transform _moving_transform;

	[UsedImplicitly]
	void Update () {

		if (Input.GetButtonDown("Fire1")) {

			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(ray, out hit, int.MaxValue, movableLayers)) {

				StartMoving(hit.transform);

			}

		} else if (Input.GetButtonUp("Fire1")) StopMoving();
		else if (Input.GetButton("Fire1")) ContinueMoving();

	}

	void StartMoving (Transform _transform) {
		
		_moving_transform = _transform;
		_offset = _transform.GetComponent<Collider>().bounds.extents;
		_offset.x = 0;
		_offset.z = 0;

	}

	void ContinueMoving () {

		if (!_moving_transform) return;
		_moving_transform.position = CursorWorldPos(placeableLayer) + _offset;

	}

	void StopMoving () {

		if (!_moving_transform) return;
		ContinueMoving();
		_moving_transform = null;

	}

	Vector3 CursorWorldPos (LayerMask layer) {
		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

		if (Physics.Raycast(ray, out hit, int.MaxValue, layer)) {
			return hit.point;
		}

		return Vector3.zero;

	}

}
