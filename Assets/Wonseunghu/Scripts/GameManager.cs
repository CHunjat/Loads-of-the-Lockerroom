using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI & Prefabs")]
    public GameObject cardPrefab;
    public Transform gridParent;
    public TextMeshProUGUI scoreText;
    public GameObject restartButton;
    public GameObject GameStartButton;
    public GameObject GameOverText;

    [Header("Game State")]
    [HideInInspector] public Card firstSelected, secondSelected;
    [HideInInspector] public bool canClick = false; // 시작 연출 전까지 false

    [Header("Audio Settings")]
    public AudioSource audioSource;      // 소리를 재생할 스피커
    public AudioClip[] signatureSounds;  // 8명 형님들의 시그니처 사운드
    public AudioClip flipSound;          
    public AudioClip[] matchFailSound;     // 8명의 틀렸을 때 사운드
    public AudioSource bgmSource;        // [추가] 브금 전용 스피커
    public AudioClip backgroundMusic;    // 재생할 브금
    public AudioSource startVoiceSource; // 3, 2, 1 사운드가 담긴 오디오 소스
    public AudioClip countdownClip;
    [Header("Resource Settings")]
    public Sprite[] characterImages; // 1. 유니티 인스펙터에서 사진 8장을 넣을 칸

    private int flipCount = 0;
    private int matchedPairs = 0;
    private bool isGameStarted = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        if (restartButton != null) restartButton.SetActive(false);
        if (GameOverText != null) GameOverText.SetActive(false);

        if (GameStartButton != null) GameStartButton.SetActive(true);
        StartCoroutine(GameStartRoutine());
        if (bgmSource != null && backgroundMusic != null)
        {
            bgmSource.clip = backgroundMusic;
            bgmSource.loop = true; // 무한 반복 ♂️
            bgmSource.playOnAwake = true;
            bgmSource.Play();
        }
    }

    public void SetupBoard()
    {
        // 1. 16개 배열 생성
        int[] indices = { 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7 };

        // 2. 셔플 로직
        for (int i = 0; i < indices.Length; i++)
        {
            int rnd = Random.Range(i, indices.Length);
            int temp = indices[i];
            indices[i] = indices[rnd];
            indices[rnd] = temp;
        }

        // 3. 카드 생성
        foreach (int id in indices)
        {
            GameObject go = Instantiate(cardPrefab, gridParent);
            Card cardScript = go.GetComponent<Card>();
            if (cardScript != null)
            { 
                cardScript.init(id, characterImages[id]); 
            }
            else
            {
                cardScript.init(id, null); // 사진 대신 null을 넣어서 에러를 막음
            }
            // [중요] 시작 시 앞면을 보여주기 위해 강제 세팅
            cardScript.BackImage.SetActive(false);
            cardScript.FrontImage.SetActive(true);
        }
    }
    public void StartTheGame() => isGameStarted = true;
    IEnumerator GameStartRoutine()
    {
        canClick = false;

        yield return new WaitUntil(() => isGameStarted);
        if (GameStartButton != null) GameStartButton.SetActive(false);

        SetupBoard();
        yield return new WaitForSeconds(2.5f);

        Card[] allCards = Object.FindObjectsByType<Card>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);



        foreach (Card card in allCards)
        {
            card.ShowCard();
        }

        if (startVoiceSource != null && countdownClip != null)
        {
            startVoiceSource.PlayOneShot(countdownClip);
        }
        yield return new WaitForSeconds(2.8f);

        foreach (Card card in allCards)
        {
            StartCoroutine(card.FlipAnimation(true));
        }

        yield return new WaitForSeconds(0.2f);
        canClick = true;
    }

    public void CardSelected(Card card)
    {
        if (firstSelected == null)
        {
            firstSelected = card;
            audioSource.PlayOneShot(flipSound);
        }
        else
        {
            secondSelected = card;
            audioSource.PlayOneShot(flipSound);
            StartCoroutine(CheckMatch());
        }
    }

    IEnumerator ShakeUI(RectTransform uiElement)
    {
        Vector3 originPos = uiElement.localPosition;
        float elapsed = 0f;
        while (elapsed < 0.3f) // 0.2초 동안 흔듦
        {
            float x = Random.Range(-10f, 10f);
            float y = Random.Range(-10f, 10f);
            uiElement.localPosition = new Vector3(originPos.x + x, originPos.y + y, originPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        uiElement.localPosition = originPos;
    }

    IEnumerator CheckMatch()
    {
        canClick = false;
        flipCount++;
        if (scoreText != null) scoreText.text = $"Flip: {flipCount}";

        if (firstSelected.id == secondSelected.id)
        {
            // 매칭 성공
            firstSelected.SetMatched();
            secondSelected.SetMatched();
            matchedPairs++;
            audioSource.PlayOneShot(signatureSounds[firstSelected.id]);
            StartCoroutine(ShakeUI(gridParent.GetComponent<RectTransform>())); 
            if (matchedPairs == 8)
            {

                if (bgmSource != null)
                {
                    bgmSource.Stop();
                }
                if (restartButton != null) 
                {
                    GameOverText.SetActive(true);
                    restartButton.SetActive(true);
                    bgmSource.Play();
                }
            }
        }
        else
        {
            // 매칭 실패
            yield return new WaitForSeconds(1.1f);

            // 두 카드가 동시에 뒤집히는 것을 기다림
            IEnumerator flip1 = firstSelected.FlipAnimation(true);
            IEnumerator flip2 = secondSelected.FlipAnimation(true);
            audioSource.PlayOneShot(matchFailSound[firstSelected.id]);
            StartCoroutine(flip1);
            yield return StartCoroutine(flip2);
        }

        firstSelected = null;
        secondSelected = null;
        canClick = true;
    }

    public void Restart()
    {
        // 0번 인덱스 씬을 불러오기
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}