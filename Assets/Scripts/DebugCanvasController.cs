using System;
using Techniques;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugCanvasController : MonoBehaviour
{
    public Transform headsetTransform;
    public Transform canvasTransform;
    public Transform optionsCanvasTransform;

    public SharedDebugData sharedDebugData;

    public TMP_Text headsetAngleText;
    public TMP_Text gainText;
    public TMP_Text eulerAnglesText;
    public TMP_Text velocityText;
    public TMP_Text cumulativeRotationText;
    public TMP_Text initialCumulativeRotationText;
    public TMP_Text previousHeadsetRotationText;
    public TMP_Text yawText;
    public TMP_Text expectedAngleText;
    public TMP_Text Technique;
    public TMP_Text ApplyTechnique;
    public TMP_Text GainApplyAngleTechnique;

    //UI controls
    public Slider gainSlider;
    public TMP_Dropdown techniqueDropdown;
    public TMP_Dropdown applyTechniqueDropdown;
    public TMP_Dropdown gainApplyAngleDropdown;
    public Toggle applyGainToggle;
    public Toggle disableAmpIfIsSettledToggle;
    public Button setReturnGainButton;

    private ScaleHeadRotation scaleHeadRotation;

    void Start()
    {
        // Get reference to ScaleHeadRotation
        scaleHeadRotation = FindObjectOfType<ScaleHeadRotation>();

        TMP_Text[] texts =
        {
            headsetAngleText,
            gainText,
            eulerAnglesText,
            yawText,
            velocityText,
            cumulativeRotationText,
            initialCumulativeRotationText,
            previousHeadsetRotationText
        };

        // Populate each dropdown with the appropriate enum values
        PopulateDropdownWithEnumValues(techniqueDropdown, typeof(GainTechniquesEnum));
        PopulateDropdownWithEnumValues(applyTechniqueDropdown, typeof(GainApplyTechnique));
        PopulateDropdownWithEnumValues(gainApplyAngleDropdown, typeof(GainApplyAngleTechnique));

        // Set initial values of dropdowns to match the current enum values in ScaleHeadRotation
        gainSlider.value = sharedDebugData.gain;
        techniqueDropdown.value = (int)scaleHeadRotation.Technique;
        applyTechniqueDropdown.value = (int)scaleHeadRotation.ApplyTechnique;
        gainApplyAngleDropdown.value = (int)scaleHeadRotation.GainApplyAngle;
        applyGainToggle.isOn = scaleHeadRotation.isApplyGain;
        disableAmpIfIsSettledToggle.isOn = scaleHeadRotation.disableAmpIfIsSettled;

        gainSlider.onValueChanged.AddListener(OnGainSliderChanged);
        techniqueDropdown.onValueChanged.AddListener(OnTechniqueDropdownChanged);
        applyTechniqueDropdown.onValueChanged.AddListener(OnApplyTechniqueDropdownChanged);
        gainApplyAngleDropdown.onValueChanged.AddListener(OnGainApplyAngleDropdownChanged);
        applyGainToggle.onValueChanged.AddListener(OnApplyGainToggleChanged);
        disableAmpIfIsSettledToggle.onValueChanged.AddListener(
            OnDisableAmpIfIsSettledToggleChanged
        );
        setReturnGainButton.onClick.AddListener(OnSetReturnGainButtonClicked);

        int initialPosition = 0;
        int spacing = 10;

        for (int i = 0; i < texts.Length; i++)
        {
            SetTextProperties(texts[i], initialPosition - i * spacing);
        }

        RectTransform canvasRect = canvasTransform.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(400, 390);
        canvasTransform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
    }

    void Update()
    {
        gainText.text = $"Gain: {sharedDebugData.gain:F2}";
        headsetAngleText.text = $"Headset Angle: {sharedDebugData.headsetAngle:0}";
        eulerAnglesText.text = $"Euler Angles Y: {sharedDebugData.yaw:0}";
        yawText.text = $"Yaw: {sharedDebugData.yaw:0}";

        velocityText.text = $"Velocity:{sharedDebugData.velocity:0}";

        cumulativeRotationText.text = $"isSettling: {sharedDebugData.isSettling}";

        initialCumulativeRotationText.text =
            $"Rotation Delta: {sharedDebugData.headsetRotationDelta.eulerAngles.y:F3}";

        previousHeadsetRotationText.text =
            $"relative Headset Rotation: {sharedDebugData.relativeHeadsetRotation.eulerAngles:0}";

        expectedAngleText.text =
            $"Expected Angle = {sharedDebugData.gain * sharedDebugData.headsetAngle:0}"; //(Gain * Headset_Angle)

        Technique.text = $"Technique: {sharedDebugData.Technique}";
        ApplyTechnique.text = $"Apply Technique: {sharedDebugData.ApplyTechnique}";
        GainApplyAngleTechnique.text =
            $"Gain Apply Angle: {sharedDebugData.GainApplyAngleTechnique}";

        float debugCanvasDistance = 8.0f; // Distance for the debug canvas
        // float optionsCanvasDistance = 6.0f; // Distance for the options canvas
        float verticalOffset = 0.0f; // Vertical offset to avoid overlap
        // float horizontalOffset = 1.2f; // Horizontal offset for better separation

        // Position and rotate the debug canvas
        canvasTransform.position =
            headsetTransform.position
            + headsetTransform.forward * debugCanvasDistance
            + Vector3.up * verticalOffset;
        canvasTransform.rotation = Quaternion.LookRotation(
            canvasTransform.position - headsetTransform.position
        );
    }

    private void SetTextProperties(TMP_Text textComponent, float yOffset)
    {
        RectTransform rectTransform = textComponent.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(400, 40);
        rectTransform.anchoredPosition = new Vector2(0, yOffset);
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
        rectTransform.pivot = new Vector2(0, 1);
        textComponent.color = Color.black;
    }

    public void OnGainSliderChanged(float value)
    {
        scaleHeadRotation.SetGain(value);
    }

    public void OnTechniqueDropdownChanged(int index)
    {
        scaleHeadRotation.SetTechnique(index);
    }

    public void OnApplyTechniqueDropdownChanged(int index)
    {
        scaleHeadRotation.SetApplyTechnique(index);
    }

    public void OnGainApplyAngleDropdownChanged(int index)
    {
        scaleHeadRotation.SetGainApplyAngle(index);
    }

    void OnApplyGainToggleChanged(bool isOn)
    {
        scaleHeadRotation.SetIsApplyGain(isOn);
    }

    void OnDisableAmpIfIsSettledToggleChanged(bool isOn)
    {
        scaleHeadRotation.SetDisableAmpIfIsSettled(isOn);
    }

    void OnSetReturnGainButtonClicked()
    {
        scaleHeadRotation.Technique = GainTechniquesEnum.constant;
        techniqueDropdown.value = (int)scaleHeadRotation.Technique;
        Debug.Log("scaleHeadRotation.VirtualAngle" + scaleHeadRotation.VirtualAngle);
        Debug.Log("scaleHeadRotation.PhysicalAngle" + scaleHeadRotation.PhysicalAngle);
        if (scaleHeadRotation.VirtualAngle < 0 && scaleHeadRotation.PhysicalAngle > 0)
        {
            scaleHeadRotation.SetGain(
                Mathf.Abs(
                    (180 + (scaleHeadRotation.VirtualAngle + 180)) / scaleHeadRotation.PhysicalAngle
                )
            );
        }
        else
        {
            scaleHeadRotation.SetGain(
                Mathf.Abs(scaleHeadRotation.VirtualAngle)
                    / Mathf.Abs(scaleHeadRotation.PhysicalAngle)
            );
        }
    }

    void PopulateDropdownWithEnumValues(TMP_Dropdown dropdown, Type enumType)
    {
        dropdown.ClearOptions();
        // Get all enum names as a list of strings
        var enumNames = Enum.GetNames(enumType);

        // Convert to a list and assign to the dropdown options
        dropdown.AddOptions(new System.Collections.Generic.List<string>(enumNames));
    }
}
