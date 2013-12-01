using UnityEngine;
using UnityEditor;
using System.Collections;

public class tk2dSpriteAnimationPreview 
{
	private void Init()
	{
	}

	public void Destroy()
	{
		tk2dSpriteThumbnailCache.Done();
		tk2dGrid.Done();
	}

	void Repaint() { HandleUtility.Repaint(); }

	public int Frame { get; set; }
	Vector2 translate = Vector2.zero;
	float scale = 1.0f;
	bool dragging = false;

	public void ResetTransform()
	{
		scale = 1.0f;
		translate.Set(0, 0);
		Repaint();
	}

	public void Draw(Rect r, tk2dSpriteDefinition sprite)
	{
		Init();

		Event ev = Event.current;
		switch (ev.type)
		{
			case EventType.MouseDown:
				if (r.Contains(ev.mousePosition))
				{
					dragging = true;
					ev.Use();
				}
				break;
			case EventType.MouseDrag:
				if (dragging && r.Contains(ev.mousePosition)) 
				{
					translate += ev.delta;
					ev.Use();
					Repaint();
				}
				break;
			case EventType.MouseUp:
				dragging = false;
				break;
			case EventType.ScrollWheel:
				if (r.Contains(ev.mousePosition)) 
				{
					scale = Mathf.Clamp(scale + ev.delta.y * 0.1f, 0.1f, 10.0f);
					ev.Use();
					Repaint();
				}
				break;
		}

		tk2dGrid.Draw(r, translate);

		// Draw axis
		Vector2 axisPos = new Vector2(r.center.x + translate.x, r.center.y + translate.y);
		if (axisPos.y > r.yMin && axisPos.y < r.yMax) {
			Handles.color = new Color(1, 0, 0, 0.5f);
			Handles.DrawLine(new Vector2(r.x, r.center.y + translate.y), new Vector2(r.x + r.width, r.center.y + translate.y));
		}
		if (axisPos.x > r.xMin && axisPos.x < r.xMax) {
			Handles.color = new Color(0, 1, 0, 0.5f);
			Handles.DrawLine(new Vector2(r.center.x + translate.x, r.y), new Vector2(r.center.x + translate.x, r.y + r.height));
		}
		Handles.color = Color.white;

		// Draw sprite
		if (sprite != null)
		{
			tk2dSpriteThumbnailCache.DrawSpriteTextureCentered(r, sprite, translate, scale, Color.white);
		}
	}
}
