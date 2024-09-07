using UnityEngine;
//using UnityEngine.iOS;
public class Links : Singleton<Links>
{
    public void Exit()
    {
        Application.Quit();
    }
    public void OpenLink(string link) {
        Application.OpenURL(link);
    
    }
    public void RateUS()
    {

        ///For iOS
      //  Device.RequestStoreReview();
    }
}
