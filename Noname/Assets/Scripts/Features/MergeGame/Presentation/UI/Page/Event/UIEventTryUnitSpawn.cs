using MyProject.Common.UI;

namespace MyProject.MergeGame
{
    /// <summary>
    /// 유닛 스폰 입력을 알리는 UI 이벤트입니다.
    /// </summary>
    public sealed class UIEventTryUnitSpawn : UIEventContext
    {
        public UIEventTryUnitSpawn(UIEventType eventType, object source)
            : base(eventType, source)
        {
        }
    }
}
