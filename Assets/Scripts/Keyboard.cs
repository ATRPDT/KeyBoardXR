using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SwipeType;
using System.IO;

namespace SwipeKeyboard
{
    public class Keyboard : MonoBehaviour
    {
        public Text textBox;
        public float sensitivity = 0.1f;

        [Header("Массив кнопок")]
        public KeyboardButton[] keyboardButtons;

        [Header("Анимация свайпов")]
        public LineTrailAnimation lineTrailAnimation;

        [Header("Анимация клавиш")]
        public KeysAnimation keysAnimation;

        [Header("Подсказки")]
        public KeyboardHints keyboardHints;

        private Vector3 mouseDelta;
        
        [SerializeField] private Vector3 oldHitPosition;
        
        [HideInInspector] public InputString inputString = new InputString();

        //private string currentWord = "";
        private GameObject currentKey = null;

        private bool isMouseDown = false;

        private bool isReadyToFirstPress = true;
        private bool isfirstPressed;
        
        public float timeStep = 0.5f;
        public float _mouseDelta;

        private MatchSwipeType swipeType;

        //private SpellChecker spellChecker;
        private SymSpellManager symSpellManager;

        private void Start()
        {
            
            symSpellManager = new SymSpellManager();

            //CalculateDelta(Vector3.zero, Vector3.zero);

            lineTrailAnimation.CreateTrailObject();
        }
        private void FixedUpdate()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;

            Physics.Raycast(ray, out hit);

            CalculateDelta(oldHitPosition, hit.point, out mouseDelta);

            oldHitPosition = hit.point;
        }

        private void Update()
        {
            
            
            Debug.Log(Mathf.Sqrt(mouseDelta.x * mouseDelta.x + mouseDelta.y * mouseDelta.y));

            if (Input.GetButtonUp("Fire1"))
            {
                timeStep = 0.5f;
                isMouseDown = false;
                isfirstPressed = false;
                isReadyToFirstPress = true;

            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;

            if (Input.GetButton("Fire1"))
            {
                if (Physics.Raycast(ray, out hit))
                {
                    KeyboardButton button = FindKeyboardButton(hit.transform.gameObject);
                    currentKey = hit.transform.gameObject;
                    if (button != null)
                    {
                        if (mouseDelta.sqrMagnitude < sensitivity)
                        {

                            if (timeStep <= 0)
                            {

                                if (isfirstPressed == true)
                                {
                                    isReadyToFirstPress = false;
                                }

                                timeStep = 0.5f;




                                ButtonPress(button);

                                isfirstPressed = true;

                            }

                            /*
                            if (currentKey != button.buttonObject)
                            {
                                isfirstPressed = false;
                                isReadyToFirstPress = true;
                            }
                            */
                            // currentKey = hit.transform.gameObject;

                            if (isfirstPressed == true && isReadyToFirstPress == true)
                            {
                                timeStep -= Time.deltaTime;
                            }
                            else
                            {
                                timeStep -= 10 * Time.deltaTime;
                            }

                        }
                    }

                    else if (keyboardHints.GetHintIndex(hit.transform.gameObject) != -1)
                    {
                        if (mouseDelta.sqrMagnitude < sensitivity)
                            if ((currentKey != hit.transform.gameObject) || !isMouseDown)
                            {

                                inputString.AddWord(keyboardHints.GetHintText(keyboardHints.GetHintIndex(hit.transform.gameObject)));

                                textBox.text = inputString.text;
                                keyboardHints.RemoveAll();
                            }
                    }
                    else
                    {
                        isfirstPressed = false;
                        isReadyToFirstPress = true;
                        timeStep = 0;
                    }
                }
            }

            else
            {

                isMouseDown = false;
            }

            lineTrailAnimation.UpdateTrailPosition();
            keysAnimation.UpdateSelectedImages();
        }
        private void ButtonPress(KeyboardButton button)
        {
            keysAnimation.AddSelectedImage(button.buttonObject.transform.gameObject.GetComponent<Image>());
            
            if (button.isCommandButton)
            {
                string keyValue = button.buttonValue; //.transform.gameObject.GetComponent<KeyControl>().keyValue;
                switch (keyValue)
                {
                    case "backspace":
                        inputString.RemoveFromEnd(1);
                        textBox.text = inputString.text;
                        UpdateHints();
                        break;
                }
            }
            else
            {

                if (button.buttonValue == " ")
                {
                    keyboardHints.RemoveAll();
                    inputString.AddWord(symSpellManager.GetSuggestion(inputString.lastWord));
                }
                else
                {
                    inputString.Add(button.buttonValue);
                    UpdateHints();
                }
                
                textBox.text = inputString.text;
            }
        }

        private void UpdateHints()
        {
            if (inputString.lastWord == "")
            {
                keyboardHints.RemoveAll();
                return;
            }
                
            string[] suggestionWords = symSpellManager.GetSuggestions(inputString.lastWord, 4);
            keyboardHints.Create(suggestionWords);
        }

        private void CalculateDelta(Vector3 oldVec, Vector3 Vec, out Vector3 delta)
        {
            delta = Vec - oldVec;
        }

        private string[] GetSuggestionWords(string inputWord, int count)
        {
            List<string> suggestWords = new List<string>();
            foreach (var x in swipeType.GetSuggestion(inputWord, count))
            {
                suggestWords.Add(x);
            }
            
            return suggestWords.Count > 0 ? suggestWords.ToArray() : new string[] { inputWord };
        }
        private KeyboardButton FindKeyboardButton(GameObject obj)
        {
            foreach (var b in keyboardButtons)
                if (b.buttonObject == obj) return b;
            return null;
        }
        
        [System.Serializable]
        public class LineTrailAnimation
        {
            public Material material;
            public AnimationCurve widthCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 2f), new Keyframe(1.0f, 0f) });
            public float lifeTime = 0.5f;
            private GameObject trail;
            
            public void CreateTrailObject()
            {
                trail = Instantiate(new GameObject(), Vector3.zero, Quaternion.identity);
                trail.AddComponent<TrailRenderer>();
                trail.GetComponent<TrailRenderer>().material = material;
                trail.GetComponent<TrailRenderer>().widthCurve = widthCurve;
                trail.GetComponent<TrailRenderer>().time = lifeTime;
                trail.GetComponent<TrailRenderer>().sortingOrder = 1;
                trail.GetComponent<TrailRenderer>().emitting = false;
                trail.name = "TrailCursor";
            }
            public void UpdateTrailPosition()
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                    trail.transform.position = hit.point;
                if (Input.GetButton("Fire1"))
                {
                    trail.GetComponent<TrailRenderer>().emitting = true;
                }
                else
                {
                    trail.GetComponent<TrailRenderer>().emitting = false;
                }
            }
        }

        [System.Serializable]
        public class KeysAnimation
        {
            public Color keyPressedColor;
            private Color keyDefaultColor = Color.white;
            public float keyColorChangeSpeed = 0.05f;

            private List<Image> selectedKeys = new List<Image>();

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
            public void UpdateSelectedImages()
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

        [System.Serializable]
        public class KeyboardHints
        {
            public RectTransform hintArea;
            public Color selectedHintColor;
            public int count;
            public float offsetBetween = 10f;
            public Vector2 offsetEdge = new Vector2(10, 10);
            public int selectedHintIndex { get; private set; }

            private List<KeyboardButton> hints = new List<KeyboardButton>();

            public void Create(string[] hintTexts)
            {
                this.count = hintTexts.Length;

                if (count <= 0)
                    return;

                selectedHintIndex = count > 1 ? 1 : 0;

                if (hints.Count > 0) RemoveAll();
                //Vector2 offsetPosition = new Vector2(hintArea.position.x, hintArea.position.y);
                Vector2 hintSize = (new Vector2(hintArea.sizeDelta.x, hintArea.sizeDelta.y) - offsetEdge * 2 - new Vector2(offsetBetween * (count - 1), 0)) / new Vector2(count, 1);
                for(int i = 0; i < count; i++)
                {
                    GameObject hintObj = new GameObject("hint" + i.ToString());
                    hintObj.transform.rotation = hintArea.gameObject.transform.rotation;
                    hintObj.transform.parent = hintArea.transform;
                    hintObj.AddComponent<Image>();
                    hintObj.GetComponent<RectTransform>().localScale = Vector3.one;
                    hintObj.GetComponent<RectTransform>().sizeDelta = hintSize;
                    hintObj.transform.localPosition = (hintSize / 2 + (hintSize + new Vector2(offsetBetween, 0)) * new Vector2(i, 0) - hintArea.sizeDelta / 2 + offsetEdge);
                    hintObj.AddComponent<BoxCollider>();
                    hintObj.GetComponent<BoxCollider>().size = new Vector3(hintSize.x, hintSize.y, 1);
                    hintObj.GetComponent<BoxCollider>().center = new Vector3(0, 0, -1);

                    if (i == selectedHintIndex)
                        hintObj.GetComponent<Image>().color = selectedHintColor;

                    GameObject hintText = new GameObject("Text");
                    hintText.transform.position = hintObj.transform.position;
                    hintText.transform.rotation = hintObj.transform.rotation;
                    hintText.transform.parent = hintObj.transform;
                    hintText.AddComponent<Text>();
                    hintText.GetComponent<RectTransform>().localScale = Vector3.one;
                    hintText.GetComponent<RectTransform>().sizeDelta = hintSize;
                    hintText.GetComponent<Text>().resizeTextForBestFit = true;
                    hintText.GetComponent<Text>().font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
                    hintText.GetComponent<Text>().color = Color.black;
                    hintText.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;

                    string text;
                    if (i == 0)
                        text = "\"" + hintTexts[i] + "\"";
                    else
                       text =  hintTexts[i];

                    hintText.GetComponent<Text>().text = text;

                    hints.Add(new KeyboardButton(hintObj, false, hintTexts[i]));
                }
            }
            public void RemoveAll()
            {
                for(int i = 0; i < hints.Count; i++)
                {
                    Destroy(hints[i].buttonObject);
                }
                hints.Clear();
            }
            public bool SetHintText(int index, string text)
            {
                if (index < 0 || index > hints.Count - 1) 
                    return false;

                hints[index].buttonObject.transform.GetChild(0).gameObject.GetComponent<Text>().text = text;
                hints[index].buttonValue = text;
                return true;
            }

            public bool SetHintTexts(string[] hintTexts)
            {
                if (hintTexts.Length != hints.Count)
                    return false;
                for(int i = 0; i < hints.Count; i++)
                {
                    hints[i].buttonObject.transform.GetChild(0).gameObject.GetComponent<Text>().text = hintTexts[i];
                    hints[i].buttonValue = hintTexts[i];
                }
                    
                return true;
            }
            public string GetHintText(int index)
            {
                if (index < 0 || index > hints.Count - 1)
                    return "";
                return hints[index].buttonValue;
            }
            public int GetHintIndex(GameObject selectHint)
            {
                for (int i = 0; i < hints.Count; i++)
                {
                    if(selectHint == hints[i].buttonObject)
                    {
                        return i;
                    }
                }
                return -1;
            }

        }
        
        [System.Serializable]
        public class KeyboardButton
        {
            public GameObject buttonObject;
            public bool isCommandButton = false;
            public string buttonValue;

            public KeyboardButton(GameObject obj, bool isCommandButton, string buttonValue)
            {
                this.buttonObject = obj;
                this.isCommandButton = isCommandButton;
                this.buttonValue = buttonValue;
            }
        }

        public class InputString
        {
            public string text { get { return _text + lastWord; } private set { _text = value; } }

            private string _text;
            public string lastWord { get; private set; }

            public InputString()
            {
                text = "";
            }
            public InputString(string textValue)
            {
                text = "";
                Add(textValue);
            }
            
            public void Add(string text)
            {
                int splitIndex = GetLastWordStart(text);
                if (splitIndex == text.Length - 1)
                {
                    this.text += text;
                    lastWord = "";
                }
                else if(splitIndex == -1)
                {
                    lastWord += text;
                }
                else
                {
                    this.text += text.Substring(0, splitIndex + 1);
                    this.lastWord = text.Substring(splitIndex + 1, text.Length - splitIndex - 1);
                }
            }
            public void AddWord(string word)
            {
                lastWord = "";
                this.text += word + " ";
            }
            private int GetLastWordStart(string text)
            {
                int splitIndex = -1;
                for (int i = text.Length - 1; i >= 0; i--)
                {
                    if (!char.IsLetter(text[i]))
                    {
                        splitIndex = i;
                        break;
                    }
                }
                return splitIndex;
            }
            public void RemoveFromEnd(int count = 1)
            {
                if (text.Length == 0) return;
                if (count > text.Length) count = text.Length;
                if (count < 1) count = 1;
               
                string newText = text.Remove(text.Length - count);

                text = "";
                lastWord = "";

                Add(newText);
            }
        }
        private class Timer
        { 

        }


    }

}

