using UnityEngine;
using System.Collections;

public class FixXenonTextMesh : MonoBehaviour
{
#if UNITY_XENON
    [SerializeField]
	UnityEngine.UI.Text textMesh;

	[SerializeField]
	private TextAnchor alignment;

	[SerializeField]
	private bool useRichText = false;
#endif

	void OnEnable()
    {
#if UNITY_XENON
        if (textMesh)
        {
            textMesh.alignment = alignment;

            if (useRichText)
            {
                var reg = new System.Text.RegularExpressions.Regex(@"<\/?color(?:=#[0-z]{6})?>");

                textMesh.text = reg.Replace(textMesh.text, string.Empty);
            }
        }
#endif
	}
}
