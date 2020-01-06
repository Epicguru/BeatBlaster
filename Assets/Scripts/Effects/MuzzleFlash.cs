
using UnityEngine;

[RequireComponent(typeof(PoolObject))]
public class MuzzleFlash : MonoBehaviour
{
    [Header("Refs")]
    public MeshRenderer Renderer;

    public float Life = 0.1f;

    private float timer;

    private void UponSpawn()
    {
        timer = Life;        
    }

    private void Update()
    {
        foreach (var mat in Renderer.materials)
        {
            var c = mat.color;
            c.a = timer / Life;
            mat.color = c;
        }

        timer -= Time.deltaTime;
        if(timer <= 0f)
        {
            PoolObject.Despawn(this.gameObject);
        }
    }
}
