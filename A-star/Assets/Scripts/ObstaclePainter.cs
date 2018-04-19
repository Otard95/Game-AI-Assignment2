using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class ObstaclePainter : MonoBehaviour {

	[SerializeField] GameObject prefab;
	[SerializeField] Transform parent;
	[SerializeField] LayerMask placeableLayer = 512;
	[SerializeField] float minimumPaintDistance = .2f;
	[SerializeField] Vector3 offset;
	
	GameObject _currently_painting;
	Vector3 _start_point;

	[UsedImplicitly]
	void Update () {

		if (Input.GetButtonDown("Fire1")) {

			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(ray, out hit, int.MaxValue, placeableLayer)) {
				
				StartObstaclePlacement(hit.point);

			}

		}

		if (Input.GetButton("Fire1")) {
			ContinueObstaclePlacing(CursorWorldPos(placeableLayer));
		}

		if (Input.GetButtonUp("Fire1")) {
			EndObstaclePlacement(CursorWorldPos(placeableLayer));
		}

	}

	void StartObstaclePlacement (Vector3 startPoint) {

		_start_point = startPoint + offset;
		if (parent)
			_currently_painting = Instantiate(prefab, startPoint, Quaternion.identity, parent);
		else
			_currently_painting = Instantiate(prefab, startPoint, Quaternion.identity);

	}

	void ContinueObstaclePlacing (Vector3 endPoint) {

		if (_currently_painting == null) return;

		endPoint += offset;

		Vector3 mouseDrag = endPoint - _start_point;

		if (Mathf.Abs(mouseDrag.x) < minimumPaintDistance) {
			mouseDrag.x = 0;
		}
		if (Mathf.Abs(mouseDrag.z) < minimumPaintDistance) {
			mouseDrag.z = 0;
		}

		Vector3 finalPos = _start_point + (mouseDrag) / 2;
		Vector3 finalScale = Vector3.one + Vector3.right * Mathf.Abs(mouseDrag.x) + Vector3.forward * Mathf.Abs(mouseDrag.z);

		_currently_painting.transform.position = finalPos;
		_currently_painting.transform.localScale = finalScale;

	}

	void EndObstaclePlacement (Vector3 endPoint) {

		ContinueObstaclePlacing(endPoint);

		_currently_painting = null;

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
