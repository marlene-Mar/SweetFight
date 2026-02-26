using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI dialogueText;

    private List<Line> currentLines;
    private int currentIndex;

    private bool isDialogueActive = false;
    private GuardianController currentGuardian = null;
    private CamemiController currentCamemi = null;
    private PompompurinController playerController;

    [Header("Cameras")]
    public GameObject freeLookCamera;
    public GameObject dialogueCamera;

    public void StartGuardianDialogue(Dialogos dialogo, GuardianController guardian)
    {
        // Prevenir iniciar otro diálogo si ya hay uno activo
        if (isDialogueActive)
        {
            Debug.LogWarning("DialogueManager: Ya hay un diálogo activo, se ignora el nuevo.");
            return;
        }

        isDialogueActive = true;
        dialoguePanel.SetActive(true);

        currentGuardian = guardian;
        currentLines = dialogo.conversationLines;
        currentIndex = 0;

        //Cambiar cámaras
        if (freeLookCamera != null) freeLookCamera.SetActive(false);
        if (dialogueCamera != null) dialogueCamera.SetActive(true);

        // Deshabilitar controles del jugador
        DisablePlayerControls();
        DisplayLine();
    }

    public void StartCamemiDialogue(Dialogos dialogo, CamemiController camemi)
    {
        // Prevenir iniciar otro diálogo si ya hay uno activo
        if (isDialogueActive)
        {
            Debug.LogWarning("DialogueManager: Ya hay un diálogo activo, se ignora el nuevo.");
            return;
        }

        isDialogueActive = true;
        dialoguePanel.SetActive(true);

        currentCamemi = camemi;
        currentLines = dialogo.conversationLines;
        currentIndex = 0;

        //Cambiar cámaras
        if (freeLookCamera != null) freeLookCamera.SetActive(false);
        if (dialogueCamera != null) dialogueCamera.SetActive(true);

        // Deshabilitar controles del jugador
        DisablePlayerControls();
        DisplayLine();
    }

    public void ForceCloseDialogue()
    {
        if (freeLookCamera != null) freeLookCamera.SetActive(true);
        if (dialogueCamera != null) dialogueCamera.SetActive(false);

        dialoguePanel.SetActive(false);

        currentGuardian = null;
        currentCamemi = null;
        isDialogueActive = false;

        EnablePlayerControls();
    }

    void DisplayLine()
    {
        if (currentIndex < currentLines.Count)
        {
            speakerText.text = currentLines[currentIndex].speakerName;
            dialogueText.text = currentLines[currentIndex].dialogueLine;
        }
        else
        {
            EndDialogue();
        }
    }

    public void NextLine()
    {
        currentIndex++;
        DisplayLine();
    }

    void EndDialogue()
    {
        if (freeLookCamera != null) freeLookCamera.SetActive(true);
        if (dialogueCamera != null) dialogueCamera.SetActive(false);

        dialoguePanel.SetActive(false);

        if (currentGuardian != null)
        {
            currentGuardian.EndDialogue();
            currentGuardian = null;
        }

        if (currentCamemi != null)
        {
            currentCamemi.EndDialogue();
            currentCamemi = null;
        }

        EnablePlayerControls();
        isDialogueActive = false;
    }


//═════════════════════════════════════════════════════════════════
//  CONTROL DEL JUGADOR
// ═════════════════════════════════════════════════════════════════
void DisablePlayerControls()
{
    GameObject player = GameObject.FindGameObjectWithTag("Player");
    if (player != null)
    {
        playerController = player.GetComponent<PompompurinController>();
        if (playerController != null)
        {
            playerController.EnterDialogue();
            playerController.enabled = false;
        }
    }
}

void EnablePlayerControls()
{
    if (playerController != null)
    {
          playerController.ExitDialogue();
            playerController.enabled = true;
        }
    }
}