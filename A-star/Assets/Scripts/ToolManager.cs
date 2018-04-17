using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
public class SetActive : UnityEvent<bool> {
}

[System.Serializable]
public class ToolUISet {
	public MonoBehaviour component;
	public Button toolBtn;
	public bool active;
}

[System.Serializable]
public class ToggleBtn {
	public bool active;
	public Button button;
	public Image btnImage;
	public Sprite activeImage;
	public Sprite inactiveImage;
	public SetActive toggleEvent;
}

public class ToolManager : MonoBehaviour {

	[SerializeField] Color activeColor = Color.green;
	[SerializeField] Color inactiveColor = Color.gray;

	[SerializeField] ToolUISet[] singleSelectTools;
	[SerializeField] ToggleBtn[] toggleableTools;

	[UsedImplicitly]
	void Start () {

		foreach (var tool in singleSelectTools) {
			tool.toolBtn.GetComponent<Image>().color = tool.active ? activeColor : inactiveColor;
			tool.component.enabled = tool.active;

			tool.toolBtn.onClick.AddListener(() => {
				DeactivateSingleSelectTools();
				tool.active = true;
				tool.toolBtn.GetComponent<Image>().color = tool.active ? activeColor : inactiveColor;
				tool.component.enabled = tool.active;
			});
		}

		foreach (var toggleBtn in toggleableTools) {

			UpdateToggleButton(toggleBtn);

			toggleBtn.button.onClick.AddListener(() => {
				toggleBtn.active = !toggleBtn.active;
				UpdateToggleButton(toggleBtn);
			});

		}

	}

	void UpdateToggleButton(ToggleBtn btn)
	{
		if (btn.btnImage) {
			btn.btnImage.sprite = btn.active ? btn.activeImage : btn.inactiveImage;
		}

		btn.button.GetComponent<Image>().color = btn.active ? activeColor : inactiveColor;
		if (btn.toggleEvent != null) btn.toggleEvent.Invoke(btn.active);
	}

	void DeactivateSingleSelectTools () {
		foreach (var tool in singleSelectTools) {
			tool.active = false;
			tool.toolBtn.GetComponent<Image>().color = tool.active ? activeColor : inactiveColor;
			tool.component.enabled = tool.active;
		}
	}

}