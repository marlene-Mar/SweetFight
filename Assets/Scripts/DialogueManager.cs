using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public GameObject responseButtonsPanel;
    public Button responseButton;

    [Header("Configuración")]
    public float textSpeed = 0.05f;

    private GuardianController currentGuardian;
    private PompompurinController playerController;
    private int currentDialogueIndex = 0;
    private bool isTyping = false;

    public GameObject freeLookCamera;
    public GameObject dialogueCamera;

    [System.Serializable]
    public class DialogueLine
    {
        public string speaker;
        public string text;
        public bool requiresPlayerResponse;
        public string responseButtonText;
    }

    private List<DialogueLine> guardianConversation = new List<DialogueLine>()
    {
        new DialogueLine
        {
            speaker = "Guardian",
            text = "Hola, escuché que había un desconocido, te estaba buscando.",
            requiresPlayerResponse = true,
            responseButtonText = "Hola, soy Pompompurin. Vengo en búsqueda de Camemi"
        },
        new DialogueLine
        {
            speaker = "Guardian",
            text = "Esa rata astuta, está asustando a los habitantes, no puedo derrotarlo solo.",
            requiresPlayerResponse = true,
            responseButtonText = "Vengo a derrotarlo"
        },
        new DialogueLine
        {
            speaker = "Guardian",
            text = "JAJA, ¿Tú?, si te ves más adorable que yo. Te ayudaré si me derrotas.",
            requiresPlayerResponse = true,
            responseButtonText = "¡Hecho!"
        }
    };

    void Start()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (responseButtonsPanel != null)
            responseButtonsPanel.SetActive(false);
    }

    public void StartGuardianDialogue(GuardianController guardian)
    {
        freeLookCamera.SetActive(false);
        dialogueCamera.SetActive(true);

        currentGuardian = guardian;
        currentDialogueIndex = 0;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        DisablePlayerControls();
        DisplayNextLine();
    }

    void DisablePlayerControls()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerController = player.GetComponent<PompompurinController>();
            if (playerController != null)
            {
                playerController.EnterDialogue();  // ← Activar estado de diálogo
                playerController.enabled = false;
            }
        }
    }

    void EnablePlayerControls()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            if (playerController != null)
            {
                playerController.ExitDialogue();    // ← Primero salir del diálogo
                playerController.enabled = true;     // ← Luego habilitar el script
            }
        }
    }

    void DisplayNextLine()
    {
        if (currentDialogueIndex >= guardianConversation.Count)
        {
            EndDialogue();
            return;
        }

        DialogueLine currentLine = guardianConversation[currentDialogueIndex];

        StopAllCoroutines();
        StartCoroutine(TypeText(currentLine.text));

        if (currentLine.requiresPlayerResponse)
        {
            SetupResponseButton(currentLine.responseButtonText);
        }
        else
        {
            if (responseButtonsPanel != null)
                responseButtonsPanel.SetActive(false);
        }
    }

    IEnumerator TypeText(string text)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in text.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;
    }

    void SetupResponseButton(string buttonText)
    {
        if (responseButtonsPanel != null)
            responseButtonsPanel.SetActive(true);

        if (responseButton != null)
        {
            TextMeshProUGUI buttonTextComponent = responseButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonTextComponent != null)
                buttonTextComponent.text = buttonText;

            responseButton.onClick.RemoveAllListeners();
            responseButton.onClick.AddListener(OnResponseButtonClicked);
        }
    }

    public void OnResponseButtonClicked()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueText.text = guardianConversation[currentDialogueIndex].text;
            isTyping = false;
            return;
        }

        currentDialogueIndex++;

        if (currentDialogueIndex >= guardianConversation.Count)
        {
            EndDialogue();
            StartCombat();
        }
        else
        {
            DisplayNextLine();
        }
    }

    void EndDialogue()
    {
        freeLookCamera.SetActive(true);
        dialogueCamera.SetActive(false);

        // Avisar al guardián primero
        if (currentGuardian != null)
        {
            currentGuardian.EndDialogue();
            currentGuardian = null;
        }

        // Cerrar UI
        if (dialoguePanel != null && dialoguePanel.activeSelf)
            dialoguePanel.SetActive(false);

        if (responseButtonsPanel != null && responseButtonsPanel.activeSelf)
            responseButtonsPanel.SetActive(false);

        // Devolver controles
        EnablePlayerControls();
    }

    void StartCombat()
    {
        if (currentGuardian != null && playerController != null)
        {
            CombatManager cm = FindObjectOfType<CombatManager>();
            cm.StartCombat(currentGuardian, playerController);
            playerController.StartCombatAfterDialogue();
        }
    }
}

//using UnityEngine;

//public class DialogueManager : MonoBehaviour
//{
//    public GameObject dialoguePanel;

//    private GuardianController currentGuardian;

//    public void StartGuardianDialogue(GuardianController guardian)
//    {
//        currentGuardian = guardian;

//        dialoguePanel.SetActive(true);

//        GameFlowManager.Instance.ChangeState(GameState.Dialogue);
//    }

//    public void EndDialogue()
//    {
//        dialoguePanel.SetActive(false);

//        GameFlowManager.Instance.combatManager.StartCombat(
//            currentGuardian,
//            GameFlowManager.Instance.player
//        );
//    }
//}