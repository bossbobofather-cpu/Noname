using UnityEngine;
using MyProject.Common.GameMode;
using MyProject.Common.UI;

namespace MyProject.MergeGame
{
    /// <summary>
    /// UI 관리 모듈입니다.
    /// </summary>
    public sealed class UIModule : ModuleBase
    {
        [SerializeField] private UIPageBase _hudPrefab;

        private UIPageBase _hudInstance;


        protected override void OnInit()
        {
            base.OnInit();
        }

        protected override void OnStartup()
        {
            base.OnStartup();

            OpenHUD();
        }

        protected override void OnShutdown()
        {
            base.OnShutdown();
        }

        private void OpenHUD()
        {
            
            if(_hudPrefab == null)
            {
                Debug.LogError("HUD Prefab Is NULL");
                return;
            }

            _hudInstance = UIManager.Instance.OpenPage(_hudPrefab);
            if(_hudInstance == null)
            {
                Debug.LogError("Failed Instantiating HUD Page.");
                return;
            }
        }
    }
}
