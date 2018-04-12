using UnityEngine;

[System.Serializable]
public class PathNode
{
	public PathNode(Vector2 grid_pos,
									bool walkable,
									Vector3 world_pos,
									float _gCost = float.MaxValue,
									float _hCost = float.MaxValue,
									float _fCost = float.MaxValue)
	{
		_grid_pos = grid_pos;
		_walkable = walkable;
		_world_pos = world_pos;

		parent = this;
		gCost = _gCost;
		hCost = _hCost;
		fCost = _fCost;
	}

	readonly Vector2 _grid_pos;
	readonly bool _walkable;
	readonly Vector3 _world_pos;
	public Vector2 GridPos
	{
		get { return _grid_pos; }
	}
	public bool Walkable
	{
		get { return _walkable; }
	}
	public Vector3 WorldPos
	{
		get { return _world_pos; }
	}

	public PathNode parent;
	public float gCost;
	public float hCost;
	public float fCost;
}
