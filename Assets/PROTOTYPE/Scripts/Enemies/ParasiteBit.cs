using UnityEngine;

namespace StarSalvager.Prototype
{
    [System.Obsolete("Prototype Only Script")]
//Parasite enemy type
    public class ParasiteBit : Enemy
    {
        //Enemy targeting
        LayerMask brickMask;
        Bot bot;

        //Init
        protected override void Init()
        {
            base.Init();
            Vector3 dest = GameController.Instance.bot.transform.position;
            brickMask = LayerMask.GetMask("Brick");
            bot = GameController.Instance.bot;
        }

        //Move toward and attach to a targeted brick
        protected override void UpdateLiveBehaviour()
        {
            base.UpdateLiveBehaviour();
            float step = data.speed * Time.deltaTime;
            RaycastHit2D rH = Physics2D.BoxCast(transform.position, Vector2.one * ScreenStuff.colSize, 0,
                bot.transform.position - transform.position, step, brickMask);
            if (rH.collider != null)
            {
                bot.ResolveEnemyCollision(gameObject);
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, bot.transform.position, step);
            }
        }

        //Destroy after awarding points
        protected override void OnEnemyDeath()
        {
            base.OnEnemyDeath();
            Destroy(gameObject);
        }
    }

}