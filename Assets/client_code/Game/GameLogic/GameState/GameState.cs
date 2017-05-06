using UnityEngine;
using System.Collections;

namespace CustomGame
{
    public abstract class GameState
    {
        protected GameStateType GSType = GameStateType.EmNone;
        public GameStateType GetStateType() { return GSType; }

        public virtual bool TryEnterState(GameState preState) { return false; }
        public abstract void EnterState(GameState preState);
        public virtual bool TryLeaveState(GameState nextState) { return false; }
        public abstract void LeaveState(GameState nextState);

        public abstract void OnUpdate();
    }
}