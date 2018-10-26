using System;
using UnityEngine;
using UnityEngine.UI;
using GOTO.Utilities;

namespace GOTO.Logic.Actions
{
	[Serializable]
	[ActionCategory("Tween/Entrances")]
	public class ScaleInAction : ScaleBaseAction
	{
		protected override void OnStart()
		{
			if (transform != null && !transform.gameObject.activeSelf)
				transform.gameObject.SetActive(true);

			base.OnStart();
		}

		protected override void OnUpdate()
		{
			float time = seconds.GetValue<float>(this);
			if (timeElapsed < time)
			{
				timeElapsed += Time.deltaTime;
				float normalizedTime = Mathf.Clamp01(timeElapsed / time);
				SetScale(Easing.Interpolate(normalizedTime, easing), axis);
			}
			else
			{
				if (transform != null)
					transform.localScale = end;
				SetFinished();
			}
		}
	}
}
