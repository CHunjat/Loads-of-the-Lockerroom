using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class TitleManager : MonoBehaviour
{
    [Header("사운드 설정")]
    public AudioSource bgmPlayer;       // 타이틀 배경음악 플레이어
    public AudioSource sfxPlayer;       // 효과음 플레이어
    public AudioClip clickSound;        // 버튼 클릭 효과음 파일
    public AudioClip countdownSound;    // 3초 남았을 때 재생할 킹받는 효과음!

    [Header("페이드 아웃 설정")]
    public Image fadeImage;             // 화면을 덮을 검은색 이미지
    public float fadeDuration = 1.5f;   // 화면이 까매지는 데 걸리는 시간 (초)

    [Header("버튼 잠금 설정 (강제 시청용)")]
    public Button startButton;          // 시작 버튼 컴포넌트
    public TextMeshProUGUI countdownText; // 실시간 카운트다운을 띄울 텍스트!

    [Header("고퀄리티 카메라 연출")]
    public Camera mainCamera;           // 흔들림 연출을 줄 메인 카메라
    public float swaySpeed = 0.5f;      // 카메라 흔들림 속도
    public float swayAmount = 1.5f;     // 카메라 흔들림 강도

    [Header("♂ 딥 다크 판타지 연출 ♂")]
    public bool enableDeepDarkVibe = true; // 치명적인 모드 켜기/끄기
    public float heartbeatSpeed = 4f;      // 심장 박동(줌) 속도
    public float heartbeatAmount = 1.5f;   // 심장 박동 강도

    private Quaternion initialCameraRot; // 카메라 초기 각도 저장용
    private float initialFOV;            // 카메라 초기 시야각(줌) 저장용

    private bool isStarting = false;    // 중복 클릭 방지용 변수

    void Start()
    {
        // 씬이 시작되면 페이드 이미지를 투명하게 만들고 Raycast Target을 꺼서 버튼 클릭을 방해하지 않게 합니다.
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
            fadeImage.raycastTarget = false;
        }

        // BGM이 할당되어 있고, 스피커 전원이 켜져 있을 때만 재생 (에러 방지)
        if (bgmPlayer != null && bgmPlayer.gameObject.activeInHierarchy && bgmPlayer.enabled && !bgmPlayer.isPlaying)
        {
            bgmPlayer.Play();
        }

        // 시작하자마자 10초 버튼 잠금 코루틴 실행
        if (startButton != null)
        {
            StartCoroutine(LockStartButtonRoutine());
        }

        // 카메라 초기 각도 및 시야각(FOV) 저장
        if (mainCamera != null)
        {
            initialCameraRot = mainCamera.transform.rotation;
            initialFOV = mainCamera.fieldOfView;
        }
    }

    void Update()
    {
        if (mainCamera != null && !isStarting)
        {
            // 1. 기본 카메라 숨쉬기(Sway) 연출
            float swayX = Mathf.Sin(Time.time * swaySpeed) * swayAmount;
            float swayY = Mathf.Cos(Time.time * swaySpeed * 0.8f) * swayAmount;
            mainCamera.transform.rotation = initialCameraRot * Quaternion.Euler(swayX, swayY, 0);

            // 2. ♂ 딥 다크 박동(Heartbeat) 연출 ♂
            if (enableDeepDarkVibe)
            {
                // 화면이 심장 뛰듯이 미세하게 줌인/줌아웃 반복
                mainCamera.fieldOfView = initialFOV - Mathf.Abs(Mathf.Sin(Time.time * heartbeatSpeed)) * heartbeatAmount;

                // 텍스트가 흰색 ↔ 핫핑크색으로 치명적으로 깜빡임
                if (countdownText != null)
                {
                    Color hotPink = new Color(1f, 0.2f, 0.8f); // 핫핑크
                    countdownText.color = Color.Lerp(Color.white, hotPink, Mathf.PingPong(Time.time * 2f, 1f));
                }
            }
        }
    }

    // 10초간 춤을 강제 시청하게 만드는 실시간 카운트다운 코루틴
    private IEnumerator LockStartButtonRoutine()
    {
        startButton.interactable = false;
        int waitTime = 10;

        while (waitTime > 0)
        {
            if (countdownText != null)
            {
                countdownText.text = $"{waitTime}초 후 락커룸 입장가능...";
            }

            if (waitTime == 3 && sfxPlayer != null && countdownSound != null)
            {
                sfxPlayer.PlayOneShot(countdownSound);
            }

            yield return new WaitForSeconds(1f);
            waitTime--;
        }

        if (countdownText != null)
        {
            countdownText.text = "들어올땐 마음대로였지만 나갈땐 아니란다 ♂";
            countdownText.color = new Color(1f, 0.2f, 0.8f); // 마지막엔 완전한 핫핑크로 고정
        }

        startButton.interactable = true;
    }

    public void OnClickStartGame()
    {
        if (isStarting) return;
        isStarting = true;

        if (sfxPlayer != null && clickSound != null)
        {
            sfxPlayer.PlayOneShot(clickSound);
        }

        StartCoroutine(FadeOutAndLoadScene("MainScene"));
    }

    private IEnumerator FadeOutAndLoadScene(string sceneName)
    {
        if (fadeImage != null)
        {
            fadeImage.raycastTarget = true;
            float elapsedTime = 0f;
            Color c = fadeImage.color;
            float startVolume = (bgmPlayer != null) ? bgmPlayer.volume : 0f;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                c.a = Mathf.Clamp01(elapsedTime / fadeDuration);
                fadeImage.color = c;

                if (bgmPlayer != null)
                {
                    bgmPlayer.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / fadeDuration);
                }

                yield return null;
            }
        }
        SceneManager.LoadScene(sceneName);
    }

    public void OnClickQuitGame()
    {
        if (isStarting) return;

        if (sfxPlayer != null && clickSound != null)
        {
            sfxPlayer.PlayOneShot(clickSound);
        }

        Application.Quit();
    }
}