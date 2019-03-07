using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Globalization;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;
using System;

public class VoiceUserInterface : MonoBehaviour {

 
    // Settings
    [Header("Voice User Interface Main Settings")]

        [SerializeField]
        bool _wakeUpButtonsEnabled;      // VUI listens if a wake up button is pressed

        [SerializeField]
        bool _wakeUpWordEnabled;         // VUI listens if wake up word is spoken

        [SerializeField]
        bool _onHoverEnabled;           // VUI listens if an element is being pointed at

        [SerializeField]
        float _waitTimerLength; 

    [Header("Voice User Interface Wake Up Settings ")]

        // Wake Up button
        [SerializeField]
        KeyCode[] _wakeUpButtons;

        // Wake Up word
        [SerializeField]
        string _wakeUpWord;

    
    // The confidence level at which the recognizer will begin accepting phrases.
    [SerializeField] UnityEngine.Windows.Speech.ConfidenceLevel minimumConfidence;

    // Specifies the scenario for which a specific dictation recognizer should optimize.
    [SerializeField] UnityEngine.Windows.Speech.DictationTopicConstraint topicConstraint;







    [Header("Dictation Recognizer")]
    [SerializeField] private Text m_Hypotheses;
    [SerializeField] private Text m_Recognitions;
    private DictationRecognizer m_DictationRecognizer;

    [ReadOnly] public UnityEngine.Windows.Speech.SpeechSystemStatus r_status;
    [ReadOnly] public float r_autoSilenceTimeout;
    [ReadOnly] public float r_initialSilenceTimeout;




    // A boolean that flags wheter the dictation recognizer is running or not
    [ReadOnly] public bool isDictationRunning;





    // In-Game variables
    [Header("Language variables")]
    [ReadOnly] public string language;
    [ReadOnly] public string languageCode;

    [ReadOnly] public bool _isAwake = false;
    [ReadOnly] public float _waitTimer = 0;
    [ReadOnly] public bool _wakeUpEnabled = true;



    [Header("Microphone variables")]

    // A boolean that flags wheter there's a connected microphone
    [ReadOnly] public bool micConnected = false;

    // An intenger that stores the number of connected microphones
    [ReadOnly] public int numMics = 0;

    // The identifier of the current microphone
    [ReadOnly] public int currentMic = -1;

    // A list of Microphones and relevant data
    [ReadOnly] public MicrophoneObject[] microphones;


    
    






    [System.Serializable]
    public class MicrophoneObject : System.Object
    {
        public string name;
        public int minFreq;
        public int maxFreq;
    }

    // Use this for initialization
    void Start () {

        initMicrophone();
        initDictation();


    }



    // Update is called once per frame
    void Update() {
        
        // Wake up Voice User Interface
        if (_wakeUpEnabled)
        {
            // Check if wake up button has been pressed
            if (_wakeUpButtonsEnabled)
            {
                foreach (KeyCode key in _wakeUpButtons)
                {
                    if (Input.GetKey(key))
                    {
                        Debug.Log(key);
                        _isAwake = true;
                        _waitTimer = _waitTimerLength;
                        break;
                    }
                }
            }
        }


        if (!_isAwake)
        {

        }
        else
        {
            if (_waitTimer <= 0)
            {
                _isAwake = false;
                _waitTimer = 0;
            }
            else
            {
                _waitTimer -= Time.deltaTime;
            }
        }

	}

    void initMicrophone()
    {

        //Initialize default values
        currentMic = -1;

        //An integer that stores the number of connected microphones  
        numMics = Microphone.devices.Length;

        //Check if there is at least one microphone connected  
        if (numMics <= 0)
        {
            //Throw a warning message at the console if there isn't  
            Debug.LogWarning("No microphone connected!");
        }
        else //At least one microphone is present  
        {
            //Set 'micConnected' to true  
            micConnected = true;

            //Set default microphone as the first one
            currentMic = 0;

            // Initialize the list of microphone objects
            microphones = new MicrophoneObject[numMics];

            // Set the recording capabilities and name of each microphone  
            for (int i = 0; i < numMics; i++)
            {
                microphones[i] = new MicrophoneObject();
                microphones[i].name = Microphone.devices[i];
                Microphone.GetDeviceCaps(microphones[i].name, out microphones[i].minFreq, out microphones[i].maxFreq);

                //According to the documentation, if both minimum and maximum frequencies are zero, the microphone supports any recording frequency...  
                if (microphones[i].minFreq == 0 && microphones[i].maxFreq == 0)
                {
                    //...meaning 44100 Hz can be used as the recording sampling rate for the current microphone  
                    microphones[i].maxFreq = 44100;
                }
            }

        }
    }
    
    void initDictation()
    {
        //  DictationRecognizer listens to speech input and attempts to determine what phrase was uttered.
        //  Users can register and listen for hypothesis and phrase completed events.Start() and Stop() methods respectively enable and disable dictation recognition.
        //  Once done with the recognizer, it must be disposed using Dispose() method to release the resources it uses.
        //  It will release these resources automatically during garbage collection at an additional performance cost if they are not released prior to that.

        if (micConnected)
        {

            // Construct of Dictation Recognizer
            try
            {
                m_DictationRecognizer = new DictationRecognizer(minimumConfidence, topicConstraint);
                Debug.LogFormat("Dictation Recognizer has ben initializated. Status: {0}", m_DictationRecognizer.Status.ToString());
            }
            catch(ArgumentException e)
            {
                Debug.LogErrorFormat(e.ToString());
            }


            m_DictationRecognizer.DictationResult += (text, confidence) =>
            {
                Debug.LogFormat("Dictation result: {0}", text);
                m_Recognitions.text += text + "\n";
            };

            m_DictationRecognizer.DictationHypothesis += (text) =>
            {
                Debug.LogFormat("Dictation hypothesis: {0}", text);
                m_Hypotheses.text += text;
            };

            m_DictationRecognizer.DictationComplete += (completionCause) =>
            {
                if (completionCause != DictationCompletionCause.Complete)
                    Debug.LogErrorFormat("Dictation completed unsuccessfully: {0}.", completionCause);
            };

            m_DictationRecognizer.DictationError += (error, hresult) =>
            {
                Debug.LogErrorFormat("Dictation error: {0}; HResult = {1}.", error, hresult);
            };

            m_DictationRecognizer.Start();
        }
        else
        {
            Debug.LogErrorFormat("Microphone was not found. Dictation initialization unsuccessful");
        }

    }

    void initLanguage()
    {
        Debug.Log(Application.systemLanguage);
    }

    public class ReadOnlyAttribute : PropertyAttribute
    {

    }

    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property,
                                                GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position,
                                   SerializedProperty property,
                                   GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }


}
