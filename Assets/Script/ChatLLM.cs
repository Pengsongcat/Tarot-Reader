using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using Newtonsoft.Json;
using TMPro;
using UnityEngine.UI;
using UnityEditor.VersionControl;
using Meta.WitAi.TTS.Utilities;
using System.Text.RegularExpressions;
using UnityEngine.Windows;
using System.Linq;
using Unity.VisualScripting;
using System;

public class ChatLLM : MonoBehaviour 
{
    [SerializeField] private TMP_InputField questionInputField;
    [SerializeField] private TextMeshProUGUI confirmedQuestionTMP;
    [SerializeField] private TextMeshProUGUI tarotDrawnTMP;
    [SerializeField] private TextMeshProUGUI responseTMP;

    [SerializeField] private GameObject QuestionPanel;
    [SerializeField] private GameObject TarotPanel;
    [SerializeField] private GameObject ReadingPanel;
    [SerializeField] private GameObject backButton;

    [SerializeField] private Button confirmQButton;
    [SerializeField] private Button drawTarotButton;
    [SerializeField] private Button genReadingButton;
    [SerializeField] private TTSSpeaker speaker;
    [SerializeField] private TarotDrawer tarotDrawer;

    // private string LLMmodel = "DeepSeek-R1-Distill-Qwen-7B";
    private string LLMmodel = "Llama 3.2 3B Instruct";
    private string apiUrl = "http://localhost:4891/v1/chat/completions";
    private string systemInput = "You are a gifted tarot reader.";

    // // This is for Deepseek
    // private string promptBeginning = "You are a professional and gifted tarot reader, read my tarot cards and answer my question in a narrative, mysterious, serene tone, answer briefly in less than 6 sentences, you must trust tarot's answer and give clear interpretations. Never refuse reading. I want to know ";
    // private string promptMiddle = ". The tarot cards are ";
    // private string promptEnding = ". Forget previous questions.";

    // This is for Llama
    private string promptBeginning = "I want to know ";
    private string promptMiddle = ". Don't shuffle or draw cards for me. I have draw ";
    private string promptEnding = ". Read every tarot cards and answer my question in a narrative tone, answer briefly in less than 6 sentences. Interpret every Tarot Card. Forget previous questions.";

    private void Start()
    {
        //speaker.Speak("Hi, I'm here!");
        confirmQButton.onClick.AddListener(ConfirmQuestion);
        drawTarotButton.onClick.AddListener(tarotDrawer.DrawTarot);
        genReadingButton.onClick.AddListener(SendMessageToLLM);
        // backButton.onClick.AddListener(BackToBegin);

        QuestionPanel.SetActive(true);
        TarotPanel.SetActive(false);
        ReadingPanel.SetActive(false);
    }

    private void ConfirmQuestion()
    {
        string questionInput = questionInputField.text;
        if (questionInput == "")
        {
            Speak("Please inscribe your question.");
        }
        else
        {
            confirmedQuestionTMP.text = questionInput;

            QuestionPanel.SetActive(false);
            TarotPanel.SetActive(true);
            ReadingPanel.SetActive(false);

            Speak("Gather your spirit, and draw the Tarot. You may draw at most three cards.");
        }
    }

    // This function constructs a prompt, and sends the prompt to local LLM and wait for response, then read response and put response in the text field.
    private void SendMessageToLLM()
    {
        string prompt = ConstructPrompt();

        if (prompt != null)
        {
            Debug.Log(prompt);

            QuestionPanel.SetActive(false);
            TarotPanel.SetActive(false);
            ReadingPanel.SetActive(true);

            backButton.SetActive(false);

            Speak("The cards have whispered their truth. Please wait a moment, as I unravel its message for you.");

            StartCoroutine(SendRequest(prompt));
        }
    }

    public void BackToBegin()
    {
        CleanUpHistory();

        QuestionPanel.SetActive(true);
        TarotPanel.SetActive(false);
        ReadingPanel.SetActive(false);

        StartCoroutine(Wait());
        Speak("Close your eyes, take a deep breath, write down the question you want to ask the Tarot.");
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(1f);
    }


    private void CleanUpHistory()
    {
        questionInputField.text = "";
        confirmedQuestionTMP.text = "";
        tarotDrawnTMP.text = "";
        responseTMP.text = "";
    }

    // Construct prompt to be send to LLM
    private string ConstructPrompt()
    {
        string tarotQuestion = confirmedQuestionTMP.text;
        if (tarotQuestion == "")
        {
            // Speak("Please write down the question you want to ask the Tarot.");
            Debug.Log("No Question");
            return null;
        }

        string tarotDrawn = tarotDrawnTMP.text.Trim().Replace("\n", ", ");
        if (tarotDrawn == "")
        {
            // Speak("You need to draw at lease one tarot card for the divination.");
            Debug.Log("No Tarot");
            return null;
        }

        string prompt = promptBeginning + tarotQuestion + promptMiddle + tarotDrawn + promptEnding;     // concat prompt

        return prompt;
    }

    // send the prompt to local LLM and wait for response, then read response and put response in the text field.
    private IEnumerator SendRequest(string prompt)
    {
        confirmQButton.interactable = false;
        drawTarotButton.interactable = false;
        genReadingButton.interactable = false;
        questionInputField.interactable = false;

        var requestData = new
        {
            model = LLMmodel,
            max_tokens = 2048,
            messages = new[]
            {
                new { role = "system", content = systemInput},
                new { role = "user", content = prompt }
            }
        };

        string jsonPayload = JsonConvert.SerializeObject(requestData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + request.error);
            }
            else
            {
                string responseJson = request.downloadHandler.text;
                LLMResponse response = JsonConvert.DeserializeObject<LLMResponse>(responseJson);

                if(response != null && response.choices.Length > 0)
                {
                    string responseString = EditResponse(response.choices[0].message.content);
                    responseTMP.text = responseString;

                    backButton.SetActive(true);
                    Debug.Log(responseString);
                    Speak(responseString);
                }
            }
        }

        confirmQButton.interactable = true;
        drawTarotButton.interactable = true;
        genReadingButton.interactable = true;
        questionInputField.interactable = true;
    }


    private string EditResponse(string origResponse)
    {
        // get rid of <think>
        string wholeResponseString = Regex.Replace(origResponse, @"<think>.*?</think>", "", RegexOptions.Singleline);    

        // take first 5 paragraph in case too long
        string[] paragraphString = wholeResponseString.Trim().Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        string responseString = string.Join("\n\n", paragraphString.Length >= 5 ? paragraphString.Take(4) : paragraphString);

        return responseString;
    }

    private void Speak(string content)
    {
        speaker.Stop();
        speaker.Speak(content);
    }


    [System.Serializable]
    public class LLMResponse
    {
        public Choice[] choices;
    }

    [System.Serializable]
    public class Choice
    {
        public Message message;
    }

    [System.Serializable]
    public class Message
    {
        public string content;
    }
}
