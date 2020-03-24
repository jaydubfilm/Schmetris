using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBulletV2 : MonoBehaviour
{

    float speed = 1;
    public bool isInvader;
    public bool isMosquito;
    public bool isMama;
    float timeAtInstantiation;
    float bulletDuration;
    int bulletDamage;
    Vector3 dirToPlayer;
    SpriteRenderer spriteRenderer;
    Rigidbody2D rb2d;
    Transform target;

    private void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        #region Invader

        if (isInvader)
            transform.Translate(-transform.up * speed * Time.deltaTime);

        #endregion

        #region Mosquito

        if (isMosquito)
        {

            transform.position += dirToPlayer * speed * Time.deltaTime;
            
            if (Time.time - timeAtInstantiation > bulletDuration)
            {
                Destroy(gameObject);
            }
        }
        #endregion

        #region Mama

        if (isMama)
        {
            rb2d.bodyType = RigidbodyType2D.Dynamic;

            Vector3 angleToPlayer = (target.position -transform.position).normalized;
            rb2d.AddForce(new Vector2(angleToPlayer.x, angleToPlayer.y) * speed);

        }

        #endregion
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
            Damage(other.transform.GetComponentInParent<Brick>());
            Destroy(gameObject);
            return;
        }

        if (other.transform.GetComponentInParent<Bit>() != null)
        {
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

    public void MamaBulletBehaviour(Transform player, Sprite sprite, float bulletSpeed, float bulletLifetime, int damageDealt)
    {

        BulletSetup(sprite, bulletSpeed, bulletLifetime, damageDealt);
        target = player;
        print("Fire mama");

        transform.eulerAngles = new Vector3(0, 90, 0);
        isMama = true;
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
