using System.Security.Cryptography.X509Certificates;
using JetBrains.Annotations;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class NavSurface : MonoBehaviour {

	[SerializeField] int resolution = 2;
	[SerializeField] float obstaclePadding = 1.2f;
	[SerializeField] LayerMask obstacleLayer;
	[Tooltip("Continuasly update nodes at runtime")]
	[SerializeField]
	bool dynamicNodes = false;
	[Tooltip("In seconds")]
	[SerializeField]
	float updateInterval = 0.01f;

	[SerializeField][HideInInspector] bool[] nodes;

	[SerializeField][HideInInspector] int width;
	[SerializeField][HideInInspector] int height;

	float _last_update;

	[UsedImplicitly]
	void Start () {
		if (nodes == null || nodes.Length == 0) BakeNodes();
		_last_update = updateInterval;
	}

	[UsedImplicitly]
	void Update () {
		if (nodes == null || nodes.Length == 0 || (_last_update <= 0 && dynamicNodes)) {
			_last_update = updateInterval;
			BakeNodes();
		}
		if (dynamicNodes) _last_update -= Time.deltaTime;
	}

	public void BakeNodes () {
		var c = GetComponent<Collider>();

		width = Mathf.FloorToInt(c.bounds.size.x * resolution) + 1;
		height = Mathf.FloorToInt(c.bounds.size.z * resolution) + 1;

		nodes = new bool[width * height];

		for (int i = 0; i < width; i++) {
			for (int j = 0; j < height; j++) {

				Vector3 pos = NodePos(i, j);

				if (Physics.OverlapSphere(pos, obstaclePadding, obstacleLayer).Length > 0) {
					nodes[i + j * height] = false;
				} else {
					nodes[i + j * height] = true;
				}

			}
		}
	}

	public void ClearNodes () {
		height = 0;
		width = 0;
		nodes = null;
	}

	[UsedImplicitly]
	void OnDrawGizmosSelected () {
		if (nodes == null) return;

		for (int i = 0; i < width; i++) {
			for (int j = 0; j < height; j++) {
				Vector3 pos = NodePos(i, j);

				Gizmos.color = nodes[i + j * height] ? Color.green : Color.red;
				Gizmos.DrawWireSphere(pos, obstaclePadding);
			}
		}
	}

	Vector3 NodePos (int x, int y) {
		return transform.position + transform.right * ((x - (width - 1) / 2f) / resolution) + transform.forward * ((y - (height - 1) / 2f) / resolution);
	}

}
