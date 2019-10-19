
using UnityEngine;

public class ItemAngleOffset : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    public Animator Anim;

    public float Scale = 1f;
    [Range(0f, 1f)]
    public float AnimScale = 1f;
    public Vector2 Magnitude = new Vector2(0.1f, 0.1f);
    public Vector2 Frequency = Vector2.one * 0.5f;

    [Header("Punch")]
    [Range(0f, 1f)]
    public float PunchDecay = 0.95f;
    [Range(0f, 1f)]
    public float PunchVelDecay = 0.85f;

    private Vector3 punch;
    private Vector3 punchVel;
    private float timer;

    private void Awake()
    {
        InvokeRepeating("UpdatePunch", 0f, 1f / 60f);
    }

    private void LateUpdate()
    {
        Anim.SetLayerWeight(1, 1f - Mathf.Clamp01(AnimScale));
        float x = Mathf.Cos(Time.time * Mathf.PI * 2 * Frequency.x) * Magnitude.x;
        float y = Mathf.Sin(Time.time * Mathf.PI * 2 * Frequency.y) * Magnitude.y;
        transform.localEulerAngles = new Vector3(x * Scale, y * Scale, 0f) + punch;
    }

    public void AddPunch(Vector3 angles)
    {
        punchVel += angles;
    }

    private void UpdatePunch()
    {
        punch += punchVel * (1f / 60f);
        punch *= PunchDecay;
        punchVel *= PunchVelDecay;
    }
}
