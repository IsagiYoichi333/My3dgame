using System.Collections.Generic;
using UnityEngine;

public class Tube : MonoBehaviour
{
    public enum Color { Red, Blue, Green, Yellow, Purple, Orange, None }

    public int capacity = 4; // Number of segments the tube can hold
    public List<Color> segments = new List<Color>(); // From bottom to top

    public AudioClip pourSound; // Assign in inspector
    private AudioSource audioSource;

    // Initialize tube with optional starting segments
    public void Initialize(List<Color> startingSegments)
    {
        segments = new List<Color>(startingSegments);
    }

    // Check if tube is full
    public bool IsFull()
    {
        return segments.Count >= capacity;
    }

    // Check if tube is empty
    public bool IsEmpty()
    {
        return segments.Count == 0;
    }

    // Get the top color (if any)
    public Color GetTopColor()
    {
        if (IsEmpty())
            return Color.None;
        return segments[segments.Count - 1];
    }

    // Get the number of consecutive top color segments
    public int GetTopColorCount()
    {
        if (IsEmpty())
            return 0;
        Color topColor = GetTopColor();
        int count = 0;
        for (int i = segments.Count - 1; i >= 0; i--)
        {
            if (segments[i] == topColor)
                count++;
            else
                break;
        }
        return count;
    }

    // Pour from this tube to another tube
    // Returns number of segments poured, or 0 if cannot pour
    public int PourTo(Tube otherTube)
    {
        if (IsEmpty() || otherTube.IsFull())
            return 0;

        Color topColor = GetTopColor();
        int topColorCount = GetTopColorCount();

        // Check if other tube is empty or has same top color
        if (!otherTube.IsEmpty() && otherTube.GetTopColor() != topColor)
            return 0;

        // Calculate how many segments we can pour (limited by other tube's free space)
        int spaceInOther = otherTube.capacity - otherTube.segments.Count;
        int toPour = Mathf.Min(topColorCount, spaceInOther);

        if (toPour <= 0)
            return 0;

        // Remove top segments from this tube
        segments.RemoveRange(segments.Count - toPour, toPour);

        // Add them to other tube
        for (int i = 0; i < toPour; i++)
        {
            otherTube.segments.Add(topColor);
        }

        // Play pour sound if we have one and audioSource is ready
        if (pourSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(pourSound);
        }

        return toPour;
    }

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        // Optional: set audioSource properties
        audioSource.playOnAwake = false;
    }
}