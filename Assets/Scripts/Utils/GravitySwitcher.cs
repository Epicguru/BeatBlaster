
using UnityEngine;

public class GravitySwitcher : MonoBehaviour
{
    public Vector3[] Gravities = new Vector3[1] { Vector3.down * 17f };
    public float LerpTime = 1f;
    public AnimationCurve LerpCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    public float ControllerMultiplier = 1.7f;
    public CustomCharacterController Controller;

    private int index = 0;
    private float timer = 0f;
    private Vector3 final;

    private void Awake()
    {
        timer = LerpTime + 1f;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            index++;
            if (index >= Gravities.Length)
                index = 0;
            timer = 0f;
        }

        timer += Time.deltaTime;
        float p = Mathf.Clamp01(timer / LerpTime);
        float x = LerpCurve.Evaluate(p);

        Vector3 start = Gravities[index - 1 < 0 ? Gravities.Length - 1 : index - 1];
        Vector3 end = Gravities[index];

        final = Vector3.Lerp(start, end, x);

        if(Physics.gravity != final)
            Physics.gravity = final;

        if(Controller != null)
            Controller.Gravity = final * ControllerMultiplier;
    }
}
