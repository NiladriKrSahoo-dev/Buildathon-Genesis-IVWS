using UnityEngine;

public class Interactable : MonoBehaviour
{
    public string promptText = "Press E to interact";
    protected bool hasInteracted = false;

    public virtual void Interact()
    {
        hasInteracted = true;
        Debug.Log(gameObject.name + " was interacted with.");
    }
}