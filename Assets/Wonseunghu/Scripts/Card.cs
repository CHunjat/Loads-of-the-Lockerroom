using UnityEngine;
using TMPro;
using System.Collections;
using System;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    //카드 맞추기 필요한것

    //16개의 이미지 - 8개의 이미지 한쌍씩
    //각 인물을 담을변수 id
    //카드 뒤집는 코루틴 ien
    //맞았는지 안맞았는지 확인하는 booltype 변수
    //각 게임오브젝트 가져오기
    public GameObject FrontImage;
    public GameObject BackImage;
    public bool IsMatch;
    public int id;
    public TextMeshProUGUI idText; //임시 -> 프론트로 바꿀꺼
   
        
    public void init(int charachtorid,Sprite image) //이렇게 이미지 하는방법이..!
    {
        id = charachtorid;
        IsMatch = false; // 여기서 초기화
        if (image != null)
        {
            FrontImage.GetComponent<UnityEngine.UI.Image>().sprite = image;
        }
        if (idText != null) idText.text = id.ToString();
        BackImage.SetActive(true);
        FrontImage.SetActive(false);
    }

    public void Onclick()
    {
        if (IsMatch || !GameManager.Instance.canClick) return;
        if (this == GameManager.Instance.firstSelected) return;

        StartCoroutine(FlipAnimation(false));
        GameManager.Instance.CardSelected(this);
    }

    public IEnumerator FlipAnimation(bool back)
    {
        Image cardImg = FrontImage.GetComponent<Image>();
        if (!back)
        {
            if (cardImg != null) cardImg.color = Color.yellow; 
        }

        float time = 0;
        float flipSpeed = 0.09f;
        while (time < flipSpeed)
        {
            time += Time.deltaTime;
            transform.localScale = new Vector3(Mathf.Lerp(1, 0, time / 0.1f), 1, 1);
            yield return null;
        }
        //클릭시 텍스트 교체

        if (BackImage != null)
        {
            Console.WriteLine("이미지 없음");
        }

        if (back)
        {
            BackImage.SetActive(true);
            FrontImage.SetActive(false);
            if (cardImg != null) cardImg.color = Color.white;

        }
        else
        {
            BackImage.SetActive(false);
            FrontImage.SetActive(true);
        }

        time = 0;
        while (time < flipSpeed)
        {
            time += Time.deltaTime;
            transform.localScale = new Vector3(Mathf.Lerp(0, 1, time / 0.1f), 1, 1);
            yield return null;
        }
        if (!back)
        {
            for (int i = 0; i < 3; i++)
            {
                if (cardImg != null) cardImg.color = Color.yellow;
                yield return new WaitForSeconds(0.11f);
                if (cardImg != null) cardImg.color = Color.white;
                yield return new WaitForSeconds(0.11f);
            }
        }

    }

   

    public void SetMatched()
    {
        IsMatch = true;

    }

    public void ShowCard()
    {
        BackImage.SetActive(false);
        FrontImage.SetActive(true);
        // 테스트용 텍스트도 같이 켜주기
        if (idText != null) idText.gameObject.SetActive(true);
    }

    // 애니메이션 없이 즉시 뒷면을 보여주는 함수
    public void HideCard()
    {
        BackImage.SetActive(true);
        FrontImage.SetActive(false);
        if (idText != null) idText.gameObject.SetActive(false);
    }
}