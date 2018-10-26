using System;
using UnityEngine;
using UnityEngine.UI;
using GOTO.Utilities;

namespace GOTO.Logic.Actions
{
	[Serializable]
	[ActionCategory("Tween/Entrances")]
	public class FadeInAction : FadeBaseAction
	{
		protected override void OnStart()
		{
			if (component != null && !component.gameObject.activeSelf)
				component.gameObject.SetActive(true);

			base.OnStart();
		}

		protected override void OnUpdate()
		{
			float time = seconds.GetValue<float>(this);
			if (timeElapsed < time)
			{
				timeElapsed += Time.deltaTime;
				float normalizedTime = Mathf.Clamp01(timeElapsed / time);
				SetAlpha(Easing.Interpolate(normalizedTime, easing));
			}
			else
			{
				SetAlpha(1f);
				SetFinished();
			}
		}
	}
}
