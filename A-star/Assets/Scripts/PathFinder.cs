using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Analytics;

public class PathFinder : MonoBehaviour {

	public bool visualizeAlgorithm { get; set; }
	public int stepsPerSecond = 5;
	public float nodeSize = .25f;

	public bool showFinalPath { get; set; }
	public bool untilNextSearch { get; set; }
	public float timeVisible = 3f;

	public delegate void PathCallback (Vector3[] path);

	float[] g, h, f;

	Vector2[] cameFrom;

	List<Vector2> closedList;
	List<Vector2> openList;

	NavSurface _surface;
	List<Vector3> _final_path;
	float _time_shown = 0;

	#region Singelton

	static PathFinder _instance;

	public static PathFinder Instance {
		get { return _instance; }
	}

	void Awake () {
		_instance = this;
	}

	#endregion

	public void GetPath (Vector3 from, Vector3 to, NavSurface targetSurface, PathCallback callback) {

		StopAllCoroutines();

		if (untilNextSearch) {
			_final_path = null;
		}

		_surface = targetSurface;

		bool[] nodes = targetSurface.Nodes;
		Vector2 size = targetSurface.Size;

		Vector2 start = targetSurface.ClosestNodeToPoint(from);
		Vector2 end = targetSurface.ClosestNodeToPoint(to);

		if (g == null) {

			g = new float[(int) (size.x * size.y)];
			h = new float[(int) (size.x * size.y)];
			f = new float[(int) (size.x * size.y)];
			cameFrom = new Vector2[(int) (size.x * size.y)];

			openList = new List<Vector2>();
			closedList = new List<Vector2>();

		} else if (g.Length < size.x * size.y) {

			g = new float[(int) (size.x * size.y)];
			h = new float[(int) (size.x * size.y)];
			f = new float[(int) (size.x * size.y)];
			cameFrom = new Vector2[(int) (size.x * size.y)];

		}

		openList.Clear();
		closedList.Clear();

		for (int i = 0; i < g.Length; i++) {
			g[i] = float.MaxValue;
			h[i] = float.MaxValue;
			f[i] = float.MaxValue;
			cameFrom[i] = Vector2.down;
		}

		StartCoroutine(AStar(start, end, size, nodes, targetSurface, callback));

	}

	IEnumerator AStar (Vector2 start, Vector2 end, Vector2 size, bool[] nodes, NavSurface surface, PathCallback callback) {

		bool pathFound = false;

		// Initially, only the start node is known.
		openList.Add(start);

		// The cost of going from start to start is zero.
		g[Index(start, size.x)] = 0;

		// For the first node, the final and heuristic cost is the same.
		h[Index(start, size.x)] = Vector2.Distance(start, end);
		f[Index(start, size.x)] = h[Index(start, size.x)];

		// while openSet is not empty
		while (openList.Count > 0) {
			// current:= the node in openSet having the lowest fScore[] value
			Vector2 current = GetBestNode(openList, (int)size.x);

			// if we found the path
			if (current == end) {
				// return reconstructed path
				callback(ReconstructPath(current, (int) size.x, surface));
				if (showFinalPath) {
					openList.Clear();
					closedList.Clear();
				}
				pathFound = true;
				break;
			}

			// remove the current node from the open list
			openList.Remove(current);

			// and add it to the closed list
			closedList.Add(current);

			// for each neighbor of current
			for (int i = (int) Mathf.Max(current.x - 1, 0); i <= (int) Mathf.Min(current.x + 1, size.x - 1); i++) {
				for (int j = (int) Mathf.Max(current.y - 1, 0); j <= (int) Mathf.Min(current.y + 1, size.y - 1); j++) {

					Vector2 neighbor = new Vector2(i,j);

					// Allthough current is in the closed list and would be ignored this is cheaper for the CPU
					if (neighbor == current)
						continue;

					// if neighbor in closedSet
					if (!nodes[Index(neighbor, size.x)] || closedList.Contains(neighbor))
						continue;

					// if neighbor not in openList: Discover a new node. Add it to open list
					if (!openList.Contains(neighbor)) {
						openList.Add(neighbor);
						h[Index(neighbor, size.x)] = Vector2.Distance(neighbor, end);
					}

					// The distance from start to a neighbor
					// the "dist_between" function may vary as per the solution requirements.
					float newGCost = g[Index(current, size.x)] + Vector2.Distance(current, neighbor);

					// if gCost is less then the current gCost keep the new else continue
					if (newGCost > g[Index(neighbor, size.x)])
						continue;

					// // This path is the best until now. Record it!
					cameFrom[Index(neighbor, size.x)] = current;
					g[Index(neighbor, size.x)] = newGCost;

					// update fCost for neighbor
					f[Index(neighbor, size.x)] = g[Index(neighbor, size.x)] + h[Index(neighbor, size.x)];

				}
			}

			if (visualizeAlgorithm) { // If we are visualizing the algorithm
																// wait for (1/steps per second) seconds
				yield return new WaitForSeconds(1f / stepsPerSecond);
			}

		}

		if (!pathFound)
			callback(null);

		yield return null;

	}
	
	int Index (Vector2 pos, float width) {
		return (int) (pos.x + pos.y * width);
	}

	Vector2 GetBestNode (List<Vector2> nodes, int width) {

		Vector2 best = nodes[0];

		foreach (var node in nodes) {
			if (f[Index(node, width)] < f[Index(best, width)])
				best = node;
			else if (f[Index(node, width)] == f[Index(best, width)]) {
				if (h[Index(node, width)] < h[Index(best, width)])
					best = node;
			}
		}

		return best;

	}

	Vector3[] ReconstructPath (Vector2 current, int width, NavSurface surface) {

		// total_path:= [current]
		List<Vector3> finalPath = new List<Vector3>();

		while (cameFrom[Index(current, width)] != Vector2.down) {

			finalPath.Add(surface.NodeWorldPos(current));
			current = cameFrom[Index(current, width)];

		}

		// Path is now from end to start so
		finalPath.Reverse();

		if (showFinalPath) {
			_final_path = finalPath;
		}

		// return final_path
		return finalPath.ToArray();

	}

	[UsedImplicitly]
	void OnDrawGizmos () {

		if (visualizeAlgorithm && closedList != null && closedList.Count > 0) {
			foreach (var node in closedList) {
				_surface.HighlightNode(node, Color.red, .2f);
			}
		}

		if (visualizeAlgorithm && openList != null && openList.Count > 0) {
			foreach (var node in openList) {
				_surface.HighlightNode(node, Color.blue, .2f);
			}
		}

		Gizmos.color = Color.green;
		if (showFinalPath && _final_path != null && _final_path.Count > 0) {
			for (int i = 0; i < _final_path.Count - 1; i++) {
				Gizmos.DrawLine(_final_path[i], _final_path[i + 1]);
			}
		}

		if (showFinalPath && !untilNextSearch && _final_path != null) {

			_time_shown += Time.deltaTime;

			if (_time_shown >= timeVisible) {
				_final_path = null;
				_time_shown = 0;
			}

		}

	}

}
