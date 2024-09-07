using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    private LevelSelectionPanel levelSelectionPanel;
 
    public void AllMethods()
    {
        //////Show Panel by Type//////
        UIManager.Instance.ShowPanel<LevelSelectionPanel>();

        //////Show Panel by Type and Do Action When Called//////
        UIManager.Instance.ShowPanel<LevelSelectionPanel>(HelloWorld);

        //////Show Panel by through Parameter (Can Be used in inpsector)/////

        UIManager.Instance.ShowPanel(levelSelectionPanel);

        ///=============>>>>>> Same like above to Show Overlay Panels; <<<<<============//////
        UIManager.Instance.ShowPanelOverlay<LevelSelectionPanel>();
        UIManager.Instance.ShowPanelOverlay<LevelSelectionPanel>(HelloWorldOverlay);
        UIManager.Instance.ShowPanelOverlay(levelSelectionPanel);


    }

    public void HelloWorld()
    {
        Debug.Log("THIS IS HELLO WORLD WHEN PANEL TURNED ON");
    }  
    public void HelloWorldOverlay()
    {
        Debug.Log("THIS IS HELLO WORLD WHEN ```OVERLAY``` PANEL TURNED ON");
    }
  
}
