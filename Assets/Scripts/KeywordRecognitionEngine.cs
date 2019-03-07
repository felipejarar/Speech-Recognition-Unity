using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Collections;
using UnityEngine.Windows.Speech;

public class KeywordRecognitionEngine : MonoBehaviour
{

    // Directory of the XML File that specifies the grammar constraints of the Keyword Recognizer
    [SerializeField] string r_gramarFile;

    // GrammarRecognizer object that listen to general purposes commands
    private GrammarRecognizer r_grammar;

    // List of GrammarRecognizer objects, each one specific containing commands specific to a certain context
    [SerializeField] private List<GrammarContext> r_contextualGrammar;

    // Minimum Confidence level
    [SerializeField] ConfidenceLevel r_minimumConfidenceLevel;


    [Header("Wake up configurations ")]

    // A boolean that flags wheter the VUI will listen to a wake up word before any command or not.
    [SerializeField] bool r_wakeUpWordEnabled;

    // The time length in seconds before VUI stop listening to commands due to lack of audio input
    [SerializeField] float r_timeOutSeconds;

    // Timeout counter before VUI stop listening to commands due to lack of audio input
    [ReadOnly] public float r_timeOutCounter;

    // A list of UnityEvents that must be called after the VUI has recognized certain keyword or phrase
    [SerializeField] public GrammarActions[] r_grammarActions;

    // A list of UnityEvents that must be called after the VUI has reached the time out and must be put to sleep
    [SerializeField] public UnityEngine.Events.UnityEvent r_timeOutActions;

    // A Dictionary that makes easier to look up for the action of a certain keyword or phrase
    private IDictionary<string, UnityEngine.Events.UnityEvent> r_grammarActionsDictionary;




    // Use this for initialization
    void Start () {
        loadGrammar();
    }
	
	// Update is called once per frame
	void Update () {

        if (r_timeOutCounter > 0)
        {
            r_timeOutCounter -= Time.deltaTime;
            if (r_timeOutCounter <= 0)
            {
                r_timeOutActions.Invoke();
            }
        }
	}

    void loadGrammar()
    {
        if (string.IsNullOrEmpty(r_gramarFile))
        {
            Debug.LogErrorFormat("Grammar File hasn't been specified");
        }
        else
        {
            try
            {
                r_grammar = new GrammarRecognizer(Application.dataPath + "/StreamingAssets/" + r_gramarFile, ConfidenceLevel.Low);
            }
            catch (UnityException e)
            {
                Debug.LogErrorFormat(e.ToString());
                Debug.LogErrorFormat("The error might be because the file '" + r_gramarFile + "' doesn't exists in the StreamingAssets folder");
            }

            if (r_grammar != null)
            {

                r_grammar.OnPhraseRecognized += Grammar_OnPhraseRecognized;
                r_grammar.Start();

                r_grammarActionsDictionary = new Dictionary<string, UnityEngine.Events.UnityEvent>();
                foreach (GrammarActions ga in r_grammarActions) { 
                    r_grammarActionsDictionary.Add(new KeyValuePair<string, UnityEngine.Events.UnityEvent>(ga.id, ga.action));
                }
            }
        }

    }

    private void Grammar_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {

        // Check if phrase recognized reaches the minimum confidence level required
        if (args.confidence <= r_minimumConfidenceLevel)
        {
            // In case the VUI was expecting to receive a command
            if (r_timeOutCounter > 0 || !r_wakeUpWordEnabled)
            {
                // Search for the id of the command
                string command_id = null;
                if (args.semanticMeanings != null)
                {
                    foreach (SemanticMeaning element in args.semanticMeanings)
                    {
                        if (String.Equals(element.key, "id"))
                        {
                            command_id = element.values[0]; break;
                        }
                    }
                }
                else
                {
                    Debug.LogErrorFormat("There has been an error with the semanting meaning of {0}. Probably it's incorrectly specified in the XML file", args.text);
                }

                // Use the id of the command to invoke the unity actions linked to the command
                if (!String.IsNullOrEmpty(command_id) && r_grammarActionsDictionary.ContainsKey(command_id))
                {
                    r_grammarActionsDictionary[command_id].Invoke();
                    Debug.LogFormat("Command pronounced: {0}", args.text);
                }
                else
                {
                    Debug.LogErrorFormat("The phrase '{0}' doens't have an id assigned", args.text);
                }

                // Check if the command is related to a certain context


            }

            // In case the VUI was expecting to receive a wake up word
            else if (r_wakeUpWordEnabled)
            {

                // Checks the semantic meanings if the 'wake_up' key has a true value assigned
                string wakeup_id = "wake_up";
                bool wakeupcheck = false;
                foreach (SemanticMeaning element in args.semanticMeanings)
                {
                    if (String.Equals(element.key, "wake_up") && Convert.ToBoolean(element.values[0]) )
                    {
                        if (r_grammarActionsDictionary.ContainsKey(wakeup_id))
                        {
                            r_grammarActionsDictionary[wakeup_id].Invoke();
                        }
                        r_timeOutCounter = r_timeOutSeconds;
                        wakeupcheck = true;
                        break;
                    }
                }
                
                // Debug prints
                if (wakeupcheck)
                {
                    Debug.LogFormat("A wake up word has been pronounced: {0}", args.text);
                }
                else
                {
                    Debug.LogFormat("System recognized the keyword ({0}), but the VUI must be awaken first", args.text);
                }


            }


        }
        else
        {
            Debug.LogFormat("Minimum confidence level ({0}) not reached for: {1} ({2}) ", r_minimumConfidenceLevel, args.text, args.confidence);
        }


    }

    private void LoadContext(string context)
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


[Serializable]
public class GrammarActions
{
    public string id;
    public UnityEngine.Events.UnityEvent action;
    public string context_id;
}

 [Serializable]
 public class GrammarContext
    {
        public string context_id;
        public string context_file;
        GrammarRecognizer recognizer;

    /**
    public void initializeRecognizer()
    {

        if (string.IsNullOrEmpty(context_file))
        {
            Debug.LogErrorFormat("Grammar File hasn't been specified for the context of id {0}", context_id);
        }
        else
        {
            try
            {
            recognizer = new GrammarRecognizer(Application.dataPath + "/StreamingAssets/" + context_file, ConfidenceLevel.Low);
            }
            catch (UnityException e)
            {
                Debug.LogErrorFormat(e.ToString());
                Debug.LogErrorFormat("The error might be because the file '" + context_file + "' doesn't exists in the StreamingAssets folder");
            }

            if (recognizer != null)
            {

                recognizer.OnPhraseRecognized += Grammar_OnPhraseRecognized;
                recognizer.Start();

                r_grammarActionsDictionary = new Dictionary<string, UnityEngine.Events.UnityEvent>();
                foreach (GrammarActions ga in r_grammarActions)
                {
                    r_grammarActionsDictionary.Add(new KeyValuePair<string, UnityEngine.Events.UnityEvent>(ga.id, ga.action));
                }
            }
        }

        
    }**/
}

public static class GrammarContextExtention
{
    public static GrammarContext GetGrammarContextByID(this List<GrammarContext> list, string id)
    {
        foreach (GrammarContext gc in list)
        {
            if (String.Equals(gc.context_id, id))
            {
                return gc;
            }
        }
        return null;
    }
}
