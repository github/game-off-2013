using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("2D Toolkit/UI/Core/tk2dUILayoutContainerSizer")]
public class tk2dUILayoutContainerSizer : tk2dUILayoutContainer {
	public bool horizontal = false; // otherwise vertical
	public bool expand = true;
	public Vector2 margin = Vector2.zero;
	public float spacing = 0;

	protected override void DoChildLayout() {
		int n = layoutItems.Count;
		if (n == 0) return;

		float w = bMax.x - bMin.x - 2.0f * margin.x;
		float h = bMax.y - bMin.y - 2.0f * margin.y;
		float totalSpace = (horizontal ? w : h) - spacing * (float)(n - 1);
		float percentageTotal = 1;
		float space = totalSpace;
		float proportionSum = 0;

		float[] childSize = new float[n];
		for (int i = 0; i < n; ++i) {
			var item = layoutItems[i];
			if (item.fixedSize) {
				childSize[i] = horizontal ? (item.layout.bMax.x - item.layout.bMin.x) : (item.layout.bMax.y - item.layout.bMin.y);
				space -= childSize[i];
			} else if (item.fillPercentage > 0) {
				float frc = percentageTotal * item.fillPercentage / 100.0f;
				childSize[i] = totalSpace * frc;
				space -= childSize[i];
				percentageTotal -= frc;
			} else {
				proportionSum += item.sizeProportion;
			}
		}
		for (int i = 0; i < n; ++i) {
			var item = layoutItems[i];
			if (!item.fixedSize && item.fillPercentage <= 0) {
				childSize[i] = space * item.sizeProportion / proportionSum;
			}
		}

		Vector3 pMin = Vector3.zero;
		Vector3 pMax = Vector3.zero;
		float p = 0;
		Matrix4x4 m = transform.localToWorldMatrix;
		if (horizontal) {
			innerSize = new Vector2(2 * margin.x + spacing * (n - 1), bMax.y - bMin.y);
		} else {
			innerSize = new Vector2(bMax.x - bMin.x, 2 * margin.y + spacing * (n - 1));
		}
		for (int i = 0; i < n; ++i) {
			var item = layoutItems[i];
			Matrix4x4 itemToLocal = item.gameObj.transform.localToWorldMatrix * transform.worldToLocalMatrix;

			if (horizontal) {
				if (expand) {
				     pMin.y = bMin.y + margin.y;
				     pMax.y = bMax.y - margin.y;
				} 
				else {
				     pMin.y = itemToLocal.MultiplyPoint(item.layout.bMin).y;
				     pMax.y = itemToLocal.MultiplyPoint(item.layout.bMax).y;
				}
				pMin.x = bMin.x + margin.x + p;
				pMax.x = pMin.x + childSize[i];
			} else {
				if (expand) {
				     pMin.x = bMin.x + margin.x;
				     pMax.x = bMax.x - margin.x;
				} 
				else {
				     pMin.x = itemToLocal.MultiplyPoint(item.layout.bMin).x;
				     pMax.x = itemToLocal.MultiplyPoint(item.layout.bMax).x;
				}
				pMax.y = bMax.y - margin.y - p;
				pMin.y = pMax.y - childSize[i];
			}
			item.layout.SetBounds(m.MultiplyPoint(pMin), m.MultiplyPoint(pMax));
			p += childSize[i] + spacing;
			if (horizontal) innerSize.x += childSize[i];
			else innerSize.y += childSize[i];
		}
	}
}
