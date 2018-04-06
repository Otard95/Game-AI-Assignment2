using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NavSurface))]
public class NavSurfaceEditor : Editor {

	public override void OnInspectorGUI () {
		DrawDefaultInspector();

		NavSurface nav_surface = (NavSurface) target;

		GUILayout.BeginHorizontal();

		if (GUILayout.Button("Bake Navigation Nodes")) {
			nav_surface.BakeNodes();
		}

		if (GUILayout.Button("Clear Navigation Nodes")) {
			nav_surface.ClearNodes();
		}

		GUILayout.EndHorizontal();

	}

}
