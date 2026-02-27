using UnityEngine;
using TMPro;
using System.Collections.Generic;

// Maneja los diálogos entre el jugador y los NPCs, incluyendo la UI y el cambio de cámaras
public class DialogueManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI dialogueText;

    private List<Line> currentLines;
    private int currentIndex;

    public bool isDialogueActive = false; 
    private GuardianController currentGuardian = null;
    private CamemiController currentCamemi = null;
    private PompompurinController playerController;

    [Header("Cameras")]
    public GameObject freeLookCamera;
    public GameObject dialogueCamera;

    // Inicia un diálogo con un guardián específico
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

    // Inicia un diálogo con Camemi
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

    // Fuerza el cierre del diálogo
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

    // Muestra la línea actual del diálogo
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

    // Avanza a la siguiente línea del diálogo
    public void NextLine()
    {
        currentIndex++;
        DisplayLine();
    }

    // Termina el diálogo y restaura el estado del juego
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

    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }

    //══════════════════════════════
    //  CONTROL DEL JUGADOR
    // ══════════════════════════════

    // Deshabilita los controles del jugador durante un diálogo
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

    // Restaura los controles del jugador después de un diálogo
    void EnablePlayerControls()
    {
        if (playerController != null)
        {
            playerController.ExitDialogue();
                playerController.enabled = true;
            }
        }
    }