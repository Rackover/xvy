using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

public class MakeInputs
{

    [MenuItem("Tools/Make inputs")]
    public static void MakeInputMappings()
    {
        string filePath = System.IO.Path.GetFullPath("ProjectSettings/InputManager.asset");

        if (File.Exists(filePath))
        {
            StringBuilder inputBuilder = new StringBuilder();

            inputBuilder.AppendLine(
@"%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!13 &1
InputManager:
  m_ObjectHideFlags: 0
  serializedVersion: 2
  m_Axes:");

            for (int playerIndex = 0; playerIndex < Level.PLAYERS; playerIndex++)
            {
                for (int i = 0; i < NativeUnityInputMappings.axes.Length; i++)
                {
                    AddInput(inputBuilder, playerIndex, ref NativeUnityInputMappings.axes[i]);
                }

                for (int i = 0; i < NativeUnityInputMappings.buttons.Length; i++)
                {
                    AddInput(inputBuilder, playerIndex, ref NativeUnityInputMappings.buttons[i]);
                }
            }

            string result = inputBuilder.ToString();

            Debug.Log("Done writing inputs to "+filePath);
            File.WriteAllText(filePath, result);

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }
        else
        {
            Debug.LogError("Does not exist: " + filePath);
        }
    }

    private static void AddInput(StringBuilder builder, int playerIndex, ref NativeUnityInputMappings.Button button)
    {
        builder.AppendLine("  - serializedVersion: 3");
        builder.AppendLine("    m_Name: " + NativeUnityInputMappings.GetVirtualInputName(button.name, playerIndex));
        builder.AppendLine("    descriptiveName:");
        builder.AppendLine("    descriptiveNegativeName:");

        builder.AppendLine("    negativeButton:");
        builder.AppendLine("    positiveButton: joystick "+(playerIndex + 1)+" "+button.path);
        builder.AppendLine("    altNegativeButton:");
        builder.AppendLine("    altPositiveButton:");

        builder.AppendLine("    gravity: 1000");
        builder.AppendLine("    dead: 0.001");
        builder.AppendLine("    sensitivity: 1000");
        builder.AppendLine("    snap: 0");
        builder.AppendLine("    invert: 0");
        builder.AppendLine("    type: 0");
        builder.AppendLine("    axis: 0");
        builder.AppendLine("    joyNum: " + (playerIndex + 1));
    }

    private static void AddInput(StringBuilder builder, int playerIndex, ref NativeUnityInputMappings.Axis axis)
    {
        builder.AppendLine("  - serializedVersion: 3");
        builder.AppendLine("    m_Name: " + NativeUnityInputMappings.GetVirtualInputName(axis.name, playerIndex));
        builder.AppendLine("    descriptiveName:");
        builder.AppendLine("    descriptiveNegativeName:");
        builder.AppendLine("    negativeButton:");
        builder.AppendLine("    positiveButton:");
        builder.AppendLine("    altNegativeButton:");
        builder.AppendLine("    altPositiveButton:");

        builder.AppendLine("    gravity: 0");
        builder.AppendLine("    dead: 0.1");
        builder.AppendLine("    sensitivity: 1");
        builder.AppendLine("    snap: 0");
        builder.AppendLine("    invert: 0");
        builder.AppendLine("    type: 2");
        builder.AppendLine("    axis: "+axis.index);
        builder.AppendLine("    joyNum: "+ (playerIndex + 1));
    }
}
