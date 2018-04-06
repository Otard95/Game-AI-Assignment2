using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

	NavSurface target;
	Vector2 targetNode;

	void Update () {

		/**
		 * # NavSurface Test
		*/

		if (Input.GetMouseButtonDown(0)) {

			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(ray, out hit))
			{
				target = hit.transform.GetComponent<NavSurface>();
				if (!target) return;

				targetNode = target.ClosestNode(hit.point);
			}

		}

	}

	void OnDrawGizmos ()
	{
		
		if (target) {
			target.HightlightNode(targetNode);
		}

	}

}
