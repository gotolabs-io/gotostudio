using System;
using UnityEngine;
using GOTO.Utilities;

namespace GOTO.Logic.Actions
{
	[Serializable]
	[HideAction]
	public class RotateBaseAction : Action
	{
		public enum Axis { X, Y, Z }

		public Value gameObject = new Value(typeof(UnityEngine.GameObject));
		public Easing.Function easing;
		public Axis axis = Axis.Z;
		public Value angle = new Value(typeof(float), -90f);
		public Value seconds = new Value(typeof(float), 1f);

		protected float timeElapsed;
		protected Transform transform;
		protected bool initialized;
		protected Quaternion start;
		protected Quaternion end;

		protected override void OnStart()
		{
			InitializeCache();
			timeElapsed = 0f;
		}

		private void InitializeCache()
		{
			if (initialized)
				return;

			GameObject go = gameObject.GetValue(this) as GameObject;
			if (go != null)
			{
				transform = go.transform;
				end = transform.rotation;
				switch (axis)
				{
					case Axis.X: start = Quaternion.Euler(end.eulerAngles.x + angle.GetValue<float>(this), end.eulerAngles.y, end.eulerAngles.z); break;
					case Axis.Y: start = Quaternion.Euler(end.eulerAngles.x, end.eulerAngles.y + angle.GetValue<float>(this), end.eulerAngles.z); break;
					case Axis.Z: start = Quaternion.Euler(end.eulerAngles.x, end.eulerAngles.y, end.eulerAngles.z + angle.GetValue<float>(this)); break;
				}
			}

			initialized = true;
		}

		protected void SetRotate(float normalized, Axis axis)
		{
			if (transform != null)
			{
				transform.rotation = Quaternion.LerpUnclamped(start, end, normalized);
			}
		}
	}
}
