

public class LevelCompletePanel : UIPanel
{
    private void Start()
    {
        if (GameConstant.Haptics)
        {
            Haptics.Instance.PlaySimple();
        }
    }
}
