using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasWin : UICanvas
{
    public void NextLVBTN()
    {
        AudioManager.Instance?.PlayButton();
        LevelManager.Instance.NextLevel();
        UIManager.Instance.CloseUIDirectly<CanvasWin>();
    }

    public void ReplayBTn()
    {
        AudioManager.Instance?.PlayButton();
        LevelManager.Instance.Replay();
        UIManager.Instance.CloseUIDirectly<CanvasWin>();
    }
}
