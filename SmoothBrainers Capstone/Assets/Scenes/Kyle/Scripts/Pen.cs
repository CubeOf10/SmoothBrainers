using RedGirafeGames.Agamotto.Demo.TowerDefenseDemo.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pen : MonoBehaviour
{
    GameObject rightThumb;

    [Header("Pen Properties")]
    public Transform tip;
    public Material drawingMaterial;
    public Material tipMaterial;

    // Width Properties
    [Range(0.01f, 0.1f)]
    public float penWidth = 0.01f;
    float[] penWidths = { 0.01f, 0.02f, 0.04f };
    int currentWidthIndex = 0;

    // Colour Properties
    public Color[] penColors;
    private int currentColorIndex;

    List<LineRenderer> drawings = new List<LineRenderer>();
    private LineRenderer currentDrawing;
    private int index;

    private bool drawing = false;
    private bool showDrawing = true;
    private bool startDrawing = false;



    private void Start()
    {
        currentColorIndex = 0;
        tipMaterial.color = penColors[currentColorIndex];

        rightThumb = GameObject.Find("r_thumb_finger_pad_marker");
    }

    private void Update()
    {
        if (drawing)
        {
            Draw();
        }
        else if (currentDrawing != null)
        {
            currentDrawing = null;
        }
    }

    private void Draw()
    {
        if (currentDrawing == null)
        {
            index = 0;
            currentDrawing = new GameObject().AddComponent<LineRenderer>();

            // Setting properties
            currentDrawing.material = drawingMaterial;
            currentDrawing.startColor = currentDrawing.endColor = penColors[currentColorIndex];
            currentDrawing.startWidth = currentDrawing.endWidth = penWidth;
            currentDrawing.positionCount = 1;
            currentDrawing.SetPosition(0, tip.position);

            // Add the current drawing to the array
            drawings.Add(currentDrawing);
        }
        else
        {
            // The current drawing
            var currentPos = currentDrawing.GetPosition(index);
            if (Vector3.Distance(currentPos, tip.position) > 0f)
            {
                index++;
                currentDrawing.positionCount = index + 1;
                currentDrawing.SetPosition(index, tip.position);
            }
        }
    }

    // Change the colour of the pen
    public void SwitchColor()
    {
        // Loops through an array of available colours
        if (currentColorIndex == penColors.Length - 1)
        {
            currentColorIndex = 0;
        }
        else
        {
            currentColorIndex++;
        }
        tipMaterial.color = penColors[currentColorIndex];
    }

    // Change the width of the pen
    public void ChangeWidth()
    {
        // Loops through an array of set widths
        currentWidthIndex = (currentWidthIndex + 1) % penWidths.Length;
        penWidth = penWidths[currentWidthIndex];
    }

    // Clears all of the drawings in the scene
    public void ClearAll()
    {
        if (drawings.Count  > 0)
        {
            foreach (var drawing in drawings)
            {
                Destroy(drawing.gameObject);
            }
            drawings.Clear();
        }
    }

    // Undo the last drawing
    public void UndoAction()
    {
        if (drawings.Count > 0)
        {
            int lastIndex = drawings.Count - 1;
            Destroy(drawings[lastIndex].gameObject);
            drawings.RemoveAt(lastIndex);
        }
    }

    // Show or Hide all drawings
    public void ShowHideDrawing()
    {
        if (drawings.Count > 0)
        {
            // Hide Drawings
            if (showDrawing)
            {
                foreach (var drawing in drawings)
                {
                    drawing.enabled = false;
                }
                showDrawing = false;
            }

            // Show Drawings
            else
            {
                foreach (var drawing in drawings)
                {
                    drawing.enabled = true;
                }
                showDrawing = true;
            }
        }
    }

    // Start or Stop drawing
    public void StartStopDrawing()
    {
        if (!startDrawing)
        {
            startDrawing = true;
        }
        else
        {
            startDrawing = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // When the user is "pinching" (Colliders on the Index and Thumb)
        if (other.gameObject.name == rightThumb.name && startDrawing == true)
        {
            drawing = true;
        }
    }

    // User stopped "pinching"
    private void OnTriggerExit(Collider other)
    {
        drawing = false;
    }
}