using UnityEngine;
using System.Collections;
using CustomUtil;
using System.Collections.Generic;

namespace CustomGame
{
    public class GameStateManager : Singleton<GameStateManager>
    {
        private GameState m_CurrentState = null;
        private Dictionary<GameStateType, GameState> m_StateMap = new Dictionary<GameStateType, GameState>();

        public GameStateManager()
        {
            //m_StateMap.Add(GameStateType.EMainInterface, );
            //m_StateMap.Add(GameStateType.EMainGame, );
        }

        public void OnUpdate()
        {
            if (m_CurrentState != null) m_CurrentState.OnUpdate();
        }

        /// <summary>
        /// 设置当前游戏状态
        /// </summary>
        /// <param name="type">游戏状态类型</param>
        /// <returns></returns>
        public bool SetActiveState(GameStateType type)
        {
            if (m_CurrentState != null)
            {
                if (m_CurrentState.GetStateType() != type)
                {
                    GameState nextState = null;
                    if (m_StateMap.TryGetValue(type, out nextState))
                    {
                        // 尝试进入状态 
                        if ( m_CurrentState.TryLeaveState(nextState) && nextState.TryEnterState(m_CurrentState) )
                        {
                            UnityCustomUtil.CustomLog("StateSwitch[" + m_CurrentState.GetStateType().ToString() + "]--->>>[" + nextState.GetStateType().ToString() + "]");
                            m_CurrentState.LeaveState(nextState);
                            m_CurrentState = nextState;
                            m_CurrentState.EnterState(m_CurrentState);
                            return true;
                        }
                    }
                    else
                    {
                        if (m_CurrentState.TryLeaveState(null))
                        {
                            UnityCustomUtil.CustomLog("StateSwitch[" + m_CurrentState.GetStateType().ToString() + "]--->>>[null]");
                            m_CurrentState.LeaveState(null);
                            m_CurrentState = null;
                            return true;
                        }
                    }
                }
            }
            else
            {
                GameState nextState = null;
                if (m_StateMap.TryGetValue(type, out nextState))
                {
                    // 尝试进入状态
                    if (nextState.TryEnterState(null))
                    {
                        UnityCustomUtil.CustomLog("StateSwitch[null]--->>>[" + nextState.GetStateType().ToString() + "]");
                        m_CurrentState = nextState;
                        m_CurrentState.EnterState(null);
                        return true;
                    }
                }
            }
            return false;
        }
    }
}

