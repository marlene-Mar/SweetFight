using UnityEngine;

public class NPCDialogue : MonoBehaviour
{
    public DialogueManager dialogueManager;
    public Dialogos dialogo;

    private GuardianController guardian;
    private CamemiController camemi;

    private void Start()
    {
        // Detecta automáticamente qué tipo de NPC es
        guardian = GetComponent<GuardianController>();
        camemi = GetComponent<CamemiController>();
    }

    // Este método lo llamas cuando el jugador interactúa (por botón, trigger, etc.)
    public void StartConversation()
    {
        if (dialogueManager == null || dialogo == null)
        {
            Debug.LogWarning("Falta asignar DialogueManager o Dialogo.");
            return;
        }

        if (guardian != null)
        {
            dialogueManager.StartGuardianDialogue(dialogo, guardian);
        }
        else if (camemi != null)
        {
            dialogueManager.StartCamemiDialogue(dialogo, camemi);
        }
        else
        {
            Debug.LogWarning("Este NPC no tiene GuardianController ni CamemiController.");
        }
    }
}