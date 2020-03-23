using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBulletV2 : MonoBehaviour
{

    float speed = 1;
    public bool isInvader;
    public bool isMosquito;
    float timeAtInstantiation;
    float bulletDuration;
    int bulletDamage;
    Vector3 dirToPlayer;
    SpriteRenderer spriteRenderer;

    private void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(isInvader)
            transform.Translate(-transform.up * speed * Time.deltaTime);

        if (isMosquito)
        {

            transform.position += dirToPlayer * speed * Time.deltaTime;
            
            if (Time.time - timeAtInstantiation > bulletDuration)
            {
                Destroy(gameObject);
            }
        }
    }

    void Damage(Brick brick)
    {

        brick.AdjustHP(-bulletDamage);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {   

        //hit a block on player ship
        if(other.transform.GetComponentInParent<Brick>() != null)
        {
            print("dealing " + bulletDamage);
            Damage(other.transform.GetComponentInParent<Brick>());
            Destroy(gameObject);
            return;
        }

        if (other.transform.GetComponentInParent<Bit>() != null)
        {
            print("hit floating brick");
            Destroy(gameObject);
            //hit a floating brick
            return;
        }
    }

    public void MosquitoBulletBehaviour(Vector3 target, Sprite sprite, Vector3 dir, float bulletLifetime, float bulletSpeed, int damageDealt)
    {

        BulletSetup(sprite, bulletSpeed, bulletLifetime, damageDealt);

        dirToPlayer = dir;
        transform.LookAt(target);
        isMosquito = true;
    }

    public void InvaderBulletBehaviour(Sprite sprite, float bulletSpeed, float bulletLifetime, int damageDealt)
    {

        BulletSetup(sprite, bulletSpeed, bulletLifetime, damageDealt);
        transform.eulerAngles = new Vector3(0, 90, 0);
        isInvader = true;
    }

    void BulletSetup(Sprite sprite, float bulletSpeed, float bulletLifetime, int damageDealt)
    {

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        bulletDuration = bulletLifetime;
        speed = bulletSpeed;
        bulletDamage = damageDealt;
        timeAtInstantiation = Time.timeSinceLevelLoad;
    }
}
