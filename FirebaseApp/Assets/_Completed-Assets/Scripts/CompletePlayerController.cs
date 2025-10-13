using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem; // 👈 Importante para el nuevo sistema de Input

[RequireComponent(typeof(Rigidbody2D))]
public class CompletePlayerController_NewInput : MonoBehaviour
{
    [Header("Movimiento")]
    public float speed = 5f;

    public Label countText;
    public Label winText;

    public GameObject root;
    private UIDocument uiDocument;

    private Rigidbody2D rb2d;
    private int count;
    private Vector2 movementInput; // 👈 almacena el input del nuevo sistema

    // 🔹 Método que se llama automáticamente por el nuevo Input System
    // cuando el jugador mueve el stick o presiona teclas asignadas.
    public void OnMove(InputAction.CallbackContext context)
    {
        // Leemos el vector del input (por ejemplo, WASD o joystick)
        movementInput = context.ReadValue<Vector2>();
    }

    void Start()
    {
       
        uiDocument = root.GetComponent<UIDocument>();
        winText = uiDocument.rootVisualElement.Q<Label>("WinText");
        countText = uiDocument.rootVisualElement.Q<Label>("CounterText");
        rb2d = GetComponent<Rigidbody2D>();
        count = 0;
        winText.text = "";
        SetCountText();
    }

    void FixedUpdate()
    {
        // Aplicamos el movimiento basado en el input actual
        rb2d.AddForce(movementInput * speed);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PickUp"))
        {
            other.gameObject.SetActive(false);
            count++;
            SetCountText();
        }
    }

    void SetCountText()
    {
        countText.text = "Count: " + count;
        if (count >= 12)
            winText.text = "You win!";
    }
}
