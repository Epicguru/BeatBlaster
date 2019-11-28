using System.Collections;
using UnityEngine;

public class GunSwapper : MonoBehaviour
{
    public PlayerController Character;

    public GunController[] Guns;
    public KeyCode[] Keys;

    public int CurrentIndex;

    private void Awake()
    {
        Debug.Assert(Keys.Length == Keys.Length);
        for (int i = 0; i < Guns.Length; i++)
        {
            if (i == CurrentIndex)
            {
                SetActive(Guns[i]);
            }
            else
            {
                Disable(Guns[i]);
            }
        }
    }

    private void Update()
    {
        for (int i = 0; i < Keys.Length; i++)
        {
            if (Input.GetKeyDown(Keys[i]) && CurrentIndex != i)
            {
                StartCoroutine(Disable(Guns[CurrentIndex]));
                CurrentIndex = i;
                SetActive(Guns[CurrentIndex]);
            }
        }
    }

    private void SetActive(GunController c)
    {
        c.Anim.Anim.gameObject.SetActive(true);
        c.transform.position = Vector3.zero;
        c.Anim.Equip();
        Character.Gun = c;
    }

    private IEnumerator Disable(GunController c)
    {
        //c.Anim.Anim.enabled = false;
        c.Anim.Anim.Play("Idle", 0);
        for (int i = 0; i < c.Anim.Anim.layerCount; i++)
        {
            c.Anim.Anim.SetLayerWeight(i, 0);
        }

        // Out of sight, out of mind. There is a single frame where the gun appears in the idle pose, which is correct, but also looks lame.
        // So I teleport it far away so that it can render the pose without it flickering over the screen.
        c.transform.position = Vector3.one * 1000f;

        // Wait for end of frame for that to have been applied.
        yield return new WaitForEndOfFrame();

        c.Anim.Anim.gameObject.SetActive(false);

        yield return null;
    }
}
