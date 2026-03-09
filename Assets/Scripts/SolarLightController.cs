using UnityEngine;
using UnityEngine.Events; // This lets us easily update the UI later!

public class SolarLightController : MonoBehaviour
{

  public bool isOn = true;
  public float[] pattern = { 1f, -1f, 1f };

  private int index = 0;
  private float switchTime = 0f;

  void Start()
  {
    if (isOn)
    {
      TurnOnLight();
    }
    else
    {
      TurnOffLight();
    }
  }

  void Update()
  {
    if (Time.time >= switchTime)
    {
      if (pattern[index] > 0)
      {
        TurnOnLight();
      }
      else
      {
        TurnOffLight();
      }
    }
    switchTime = Time.time + Mathf.Abs(pattern[index]);
    index = (index + 1) % pattern.Length;
  }

  public void TurnOnLight()
  {
    Light light = gameObject.GetComponent<Light>();
    light.intensity = 100f;
  }

  public void TurnOffLight()
  {
    Light light = gameObject.GetComponent<Light>();
    light.intensity = 0f;
  }
}