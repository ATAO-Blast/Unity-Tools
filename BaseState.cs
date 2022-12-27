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
    public abstract class BaseState<T> where T : Enum //ʹ��ö���������������ڷ���
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

        public void Trigger(T life)//ͳһ�����������ڷ���
        {
            mFatherState?.Trigger(life);
            Execute(life);
        }
        protected abstract string CheckCondition();
        protected abstract void Execute(T life);//���Execute������ָ�����������ڵľ���ʵ��
    }
    public abstract class HFSM<T> : MonoBehaviour where T : Enum
    {
        protected T Life;
        private Dictionary<string, BaseState<T>> mStates;//���ֵ仺��״̬

        protected BaseState<T> mCurState;//���浱ǰ״̬

        protected abstract IStateBuilder<T> Builder { get; }

        private void Awake()//��Awake�г�ʼ������״̬
        {
            mStates = Builder.Create(out string defaultStateName);
            mCurState = mStates[defaultStateName];
            Life = FirstLife;
        }
        protected abstract T FirstLife { get; }
        protected void SwitchState(string nextStateName)//�����ṩ״̬�л�����
        {
            if (!mStates.TryGetValue(nextStateName, out var nextState))
            {
                throw new System.Exception("״̬������ ");
            }
            SwitchState(nextState);
        }
        protected void InitAllState(Action<BaseState<T>> callback)//��ӳ�ʼ������״̬�ķ���
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
