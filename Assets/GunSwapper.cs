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
            if(i == CurrentIndex)
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
                Disable(Guns[CurrentIndex]);
                CurrentIndex = i;
                SetActive(Guns[CurrentIndex]);
            }
        }
    }

    private void SetActive(GunController c)
    {
        //c.Anim.Anim.enabled = true;
        c.Anim.Anim.gameObject.SetActive(true);
        Character.Gun = c;
    }

    private void Disable(GunController c)
    {
        //c.Anim.Anim.enabled = false;
        c.Anim.Anim.gameObject.SetActive(false);
    }
}
