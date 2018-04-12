using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class PathFinder : MonoBehaviour {

	[SerializeField] bool visualizeAlgorithm = false;

	public delegate void PathCallback (Vector3[] path);

	#region Singelton

	static PathFinder _instance;

	public static PathFinder Instance
	{
		get { return _instance; }
	}

	void Awake ()
	{
		_instance = this;
	}

	#endregion

	public void GetPath (Vector3 from, Vector3 to, NavSurface targetSurface, PathCallback callback)
	{

		PathNode[] nodes = targetSurface.PathNodes;
		Vector2 size = targetSurface.Size;

		Vector2 start = targetSurface.ClosestNodeToPoint(from);
		Vector2 end = targetSurface.ClosestNodeToPoint(to);

		StartCoroutine(AStar(start, end, size, nodes, callback));

	}

	IEnumerator AStar (Vector2 start, Vector2 end, Vector2 size, PathNode[] nodes, PathCallback callback) {

		PathNode startNode = nodes[(int) (start.x + start.y * size.x)];
		PathNode endNode = nodes[(int) (end.x + end.y * size.x)];
		bool pathFound = false;

		// The set of nodes already evaluated
		List<PathNode> closedList = new List<PathNode>();

		// The set of currently discovered nodes that are not evaluated yet.
		List<PathNode> openList = new List<PathNode>();
		// Initially, only the start node is known.
		openList.Add(startNode);

		// The cost of going from start to start is zero.
		startNode.gCost = 0;

		// For the first node, that value is completely heuristic.
		startNode.hCost = Vector3.Distance(startNode.WorldPos, endNode.WorldPos);
		startNode.fCost = startNode.gCost + startNode.hCost;

		// while openSet is not empty
		while (openList.Count > 0) {
			// current:= the node in openSet having the lowest fScore[] value
			PathNode current = GetBestNode(openList);

			// if we found the path
			if (current == endNode) {
				// return reconstructed path
				callback(ReconstructPath(current));
				pathFound = true;
				break;
			}

			// remove the current node from the open list
			openList.Remove(current);

			// and add it to the closed list
			closedList.Add(current);

			// for each neighbor of current
			for (int i = (int) Mathf.Max(current.GridPos.x - 1, 0); i < (int) Mathf.Min(current.GridPos.x + 1, size.x - 1); i++) {
				for (int j = (int) Mathf.Max(current.GridPos.y - 1, 0); j < (int) Mathf.Min(current.GridPos.y + 1, size.y - 1); j++) {

					PathNode neighbor = nodes[(int)(i + j * size.x)];

					// Allthough current is in the closed list and would be ignored this is cheaper for the CPU
					if (neighbor == current)
						continue;

					// if neighbor in closedSet
					if (!neighbor.Walkable || closedList.Contains(neighbor))
						continue;
					
						// if neighbor not in openList: Discover a new node. Add it to open list
					if (!openList.Contains(neighbor)) { 
						openList.Add(neighbor);
						neighbor.hCost = Vector3.Distance(neighbor.WorldPos, endNode.WorldPos);
					}

					// The distance from start to a neighbor
					// the "dist_between" function may vary as per the solution requirements.
					float newGCost = current.gCost + Vector3.Distance(current.WorldPos, neighbor.WorldPos);

					// if gCost is less then the current gCost keep the new else continue
					if (newGCost > neighbor.gCost)
						continue;
					
					// // This path is the best until now. Record it!
					neighbor.parent = current;
					neighbor.gCost = newGCost;

					// update fCost for neighbor
					neighbor.fCost = neighbor.gCost + neighbor.hCost;

				}
			}
		}

		if (!pathFound)
			callback(null);

		yield return null;

	}

	PathNode GetBestNode (List<PathNode> nodes) {

		PathNode best = nodes[0];

		foreach (var node in nodes) {
			if (node.fCost < best.fCost)
				best = node;
			else if (node.fCost == best.fCost) {
				if (node.hCost < best.hCost)
					best = node;
			}
		}

		return best;

	}

	void CalcFCost (ref List<PathNode> nodes) {
		foreach (var node in nodes) {
			node.fCost = node.gCost + node.hCost;
		}
	}

	Vector3[] ReconstructPath (PathNode current) {

		//		total_path:= [current]
		List<Vector3> final_path = new List<Vector3>();

		//		while current in cameFrom.Keys:
		while (current.parent != current) {
			//        current:= cameFrom[current]
			final_path.Add(current.WorldPos);
			//				total_path.append(current)
			current = current.parent;
		}
		//		return total_path
		return final_path.ToArray();

	}

}
