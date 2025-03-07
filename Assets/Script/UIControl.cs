using Meta.WitAi.TTS.Utilities;
using UnityEngine;

public class UIControl : MonoBehaviour
{
    [SerializeField] private GameObject UIPanel;
    [SerializeField] private TTSSpeaker speaker;


    private bool firstEnter = true;

    void Start()
    {
        UIPanel.SetActive(false);
        firstEnter = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player Enter");
            UIPanel.SetActive(true);
        }

        if (firstEnter)
        {
            speaker.Stop();
            speaker.Speak("Close your eyes, take a deep breath, write down the question you want to ask the Tarot.");
            firstEnter = false;
        }

    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player Exit");
            UIPanel.SetActive(false);
        }
    }
}
