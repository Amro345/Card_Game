using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PrimeTween;

public class Controller : MonoBehaviour
{
    [Header("Game")]
    [SerializeField] Card cardPrefab;
    [SerializeField] Transform gridTransform;
    [SerializeField] GridLayoutGroup gridLayout;
    [SerializeField] Sprite[] sprites;

    [Header("UI")]
    [SerializeField] GameObject btn3x2;
    [SerializeField] GameObject btn3x3;
    [SerializeField] GameObject btn3x4;
    [SerializeField] GameObject backButton;

    [Header("Audio")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip matchSound;
    [SerializeField] AudioClip mismatchSound;
    [SerializeField] AudioClip finishSound;
    [SerializeField] AudioClip buttonSound;

    private List<Sprite> spritePairs;
    Card firstSelected;
    Card secondSelected;
    int matchCounts;

    private void Start()
    {
        gridTransform.gameObject.SetActive(false);
        backButton.SetActive(false);
    }

    // Methods without parameters for OnClick
    public void StartGame3x2() { StartGame(3, 2); }
    public void StartGame3x3() { StartGame(3, 3); }
    public void StartGame3x4() { StartGame(3, 4); }

    void StartGame(int columns, int rows)
    {
        audioSource.PlayOneShot(buttonSound);

        btn3x2.SetActive(false);
        btn3x3.SetActive(false);
        btn3x4.SetActive(false);

        gridTransform.gameObject.SetActive(true);
        backButton.SetActive(true);

        SetupGrid(columns);
        PrepareSprites(columns * rows);
        CreateCards();
    }

    void SetupGrid(int columns)
    {
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = columns;
    }

    void PrepareSprites(int totalCards)
    {
        spritePairs = new List<Sprite>();
        int pairsCount = totalCards / 2;

        for (int i = 0; i < pairsCount; i++)
        {
            Sprite s = sprites[i % sprites.Length];
            spritePairs.Add(s);
            spritePairs.Add(s);
        }

        if (totalCards % 2 != 0)
            spritePairs.Add(sprites[0]);

        Shuffle(spritePairs);
    }

    void CreateCards()
    {
        foreach (Transform child in gridTransform)
            Destroy(child.gameObject);

        for (int i = 0; i < spritePairs.Count; i++)
        {
            Card card = Instantiate(cardPrefab, gridTransform);
            card.setIconSprite(spritePairs[i]);
            card.controller = this;
        }
    }

    void Shuffle(List<Sprite> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int r = Random.Range(0, i + 1);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }

    public void setSelected(Card card)
    {
        if (card.isSelected) return;

        card.Show();

        if (firstSelected == null)
        {
            firstSelected = card;
            return;
        }

        secondSelected = card;
        StartCoroutine(CheckMatching(firstSelected, secondSelected));

        firstSelected = null;
        secondSelected = null;
    }

    IEnumerator CheckMatching(Card a, Card b)
    {
        yield return new WaitForSeconds(0.3f);

        if (a.iconSprite == b.iconSprite)
        {
            matchCounts++;
            audioSource.PlayOneShot(matchSound);

            if (matchCounts >= spritePairs.Count / 2)
            {
                audioSource.PlayOneShot(finishSound);

                PrimeTween.Sequence.Create()
                    .Chain(Tween.Scale(gridTransform, Vector3.one * 1.2f, 0.2f))
                    .Chain(Tween.Scale(gridTransform, Vector3.one, 0.1f));
            }
        }
        else
        {
            audioSource.PlayOneShot(mismatchSound);
            a.Hide();
            b.Hide();
        }
    }

    public void Back()
    {
        audioSource.PlayOneShot(buttonSound);

        ClearGrid();

        gridTransform.gameObject.SetActive(false);
        backButton.SetActive(false);

        btn3x2.SetActive(true);
        btn3x3.SetActive(true);
        btn3x4.SetActive(true);
    }

    void ClearGrid()
    {
        foreach (Transform child in gridTransform)
            Destroy(child.gameObject);

        matchCounts = 0;
        firstSelected = null;
        secondSelected = null;
    }
}
