using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text;
using System;

public class InputPrinter : MonoBehaviour
{
    [SerializeField]
    private Text text;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i <= 8; i++)
        {
            for (int input = 0; input < 20; input++)
            {
                string name = "Joystick" + (i == 0 ? "" : i.ToString()) + "Button" + input;

                KeyCode code = (KeyCode)Enum.Parse(typeof(KeyCode), name);

                sb.Append(i + "=>" + input + " : " + (Input.GetKey(code) ? 1 : 0));

                sb.Append("  ");
            }

            sb.AppendLine();
            sb.AppendLine();
        }

        text.text = sb.ToString();
    }
}
