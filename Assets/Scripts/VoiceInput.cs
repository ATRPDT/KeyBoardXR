using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;
using SwipeKeyboard;


public class VoiceInput : MonoBehaviour
{
	//[SerializeField]
	//private Text hypotheses;

	[SerializeField]
	private Toggle myToggle;

	[SerializeField]
	private Text textField;

	private DictationRecognizer dictationRecognizer;
	public bool isListening = false;

	public CheckmarkAnimation checkmarkAnimation;
	

	void Start()
	{

		dictationRecognizer = new DictationRecognizer();

		//myToggle.onValueChanged.AddListener(StartRecognition);

		dictationRecognizer.DictationResult += (text, confidence) =>
		{
			Debug.LogFormat("Dictation result: {0}", text);
			//textField.text += text + " ";
			//GetComponent<Keybo>
			GetComponent<Keyboard>().inputString.Add(text + " ");
			textField.text = GetComponent<Keyboard>().inputString.text;

		};

		/*
		dictationRecognizer.DictationHypothesis += (text) =>
		{
			Debug.Log("Hyposthesing");

			hypotheses.text = text;
		};
		*/

		dictationRecognizer.DictationError += (error, hresult) =>
		{
			Debug.LogErrorFormat("Dictation error: {0}; HResult = {1}.", error, hresult);
		};

		dictationRecognizer.DictationComplete += (cause) =>
		{

			isListening = false;
			myToggle.isOn = false;
			checkmarkAnimation.Stop();
			//dictationRecognizer.Dispose();
		};

	}

	void Update()
	{
		if (!myToggle.isOn && isListening)
		{
			isListening = false;
			dictationRecognizer.Stop();

			Debug.Log("12stop dispose");
		}

		if (myToggle.isOn && !isListening)
		{
			isListening = true;
			StartRecognition(true);
		}

	}
	private void FixedUpdate()
	{
		if (isListening)
		{
			checkmarkAnimation.Update();
		}

	}

	private void StartRecognition(bool arg0)
	{
		checkmarkAnimation.Start();
		dictationRecognizer.Start();
		Debug.Log("starting");

	}
	[Serializable]
	public class CheckmarkAnimation
	{
		public Image checkmark;
		public Image checkmarkBack;
		private float sinVal = 0;
		[SerializeField]
		private float step = 0.05f;
		public float scaleAnim = 0.5f;
		public Vector3 sinResultVec { get { return new Vector3(Mathf.Sin(sinVal) * scaleAnim, Mathf.Sin(sinVal) * scaleAnim, Mathf.Sin(sinVal) * scaleAnim); } }
		public void Start()
		{
			checkmark.transform.localScale = Vector3.one;
			sinVal = 0;
			checkmarkBack.enabled = false;

		}
		public void Update()
		{
			sinVal += step;
			if (sinVal > Mathf.PI)
			{
				sinVal = 0;
			}
			checkmark.transform.localScale = Vector3.one + sinResultVec;
		}
		public void Stop()
		{
			checkmarkBack.enabled = true;
		}
	}
}
