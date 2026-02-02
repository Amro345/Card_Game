using System.Collections;
using System.Collections.Generic;
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
    public Controller controller;

    public void onCardClick()
    {
        controller.setSelected(this);
    }

    public void setIconSprite(Sprite sp)
    {
        iconSprite = sp;
    }

    public void Show()
    {
        audioSource.PlayOneShot(flipSound);
        Tween.Rotation(transform, new Vector3(0f, 180f, 0f), 0.2f);
        Tween.Delay(0.1f, () => iconImage.sprite = iconSprite);
        isSelected = true;
    }

    public void Hide()
    {
        audioSource.PlayOneShot(flipSound);
        Tween.Rotation(transform, new Vector3(0f, 0f, 0f), 0.2f);
        Tween.Delay(0.1f, () =>
        {
            iconImage.sprite = hiddenIconSprite;
            isSelected = false;
        });
    }
}
