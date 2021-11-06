using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualizerPoint : MonoBehaviour
{
    public SpriteRenderer sprite;

    public enum Colormode
    {
        group, confidence
    };

    public Colormode displayMode = Colormode.confidence;

    public Color groupColor;
    public Color confidenceColor;

    public float confidence;
    public int group;

    private void Start()
    {
        if ((confidenceColor != null) || (groupColor != null)) applyColor();
    }

    public void initialize(Color groupColor, float confidence = 0.0f, int group = 0, Colormode mode = Colormode.confidence)
    {
        this.confidence = confidence;
        this.group = group;
        this.groupColor = groupColor;
        this.displayMode = mode;

        // set conf color based on conf
        confidenceColor = new Color(Mathf.Lerp(1, 0, confidence), Mathf.Lerp(0, 1, confidence), 0.0f, 1.0f);

        applyColor();
    }

    // ChangeColorMode applies the new mode and updates the color to match
    public void changeColorMode()
    {
        // progress w/ wraparound
        displayMode += 1;
        if (displayMode > Colormode.confidence)
            displayMode = Colormode.group;

        applyColor();
    }

    private void applyColor()
    {
        switch (displayMode)
        {
            case Colormode.confidence:
                sprite.color = confidenceColor;
                break;
            case Colormode.group:
                sprite.color = groupColor;
                break;
            default:
                Debug.LogError("ERROR: point displaymode not set");
                sprite.color = confidenceColor;
                break;
        }
    }


}
