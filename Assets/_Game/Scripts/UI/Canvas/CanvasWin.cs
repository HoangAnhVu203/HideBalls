using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasWin : UICanvas
{
    public void NextLVBTN()
    {
        LevelManager.Instance.NextLevel();
        UIManager.Instance.CloseUIDirectly<CanvasWin>();
    }

    public void ReplayBTn()
    {
        LevelManager.Instance.Replay();
        UIManager.Instance.CloseUIDirectly<CanvasWin>();
    }
}
