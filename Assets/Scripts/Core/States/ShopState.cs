using UnityEngine;
using SurvivorSeries.Utilities;

namespace SurvivorSeries.Core.States
{
    public class ShopState : GameState
    {
        public override void Enter()
        {
            Debug.Log("[State] Shop");
            Time.timeScale = 0f;

            if (ServiceLocator.TryGet<Shop.ShopManager>(out var shop))
                shop.GenerateInventory();

            if (ServiceLocator.TryGet<UI.Shop.ShopUI>(out var ui))
                ui.Show();
        }

        public override void Tick(float deltaTime) { }

        public override void Exit()
        {
            Time.timeScale = 1f;

            if (ServiceLocator.TryGet<UI.Shop.ShopUI>(out var ui))
                ui.Hide();
        }
    }
}