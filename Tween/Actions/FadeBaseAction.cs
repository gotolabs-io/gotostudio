using System;
using UnityEngine;
using UnityEngine.UI;
using GOTO.Utilities;

namespace GOTO.Logic.Actions
{
	[Serializable]
	[HideAction]
	public abstract class FadeBaseAction : Action
	{
		public Value gameObject = new Value(typeof(UnityEngine.GameObject));
		public Easing.Function easing;
		public Value seconds = new Value(typeof(float), 1f);

		protected float timeElapsed;
		protected Component component;

		protected override void OnStart()
		{
			InitializeCache();
			timeElapsed = 0f;
		}

		private void InitializeCache()
		{
			if (component == null)
			{
				GameObject go = gameObject.GetValue(this) as GameObject;
				if (go != null)
				{
					if (component == null)
						component = go.GetComponent<SpriteRenderer>();
					if (component == null)
						component = go.GetComponent<Renderer>();
					if (component == null)
						component = go.GetComponent<Image>();
					if (component == null)
						component = go.GetComponent<RawImage>();
					if (component == null)
						component = go.GetComponent<Text>();
				}
			}
		}

		protected void SetAlpha(float alpha)
		{
			if (component is SpriteRenderer)
			{
				SpriteRenderer spriteRenderer = component as SpriteRenderer;
				spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, alpha);
			}
			else if (component is Renderer)
			{
				Renderer renderer = component as Renderer;
				renderer.material.color = new Color(renderer.sharedMaterial.color.r, renderer.sharedMaterial.color.g, renderer.sharedMaterial.color.b, alpha);
			}
			else if (component is Image)
			{
				Image image = component as Image;
				image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);
			}
			else if (component is RawImage)
			{
				RawImage rawImage = component as RawImage;
				rawImage.color = new Color(rawImage.color.r, rawImage.color.g, rawImage.color.b, alpha);
			}
			else if (component is Text)
			{
				Text text = component as Text;
				text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
			}
		}
	}
}
