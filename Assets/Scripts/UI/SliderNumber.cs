using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderNumber : MonoBehaviour
{
	Text text_ui;

	private void Start()
	{
		text_ui = GetComponent<Text>();
	}

	public void ChangeNumber(float x)
	{
		text_ui.text = Mathf.RoundToInt(x).ToString();
	}
}