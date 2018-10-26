using System;
using UnityEngine;
using GOTO.Utilities;

namespace GOTO.Logic.Actions
{
	[Serializable]
	[ActionCategory("Tween/Exits")]
	public class ScaleOutAction : ScaleBaseAction
	{
		protected override void OnUpdate()
		{
			float time = seconds.GetValue<float>(this);
			if (timeElapsed < time)
			{
				timeElapsed += Time.deltaTime;
				float normalizedTime = Mathf.Clamp01(timeElapsed / time);
				SetScale(Easing.Interpolate(1f - normalizedTime, easing), axis);
			}
			else
			{ 
				if (transform != null && transform.gameObject.activeSelf)
					transform.gameObject.SetActive(false);

				SetFinished();
			}
		}
	}
}
