using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DynamicText : MonoBehaviour
{
    [SerializeField]
    float randomTextChance;

    private string fullText;
    private int index;
    private TMP_Text text;

    private int frame;

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<TMP_Text>();
        fullText = text.text;

        // Auto size for now, so we can get the optimum size
        text.enableAutoSizing = true;

        // Make transparent so we can get the text size later
        var color = text.color;
        text.color = new Color(color.r, color.g, color.b, 0f);

        index = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (++frame < 5)
        {
            return;
        }

        // Fix the size to what the final size would be
        if (frame == 5)
        {            
            var size = text.fontSize;  // Should be the correct auto size now           
            text.enableAutoSizing = false;
            text.fontSize = size;
            text.text = "";

            // Make visible again
            var color = text.color;
            text.color = new Color(color.r, color.g, color.b, 1f);
        }

        if (index < fullText.Length && Random.value > 1 - randomTextChance)
        {
            text.text = fullText.Substring(0, ++index);
        }
    }
}
