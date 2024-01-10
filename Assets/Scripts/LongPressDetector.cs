using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class LongPressDetector : MonoBehaviour
{
    public float longPressDuration = 4f; // Duration for long press
    public TMP_Text cheatMenuText; // Assign the TextMeshPro element in the inspector
    private float pressStartTime;
    private bool isPressStarted;

    void Update()
    {
        // Handle touch input for mobile
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0); // Get the first touch
            
            if (touch.phase == TouchPhase.Began)
            {
                StartPress();
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                EndPress();
            }
        }
        // Handle mouse input for PC
        else if (Input.GetMouseButtonDown(0)) // On mouse button press start
        {
            StartPress();
        }
        else if (Input.GetMouseButtonUp(0)) // On mouse button release
        {
            EndPress();
        }

        // Check if the press is ongoing and the duration is exceeded
        if (isPressStarted && Time.time - pressStartTime >= longPressDuration)
        {
            isPressStarted = false; // Prevent multiple triggers
            ActivateCheatMenu();
        }
    }

    private void StartPress()
    {
        pressStartTime = Time.time;
        isPressStarted = true;
    }

    private void EndPress()
    {
        isPressStarted = false;
    }

    void ActivateCheatMenu()
    {
        cheatMenuText.gameObject.SetActive(true); // Enable the TextMeshPro text
        // You can add more cheat activation code here
    }
}