using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyboardAnimation : MonoBehaviour
{
    public Color keyPressedColor;
    private Color keyDefaultColor = Color.white;
    public float keyColorChangeSpeed = 0.05f;

    private List<Image> selectedKeys = new List<Image>();

    void Update()
    {
        UpdateSelectedImages();
    }
    public void AddSelectedImage(Image image)
    {
        image.color = keyPressedColor;
        foreach (var i in selectedKeys)
        {
            if (image == i)
            {
                return;
            }
        }
        selectedKeys.Add(image);
    }
    private void UpdateSelectedImages()
    {
        for (int i = 0; i < selectedKeys.Count; i++)
        {
            Color c = selectedKeys[i].color;
            selectedKeys[i].color = new Color(Mathf.Clamp01(c.r + keyColorChangeSpeed), Mathf.Clamp01(c.g + keyColorChangeSpeed), Mathf.Clamp01(c.b + keyColorChangeSpeed));
            if (selectedKeys[i].color == keyDefaultColor)
            {
                selectedKeys.RemoveAt(i);
                i--;
            }
        }

    }
    
}
