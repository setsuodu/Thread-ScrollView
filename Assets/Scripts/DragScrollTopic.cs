using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DragScrollTopic : MonoBehaviour
{
	public ScrollRect m_scrollRect;
	public RectTransform m_image;
	public float _y;

	void Start()
	{

	}

	void Update()
	{
        _y = m_scrollRect.content.anchoredPosition.y / 500;

		if(_y < 0)
		{
			m_image.localScale = Vector3.one * (1 - _y);
		}
		else if(_y > 0 && _y < 0.6f)
		{
			m_image.anchoredPosition = new Vector2(0, _y * 500);
            m_image.localScale = Vector3.one;
		}
		else
		{
			m_image.anchoredPosition = new Vector2(0, 300);
            m_image.localScale = Vector3.one;
		}
	}
}
