using JetBrains.Annotations;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class NavSurface : MonoBehaviour {

	/**
	 * # Unity Proporties
	*/
	[SerializeField] int resolution = 2;
	[SerializeField] float obstaclePadding = .3f;
	[SerializeField] LayerMask obstacleLayer = 8;

	[Tooltip("Continuasly update nodes at runtime")]
	[SerializeField]
	bool dynamicNodes = false;

	[Tooltip("In seconds")]
	[SerializeField]
	float updateInterval = 0.01f;

	/**
	 * # Persistent fields
	*/
	[SerializeField] [HideInInspector] bool[] _nodes;
	[SerializeField] [HideInInspector] PathNode[] _path_nodes;

	[SerializeField] [HideInInspector] int _width;
	[SerializeField] [HideInInspector] int _height;
	[SerializeField] [HideInInspector] float _translation_x;
	[SerializeField] [HideInInspector] float _translation_y;

	/**
	 * # Proporties
	*/
	public bool[] Nodes {
		get { return _nodes; }
	}

	public Vector2 Size { get { return new Vector2(_width, _height); } }

	/**
	 * # Componets
	*/

	Collider col;

	/**
	 * # Private Fields
	*/
	float _last_update;

	/**
	 * # Unity Methods
	*/
	[UsedImplicitly]
	void Start () {
		col = GetComponent<Collider>();
		if (_nodes == null || _nodes.Length == 0) BakeNodes();
		_last_update = updateInterval;
		_path_nodes = GeneratePathNodes();
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

	public void BakeNodes () {

		Vector3 bounds = GetLoaclBoundingBox();

		_width = Mathf.FloorToInt(bounds.x * resolution);
		_height = Mathf.FloorToInt(bounds.z * resolution);

		_translation_x = bounds.x / _width;
		_translation_y = bounds.z / _height;

		_nodes = new bool[_width * _height];
		if (!dynamicNodes)
			_path_nodes = new PathNode[_width * _height];
		else _path_nodes = null;

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
		_path_nodes = null;
	}

	PathNode[] GeneratePathNodes () {

		var nodes = new PathNode[_nodes.Length];

		for (int j = 0; j < _height; j++) {
			for (int i = 0; i < _width; i++) {

				nodes[i + j * _width] = new PathNode(new Vector2(i, j), _nodes[i + j * _width], NodeWorldPos(i,j));

			}
		}

		return nodes;
	}

	Vector3 NodeWorldPos (int x, int y) {

		Vector3 bounds = GetLoaclBoundingBox() / 2;

		return transform.position + // base position
					 transform.right * (x * _translation_x + _translation_x * .5f - bounds.x) + // Local x to world
					 transform.forward * (y * _translation_y + _translation_y * .5f - bounds.z); // Local y to world

	}
	public Vector3 NodeWorldPos (Vector2 node) {

		Vector3 bounds = GetLoaclBoundingBox() / 2;

		return transform.position + // base position
					 transform.right * (node.x * _translation_x + _translation_x * .5f - bounds.x) + // Local x to world
					 transform.forward * (node.y * _translation_y + _translation_y * .5f - bounds.z); // Local y to world

	}

	Vector3 GetLoaclBoundingBox () {

		if (!col)
			col = GetComponent<Collider>();

		if (col is BoxCollider) {
			Vector3 size =  ((BoxCollider) col).size;
			return new Vector3(size.x * transform.localScale.x, size.y * transform.localScale.y, size.z * transform.localScale.z);
		}

		if (col is SphereCollider) {
			var radius = ((SphereCollider)col).radius;
			return new Vector3(radius * 2 * transform.localScale.x, radius * 2 * transform.localScale.y, radius * 2 * transform.localScale.z);
		}

		if (col is CapsuleCollider) {
			var radius = ((CapsuleCollider)col).radius;
			var height = ((CapsuleCollider)col).height;
			var direction = ((CapsuleCollider)col).direction;

			var directionArray = new [] { Vector3.right, Vector3.up, Vector3.forward };
			var result = new Vector3();
			for (int i = 0; i < 3; i++) {
				if (i == direction)
					result += directionArray[i] * height;
				else
					result += directionArray[i] * radius * 2;
			}

			return new Vector3(result.x * transform.localScale.x, result.y * transform.localScale.y, result.z * transform.localScale.z);
		}

		if (col is MeshCollider) {
			Vector3 size = ((MeshCollider) col).sharedMesh.bounds.size;
			return new Vector3(size.x * transform.localScale.x, size.y * transform.localScale.y, size.z * transform.localScale.z);
		}

		return Vector3.zero;
	}

	public void HightlightNode (Vector2 pos) {

		Gizmos.color = Color.magenta;
		Gizmos.DrawWireSphere(NodeWorldPos((int) pos.x, (int) pos.y), obstaclePadding * 1.01f);

	}

	public void HightlightNode (Vector2 pos, Color color) {

		Gizmos.color = color;
		Gizmos.DrawWireSphere(NodeWorldPos((int) pos.x, (int) pos.y), obstaclePadding * 1.01f);

	}

	public void HightlightNode (Vector2 pos, Color color, float size) {

		Gizmos.color = color;
		Gizmos.DrawWireSphere(NodeWorldPos((int) pos.x, (int) pos.y), size);

	}

	public Vector2 ClosestNodeToPoint (Vector3 pos) {

		// transform 'pos' to be relative to the surface.
		pos = pos - transform.position;
		// translate to surface coordinate system and account for x and y translation
		pos = (Vector3.Dot(pos, transform.right) - _translation_x / 2) * Vector3.right + (Vector3.Dot(pos, transform.forward) - _translation_y / 2) * Vector3.forward;
		// Account for object position being centered in its bounds
		pos += GetLoaclBoundingBox() / 2;
		// translate to node coordinates
		pos *= resolution;
		// round and return
		return new Vector2(Mathf.Clamp(Mathf.RoundToInt(pos.x), 0, _width - 1), Mathf.Clamp(Mathf.RoundToInt(pos.z), 0, _height - 1));

	}

}
