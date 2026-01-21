using MyProject.Common.GameEvent;
using MyProject.Common.UI;

namespace MyProject.MergeGame
{
    /// <summary>
    /// 머지게임 인게임 HUD 페이지입니다.
    /// </summary>
    public class Page_IngameHUD : UIPageBase
    {
        /// <summary>
        /// 스폰 버튼 클릭 시 호출됩니다.
        /// </summary>
        public void OnClick_SpawnUnit()
        {
            GameEventHub.Publish(
                new GameUIContextSupplyEvent(
                    new UIEventTryUnitSpawn(UIEventType.Button_Click, this),
                    this));
        }
    }
}
