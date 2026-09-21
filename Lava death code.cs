using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Lava : MonoBehaviour
{
    [Header("Teleport Coordinates")]
    public float teleportX;
    public float teleportY;
    public float teleportZ;

    [Header("Message Settings")]
    public float messageDuration = 3f;

    private GameObject messageObject;
    private Text messageText;

    private void Start()
    {
        CreateDeathMessage();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Teleport player
            other.transform.position = new Vector3(
                teleportX,
                teleportY,
                teleportZ
            );

            // Show message
            ShowDeathMessage();
        }
    }

    private void CreateDeathMessage()
    {
        // Find or create a Canvas
        Canvas canvas = FindAnyObjectByType<Canvas>();

        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("LavaMessageCanvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        // Create text object
        messageObject = new GameObject("LavaDeathMessage");
        messageObject.transform.SetParent(canvas.transform, false);

        messageText = messageObject.AddComponent<Text>();

        messageText.text =
            "You Died because of Lava. You were sent to the nearest checkpoint";

        messageText.color = Color.red;
        messageText.fontSize = 24;
        messageText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        messageText.alignment = TextAnchor.UpperLeft;

        // Position in top-left
        RectTransform rect = messageObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);

        rect.anchoredPosition = new Vector2(20, -20);
        rect.sizeDelta = new Vector2(800, 60);

        // Hide it initially
        messageObject.SetActive(false);
    }

    private void ShowDeathMessage()
    {
        if (messageObject != null)
        {
            StopAllCoroutines();
            messageObject.SetActive(true);
            StartCoroutine(HideDeathMessage());
        }
    }

    private IEnumerator HideDeathMessage()
    {
        yield return new WaitForSeconds(messageDuration);

        if (messageObject != null)
        {
            messageObject.SetActive(false);
        }
    }
}
