using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PathFinder))]
public class PathFinderEditor : Editor {

	GUIStyle _header_style;

	[UsedImplicitly]
	void Awake () {
		_header_style = new GUIStyle();

		_header_style.fontSize = 11;
		_header_style.fontStyle = FontStyle.Bold;
		_header_style.padding = new RectOffset(2, 2, 4, 4);
	}

	public override void OnInspectorGUI () {

		if (_header_style == null) {
			Awake();
		}

		PathFinder pf = target as PathFinder;

		if (!pf) {
			Debug.LogError("PathFinderEditor - pf (Pathfinder) undefined | 9:11");
			return;
		}

		EditorGUILayout.LabelField("Algorithm", _header_style);

		pf.visualizeAlgorithm = EditorGUILayout.Toggle("Visualize Algorithm",
																									 pf.visualizeAlgorithm);

		using (new EditorGUI.DisabledScope(!pf.visualizeAlgorithm)) {

			pf.stepsPerSecond = EditorGUILayout.IntField("Steps Per Second",
																										pf.stepsPerSecond);

			pf.nodeSize = EditorGUILayout.FloatField("Node Size", pf.nodeSize);
		}

		EditorGUILayout.LabelField("Final Path", _header_style);

		pf.showFinalPath = EditorGUILayout.Toggle("Show Final Path",
																							pf.showFinalPath);

		using (new EditorGUI.DisabledScope(!pf.showFinalPath)) {

			pf.untilNextSearch = EditorGUILayout.Toggle("Until Next Search",
																									pf.untilNextSearch);

			using (new EditorGUI.DisabledScope(pf.untilNextSearch)) {

				pf.timeVisible = EditorGUILayout.FloatField("Time Visible",
																										pf.timeVisible);

			}
		}

	}

}
