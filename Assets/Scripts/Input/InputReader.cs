using System;
using UnityEngine;
using UnityEngine.InputSystem;
using SurvivorSeries.Utilities;

namespace SurvivorSeries.Input
{
    /// <summary>
    /// Wraps the generated InputSystem_Actions class and exposes C# events.
    /// All other systems subscribe to these events — no direct InputSystem calls elsewhere.
    /// </summary>
    public class InputReader : MonoBehaviour,
        InputSystem_Actions.IPlayerActions,
        InputSystem_Actions.IUIActions
    {
        private InputSystem_Actions _actions;

        // Player events
        public event Action<Vector2> OnMove;
        public event Action<Vector2> OnLook;
        public event Action OnAttackStarted;
        public event Action OnAttackCancelled;
        public event Action OnInteractStarted;
        public event Action OnInteractCancelled;
        public event Action OnPausePressed;
        public event Action OnPreviousPressed;
        public event Action OnNextPressed;

        // UI events
        public event Action OnUISubmit;
        public event Action OnUICancel;

        private void Awake()
        {
            _actions = new InputSystem_Actions();
            _actions.Player.SetCallbacks(this);
            _actions.UI.SetCallbacks(this);
            ServiceLocator.Register<InputReader>(this);
        }

        private void OnEnable() => EnablePlayerMap();
        private void OnDisable() => _actions?.Disable();
        private void OnDestroy() => ServiceLocator.Unregister<InputReader>();

        public void EnablePlayerMap()
        {
            _actions.UI.Disable();
            _actions.Player.Enable();
        }

        public void EnableUIMap()
        {
            _actions.Player.Disable();
            _actions.UI.Enable();
        }

        public void DisableAll() => _actions.Disable();

        // ─── IPlayerActions ───────────────────────────────────────────────

        void InputSystem_Actions.IPlayerActions.OnMove(InputAction.CallbackContext ctx)
            => OnMove?.Invoke(ctx.ReadValue<Vector2>());

        void InputSystem_Actions.IPlayerActions.OnLook(InputAction.CallbackContext ctx)
            => OnLook?.Invoke(ctx.ReadValue<Vector2>());

        void InputSystem_Actions.IPlayerActions.OnAttack(InputAction.CallbackContext ctx)
        {
            if (ctx.started)  OnAttackStarted?.Invoke();
            if (ctx.canceled) OnAttackCancelled?.Invoke();
        }

        void InputSystem_Actions.IPlayerActions.OnInteract(InputAction.CallbackContext ctx)
        {
            if (ctx.started)  OnInteractStarted?.Invoke();
            if (ctx.canceled) OnInteractCancelled?.Invoke();
        }

        void InputSystem_Actions.IPlayerActions.OnJump(InputAction.CallbackContext ctx) { }
        void InputSystem_Actions.IPlayerActions.OnCrouch(InputAction.CallbackContext ctx) { }
        void InputSystem_Actions.IPlayerActions.OnSprint(InputAction.CallbackContext ctx) { }

        void InputSystem_Actions.IPlayerActions.OnPrevious(InputAction.CallbackContext ctx)
        {
            if (ctx.started) OnPreviousPressed?.Invoke();
        }

        void InputSystem_Actions.IPlayerActions.OnNext(InputAction.CallbackContext ctx)
        {
            if (ctx.started) OnNextPressed?.Invoke();
        }

        // ─── IUIActions ───────────────────────────────────────────────────

        void InputSystem_Actions.IUIActions.OnNavigate(InputAction.CallbackContext ctx) { }

        void InputSystem_Actions.IUIActions.OnSubmit(InputAction.CallbackContext ctx)
        {
            if (ctx.started) OnUISubmit?.Invoke();
        }

        void InputSystem_Actions.IUIActions.OnCancel(InputAction.CallbackContext ctx)
        {
            if (ctx.started)
            {
                OnUICancel?.Invoke();
                OnPausePressed?.Invoke();
            }
        }

        void InputSystem_Actions.IUIActions.OnPoint(InputAction.CallbackContext ctx) { }
        void InputSystem_Actions.IUIActions.OnClick(InputAction.CallbackContext ctx) { }
        void InputSystem_Actions.IUIActions.OnRightClick(InputAction.CallbackContext ctx) { }
        void InputSystem_Actions.IUIActions.OnMiddleClick(InputAction.CallbackContext ctx) { }
        void InputSystem_Actions.IUIActions.OnScrollWheel(InputAction.CallbackContext ctx) { }
        void InputSystem_Actions.IUIActions.OnTrackedDevicePosition(InputAction.CallbackContext ctx) { }
        void InputSystem_Actions.IUIActions.OnTrackedDeviceOrientation(InputAction.CallbackContext ctx) { }
    }
}
