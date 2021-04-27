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
        [Header("Кнопки на клавиатуре")]
        public KeyboardButton[] keyboardButtons;

        [Header("Анимация свайпов")]
        public LineTrailAnimation lineTrailAnimation;

        [Header("Анимация клавиш")]
        public KeysAnimation keysAnimation;

        [Header("Подсказки слов")]
        public KeyboardHints keyboardHints;

        private Vector3 mouseDelta;
        private Vector3 oldMousePosition;
        [HideInInspector]
        public InputString inputString = new InputString();
        //private string currentWord = "";
        private GameObject currentKey = null;

        private bool isMouseDown = false;

        private MatchSwipeType swipeType;

        private SpellChecker spellChecker;

        private void Start()
        {
            //swipeType = new MatchSwipeType(File.ReadAllLines(@"E:\UnityPr\KeyBoardUnited\Assets\SwipeType\EnglishDictionary.txt"));

            spellChecker = new SpellChecker();

            CalculateMouseDalta();

            lineTrailAnimation.CreateTrailObject();
        }


        private void FixedUpdate()
        {
            CalculateMouseDalta();

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;

            if (Input.GetButton("Fire1"))
            {
                if (Physics.Raycast(ray, out hit))
                {
                    KeyboardButton button = FindKeyboardButton(hit.transform.gameObject);
                    if (button != null)
                    {
                        if (mouseDelta.sqrMagnitude < sensitivity)
                            if ((currentKey != button.buttonObject) || !isMouseDown)
                            {
                                keysAnimation.AddSelectedImage(button.buttonObject.transform.gameObject.GetComponent<Image>());

                                if (button.isCommandButton)
                                {
                                    string keyValue = button.buttonValue;//.transform.gameObject.GetComponent<KeyControl>().keyValue;
                                    switch (keyValue)
                                    {
                                        case "backspace":
                                            inputString.RemoveFromEnd(1);
                                            textBox.text = inputString.text;
                                            break;
                                    }
                                }
                                else
                                {
                                    

                                    if (button.buttonValue == " ")
                                    {
                                        keyboardHints.RemoveAll();
                                        inputString.AddWord(spellChecker.Correct(inputString.lastWord));
                                        //Debug.Log(spellChecker.Correct(inputString.lastWord));
                                    }
                                    else
                                    {
                                        /*
                                        string[] suggestionWords = GetSuggestionWords(inputString.lastWord, 3);
                                        if(suggestionWords.Length > 0 && suggestionWords[0] != "")
                                        {
                                            keyboardHints.Create(suggestionWords.Length);//Изменить
                                            keyboardHints.SetHintTexts(suggestionWords);
                                        }
                                       */
                                        inputString.Add(button.buttonValue);
                                    }
                                    
                                    textBox.text = inputString.text;

                                }

                                currentKey = hit.transform.gameObject;
                                isMouseDown = true;
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

                }
            }
            if (Input.GetButtonUp("Fire1")) isMouseDown = false;

            lineTrailAnimation.UpdateTrailPosition();
            keysAnimation.UpdateSelectedImages();
        }

        private void CalculateMouseDalta()
        {
            mouseDelta = oldMousePosition - Input.mousePosition;
            oldMousePosition = Input.mousePosition;
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
            public int count;
            public float offsetBetween = 10f;
            public Vector2 offsetEdge = new Vector2(10, 10);

            private List<KeyboardButton> hints = new List<KeyboardButton>();

            public void Create(int count)
            {
                this.count = count;
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

                    hints.Add(new KeyboardButton(hintObj, false, ""));
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

    }

    
    
}

