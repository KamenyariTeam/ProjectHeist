using UnityEngine;

namespace Characters.AI.Enemy
{
    public abstract class BaseEnemyState : IAIStateLogic
    {
        protected EnemyLogic EnemyLogic { get; private set; }

        public virtual void Init(IAILogic enemyLogic)
        {
            EnemyLogic = enemyLogic as EnemyLogic;
            Debug.Assert(EnemyLogic == null, "EnemyLogic is null");
        }

        public virtual void OnEnter()
        {
        }

        public virtual void OnExit()
        {
        }

        public virtual AIState OnUpdate(float deltaTime)
        {
            return AIState.Patrolling;
        }
    }
}