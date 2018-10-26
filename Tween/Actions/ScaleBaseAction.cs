using System;
using UnityEngine;
using GOTO.Utilities;

namespace GOTO.Logic.Actions
{
	[Serializable]
	[HideAction]
	public class ScaleBaseAction : Action
	{
		public enum Axis { All, X, Y, Z }

		public Value gameObject = new Value(typeof(UnityEngine.GameObject));
		public Easing.Function easing;
		public Axis axis = Axis.All;
		public Value seconds = new Value(typeof(float), 1f);

		protected float timeElapsed;
		protected Transform transform;
		protected bool initialized;
		protected Vector3 start;
		protected Vector3 end;
		protected Vector3 delta;

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
				end = transform.localScale;
				switch(axis)
				{
					case Axis.All: start = Vector3.zero; break;
					case Axis.X: start = new Vector3(0f, end.y, end.z); break;
					case Axis.Y: start = new Vector3(end.x, 0f, end.z); break;
					case Axis.Z: start = new Vector3(end.x, end.y, 0f); break;
				}
			}

			initialized = true;
		}

		protected void SetScale(float normalized, Axis axis)
		{
			if (transform != null)
			{
				transform.localScale = Vector3.LerpUnclamped(start, end, normalized);
			}
		}
	}
}
