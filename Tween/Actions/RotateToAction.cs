using GOTO.Utilities;
using System;
using UnityEngine;

namespace GOTO.Logic.Actions
{
	[Serializable]
	[ActionCategory("Tween")]
	public class RotateToAction : Action
	{
		public Value gameObject = new Value(typeof(UnityEngine.GameObject));
		public Value rotation = new Value(typeof(Vector3));
		public Easing.Function easing;
		public Value seconds = new Value(typeof(float), 1f);

		private float time;
		private float elapsedTime;
		private Transform transform;
		private Quaternion from;
		private Quaternion to;

		protected override void OnStart()
		{
			GameObject go = gameObject.GetValue<GameObject>(this);
			if (go != null)
			{
				transform = go.transform;
				from = transform.rotation;
				time = seconds.GetValue<float>(this);
				elapsedTime = 0f;
				to = Quaternion.Euler(rotation.GetValue<Vector3>(this));
			}
		}

		protected override void OnUpdate()
		{
			if (transform != null)
			{
				float normalizedTime = elapsedTime / time;
				float interpolation = Easing.Interpolate(normalizedTime, easing);
				transform.rotation = Quaternion.LerpUnclamped(from, to, interpolation);
			}
			elapsedTime += Time.deltaTime;

			if (elapsedTime >= time)
			{
				transform.rotation = to;
				SetFinished();
			}
		}
	}
}
