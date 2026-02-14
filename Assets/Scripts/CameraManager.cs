using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public GameObject freeLookCam;
    public GameObject dialogueCam;

    public void SetFreeLook()
    {
        freeLookCam.SetActive(true);
        dialogueCam.SetActive(false);
    }

    public void SetDialogueCamera()
    {
        freeLookCam.SetActive(false);
        dialogueCam.SetActive(true);
    }
}