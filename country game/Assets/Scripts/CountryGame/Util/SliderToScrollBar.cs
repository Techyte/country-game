namespace CountryGame.Util
{
    using UnityEngine.UI;

    public class ScrollRectFix : ScrollRect {
	
        override protected void LateUpdate() {

            base.LateUpdate();

            if (this.verticalScrollbar) {

                this.verticalScrollbar.size=0;
            }
        }
	
        override public void Rebuild(CanvasUpdate executing) {

            base.Rebuild(executing);

            if (this.verticalScrollbar) {

                this.verticalScrollbar.size=0;
            }
        }
    }
}