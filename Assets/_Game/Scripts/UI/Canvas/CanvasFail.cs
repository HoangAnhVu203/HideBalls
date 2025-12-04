using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasFail : UICanvas
{
    public void RePlayBTN()
    {
        AudioManager.Instance?.PlayButton();
        LevelManager.Instance.Replay();
        UIManager.Instance.CloseUIDirectly<CanvasFail>();
    }
}
