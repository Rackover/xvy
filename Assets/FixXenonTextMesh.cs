using UnityEngine;
using System.Collections;

public class FixXenonTextMesh : MonoBehaviour {

	[SerializeField]
	UnityEngine.UI.Text textMesh;

	[SerializeField]
	private TextAnchor alignment;

	[SerializeField]
	private bool useRichText = false;

	void OnEnable()
    {
#if X360

		textMesh.alignment = alignment;
		textMesh.supportRichText = useRichText;
#endif
	}
}
