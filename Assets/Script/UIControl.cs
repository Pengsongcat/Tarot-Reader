using UnityEngine;

public class UIControl : MonoBehaviour
{
    [SerializeField] private GameObject UIPanel;

    void Start()
    {
        UIPanel.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player Enter");
            UIPanel.SetActive(true);
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
