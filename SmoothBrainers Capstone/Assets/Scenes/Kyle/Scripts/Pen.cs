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
    [Range(0.01f, 0.1f)]
    public float penWidth = 0.01f;
    float[] penWidths = { 0.01f, 0.02f, 0.04f };
    int currentWidthIndex = 0;
    public Color[] penColors;

    List<LineRenderer> drawings = new List<LineRenderer>();
    private LineRenderer currentDrawing;
    private int index;
    private int currentColorIndex;

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

        //if (Input.GetKeyDown(KeyCode.C))
        //{
        //    ClearAll();
        //}
        //else if (Input.GetKeyDown(KeyCode.U))
        //{
        //    UndoAction();
        //}
        //else if (Input.GetKeyDown(KeyCode.O))
        //{
        //    ShowDrawing();
        //}
        //else if (Input.GetKeyDown(KeyCode.P))
        //{
        //    HideDrawing();
        //}
        //else if (Input.GetKeyDown(KeyCode.T))
        //{
        //    SwitchColor();
        //}
        //else if (Input.GetKeyDown(KeyCode.W))
        //{
        //    ChangeWidth();
        //}
    }

    private void Draw()
    {
        if (currentDrawing == null)
        {
            index = 0;
            currentDrawing = new GameObject().AddComponent<LineRenderer>();
            currentDrawing.material = drawingMaterial;
            currentDrawing.startColor = currentDrawing.endColor = penColors[currentColorIndex];
            currentDrawing.startWidth = currentDrawing.endWidth = penWidth;
            currentDrawing.positionCount = 1;
            currentDrawing.SetPosition(0, tip.position);

            drawings.Add(currentDrawing);
        }
        else
        {
            var currentPos = currentDrawing.GetPosition(index);
            if (Vector3.Distance(currentPos, tip.position) > 0f)
            {
                index++;
                currentDrawing.positionCount = index + 1;
                currentDrawing.SetPosition(index, tip.position);
            }
        }
    }

    public void SwitchColor()
    {
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

    public void ChangeWidth()
    {
        currentWidthIndex = (currentWidthIndex + 1) % penWidths.Length;
        penWidth = penWidths[currentWidthIndex];
    }

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

    public void UndoAction()
    {
        if (drawings.Count > 0)
        {
            int lastIndex = drawings.Count - 1;
            Destroy(drawings[lastIndex].gameObject);
            drawings.RemoveAt(lastIndex);
        }
    }

    public void ShowHideDrawing()
    {
        if (drawings.Count > 0)
        {
            if (showDrawing)
            {
                foreach (var drawing in drawings)
                {
                    drawing.enabled = false;
                }
                showDrawing = false;
            }
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
        if (other.gameObject.name == rightThumb.name && startDrawing == true)
        {
            drawing = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        drawing = false;
    }
}