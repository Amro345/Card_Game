using UnityEngine;
using UnityEngine.UI;
using PrimeTween;

public class Card : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip flipSound;

    public Sprite hiddenIconSprite;
    public Sprite iconSprite;

    public bool isSelected;
    public bool isMatched;

    public int cardID;

    public Controller controller;

    public void onCardClick()
    {
        controller.setSelected(this);
    }

    public void setIconSprite(Sprite sp)
    {
        iconSprite = sp;
    }

    // Show card
    public void Show()
    {
        audioSource.PlayOneShot(flipSound);

        Tween.Rotation(transform, new Vector3(0f, 180f, 0f), 0.2f);

        Tween.Delay(0.1f, () =>
        {
            iconImage.sprite = iconSprite;
        });

        isSelected = true;
    }

    // Hide card
    public void Hide()
    {
        audioSource.PlayOneShot(flipSound);

        Tween.Rotation(transform, Vector3.zero, 0.2f);

        Tween.Delay(0.1f, () =>
        {
            iconImage.sprite = hiddenIconSprite;
            isSelected = false;
        });
    }

    // Hide instantly (used on load)
    public void HideInstant()
    {
        if (isMatched)
        {
            iconImage.sprite = iconSprite;
            transform.rotation = Quaternion.Euler(0, 180, 0);
            isSelected = true;
            return;
        }

        iconImage.sprite = hiddenIconSprite;
        isSelected = false;
        transform.rotation = Quaternion.identity;
    }
}
