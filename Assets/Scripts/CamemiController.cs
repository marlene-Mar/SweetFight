using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using System.Collections;

public class CamemiController : MonoBehaviour
{
    [Header("Configuración")]
    public Transform jugador;
    public float patrolRadius = 2.0f;
    public float waitTimeBetweenPoints = 2.0f;
    public float radioDeteccion = 3.0f;
    public float vidaActual = 100f;

    private NavMeshAgent agent;
    private Animator anim;
    private float waitTimer;
    private bool enCombate = false;
    private bool secuenciaIniciada = false;
    private int contadorGolpesInterno = 0;
    private bool esBloqueando = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        // Estado inicial: IDLE -> CAMINAR (vía Patrol)
    }

    void Update()
    {
        if (vidaActual <= 0) return;

        float distancia = Vector3.Distance(transform.position, jugador.position);

        if (!enCombate && !secuenciaIniciada)
        {
            PatrolBehaviour();

            // 3. CAMINAR -> STOP WALKING (Condición: PLAYER = true)
            if (distancia <= radioDeteccion)
            {
                StartCoroutine(SecuenciaNarrativa());
            }
        }
        else if (enCombate)
        {
            PerseguirYAtacar();
        }
    }

    // --- MOVIMIENTO Y PATRULLA ---
    public void PatrolBehaviour()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            anim.SetBool("Walk", false); // IDLE
            waitTimer += Time.deltaTime;

            if (waitTimer >= waitTimeBetweenPoints)
            {
                MoveToRandomPoint();
                waitTimer = 0f;
            }
        }
        else
        {
            anim.SetBool("Walk", true); // CAMINAR
        }
    }

    public void MoveToRandomPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius + transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    // --- FLUJO DE DIÁLOGO Y COMBATE ---
    IEnumerator SecuenciaNarrativa()
    {
        secuenciaIniciada = true;
        agent.isStopped = true;

        // CAMINAR -> STOP WALKING
        anim.SetBool("PLAYER", true);
        yield return new WaitForSeconds(1f);

        // STOP WALKING -> IDLE
        anim.SetBool("INDIALOGUE", true);
        Debug.Log("Villano: ¡Hablando...!");
        yield return new WaitForSeconds(3f);

        // IDLE -> POSICIÓN PELEA
        anim.SetBool("Combat", true);
        enCombate = true;
        agent.isStopped = false;

        // Iniciar rutinas de combate
        StartCoroutine(RutinaBloqueo());
    }

    void PerseguirYAtacar()
    {
        agent.SetDestination(jugador.position);
        transform.LookAt(new Vector3(jugador.position.x, transform.position.y, jugador.position.z));

        // Lógica de ataque por tiempo (ejemplo cada 2 segundos intenta atacar)
        waitTimer += Time.deltaTime;
        if (waitTimer >= 2f && Vector3.Distance(transform.position, jugador.position) < 2f)
        {
            EjecutarAtaque();
            waitTimer = 0f;
        }
    }

    void EjecutarAtaque()
    {
        contadorGolpesInterno++;
        anim.SetInteger("ContAttack1", contadorGolpesInterno);

        // POSICIÓN PELEA -> GOLPE1
        anim.SetTrigger("Attack1");

        // Si ya dio 2 golpes, el Animator pasará a GOLPE2 por la condición ContAttack1 > 2
        if (contadorGolpesInterno >= 3)
        {
            contadorGolpesInterno = 0;
            anim.SetInteger("ContAttack1", 0);
        }
    }

    IEnumerator RutinaBloqueo()
    {
        while (vidaActual > 0)
        {
            yield return new WaitForSeconds(10f);
            esBloqueando = true;
            anim.SetTrigger("Block"); // ANYSTATE -> BLOQUEARGOLPE
            yield return new WaitForSeconds(1.5f); // Tiempo que dura el bloqueo
            esBloqueando = false;
        }
    }

    // --- RECIBIR DAÑO Y MUERTE ---
    public void RecibirDaño(float daño)
    {
        if (esBloqueando || vidaActual <= 0) return;

        vidaActual -= daño;
        anim.SetTrigger("RecibirGolpe"); // GOLPE1/2 -> RECIBIRGOLPE
        Debug.Log("Vida Villano: " + vidaActual);

        if (vidaActual <= 0) Morir();
    }

    void Morir()
    {
        StopAllCoroutines();
        enCombate = false;
        agent.isStopped = true;
        anim.SetBool("Die", true); // ANYSTATE -> MORIR
        Debug.Log("Villano derrotado. Fin del juego.");
        Invoke("RegresarAlMenu", 5f);
    }

    void RegresarAlMenu()
    {
        SceneManager.LoadScene("MenuInicio");
    }
}