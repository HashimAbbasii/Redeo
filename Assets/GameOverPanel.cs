using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class GameOverPanel : MonoBehaviour
{
    public static GameOverPanel instance;
    public GameObject gameOverPanel;
    private void Awake()
    {
        if(instance == null)
        {
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Restart()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
        gameOverPanel.SetActive(false);
    } 
    public void Home()
    {
        SceneManager.LoadScene(0);
        gameOverPanel.gameObject.SetActive(false);
    }
    public void GameOver()
    {
        gameOverPanel.SetActive(true);
        AnimatePanel(gameOverPanel.GetComponent<RectTransform>(),true);
    }
    public void AnimatePanel(RectTransform panel, bool show, float duration = 0.5f)
    {
        Vector3 startScale = show ? Vector3.zero : Vector3.one;
        Vector3 endScale = show ? Vector3.one : Vector3.zero;
        StartCoroutine(ScalePanel(panel, startScale, endScale, duration));
    }
    private IEnumerator ScalePanel(RectTransform panel, Vector3 from, Vector3 to, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            panel.localScale = Vector3.Lerp(from, to, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        panel.localScale = to; // Ensure final scale is set
    }
}
