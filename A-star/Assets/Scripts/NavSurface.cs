using JetBrains.Annotations;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class NavSurface : MonoBehaviour
{

	/**
	 * # Unity Proporties
	*/
	[SerializeField] int resolution = 2;
	[SerializeField] float obstaclePadding = .3f;
	[SerializeField] LayerMask obstacleLayer;

	[Tooltip("Continuasly update nodes at runtime")] [SerializeField]
	bool dynamicNodes = false;

	[Tooltip("In seconds")] [SerializeField]
	float updateInterval = 0.01f;

	/**
	 * # Persistent fields
	*/
	[SerializeField] [HideInInspector] bool[] _nodes;
	[SerializeField] [HideInInspector] PathNode[] _path_nodes;

	[SerializeField] [HideInInspector] int _width;
	[SerializeField] [HideInInspector] int _height;
	[SerializeField] [HideInInspector] float _ratio_x;
	[SerializeField] [HideInInspector] float _ratio_y;

	/**
	 * # Componets
	*/

	[SerializeField] Collider col;

	/**
	 * # Private Fields
	*/
	float _last_update;

	/**
	 * # Unity Methods
	*/
	[UsedImplicitly]
	void Start () {

		if (_nodes == null || _nodes.Length == 0) BakeNodes();
		_last_update = updateInterval;
	}

	[UsedImplicitly]
	void Update () {
		if (_nodes == null || _nodes.Length == 0 || (_last_update <= 0 && dynamicNodes)) {
			_last_update = updateInterval;
			BakeNodes();
		}
		if (dynamicNodes) _last_update -= Time.deltaTime;
	}

	[UsedImplicitly]
	void OnDrawGizmosSelected () {
		if (_nodes == null) return;

		for (int j = 0; j < _height; j++) {
			for (int i = 0; i < _width; i++) {
				Vector3 pos = NodeWorldPos(i, j);

				Gizmos.color = _nodes[i + j * _width] ? Color.green : Color.red;
				Gizmos.DrawWireSphere(pos, obstaclePadding);
			}
		}
	}

	/**
	 * # Private Methods
	*/

	public void BakeNodes ()
	{

		Vector3 bounds = GetLoaclBoundingBox(col);

		_width = Mathf.FloorToInt(bounds.x * resolution);
		_height = Mathf.FloorToInt(bounds.z * resolution);

		_ratio_x = bounds.x / _width;
		_ratio_y = bounds.z / _height;

		_nodes = new bool[_width * _height];
		if (!dynamicNodes)
			_path_nodes = new PathNode[_width * _height];

		for (int j = 0; j < _height; j++) {
			for (int i = 0; i < _width; i++) {

				Vector3 pos = NodeWorldPos(i, j);

				if (Physics.OverlapSphere(pos, obstaclePadding, obstacleLayer).Length > 0) {
					_nodes[i + j * _width] = false;
				} else {
					_nodes[i + j * _width] = true;
				}

				if (!dynamicNodes)
					_path_nodes[i + j * _width] = new PathNode(new Vector2(i, j), _nodes[i + j * _width], pos);

			}
		}
	}

	public void ClearNodes () {
		_height = 0;
		_width = 0;
		_nodes = null;
	}

	Vector3 NodeWorldPos (int x, int y) {
		return transform.position + transform.right * ((x - _width * .5f) * _ratio_x + (_ratio_x * .5f)) + transform.forward * ((y - _height * .5f) * _ratio_y + (_ratio_y * .5f));
	}

	Vector3 GetLoaclBoundingBox (Collider collider) {
		if (col is BoxCollider)
			return ((BoxCollider) col).size;

		if (col is SphereCollider) {
			var radius = ((SphereCollider)col).radius;
			return new Vector3(radius * 2, radius * 2, radius * 2);
		}

		if (col is CapsuleCollider) {
			var radius = ((CapsuleCollider)col).radius;
			var height = ((CapsuleCollider)col).height;
			var direction = ((CapsuleCollider)col).direction;

			var directionArray = new Vector3[] { Vector3.right, Vector3.up, Vector3.forward };
			var result = new Vector3();
			for (int i = 0; i < 3; i++) {
				if (i == direction)
					result += directionArray[i] * height;
				else
					result += directionArray[i] * radius * 2;
			}
			return result;
		}

		if (col is MeshCollider) {
			return ((MeshCollider) col).sharedMesh.bounds.size;
		}

		return Vector3.zero;
	}

	public void HightlightNode(Vector2 pos)
	{
		Gizmos.color = Color.magenta;
		Gizmos.DrawWireSphere(NodeWorldPos((int)pos.x, (int)pos.y), obstaclePadding*1.01f);
	}

	public Vector2 ClosestNodeToPoint (Vector3 pos)
	{
		// transform 'pos' to be relative to the plane.
		pos -= transform.position;
		pos += col.bounds.extents;
		// Subtract in the x and y offsets
		pos -= transform.right * _ratio_x * .5f + transform.forward * _ratio_y * .5f;
		// translate to node coordinates
		pos *= resolution;
		// round and return
		return new Vector2(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.z));
	}

}
