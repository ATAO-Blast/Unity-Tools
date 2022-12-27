using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ATAO.FSM
{
    public interface IStateBuilder<T> where T : Enum
    {
        Dictionary<string, BaseState<T>> Create(out string firstStateName);
    }
    public abstract class BaseState<T> where T : Enum //使用枚举来设置生命周期方法
    {
        private BaseState<T> mFatherState;
        public BaseState<T> SetFather(BaseState<T> father)
        {
            mFatherState = father;
            return this;
        }
        public bool Check(out string nextStateName)
        {
            if (mFatherState != null && mFatherState.Check(out nextStateName))
            {
                return true;
            }

            nextStateName = CheckCondition();
            return !string.IsNullOrEmpty(nextStateName);
        }

        public void Trigger(T life)//统一触发生命周期方法
        {
            mFatherState?.Trigger(life);
            Execute(life);
        }
        protected abstract string CheckCondition();
        protected abstract void Execute(T life);//添加Execute方法来指定各生命周期的具体实现
    }
    public abstract class HFSM<T> : MonoBehaviour where T : Enum
    {
        protected T Life;
        private Dictionary<string, BaseState<T>> mStates;//用字典缓存状态

        protected BaseState<T> mCurState;//缓存当前状态

        protected abstract IStateBuilder<T> Builder { get; }

        private void Awake()//在Awake中初始化所有状态
        {
            mStates = Builder.Create(out string defaultStateName);
            mCurState = mStates[defaultStateName];
            Life = FirstLife;
        }
        protected abstract T FirstLife { get; }
        protected void SwitchState(string nextStateName)//对外提供状态切换方法
        {
            if (!mStates.TryGetValue(nextStateName, out var nextState))
            {
                throw new System.Exception("状态不存在 ");
            }
            SwitchState(nextState);
        }
        protected void InitAllState(Action<BaseState<T>> callback)//添加初始化所有状态的方法
        {
            if (mStates == null || mStates.Count == 0) return;
            foreach (var state in mStates.Values) callback(state);
        }
        protected abstract void SwitchState(BaseState<T> nextState);
        protected abstract void SwitchLifeByRule();
        protected virtual void Update()
        {
            if (mCurState == null) return;

            mCurState.Trigger(Life);

            SwitchLifeByRule();
            if (!mCurState.Check(out string nextStateName)) return;
            SwitchState(nextStateName);
        }
    }
}
