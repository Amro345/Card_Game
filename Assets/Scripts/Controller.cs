using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
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
    [SerializeField] TextMeshProUGUI scoreText;

    [Header("Audio")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip matchSound;
    [SerializeField] AudioClip mismatchSound;
    [SerializeField] AudioClip finishSound;
    [SerializeField] AudioClip buttonSound;

    private List<Sprite> spritePairs;

    private Card firstSelected;
    private Card secondSelected;

    private int matchCounts;
    private int score;

    private int currentColumns;
    private int currentRows;

    // Store matched card IDs
    private HashSet<int> matchedCards = new HashSet<int>();

    void Start()
    {
        gridTransform.gameObject.SetActive(false);
        backButton.SetActive(false);
        scoreText.gameObject.SetActive(false);

        LoadProgress();
    }

    // Button methods
    public void StartGame3x2() { StartGame(3, 2); }
    public void StartGame3x3() { StartGame(3, 3); }
    public void StartGame3x4() { StartGame(3, 4); }

    void StartGame(int columns, int rows)
    {
        audioSource.PlayOneShot(buttonSound);

        currentColumns = columns;
        currentRows = rows;

        matchedCards.Clear();

        btn3x2.SetActive(false);
        btn3x3.SetActive(false);
        btn3x4.SetActive(false);

        gridTransform.gameObject.SetActive(true);
        backButton.SetActive(true);
        scoreText.gameObject.SetActive(true);

        score = 0;
        matchCounts = 0;

        UpdateScoreUI();

        SetupGrid(columns);
        PrepareSprites(columns * rows);
        CreateCards();

        SaveProgress();
    }

    // Setup grid columns
    void SetupGrid(int columns)
    {
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = columns;
    }

    // Prepare sprite pairs
    void PrepareSprites(int totalCards)
    {
        spritePairs = new List<Sprite>();

        int pairs = totalCards / 2;

        for (int i = 0; i < pairs; i++)
        {
            Sprite s = sprites[i % sprites.Length];

            spritePairs.Add(s);
            spritePairs.Add(s);
        }

        if (totalCards % 2 != 0)
            spritePairs.Add(sprites[0]);

        Shuffle(spritePairs);
    }

    // Create cards
    void CreateCards()
    {
        foreach (Transform child in gridTransform)
            Destroy(child.gameObject);

        for (int i = 0; i < spritePairs.Count; i++)
        {
            Card card = Instantiate(cardPrefab, gridTransform);

            card.cardID = i;
            card.setIconSprite(spritePairs[i]);
            card.controller = this;

            if (matchedCards.Contains(i))
                card.isMatched = true;

            card.HideInstant();
        }
    }

    // Shuffle cards
    void Shuffle(List<Sprite> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int r = Random.Range(0, i + 1);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }

    // When card is clicked
    public void setSelected(Card card)
    {
        if (card.isSelected || card.isMatched) return;

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

    // Check match
    IEnumerator CheckMatching(Card a, Card b)
    {
        yield return new WaitForSeconds(0.3f);

        if (a.iconSprite == b.iconSprite)
        {
            matchCounts++;
            score++;

            a.isMatched = true;
            b.isMatched = true;

            matchedCards.Add(a.cardID);
            matchedCards.Add(b.cardID);

            UpdateScoreUI();
            SaveProgress();

            audioSource.PlayOneShot(matchSound);

            if (matchCounts >= spritePairs.Count / 2)
            {
                audioSource.PlayOneShot(finishSound);

                PrimeTween.Sequence.Create()
                    .Chain(Tween.Scale(gridTransform, Vector3.one * 1.2f, 0.2f))
                    .Chain(Tween.Scale(gridTransform, Vector3.one, 0.1f));

                scoreText.text = "Great Job! Final Score: " + score;

                PlayerPrefs.DeleteAll();
            }
        }
        else
        {
            audioSource.PlayOneShot(mismatchSound);

            a.Hide();
            b.Hide();
        }
    }

    // Update score UI
    void UpdateScoreUI()
    {
        scoreText.text = "Score: " + score;
    }

    // Back button
    public void Back()
    {
        audioSource.PlayOneShot(buttonSound);

        ClearGrid();

        matchedCards.Clear();
        PlayerPrefs.DeleteAll();

        gridTransform.gameObject.SetActive(false);
        backButton.SetActive(false);
        scoreText.gameObject.SetActive(false);

        btn3x2.SetActive(true);
        btn3x3.SetActive(true);
        btn3x4.SetActive(true);
    }

    // Clear grid
    void ClearGrid()
    {
        foreach (Transform child in gridTransform)
            Destroy(child.gameObject);

        matchCounts = 0;
        score = 0;

        firstSelected = null;
        secondSelected = null;
    }

    // Save progress
    void SaveProgress()
    {
        PlayerPrefs.SetInt("Score", score);
        PlayerPrefs.SetInt("MatchCount", matchCounts);
        PlayerPrefs.SetInt("Columns", currentColumns);
        PlayerPrefs.SetInt("Rows", currentRows);

        string matched = string.Join(",", matchedCards);
        PlayerPrefs.SetString("Matched", matched);

        List<int> spriteIndexes = new List<int>();

        foreach (var sp in spritePairs)
            spriteIndexes.Add(System.Array.IndexOf(sprites, sp));

        string order = string.Join(",", spriteIndexes);
        PlayerPrefs.SetString("Order", order);

        PlayerPrefs.Save();
    }


    // Load progress
    void LoadProgress()
    {
        if (!PlayerPrefs.HasKey("Score")) return;

        score = PlayerPrefs.GetInt("Score");
        matchCounts = PlayerPrefs.GetInt("MatchCount");
        currentColumns = PlayerPrefs.GetInt("Columns");
        currentRows = PlayerPrefs.GetInt("Rows");

        matchedCards.Clear();

        if (PlayerPrefs.HasKey("Matched"))
        {
            string data = PlayerPrefs.GetString("Matched");

            if (!string.IsNullOrEmpty(data))
            {
                string[] ids = data.Split(',');

                foreach (string id in ids)
                {
                    if (int.TryParse(id, out int v))
                        matchedCards.Add(v);
                }
            }
        }

        spritePairs = new List<Sprite>();

        if (PlayerPrefs.HasKey("Order"))
        {
            string order = PlayerPrefs.GetString("Order");
            string[] data = order.Split(',');

            foreach (string s in data)
            {
                int index = int.Parse(s);
                spritePairs.Add(sprites[index]);
            }
        }

        btn3x2.SetActive(false);
        btn3x3.SetActive(false);
        btn3x4.SetActive(false);

        gridTransform.gameObject.SetActive(true);
        backButton.SetActive(true);
        scoreText.gameObject.SetActive(true);

        UpdateScoreUI();

        SetupGrid(currentColumns);
        CreateCards();
    }

}
