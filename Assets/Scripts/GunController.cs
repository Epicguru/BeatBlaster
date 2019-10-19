
using UnityEngine;

[RequireComponent(typeof(GunAnimator))]
public class GunController : MonoBehaviour
{
    public GunAnimator Anim
    {
        get
        {
            if (_gunAnim == null)
                _gunAnim = GetComponent<GunAnimator>();
            return _gunAnim;
        }
    }
    private GunAnimator _gunAnim;

    public Transform Muzzle;

    public Vector3 Offset = Vector3.zero;
    public FireMode FireMode = FireMode.Auto;
    public int MagazineCapacity = 17;
    public int CurrentBullets = 17;
    public bool BulletInChamber = true;
    public float RPM = 600f;
    public Transform ShellSpawn;
    public Vector3 MinShellVel, MaxShellVel;
    public FallingShell ShellPrefab;

    private float shootTimer = 0f;

    private void Update()
    {
        transform.localPosition = Offset;

        if (Input.GetKeyDown(KeyCode.R))
            Anim.Reload = true;
        if (Input.GetKeyDown(KeyCode.F))
            Anim.CheckMagazine = true;
        switch (FireMode)
        {
            case FireMode.Single:
                if ((Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Mouse0)) && BulletInChamber)
                    Anim.Shoot = true;
                break;
            case FireMode.Auto:
                shootTimer += Time.deltaTime;
                if(shootTimer >= 1f / (RPM / 60f))
                {
                    shootTimer = 0f;
                    if ((Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Mouse0)) && BulletInChamber)
                        Anim.Shoot = true;
                }                
                break;
        }

        Anim.ADS = Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.Mouse1);
        Anim.Run = Input.GetKey(KeyCode.LeftShift);
        Anim.Crouch = Input.GetKey(KeyCode.LeftControl);

        if (!BulletInChamber && CurrentBullets > 0 && !Anim.IsChambering)
            Anim.Chamber = true;

        Anim.IsEmpty = !BulletInChamber;
        Anim.Slide.LockOpen = !BulletInChamber;
    }

    private void UponAnimationEvent(AnimationEvent e)
    {
        string str = e.stringParameter.Trim().ToLower();

        switch (str)
        {
            case "shoot":
                if (!BulletInChamber)
                {
                    Debug.LogError("Animation-CodeState desync.");
                    break;
                }

                Debug.Log("Pew");
                if(Muzzle != null && Physics.Raycast(new Ray(Muzzle.position, Muzzle.forward), out RaycastHit hit, 100f))
                {
                    Debug.DrawLine(Muzzle.position, hit.point, Color.red, 5f);
                }

                if(ShellPrefab != null && ShellSpawn != null)
                {
                    var spawned = PoolObject.Spawn(ShellPrefab);
                    spawned.transform.position = ShellSpawn.position;
                    spawned.transform.rotation = ShellSpawn.rotation;
                    spawned.Vel = ShellSpawn.TransformVector(new Vector3(Mathf.Lerp(MinShellVel.x, MaxShellVel.x, Random.value), Mathf.Lerp(MinShellVel.y, MaxShellVel.y, Random.value), Mathf.Lerp(MinShellVel.z, MaxShellVel.z, Random.value)));
                }

                BulletInChamber = false;
                if (CurrentBullets > 0)
                {
                    CurrentBullets--;
                    BulletInChamber = true;
                }
                break;

            case "chamber":
                BulletInChamber = true;
                if (CurrentBullets > 0)
                    CurrentBullets--;
                break;

            case "reload":
                CurrentBullets = MagazineCapacity;
                break;
        }
    }
}

public enum FireMode
{
    Single,
    Auto
}