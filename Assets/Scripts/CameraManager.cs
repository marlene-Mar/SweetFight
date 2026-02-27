using UnityEngine;

//Cambio de cámara entre la cámara de movimiento libre y la cámara de diálogo
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