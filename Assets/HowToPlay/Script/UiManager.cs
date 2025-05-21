using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class UiManager : MonoBehaviour
{
    public TMP_InputField enterTheName;

    public TMP_InputField enterTheAge;
    public Button submitButton;
    
    [Header("Panel")]
    public GameObject InputPanel;
    public GameObject LoadingPanel;

    private RectTransform InputrectTransform;
    [Header("Animation Duration")]
    public float animationDuration;
    
    [Header("Slider")]
    public Slider slider;
    public GameObject gameController;
    public GameObject SpawnController;
    public GameObject FirstSpawnSheep;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InputrectTransform=InputPanel.GetComponent<RectTransform>();
        gameController.SetActive(false);
        SpawnController.SetActive(false);
        FirstSpawnSheep.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {
        if (!string.IsNullOrEmpty(enterTheName.text) && !string.IsNullOrEmpty(enterTheAge.text))
        {
            submitButton.interactable = true;
        }
        else
        {
            submitButton.interactable = false;  
        }
    }

   

    public void SubmitButton()
    {
        PlayerPrefs.SetString("name", enterTheName.text);
        PlayerPrefs.SetString("age", enterTheAge.text);
        submitButton.interactable = false;
        LoadingPanel.gameObject.SetActive(true);
        RectTransform loadingPanel=LoadingPanel.GetComponent<RectTransform>();
        StartCoroutine(MovePanel(new Vector2(0, 0), new Vector2(-768, 0), animationDuration));

    }
    private IEnumerator MovePanel(Vector2 from, Vector2 to, float duration)
    {
        float elapsed = 0f;
        InputrectTransform.anchoredPosition = from;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            InputrectTransform.anchoredPosition = Vector2.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        InputrectTransform.anchoredPosition = to; // Ensure it snaps to final position
        StartCoroutine(AnimateSlider(0f, 1f, 1.5f)); // Duration can be adjusted
        
    }
    private IEnumerator AnimateSlider(float from, float to, float duration)
    {
        float elapsed = 0f;
        slider.value = from;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            slider.value = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        slider.value = to; // Ensure final value
        LoadingPanel.gameObject.SetActive(false);
        gameController.SetActive(true);
        SpawnController.SetActive(true);
        FirstSpawnSheep.SetActive(true);
    }
}
    

