using Lofle;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Example : MonoBehaviour
{
	private StateMachine _stateMachine = null;
	private StateMachine<Example> _ownerStateMachine = null;

	//public string Foo => "Bar";
		
	private void Start ()
	{
		// 기본 상태머신
		_stateMachine = new StateMachine();

		// 모든 상태 클래스 내부에서 원하는 instance를 Owner 프로퍼티를 통해 접근 가능
		_ownerStateMachine = new StateMachine<Example>( this );

		StartCoroutine( _stateMachine.Coroutine<StartState>() );
		StartCoroutine( _ownerStateMachine.Coroutine<OwnerStartState>() );
	}

	/// <summary>
	/// 기본 State 클래스를 상속받은 StartState
	/// </summary>
	private class StartState : State
	{
		private int _count = 0;

		protected override void Begin()
		{
			_count = 10;
			// Debug.Log( $"{Owner.Foo}" ); // Owner기능이 없음, 출력 실패
		}

		protected override void Update()
		{
			if( _count-- <= 0 )
			{
				Invoke<EndState>();
			}
		}

		protected override void End()
		{
			Debug.Log( "Start 상태 이탈" );
		}
	}

	private class EndState : State
	{
		protected override void Begin()
		{
			// 처음 상태로 다시 원상복귀
			Invoke<StartState>();
		}
	}

	/// <summary>
	/// Owner 접근이 가능한 OwnerStartState
	/// State를 제네릭으로 상속받으면 동작하는 상태머신이 가진 Owner instance를 접근가능
	/// </summary>
	private class OwnerStartState : State<Example>
	{
		private int _count = 0;

		protected override void Begin()
		{
			_count = 10;
			//Debug.Log( $"{Owner.Foo}" ); // Owner는 Example의 Instance, "Bar" 출력
			Invoke<OwnerEndState>();
		}

		protected override void Update()
		{
			if( _count-- <= 0 )
			{
				Invoke<OwnerEndState>();
			}
		}

		protected override void End()
		{
			Debug.Log( "OwnerStart 상태 이탈" );
		}
	}

	private class OwnerEndState : State<Example>
	{
		protected override void Begin()
		{
			// 처음 상태로 다시 원상복귀
			Invoke<OwnerStartState>();
		}
	}
}
