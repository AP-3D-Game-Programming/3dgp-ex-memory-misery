using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class CarPathSystem : MonoBehaviour
{
    [Header("Overgang & Sfeer")]
    public Image whiteFadePanel;
    public float fadeDuration = 3f;
    public float dialogueDelay = 2f;

    [Header("Route & Sturen")]
    public Transform[] waypoints;
    public Transform steeringWheel;
    public float driveSpeed = 10f;
    public float turnSpeed = 2f;
    public float wheelSensitivity = 3f;

    [Header("Verhaal")]
    public AudioSource voiceOver;
    public string nextScene = "2_Intro_Forest";

    private int index = 0;
    private bool parked = false;
    private bool dialogueStarted = false;

    void Start()
    {
        // 1. Fade In (Wit naar helder)
        if (whiteFadePanel != null)
        {
            whiteFadePanel.color = Color.white;
            whiteFadePanel.canvasRenderer.SetAlpha(1.0f);
            whiteFadePanel.CrossFadeAlpha(0, fadeDuration, false);
        }

        // 2. Plan de dialoog in
        if (voiceOver != null)
        {
            Invoke("StartDialogueWrapper", dialogueDelay);
        }
    }

    // Dit is een kleine hulpfuctie omdat Invoke geen Coroutines kan starten
    void StartDialogueWrapper()
    {
        if (!parked && !dialogueStarted)
        {
            StartCoroutine(PlayDialogueSequence());
        }
    }

    // --- HIER ZIT DE MAGIE VOOR DE ONDERTITELING ---
    IEnumerator PlayDialogueSequence()
    {
        dialogueStarted = true;

        // 1. Start het geluid (Het hele mp3 bestand begint nu te spelen)
        voiceOver.Play();

        // --- STUKJE 1 ---
        // "Just breathe. Okay? Just... breathe."
        // Tekst blijft 4 sec staan. We wachten 4s + 0.5s pauze.
        SubtitleManager.Instance.ShowSubtitle("Just breathe. Okay? Just... breathe.", 4f);
        yield return new WaitForSeconds(4.5f);

        // "Keep your eyes on the road. Don't look at the trees. Just drive."
        // Tekst 4s. Wachten: 4s + 2.5s (grote pauze uit je tekst)
        SubtitleManager.Instance.ShowSubtitle("Keep your eyes on the road. Don't look at the trees. Just drive.", 3f);
        yield return new WaitForSeconds(5f);


        // --- STUKJE 2 ---
        // "Ten years... Ten years I stayed away."
        SubtitleManager.Instance.ShowSubtitle("Ten years... Ten years I stayed away.", 3.5f);
        yield return new WaitForSeconds(3.0f); // + 1.0s pauze

        // "The doctors said I was CRAZY to obsess over it."
        SubtitleManager.Instance.ShowSubtitle("The doctors said I was CRAZY to obsess over it.", 3.5f);
        yield return new WaitForSeconds(3.0f);

        // "'Move on, ALEX...' 'Take the PILLS, ALEX...' 'You're cured, ALEX...'"
        SubtitleManager.Instance.ShowSubtitle("'Move on, ALEX...' 'Take the PILLS, ALEX...' 'You're cured, ALEX...'", 5f);
        yield return new WaitForSeconds(5f); // + 1.2s pauze


        // --- STUKJE 3 ---
        // "BULLSHIT!!"
        SubtitleManager.Instance.ShowSubtitle("BULLSHIT", 2f);
        yield return new WaitForSeconds(2f); // + 2.0s pauze

        // --- STUKJE 4 ---
        // "If I'm cured... why do I still smell the BLEACH?"
        SubtitleManager.Instance.ShowSubtitle("If I'm cured... why do I still smell the BLEACH?", 3.5f);
        yield return new WaitForSeconds(3.5f); // + 0.5s pauze

        // "Why can I still hear the lock turning in the asylum? EVERY. SINGLE. NIGHT."
        SubtitleManager.Instance.ShowSubtitle("Why can I still hear the lock turning in the asylum? EVERY. SINGLE. NIGHT.", 5f);
        yield return new WaitForSeconds(5.5f); // + 3.0s pauze


        // --- STUKJE 5 ---
        // "They LIED. I know they lied. I wasn't SICK when I went in there..."
        SubtitleManager.Instance.ShowSubtitle("They LIED. I know they lied. I wasn't SICK when I went in there...", 4f);
        yield return new WaitForSeconds(3.5f); // + 0.8s pauze

        // "...but I was broken when I came out."
        SubtitleManager.Instance.ShowSubtitle("...but I was broken when I came out.", 3.5f);
        yield return new WaitForSeconds(4.5f); // + 2.5s pauze


        // --- STUKJE 6 ---
        // "Okay."
        SubtitleManager.Instance.ShowSubtitle("Okay.", 1.5f);
        yield return new WaitForSeconds(2.5f); // + 2.0s pauze


        // --- STUKJE 7 ---
        // "No turning back now. Just park the car. Get the files."
        SubtitleManager.Instance.ShowSubtitle("No turning back now. Just park the car. Get the files.", 10f);
        yield return new WaitForSeconds(10f);

        // "And get the hell out of there... before you lose your mind again."
        SubtitleManager.Instance.ShowSubtitle("And get the hell out of there... before you lose your mind again.", 10f);

        // Hierna stopt de tekst vanzelf omdat de ShowSubtitle tijd (4f) voorbij is.
    }

    void Update()
    {
        if (parked) return;

        if (index < waypoints.Length)
        {
            DriveAndSteer();
        }
        else
        {
            StartCoroutine(ParkAndEndScene());
        }
    }

    void DriveAndSteer()
    {
        Transform target = waypoints[index];
        transform.position = Vector3.MoveTowards(transform.position, target.position, driveSpeed * Time.deltaTime);

        Vector3 direction = (target.position - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, turnSpeed * Time.deltaTime);

            if (steeringWheel != null)
            {
                float targetSteerAngle = Vector3.SignedAngle(transform.forward, direction, Vector3.up);
                Quaternion wheelRot = Quaternion.Euler(0, 0, -targetSteerAngle * wheelSensitivity);
                steeringWheel.localRotation = Quaternion.Slerp(steeringWheel.localRotation, wheelRot, Time.deltaTime * 5f);
            }
        }

        if (Vector3.Distance(transform.position, target.position) < 0.5f)
        {
            index++;
        }
    }

    IEnumerator ParkAndEndScene()
    {
        parked = true;

        // Stop de timer uit Start() als die nog loopt!
        CancelInvoke("StartDialogueWrapper");

        if (steeringWheel) steeringWheel.localRotation = Quaternion.identity;

        float waitTime = 1f;

        if (voiceOver != null)
        {
            if (voiceOver.isPlaying)
            {
                // Situatie 1: Hij is bezig -> wacht tot hij klaar is
                waitTime = voiceOver.clip.length - voiceOver.time;
            }
            else if (!dialogueStarted)
            {
                // Situatie 2: We zijn er al, maar de dialoog was nog niet begonnen!
                // Start hem nu alsnog direct
                StartCoroutine(PlayDialogueSequence());
                waitTime = voiceOver.clip.length;
            }
        }

        // Wacht tot de audio klaar is
        yield return new WaitForSeconds(waitTime);

        // Fade Out (Naar Wit)
        if (whiteFadePanel != null)
        {
            whiteFadePanel.CrossFadeAlpha(1, 2.0f, false);
            yield return new WaitForSeconds(2.0f);
        }

        SceneManager.LoadScene(nextScene);
    }
}