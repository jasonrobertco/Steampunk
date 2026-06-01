using UnityEngine;

/// <summary>
/// Plays a one-shot sprite animation on a renderer, then destroys itself.
/// </summary>
public class OneShotSpriteEffect : MonoBehaviour
{
    private SpriteRenderer targetRenderer;
    private Sprite[] frames;
    private float frameRate;
    private float timer;
    private int frameIndex;

    public void Initialize(SpriteRenderer rendererTarget, Sprite[] animationFrames, float animationFrameRate)
    {
        targetRenderer = rendererTarget;
        frames = animationFrames;
        frameRate = Mathf.Max(1f, animationFrameRate);
        timer = 0f;
        frameIndex = 0;

        if (targetRenderer != null && frames != null && frames.Length > 0)
        {
            targetRenderer.sprite = frames[0];
        }
    }

    private void Update()
    {
        if (targetRenderer == null || frames == null || frames.Length == 0)
        {
            Destroy(gameObject);
            return;
        }

        timer += Time.deltaTime;
        int nextFrame = Mathf.FloorToInt(timer * frameRate);

        if (nextFrame == frameIndex)
        {
            return;
        }

        frameIndex = nextFrame;

        if (frameIndex >= frames.Length)
        {
            Destroy(gameObject);
            return;
        }

        targetRenderer.sprite = frames[frameIndex];
    }
}
