using JetBrains.Annotations;
using UnityEngine;

public class ObstacleRemover : MonoBehaviour {

	[SerializeField] LayerMask obstacleLayer = 256;

	[UsedImplicitly]
	void Update () {

		if (Input.GetButtonDown("Fire1")) {

			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(ray, out hit, int.MaxValue, obstacleLayer)) {

				Destroy(hit.transform.gameObject);

			}

		}

	}

}
