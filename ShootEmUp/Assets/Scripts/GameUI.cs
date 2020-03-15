using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{

    public Image fadePlane;
    public GameObject gameOverUI;

    public RectTransform newWaveBanner;
    public Text newWaveTitle;
    public Text newWaveEnemyCount;
    
    Spawner spawner;

    void Awake() {
        spawner = FindObjectOfType<Spawner> ();
        spawner.OnNewWave += OnNewWave;
    }

    void Start() {
        FindObjectOfType<Player>().OnDeath += OnGameOver;
    }

    void OnNewWave(int waveNumber) {
        string[] numbers = {"One","Two","Three","Four","Five"};
        newWaveTitle.text = " - Wave " + numbers[waveNumber-1] + " - ";
        string enemyCount = spawner.waves[waveNumber-1].enemyCount == -1 ? "Infinite" : spawner.waves[waveNumber-1].enemyCount.ToString();
        newWaveEnemyCount.text = "Enemies : " + enemyCount;

        StopCoroutine("AnimateNewWaveBanner");
        StartCoroutine("AnimateNewWaveBanner");
    }

    IEnumerator AnimateNewWaveBanner() {
        float speed = 2.5f;
        float delayTime = 2f;
        float percent = 0;
        int dir = 1;

        float endDelayTime = Time.time + 1/speed + delayTime;

        while (percent >= 0) {
            percent += Time.deltaTime * speed * dir;

            if (percent >= 1) {
                percent = 1;
                if (Time.time > endDelayTime) {
                    dir = -1;
                }
            }

            newWaveBanner.anchoredPosition = Vector2.up * Mathf.Lerp(-240, 0, percent);
            yield return null;
        }

    }

    void OnGameOver() {
        Cursor.visible = true;
        StartCoroutine(Fade(Color.clear, Color.black, 1));
        gameOverUI.SetActive(true);
    }

    IEnumerator Fade(Color from, Color to, float time) {
        float speed = 1/time;
        float percent = 0;

        while (percent < 1) {
            percent += Time.deltaTime*speed;
            fadePlane.color = Color.Lerp(from,to,percent);

            yield return null;
        }
    }

    // UI Input
    public void StartNewGame() {
        Application.LoadLevel("ShootEmUp");
    }
}
