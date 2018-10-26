using GOTO.Utilities;
using System;
using UnityEngine;

namespace GOTO.Logic.Actions
{
	[Serializable]
	[ActionCategory("Tween")]
	public class ScaleToAction : Action
	{
		public Value gameObject = new Value(typeof(UnityEngine.GameObject));
		public Value scale = new Value(typeof(Vector3), Vector3.one);
		public Easing.Function easing;
		public Value seconds = new Value(typeof(float), 1f);

		private float time;
		private float elapsedTime;
		private Transform transform;
		private Vector3 from;
		private Vector3 to;

		protected override void OnStart()
		{
			GameObject go = gameObject.GetValue<GameObject>(this);
			if(go != null)
			{
				transform = go.transform;
				from = transform.localScale;
				time = seconds.GetValue<float>(this);
				to = scale.GetValue<Vector3>(this);
				elapsedTime = 0f;
			}
		}

		protected override void OnUpdate()
		{
			if (transform != null)
			{
				float normalizedTime = elapsedTime / time;
				float interpolation = Easing.Interpolate(normalizedTime, easing);
				transform.localScale = Vector3.LerpUnclamped(from, to, interpolation);
			}
			elapsedTime += Time.deltaTime;
			
			if (elapsedTime >= time)
			{
				transform.localScale = to;
				SetFinished();
			}
		}
	}
}
