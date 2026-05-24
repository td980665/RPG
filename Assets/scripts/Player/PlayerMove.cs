using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float speed = 5f;
    private CharacterController controller;

    public bool canMove = true;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (DialogueManager.Instance.isDialogueActive)
            return;

        // 移動
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);

        // インタラクト
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }

    void TryInteract()
    {
        Debug.Log("TryInteract");

        if (DialogueManager.Instance != null &&
        DialogueManager.Instance.isDialogueActive)
        {
            return;
        }

        // プレイヤー胸あたり
        Vector3 origin = transform.position + Vector3.up * 1.2f;
        Vector3 direction = transform.forward;

        float interactDistance = 1f; // ←ここを調整

        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, interactDistance))
        {
            Debug.Log(hit.collider);

            IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact();
            }
        }
    }
}