
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
    public bool IsAutoChamber = true;

    [Header("References")]
    public PlayerController Player;
    public AudioClip[] AudioClips;

    [Header("Item")]
    public Vector3 Offset = Vector3.zero;

    [Header("Animation")]
    public Transform CameraAnim;

    [Header("Shooting")]
    public FireMode FireMode = FireMode.Auto;
    public Projectile ProjectilePrefab;
    public Transform Muzzle;
    public float MuzzleVelocity = 400;

    [Header("Advanced shooting")]
    [Range(1, 256)]
    public int BulletsPerShot = 1;
    public Vector2 BulletPathRandomness = Vector2.zero;

    [Header("Aim Down Sights")]
    public float ADSZoom = 1.25f;
    public float ADSScopeZoom = 1f;

    [Header("Stats")]
    public int MagazineCapacity = 17;
    public float RPM = 600f;

    [Header("Effects")]
    public bool SpawnUponShoot = true;
    public FallingShell ShellPrefab;
    public Transform ShellSpawn;
    public Vector3 MinShellVel = new Vector3(1, 1, 0), MaxShellVel = new Vector3(2, 2, 0);
    public MuzzleFlash MuzzleFlashPrefab;

    [Header("Recoil")]
    public Vector2 ShootPunchPitch = new Vector2(-4f, 4f);
    public Vector2 ShootPunchYaw = new Vector2(5f, 10f);
    public Vector2 ShootPunchRoll = new Vector2(-1f, 1f);
    public Vector3 HipfirePunchMultiplier = new Vector3(2f, 2f, 2f);

    [Header("Status")]
    public bool BulletInChamber = true;
    public int CurrentBullets = 17;

    private float shootTimer = 100f;    

    private void Update()
    {
        Anim.IsRecursiveReload = IsRecursiveReload;
        transform.localPosition = Offset;

        if (Input.GetKeyDown(KeyCode.R) && CurrentBullets < MagazineCapacity)
            Anim.Reload = true;
        if (Input.GetKeyDown(KeyCode.F))
            Anim.CheckMagazine = true;
        if (Input.GetKeyDown(KeyCode.G))
            Anim.Inspect = true;
        switch (FireMode)
        {
            case FireMode.Single:
                shootTimer += Time.deltaTime;
                if (shootTimer >= 1f / (RPM / 60f))
                {
                    if (Input.GetKeyDown(KeyCode.Mouse0) && BulletInChamber)
                    {
                        if ((IsRecursiveReload || !IsAutoChamber) && CurrentBullets == 0)
                            BulletInChamber = false;
                        Anim.Shoot = true;
                        shootTimer = 0f;
                    }
                }
                break;
            case FireMode.Auto:
                shootTimer += Time.deltaTime;
                if(shootTimer >= 1f / (RPM / 60f))
                {
                    if (Input.GetKey(KeyCode.Mouse0) && (BulletInChamber || (IsRecursiveReload && Anim.IsReloading)))
                    {
                        if ((IsRecursiveReload || !IsAutoChamber) && CurrentBullets == 0)
                            BulletInChamber = false;
                        Anim.Shoot = true;
                        shootTimer = 0f;
                    }
                }                
                break;
        }

        Anim.ADS = Input.GetKey(KeyCode.Mouse1);
        Anim.Run = Player.IsRunning;
        Anim.Crouch = Player.IsCrouching;

        if (!BulletInChamber && CurrentBullets > 0 && !Anim.IsChambering)
        {
            Anim.Chamber = true;
        }
        else if (BulletInChamber)
        {
            Anim.Chamber = false;
        }

        Anim.IsEmpty = !BulletInChamber;
        if(Anim.Slide != null)
            Anim.Slide.LockOpen = InvertSlideBehaviour ? BulletInChamber : !BulletInChamber;
    }

    private void UponAnimationEvent(AnimationEvent e)
    {
        string str = e.stringParameter.Trim().ToLower();

        switch (str)
        {
            case "playsound":
            case "play sound":
            case "playaudio":
            case "play audio":
                var a = Player.AudioSource;
                a.PlayOneShot(AudioClips[e.intParameter], e.floatParameter <= 0 ? 1f : e.floatParameter);
                break;

            case "spawnshell":
            case "spawn shell":
                SpawnShell();
                break;

            case "shoot":
                if (!BulletInChamber && !IsRecursiveReload && IsAutoChamber)
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
                SpawnMuzzleFlash();

                StartCoroutine(ShootLate());

                BulletInChamber = false;

                if (!IsRecursiveReload && CurrentBullets > 0 && IsAutoChamber)
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

    public void SpawnMuzzleFlash()
    {
        if(MuzzleFlashPrefab != null && Muzzle != null)
        {
            var spawned = PoolObject.Spawn(MuzzleFlashPrefab);
            spawned.transform.position = Muzzle.position;
            spawned.transform.localScale = Vector2.one * Random.Range(0.15f, 0.25f);
            spawned.transform.forward = Muzzle.forward;
            spawned.transform.Rotate(new Vector3(0f, 0f, Random.Range(0f, 360f)), Space.Self);
        }
    }

    private IEnumerator ShootLate()
    {
        yield return new WaitForEndOfFrame();
        if (ProjectilePrefab != null && Muzzle != null)
        {
            for (int i = 0; i < BulletsPerShot; i++)
            {
                var spawned = PoolObject.Spawn(ProjectilePrefab);
                Vector3 dir = Muzzle.forward;
                if(BulletPathRandomness != Vector2.zero)
                {
                    Vector3 randomSphere = Random.insideUnitSphere.normalized * Mathf.Lerp(BulletPathRandomness.x, BulletPathRandomness.y, Random.value);
                    if (randomSphere.z < 0f)
                        randomSphere.z = -randomSphere.z;
                    Vector3 inWorldSpace = Muzzle.TransformVector(randomSphere);
                    dir += inWorldSpace;
                }
                spawned.Velocity = dir * MuzzleVelocity;
                spawned.transform.position = Muzzle.transform.position + Muzzle.forward * 0.1f;
            }            
        }
    }
}

public enum FireMode
{
    Single,
    Auto
}