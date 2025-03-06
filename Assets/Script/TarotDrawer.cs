using Meta.WitAi.TTS.Utilities;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Windows;

public class TarotDrawer : MonoBehaviour
{
    // [SerializeField] private TMP_InputField questionInputField;
    [SerializeField] private TextMeshProUGUI tarotDrawnTMP;
    [SerializeField] private TTSSpeaker speaker;

    private string[] tarotCards = {
            // Major Arcana
            "The Fool",
            "The Magician",
            "The High Priestess",
            "The Empress",
            "The Emperor",
            "The Hierophant",
            "The Lovers",
            "The Chariot",
            "Strength",
            "The Hermit",
            "Wheel of Fortune",
            "Justice",
            "The Hanged Man",
            "Death",
            "Temperance",
            "The Devil",
            "The Tower",
            "The Star",
            "The Moon",
            "The Sun",
            "Judgment",
            "The World",
            // Minor Arcana - Wands
            "Ace of Wands",
            "Two of Wands",
            "Three of Wands",
            "Four of Wands",
            "Five of Wands",
            "Six of Wands",
            "Seven of Wands",
            "Eight of Wands",
            "Nine of Wands",
            "Ten of Wands",
            "Page of Wands",
            "Knight of Wands",
            "Queen of Wands",
            "King of Wands",
            // Minor Arcana - Cups
            "Ace of Cups",
            "Two of Cups",
            "Three of Cups",
            "Four of Cups",
            "Five of Cups",
            "Six of Cups",
            "Seven of Cups",
            "Eight of Cups",
            "Nine of Cups",
            "Ten of Cups",
            "Page of Cups",
            "Knight of Cups",
            "Queen of Cups",
            "King of Cups",
            // Minor Arcana - Swords
            "Ace of Swords",
            "Two of Swords",
            "Three of Swords",
            "Four of Swords",
            "Five of Swords",
            "Six of Swords",
            "Seven of Swords",
            "Eight of Swords",
            "Nine of Swords",
            "Ten of Swords",
            "Page of Swords",
            "Knight of Swords",
            "Queen of Swords",
            "King of Swords",
            // Minor Arcana - Pentacles
            "Ace of Pentacles",
            "Two of Pentacles",
            "Three of Pentacles",
            "Four of Pentacles",
            "Five of Pentacles",
            "Six of Pentacles",
            "Seven of Pentacles",
            "Eight of Pentacles",
            "Nine of Pentacles",
            "Ten of Pentacles",
            "Page of Pentacles",
            "Knight of Pentacles",
            "Queen of Pentacles",
            "King of Pentacles"
        };


    private void Start()
    {

    }

    public void DrawTarot()
    {
        // get current tarot drawn set, if have 6 cards then prevent more draw
        string tarotDrawn = tarotDrawnTMP.text;
        HashSet<string> tarotDrawnSet = new HashSet<string>(tarotDrawn.Trim().Split('\n'));
        if (tarotDrawnSet.Count >= 3)
        {
            Speak("You may draw at most three cards.");
            return;
        }

        // draw a new tarot card
        string randomTarot = RandomDraw(tarotDrawnSet);

        // write to textmeshpro
        tarotDrawn += randomTarot + "\n";
        tarotDrawnTMP.text = tarotDrawn;
    }

    private string RandomDraw(HashSet<string> tarotDrawnSet)
    {
        // draw a random tarot
        string randomTarot = tarotCards[Random.Range(0, tarotCards.Length)];

        // check for repeat
        int i = 0;  // avoid dead loop for safety
        while (tarotDrawnSet.Contains(randomTarot) && i < 100)
        {
            randomTarot = tarotCards[Random.Range(0, tarotCards.Length)];
            i++;
        }

        if (!tarotDrawnSet.Contains(randomTarot))
        {
            return randomTarot;
        }
        else
        {
            return null;
        }
    }

    private void Speak(string content)
    {
        speaker.Stop();
        speaker.Speak(content);
    }

}
