public class EnemyGeneral : Enemy
{
    protected override void OnEnemyDeath()
    {
        base.OnEnemyDeath();
        if (GetComponent<MosquitoAI>())
        {
            GetComponent<MosquitoAI>().Death();
        }
        else if (GetComponent<Mama>())
        {
            GetComponent<Mama>().Death();
        }
        else if (GetComponent<InvaderMovement>())
        {
            GetComponent<InvaderMovement>().Death();
        }
    }
}
