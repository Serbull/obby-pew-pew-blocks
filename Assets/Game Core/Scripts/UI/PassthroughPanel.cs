using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PassthroughPanel : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        PassEventThrough(eventData, ExecuteEvents.pointerDownHandler);
    }

    public void OnDrag(PointerEventData eventData)
    {
        PassEventThrough(eventData, ExecuteEvents.dragHandler);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        PassEventThrough(eventData, ExecuteEvents.endDragHandler);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        PassEventThrough(eventData, ExecuteEvents.pointerUpHandler);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        PassEventThrough(eventData, ExecuteEvents.pointerClickHandler);
    }

    private void PassEventThrough<T>(PointerEventData eventData, ExecuteEvents.EventFunction<T> handler)
        where T : IEventSystemHandler
    {
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            if (result.gameObject == gameObject) continue;

            // GetEventHandler walks up the hierarchy to find the actual handler (e.g. Button on a parent)
            var target = ExecuteEvents.GetEventHandler<T>(result.gameObject);
            if (target == null || target == gameObject) continue;

            ExecuteEvents.Execute(target, eventData, handler);
        }
    }
}
