using UnityEngine;
using System.Collections;

public class Drawer : Interactable
{
    [Header("Slide Settings")]
    public Vector3 slideOffset = new Vector3(0, 0, 0.5f); // Change this if it slides the wrong way
    public float slideSpeed = 1.5f;

    private Vector3 closedPos;
    private Vector3 openPos;
    private bool isOpen = false;
    private Coroutine slideCoroutine;

    void Start()
    {
        closedPos = transform.localPosition;
        openPos = closedPos + slideOffset;
    }

    public override void Interact()
    {
        base.Interact();
        isOpen = !isOpen;

        if (slideCoroutine != null) StopCoroutine(slideCoroutine);
        slideCoroutine = StartCoroutine(SlideDrawer(isOpen ? openPos : closedPos));
    }

    IEnumerator SlideDrawer(Vector3 target)
    {
        while (Vector3.Distance(transform.localPosition, target) > 0.001f)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, target, slideSpeed * Time.deltaTime);
            yield return null;
        }
        transform.localPosition = target;
    }
}