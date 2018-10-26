using GOTO.Utilities;
using System;
using UnityEngine;

namespace GOTO.Logic.Actions
{
	[Serializable]
	[ActionCategory("Tween")]
	public class MoveToAction : Action
	{
		public Value gameObject = new Value(typeof(UnityEngine.GameObject));
		public Value position = new Value(typeof(Vector3));
		public Easing.Function easing;
		public Value seconds = new Value(typeof(float), 1f);

		private float time;
		private float elapsedTime;
		private Transform transform;
		private Vector3 from;
		private Vector3 to;
		private bool ui;

		protected override void OnStart()
		{
			GameObject go = gameObject.GetValue<GameObject>(this);
			if (go != null)
			{
				ui = go.GetComponent<RectTransform>() != null;
				transform = go.transform;

				from = ui ? transform.localPosition : transform.position;

				time = seconds.GetValue<float>(this);
				to = position.GetValue<Vector3>(this);
				elapsedTime = 0f;
			}
		}

		protected override void OnUpdate()
		{
			if (transform != null)
			{
				float normalizedTime = elapsedTime / time;
				float interpolation = Easing.Interpolate(normalizedTime, easing);

				if(ui)
					transform.localPosition = Vector3.LerpUnclamped(from, to, interpolation);
				else
					transform.position = Vector3.LerpUnclamped(from, to, interpolation);
			}
			elapsedTime += Time.deltaTime;

			if (elapsedTime >= time)
			{
				if (ui)
					transform.localPosition = to;
				else
					transform.position = to;

				SetFinished();
			}
		}
	}
}
