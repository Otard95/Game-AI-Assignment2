using UnityEngine;

[System.Serializable]
public struct PathNode
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

		parent = grid_pos;
		gCost = _gCost;
		hCost = _hCost;
		fCost = _fCost;
	}

	Vector2 _grid_pos;
	bool _walkable;
	Vector3 _world_pos;
	public Vector2 GridPos
	{
		get { return _grid_pos; }
		private set { _grid_pos = value; }
	}
	public bool Walkable
	{
		get { return _walkable; }
		private set { _walkable = value; }
	}
	public Vector3 WorldPos
	{
		get { return _world_pos; }
		private set { _world_pos = value; }
	}

	public Vector2 parent;
	public float gCost;
	public float hCost;
	public float fCost;
}
