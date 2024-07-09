using UnityEngine;
using System.Collections;

public class BlinkingLight : MonoBehaviour {

	[SerializeField]
	private Light pointLight;

    [SerializeField]
    private LensFlare flare;

    [SerializeField]
	private float speed = 1f;

	private float baseIntensity = 0f;

	void Awake()
	{
		baseIntensity = pointLight ? pointLight.intensity : flare.brightness;
	}

	void Update () {
		float intensity = baseIntensity * ((1f + Mathf.Sin(Time.time * speed * Mathf.PI)) / 2f); 

		if (pointLight)
		{
			pointLight.intensity = intensity;
		}
		else
		{
			flare.brightness = intensity;
		}
    }
}
