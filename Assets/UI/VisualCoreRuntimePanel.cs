using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

/// <summary>
/// Runtime UI Toolkit panel to switch presets and toggle action types.
/// Attach this to a UIDocument root VisualElement.
/// </summary>
public class VisualCoreRuntimePanel : MonoBehaviour {
    public PresetController presetController;
    public List<VisualPresetSO> presets = new();
    public RuntimeActionRouter actionRouter;

    void OnEnable(){
        var root = GetComponent<UIDocument>()?.rootVisualElement;
        if (root == null) return;

        root.Clear();
        var label = new Label("Visual Core Control Panel");
        label.style.fontSize = 18;
        root.Add(label);

        // Preset dropdown
        var presetDrop = new DropdownField("Presets", presets.ConvertAll(p => p.name), 0);
        root.Add(presetDrop);

        var fadeToggle = new Toggle("Crossfade");
        fadeToggle.value = true;
        root.Add(fadeToggle);

        var fadeTime = new FloatField("Fade Time (s)");
        fadeTime.value = 1.0f;
        root.Add(fadeTime);

        var applyBtn = new Button(() => {
            var sel = presetDrop.index;
            if (sel >= 0 && sel < presets.Count) {
                var p = presets[sel];
                if (fadeToggle.value)
                    presetController?.CrossfadeTo(p, fadeTime.value);
                else
                    presetController?.Apply(p);
            }
        }) { text = "Apply Preset" };
        root.Add(applyBtn);

        root.Add(new Label("Actions:"));
        foreach (UserActionType action in System.Enum.GetValues(typeof(UserActionType))) {
            var toggle = new Toggle(action.ToString());
            toggle.value = actionRouter?.selected.Contains(action) ?? false;
            toggle.RegisterValueChangedCallback(evt => {
                if (evt.newValue)
                    actionRouter?.selected.Add(action);
                else
                    actionRouter?.selected.Remove(action);
            });
            root.Add(toggle);
        }
    }
}
