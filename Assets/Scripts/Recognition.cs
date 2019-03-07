using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;

// Namespace that provides the Microsoft API for Speech Recognition
// Requires Unity 5.4.0b2 and above to work on Windows
using UnityEngine.Windows.Speech;

public class Recognition : MonoBehaviour {

    [Serializable]
    public class KeyActionPair
    {
        public string m_keyword;
        public UnityEngine.Events.UnityEvent m_action;
    }

    // Confidence Level for the SpeechRecognition. Phrases under the specified minimum level will be ignored.
    [SerializeField]
    ConfidenceLevel m_ConfidenceLevel;

    // List of keywords that should be recognized by the Keyword Recognizer to invoke their respective events.
    [SerializeField]
    private KeyActionPair[] m_KeyActions;

    // Object that listens to speech input and attempts to match uttered phrases to a list of registered keywords.
    private KeywordRecognizer m_Recognizer;

    // Dictionary that optimize the relation lookup between a keyword and it's action
    private IDictionary<string, UnityEngine.Events.UnityEvent> m_KeyDictionary;



    // Use this for initialization
    void Start () {

        Debug.Log("MUSE! MUSICAL! STARTO!");

        initializeDictionary();
        initializeRecognizer();

    }

    private void initializeDictionary()
    {
        m_KeyDictionary = new Dictionary<string, UnityEngine.Events.UnityEvent>();
        foreach (KeyActionPair ka in m_KeyActions)
        {
            m_KeyDictionary.Add(new KeyValuePair<string, UnityEngine.Events.UnityEvent>(ka.m_keyword, ka.m_action));
        }
    }

    private void initializeRecognizer()
    {
        // InitializeDictionary should be called first
        if (m_KeyDictionary == null)
        {
            Debug.Log("Key Dictionary must be initialized first. \nInitializing Key Dictionary");
            initializeDictionary();
        }
        
        string[] keywords = m_KeyDictionary.Keys.ToArray();
        m_Recognizer = new KeywordRecognizer(keywords, m_ConfidenceLevel);
        m_Recognizer.OnPhraseRecognized += OnPhraseRecognized;
        m_Recognizer.Start();
    }
	

    private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0} ({1}){2}", args.text, args.confidence, Environment.NewLine);
        builder.AppendFormat("\tTimestamp: {0}{1}", args.phraseStartTime, Environment.NewLine);
        builder.AppendFormat("\tDuration: {0} seconds{1}", args.phraseDuration.TotalSeconds, Environment.NewLine);
        Debug.Log(builder.ToString());
        m_KeyDictionary[args.text].Invoke();
    }

}
