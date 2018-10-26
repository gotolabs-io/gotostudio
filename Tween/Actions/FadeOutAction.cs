using System;
using UnityEngine;
using GOTO.Utilities;

namespace GOTO.Logic.Actions
{
	[Serializable]
	[ActionCategory("Tween/Exits")]
	public class FadeOutAction : FadeBaseAction
	{
		protected override void OnUpdate()
		{
			float time = seconds.GetValue<float>(this);
			if (timeElapsed < time)
			{
				timeElapsed += Time.deltaTime;
				float normalizedTime = Mathf.Clamp01(timeElapsed / time);
				SetAlpha(1f - Easing.Interpolate(normalizedTime, easing));
			}
			else
			{
				if (component != null && component.gameObject.activeSelf)
					component.gameObject.SetActive(false);

				SetFinished();
			}
		}
	}
}
