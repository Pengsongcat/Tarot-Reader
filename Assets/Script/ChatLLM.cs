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
    [SerializeField] private Button confirmQButton;
    [SerializeField] private Button drawTarotButton;
    [SerializeField] private Button genReadingButton;
    [SerializeField] private TTSSpeaker speaker;
    [SerializeField] private TarotDrawer tarotDrawer;

    private string LLMmodel = "DeepSeek-R1-Distill-Qwen-7B";
    private string apiUrl = "http://localhost:4891/v1/chat/completions";
    private string systemInput = "You are a gifted tarot reader.";
    private string promptBeginning = "You are a professional and gifted tarot reader, read my tarot cards and answer my question in a narrative, mysterious, serene tone, answer briefly in less than 6 sentences, you must trust tarot's answer and give clear interpretations. Never refuse reading. I want to know ";
    private string promptMiddle = ". The tarot cards are ";
    private string promptEnding = ". Forget previous questions.";

    private void Start()
    {
        //speaker.Speak("Hi, I'm here!");
        confirmQButton.onClick.AddListener(ConfirmQuestion);
        drawTarotButton.onClick.AddListener(tarotDrawer.DrawTarot);
        genReadingButton.onClick.AddListener(SendMessageToLLM);
    }

    private void ConfirmQuestion()
    {
        string questionInput = questionInputField.text;
        if (questionInput == "")
        {
            Speak("Please write down the question you want to ask the Tarot.");
        }
        else
        {
            speaker.Stop();
            CleanUpHistory();
            tarotDrawer.questionConfirmed = true;
            confirmedQuestionTMP.text = questionInput;
        }
    }

    // This function constructs a prompt, and sends the prompt to local LLM and wait for response, then read response and put response in the text field.
    private void SendMessageToLLM()
    {
        string prompt = ConstructPrompt();

        if (prompt != null)
        {
            Debug.Log(prompt);
            StartCoroutine(SendRequest(prompt));
            tarotDrawer.questionConfirmed = false;
        }
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
            Speak("Please write down the question you want to ask the Tarot.");
            return null;
        }

        string tarotDrawn = tarotDrawnTMP.text.Trim().Replace("\n", ", ");
        if (tarotDrawn == "")
        {
            Speak("You need to draw at lease one tarot card for the divination.");
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

        // take first 3 paragraph in case too long
        string[] paragraphString = wholeResponseString.Trim().Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        string responseString = string.Join("\n\n", paragraphString.Length >= 4 ? paragraphString.Take(4) : paragraphString);

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
