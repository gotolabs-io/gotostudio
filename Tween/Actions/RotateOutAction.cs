using System;
using UnityEngine;
using UnityEngine.UI;
using GOTO.Utilities;

namespace GOTO.Logic.Actions
{
	[Serializable]
	[ActionCategory("Tween/Exits")]
	public class RotateOutAction : RotateBaseAction
	{
		protected override void OnUpdate()
		{
			float time = seconds.GetValue<float>(this);
			if (timeElapsed < time)
			{
				timeElapsed += Time.deltaTime;
				float normalizedTime = Mathf.Clamp01(timeElapsed / time);
				SetRotate(Easing.Interpolate(1.0f - normalizedTime, easing), axis);
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
