using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StarterHandler : MonoBehaviour
{
    public Slider slider;
    public RectTransform StartPanel;
    public int storeSceneValue;
    public GameOverPanel gameOverpanel;
    public GameObject InstructionPanel;
    private int checkindex = 0;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameOverpanel = FindObjectOfType<GameOverPanel>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void TaskButton(int index)
    {
        if (index == 1)
        {
            checkindex = 1;
            storeSceneValue = index;
            StartCoroutine(MovePanel(new Vector2(0, 0), new Vector2(-1000, 0), 1.5f));
        }
        else
        {
            checkindex = 2;
            storeSceneValue = index;
            StartCoroutine(MovePanel(new Vector2(0, 0), new Vector2(-1000, 0), 1.5f));
        }
    }


    public void Instruction()
    {
        InstructionPanel.gameObject.SetActive(true);
        this.gameObject.SetActive(false);
        gameOverpanel.AnimatePanel(InstructionPanel.GetComponent<RectTransform>(),true);
    }

    public void loadingSlider()
    {
        InstructionPanel.gameObject.SetActive(false);
        this.gameObject.SetActive(true);
        StartCoroutine(AnimateSlider(0f, 1f, 1.5f)); // Duration can be adjusted
    }
    private IEnumerator MovePanel(Vector2 from, Vector2 to, float duration)
    {
        float elapsed = 0f;
        StartPanel.anchoredPosition = from;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            StartPanel.anchoredPosition = Vector2.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        StartPanel.anchoredPosition = to; // Ensure it snaps to final position
        if (checkindex == 1)
        {
            Instruction();
        }
        else
        { 
            StartCoroutine(AnimateSlider(0f, 1f, 1.5f)); // Duration can be adjusted
        }
        
      

    }

    private IEnumerator AnimateSlider(float from, float to, float duration)
    {
        slider.gameObject.SetActive(true);
        float elapsed = 0f;
        slider.value = from;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            slider.value = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        slider.value = to; // Ensure final value
        SceneManagment(storeSceneValue);
    }
    public void SceneManagment(int index)
    {
        SceneManager.LoadScene(index);
    }
}
