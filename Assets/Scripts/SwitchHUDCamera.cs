using SickscoreGames.HUDNavigationSystem;
using UnityEngine;

public class SwitchHUDCamera : MonoBehaviour
{
  public HUDNavigationSystem HUDNavigationSystem;


    public void SwitchCamera(Camera cam)
    {
        HUDNavigationSystem.ChangePlayerCamera(cam);
    }
}
