using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CanvasGamePlay : UICanvas
{
    [Header("Level UI")]
    public Image ImgLV;          // ảnh nền LEVEL (panel nhỏ)
    public Text levelText;       // text "LEVEL X"
    public float showTime = 3f;

    Coroutine hideRoutine;

    void Awake()
    {
        // Ẩn lúc đầu
        if (levelText)
            levelText.gameObject.SetActive(false);

        if (ImgLV)
            ImgLV.gameObject.SetActive(false);
    }

    // =================================
    // HIỂN THỊ LEVEL
    // =================================

    public void ShowLevel(int levelIndex)
    {
        if (ImgLV)
            ImgLV.gameObject.SetActive(true);

        if (levelText)
        {
            levelText.gameObject.SetActive(true);
            levelText.text = "Level " + (levelIndex + 1);
        }

        if (hideRoutine != null)
            StopCoroutine(hideRoutine);

        hideRoutine = StartCoroutine(HideLevelText());
    }

    IEnumerator HideLevelText()
    {
        yield return new WaitForSeconds(showTime);

        if (levelText)
            levelText.gameObject.SetActive(false);

        if (ImgLV)
            ImgLV.gameObject.SetActive(false);
    }

    // =================================
    // UI BUTTON EVENTS
    // =================================

    public void SettingBTN()
    {
        UIManager.Instance.OpenUI<CanvasSetting>();
    }

    public void NextBTN()
    {
        LevelManager.Instance.NextLevel();
    }

    public void RePlayBTN()
    {
        LevelManager.Instance.Replay();
    }

    public void HintBTN()
    {
        // TODO: Hint
    }
}
