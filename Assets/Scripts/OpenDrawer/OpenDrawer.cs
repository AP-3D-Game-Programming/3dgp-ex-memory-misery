using UnityEngine;

public class OpenDrawer : MonoBehaviour
{
    public Animator ANI;

    public GameObject openText;
    public GameObject closedText;

    private bool open;
    private bool inReach;

    public static OpenDrawer current;

    void Start()
    {
        openText.SetActive(false);
        closedText.SetActive(false);

        ANI.SetBool("open", false);
        ANI.SetBool("close", false);

        open = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Reach")) return;

        current = this;
        inReach = true;

        if (!open)
            openText.SetActive(true);
        else
            closedText.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Reach")) return;

        if (current == this)
            current = null;

        inReach = false;
        openText.SetActive(false);
        closedText.SetActive(false);
    }

    public void TryInteract()
    {
        if (!inReach) return;

        if (!open)
        {
            ANI.SetBool("open", true);
            ANI.SetBool("close", false);
            open = true;
        }
        else
        {
            ANI.SetBool("open", false);
            ANI.SetBool("close", true);
            open = false;
        }

        openText.SetActive(false);
        closedText.SetActive(false);
    }
}
