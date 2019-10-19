
using System.Collections;
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
    public float ADSLerp { get { return Anim.ADSLerp; } }

    [Header("Core")]
    public bool InvertSlideBehaviour = false;
    public bool IsRecursiveReload = false;

    [Header("References")]
    public PlayerController Player;

    [Header("Item")]
    public Vector3 Offset = Vector3.zero;

    [Header("Shooting")]
    public FireMode FireMode = FireMode.Auto;
    public Projectile ProjectilePrefab;
    public Transform Muzzle;
    public float MuzzleVelocity = 400;

    [Header("Stats")]
    public int MagazineCapacity = 17;
    public float RPM = 600f;

    [Header("Shell Spawning")]
    public bool SpawnUponShoot = true;
    public FallingShell ShellPrefab;
    public Transform ShellSpawn;
    public Vector3 MinShellVel = new Vector3(1, 1, 0), MaxShellVel = new Vector3(2, 2, 0);

    [Header("Recoil")]
    public Vector2 ShootPunchPitch = new Vector2(-4f, 4f);
    public Vector2 ShootPunchYaw = new Vector2(5f, 10f);
    public Vector2 ShootPunchRoll = new Vector2(-1f, 1f);
    public Vector3 HipfirePunchMultiplier = new Vector3(2f, 2f, 2f);

    [Header("Status")]
    public bool BulletInChamber = true;
    public int CurrentBullets = 17;

    private float shootTimer = 0f;

    private void Update()
    {
        Anim.IsRecursiveReload = IsRecursiveReload;
        transform.localPosition = Offset;

        if (Input.GetKeyDown(KeyCode.R) && CurrentBullets < MagazineCapacity)
            Anim.Reload = true;
        if (Input.GetKeyDown(KeyCode.F))
            Anim.CheckMagazine = true;
        switch (FireMode)
        {
            case FireMode.Single:
                if ((Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Mouse0)) && (BulletInChamber || (IsRecursiveReload && Anim.IsReloading)))
                {
                    if (IsRecursiveReload && CurrentBullets == 0 && !Anim.IsReloading)
                        BulletInChamber = false;
                    Anim.Shoot = true;
                }
                break;
            case FireMode.Auto:
                shootTimer += Time.deltaTime;
                if(shootTimer >= 1f / (RPM / 60f))
                {
                    shootTimer = 0f;
                    if ((Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Mouse0)) && BulletInChamber)
                    {
                        if (IsRecursiveReload && CurrentBullets == 0)
                            BulletInChamber = false;
                        Anim.Shoot = true;
                    }
                }                
                break;
        }

        Anim.ADS = Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.Mouse1);
        Anim.Run = Input.GetKey(KeyCode.LeftShift);
        Anim.Crouch = Input.GetKey(KeyCode.LeftControl);

        if (!BulletInChamber && CurrentBullets > 0 && !Anim.IsChambering)
            Anim.Chamber = true;

        Anim.IsEmpty = !BulletInChamber;
        Anim.Slide.LockOpen = InvertSlideBehaviour ? BulletInChamber : !BulletInChamber;
    }

    private void UponAnimationEvent(AnimationEvent e)
    {
        string str = e.stringParameter.Trim().ToLower();

        switch (str)
        {
            case "spawnshell":
            case "spawn shell":
                SpawnShell();
                break;

            case "shoot":
                if (!BulletInChamber && !IsRecursiveReload)
                {
                    Debug.LogError("Animation-CodeState desync.");
                    break;
                }

                Vector3 multiplier = Vector3.Lerp(HipfirePunchMultiplier, Vector3.one, ADSLerp);
                Vector3 basePunch = new Vector3(-Random.Range(ShootPunchPitch.x, ShootPunchPitch.y), Random.Range(ShootPunchYaw.x, ShootPunchYaw.y), Random.Range(ShootPunchRoll.x, ShootPunchRoll.y));

                Vector3 finalPunch = new Vector3(multiplier.x * basePunch.x, multiplier.y * basePunch.y, multiplier.z * basePunch.z);

                Player.ItemOffset.AddPunch(finalPunch);

                if (SpawnUponShoot)
                    SpawnShell();

                StartCoroutine(ShootLate());

                BulletInChamber = false;

                if (!IsRecursiveReload && CurrentBullets > 0)
                {
                    CurrentBullets--;
                    BulletInChamber = true;
                }
                break;

            case "chamber":
                if (CurrentBullets > 0)
                {
                    BulletInChamber = true;
                    CurrentBullets--;
                }
                break;

            case "reload":
                if (!IsRecursiveReload)
                {
                    CurrentBullets = MagazineCapacity;
                }
                else
                {
                    if(CurrentBullets < MagazineCapacity)
                        CurrentBullets++;

                    if(CurrentBullets >= MagazineCapacity)
                    {
                        Anim.StopRecursiveReload = true;
                    }
                }
                break;
        }
    }

    public void SpawnShell()
    {
        if (ShellPrefab != null && ShellSpawn != null)
        {
            var spawned = PoolObject.Spawn(ShellPrefab);
            spawned.transform.position = ShellSpawn.position;
            spawned.transform.rotation = ShellSpawn.rotation;
            spawned.Vel = ShellSpawn.TransformVector(new Vector3(Mathf.Lerp(MinShellVel.x, MaxShellVel.x, Random.value), Mathf.Lerp(MinShellVel.y, MaxShellVel.y, Random.value), Mathf.Lerp(MinShellVel.z, MaxShellVel.z, Random.value)));
        }
    }

    private IEnumerator ShootLate()
    {
        yield return new WaitForEndOfFrame();
        if (ProjectilePrefab != null && Muzzle != null)
        {
            var spawned = PoolObject.Spawn(ProjectilePrefab);
            spawned.Velocity = Muzzle.forward * MuzzleVelocity;
            spawned.transform.position = Muzzle.transform.position + Muzzle.forward * 0.1f;

            //spawned.Velocity = transform.forward * MuzzleVelocity;
            //spawned.transform.position = transform.TransformPoint(muzzleOffset);
        }
    }
}

public enum FireMode
{
    Single,
    Auto
}