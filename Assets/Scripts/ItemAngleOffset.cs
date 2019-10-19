
using UnityEngine;

public class ItemAngleOffset : MonoBehaviour
{
    public Vector2 Magnitude = new Vector2(0.1f, 0.1f);
    public Vector2 Frequency = Vector2.one * 0.5f;
    private float timer;

    private void LateUpdate()
    {
        float x = Mathf.Cos(Time.time * Mathf.PI * 2 * Frequency.x) * Magnitude.x;
        float y = Mathf.Sin(Time.time * Mathf.PI * 2 * Frequency.y) * Magnitude.y;
        transform.localEulerAngles = new Vector3(x, y, 0f);
    }
}
