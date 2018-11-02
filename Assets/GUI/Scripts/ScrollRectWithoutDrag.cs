using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MUPS.UI
{
	public class ScrollRectWithoutDrag : ScrollRect
	{
		public override void OnDrag(PointerEventData eventData){}
		public override void OnBeginDrag(PointerEventData eventData){}
		public override void OnEndDrag(PointerEventData eventData){}
	}
}