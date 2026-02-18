using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

// ═══════════════════════════════════════════════════════════════════
//  DialogueManager  —  versión optimizada
//
//  MEJORAS APLICADAS:
//  - Previene iniciar diálogo si ya hay uno activo
//  - Limpieza de estado al finalizar diálogo
//  - Mejor manejo de coroutines
//  - Sincronización correcta con GuardianController
// ═══════════════════════════════════════════════════════════════════

public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public GameObject responseButtonsPanel;
    public Button responseButton;

    [Header("Cameras")]
    public GameObject freeLookCamera;
    public GameObject dialogueCamera;

    [Header("Configuración")]
    public float textSpeed = 0.05f;

    [Tooltip("Permite saltar el texto escribiéndose rápidamente con clic")]
    public bool allowSkipTyping = true;

    // ── Estado interno ──────────────────────────────────────────────
    private GuardianController currentGuardian;
    private PompompurinController playerController;
    private int currentDialogueIndex = 0;
    private bool isTyping = false;
    private bool isDialogueActive = false;

    // ── Estructura de diálogo ───────────────────────────────────────
    [System.Serializable]
    public class DialogueLine
    {
        public string speaker;
        [TextArea(2, 4)]
        public string text;
        public bool requiresPlayerResponse;
        public string responseButtonText;
    }

    // ── Conversación con el guardián ────────────────────────────────
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

    // ═════════════════════════════════════════════════════════════════
    //  CICLO DE VIDA
    // ═════════════════════════════════════════════════════════════════
    void Start()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (responseButtonsPanel != null)
            responseButtonsPanel.SetActive(false);
    }

    // ═════════════════════════════════════════════════════════════════
    //  INICIO DEL DIÁLOGO
    // ═════════════════════════════════════════════════════════════════
    public void StartGuardianDialogue(GuardianController guardian)
    {
        // Prevenir iniciar otro diálogo si ya hay uno activo
        if (isDialogueActive)
        {
            Debug.LogWarning("DialogueManager: Ya hay un diálogo activo, se ignora el nuevo.");
            return;
        }

        currentGuardian = guardian;
        currentDialogueIndex = 0;
        isDialogueActive = true;

        // Cambiar cámaras
        if (freeLookCamera != null) freeLookCamera.SetActive(false);
        if (dialogueCamera != null) dialogueCamera.SetActive(true);

        // Activar panel de diálogo
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        // Deshabilitar controles del jugador
        DisablePlayerControls();

        // Mostrar primera línea
        DisplayNextLine();

        Debug.Log("DialogueManager: Diálogo iniciado con " + guardian.name);
    }

    // ═════════════════════════════════════════════════════════════════
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

    // ═════════════════════════════════════════════════════════════════
    //  FLUJO DEL DIÁLOGO
    // ═════════════════════════════════════════════════════════════════
    void DisplayNextLine()
    {
        // Verificar si terminamos todas las líneas
        if (currentDialogueIndex >= guardianConversation.Count)
        {
            EndDialogue();
            return;
        }

        DialogueLine currentLine = guardianConversation[currentDialogueIndex];

        // Detener cualquier texto que se esté escribiendo
        StopAllCoroutines();

        // Iniciar animación de escritura
        StartCoroutine(TypeText(currentLine.text));

        // Configurar botón de respuesta si es necesario
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

            // Limpiar listeners previos y agregar nuevo
            responseButton.onClick.RemoveAllListeners();
            responseButton.onClick.AddListener(OnResponseButtonClicked);
        }
    }

    // ═════════════════════════════════════════════════════════════════
    //  EVENTOS DE BOTONES
    // ═════════════════════════════════════════════════════════════════
    public void OnResponseButtonClicked()
    {
        // Si el texto aún se está escribiendo, completarlo instantáneamente
        if (isTyping && allowSkipTyping)
        {
            StopAllCoroutines();
            dialogueText.text = guardianConversation[currentDialogueIndex].text;
            isTyping = false;
            return;
        }

        // Avanzar a la siguiente línea
        currentDialogueIndex++;

        if (currentDialogueIndex >= guardianConversation.Count)
        {
            EndDialogue();
        }
        else
        {
            DisplayNextLine();
        }
    }

    // ═════════════════════════════════════════════════════════════════
    //  FIN DEL DIÁLOGO
    // ═════════════════════════════════════════════════════════════════
    void EndDialogue()
    {
        Debug.Log("DialogueManager: Finalizando diálogo.");

        // Cambiar cámaras de vuelta
        if (freeLookCamera != null) freeLookCamera.SetActive(true);
        if (dialogueCamera != null) dialogueCamera.SetActive(false);

        // Cerrar UI
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (responseButtonsPanel != null)
            responseButtonsPanel.SetActive(false);

        // Limpiar texto
        if (dialogueText != null)
            dialogueText.text = "";

        // Detener animaciones de texto
        StopAllCoroutines();
        isTyping = false;

        // Devolver controles al jugador
        EnablePlayerControls();

        // Avisar al guardián que el diálogo terminó
        // IMPORTANTE: Esto debe llamarse DESPUÉS de restaurar controles
        if (currentGuardian != null)
        {
            currentGuardian.EndDialogue();
            currentGuardian = null;
        }

        // Restablecer estado
        isDialogueActive = false;
        currentDialogueIndex = 0;

        Debug.Log("DialogueManager: Diálogo completado, iniciando combate.");
    }

    // ═════════════════════════════════════════════════════════════════
    //  MÉTODOS PÚBLICOS DE UTILIDAD
    // ═════════════════════════════════════════════════════════════════

    /// <summary>Verifica si actualmente hay un diálogo activo.</summary>
    public bool IsDialogueActive() => isDialogueActive;

    /// <summary>Fuerza el cierre del diálogo (útil para casos de emergencia).</summary>
    public void ForceEndDialogue()
    {
        if (isDialogueActive)
        {
            Debug.LogWarning("DialogueManager: Forzando cierre de diálogo.");
            EndDialogue();
        }
    }

    /// <summary>Cambia la velocidad de escritura del texto.</summary>
    public void SetTextSpeed(float newSpeed)
    {
        textSpeed = Mathf.Clamp(newSpeed, 0.01f, 1f);
    }
}