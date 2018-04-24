using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class NavSurface : MonoBehaviour {

	/**
	 * # Unity Proporties
	*/
	[SerializeField] int resolution = 2;
	[SerializeField] float obstaclePadding = .3f;
	[SerializeField] readonly LayerMask obstacleLayer = 256;

	[Tooltip("Continuasly update nodes at runtime")]
	[SerializeField]
	bool dynamicNodes = false;

	[Tooltip("In seconds")] [SerializeField] float updateInterval = 0.01f;

	public bool showNodes { get; set; }

	/**
	 * # Persistent fields
	*/
	[SerializeField] [HideInInspector] bool[] _nodes;
	[SerializeField] [HideInInspector] int[] _zone_map;
	[SerializeField] [HideInInspector] List<int> _zones;

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
	Color[] _node_colors;

	/**
	 * # Unity Methods
	*/
	[UsedImplicitly]
	void Start () {
		col = GetComponent<Collider>();
		if (_nodes == null || _nodes.Length == 0) BakeNodes();
		_last_update = updateInterval;

		_node_colors = new[] { Color.green, Color.cyan, Color.blue, Color.magenta };
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
	void OnDrawGizmos () {
		if (!showNodes || _nodes == null) return;

		for (int j = 0; j < _height; j++) {
			for (int i = 0; i < _width; i++) {
				Vector3 pos = NodeWorldPos(i, j);

				if (!_nodes[i + j * _width]) Gizmos.color = Color.red;
				else Gizmos.color = _node_colors[(_zone_map[i + j * _width] - 1) % _node_colors.Length];

				Gizmos.DrawWireSphere(pos, obstaclePadding);
			}
		}
	}

	/**
	 * # Private Methods
	*/

	public void BakeNodes () {

		Vector3 bounds = GetLocalBoundingBox();

		_width = Mathf.FloorToInt(bounds.x * resolution);
		_height = Mathf.FloorToInt(bounds.z * resolution);

		if (_nodes == null || _height * _width > _nodes.Length) {

			_translation_x = bounds.x / _width;
			_translation_y = bounds.z / _height;

			_nodes = new bool[_width * _height];
			_zone_map = new int[_width * _height];
			_zones = new List<int>();

		}

		_zones.Clear();
		_zones.Add(0);

		for (int y = 0; y < _height; y++) {
			for (int x = 0; x < _width; x++) {

				Vector3 pos = NodeWorldPos(x, y);
				int i = x + y * _width;

				if (Physics.OverlapSphere(pos, obstaclePadding, obstacleLayer).Length > 0) {
					// node is not walkable
					_nodes[i] = false;
					_zone_map[i] = 0;
				} else {
					// node is walkable
					_nodes[i] = true;

					// figure out what zone id this node will have
					if (y - 1 >= 0 && _nodes[i - _width]) { // The above node is walkable use its zone id
						_zone_map[i] = _zone_map[i - _width];

						if (x - 1 >= 0 && _nodes[i - 1] &&
								_zone_map[i - 1] != _zone_map[i] &&
								ResolveZone(_zone_map[i]) != _zone_map[i - 1]) {
							// Zone to above node is connected to zone to the left
							_zones[_zone_map[i - 1]] = _zone_map[i];
						}

					} else if (x - 1 >= 0 && _nodes[i - 1]) { // The node to the laft is walkable use its id
						_zone_map[i] = _zone_map[i - 1];
					} else { // There is no conected zone. use next available id
						_zone_map[i] = _zones.Count;
						_zones.Add(_zone_map[i]);
					}
				}

			}
		}
	}

	int ResolveZone (int initial) {
		if (_zones[initial] == initial) return initial;
		return ResolveZone(_zones[initial]);
	}

	public bool NodesInConectedZones (Vector2 node1, Vector2 node2) {

		int m_zone1 = _zone_map[(int) (node1.x + node1.y * _width)];
		int m_zone2 = _zone_map[(int) (node2.x + node2.y * _width)];

		if (ResolveZone(m_zone1) == ResolveZone(m_zone2))
			return true;
		// else
		return false;

	}

	public void ClearNodes () {
		_height = 0;
		_width = 0;
		_nodes = null;
		_zone_map = null;
		_zones = null;
	}

	Vector3 NodeWorldPos (int x, int y) {

		Vector3 bounds = GetLocalBoundingBox() / 2;

		return transform.position + // base position
					 transform.right * (x * _translation_x + _translation_x * .5f - bounds.x) + // Local x to world
					 transform.forward * (y * _translation_y + _translation_y * .5f - bounds.z); // Local y to world

	}
	public Vector3 NodeWorldPos (Vector2 node) {

		Vector3 bounds = GetLocalBoundingBox() / 2;

		return transform.position + // base position
					 transform.right * (node.x * _translation_x + _translation_x * .5f - bounds.x) + // Local x to world
					 transform.forward * (node.y * _translation_y + _translation_y * .5f - bounds.z); // Local y to world

	}

	Vector3 GetLocalBoundingBox () {

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

	public void HighlightNode (Vector2 pos) {

		Gizmos.color = Color.magenta;
		Gizmos.DrawWireSphere(NodeWorldPos((int) pos.x, (int) pos.y), obstaclePadding * 1.01f);

	}

	public void HighlightNode (Vector2 pos, Color color) {

		Gizmos.color = color;
		Gizmos.DrawWireSphere(NodeWorldPos((int) pos.x, (int) pos.y), obstaclePadding * 1.01f);

	}

	public void HighlightNode (Vector2 pos, Color color, float size) {

		Gizmos.color = color;
		Gizmos.DrawWireSphere(NodeWorldPos((int) pos.x, (int) pos.y), size);

	}

	public Vector2 ClosestNodeToPoint (Vector3 pos) {

		// transform 'pos' to be relative to the surface.
		pos = pos - transform.position;
		// translate to surface coordinate system and account for x and y translation
		pos = (Vector3.Dot(pos, transform.right) - _translation_x / 2) * Vector3.right + (Vector3.Dot(pos, transform.forward) - _translation_y / 2) * Vector3.forward;
		// Account for object position being centered in its bounds
		pos += GetLocalBoundingBox() / 2;
		// translate to node coordinates
		pos *= resolution;
		// round and return
		return new Vector2(Mathf.Clamp(Mathf.RoundToInt(pos.x), 0, _width - 1), Mathf.Clamp(Mathf.RoundToInt(pos.z), 0, _height - 1));

	}

}
