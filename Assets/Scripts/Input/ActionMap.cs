
using UnityEngine;

namespace Input
{
    public abstract class ActionMap : IState
    {
        protected InputActions InputActions;
        public abstract bool HasPollable { get; }
        public ActionMap(InputActions action)
        {
            InputActions = action;
        }
        public abstract void OnEnter();

        public abstract void OnExit();

        public virtual void OnUpdate()
        {
        }
    }

    public class UIActionMap : ActionMap
    {
        private InputButton _inventory;
        private InputButton _submit;
        private InputButton _todo;
        public override bool HasPollable => false;
        public InputButton Inventory => _inventory;
        public InputButton Submit => _submit;
        public InputButton Todo => _todo;
        public UIActionMap(InputActions action) : base(action)
        {
            _inventory = new InputButton(action.UI.Inventory);
            _submit = new InputButton(action.UI.Submit);
            _todo = new InputButton(action.UI.Todo);
        }
        public override void OnEnter()
        {
            InputActions.UI.Enable();
        }

        public override void OnExit()
        {
            InputActions.UI.Disable();
        }
        public override void OnUpdate()
        {
        }
    }

    public class PlayerActionMap : ActionMap
    {
        private InputValue<Vector2> _movement;
        private InputButton _interact;
        private InputButton _inventory;
        private InputButton _previous;
        private InputButton _next;
        private InputButton[] _hotbarSlots;
        private InputButton _temp;
        private InputButton _todo;
        public InputValue<Vector2> Movement => _movement;
        public InputButton Interact => _interact;
        public InputButton Inventory => _inventory;
        public InputButton Previous => _previous;
        public InputButton Next => _next;
        public InputButton Hotbar1 => _hotbarSlots[0];
        public InputButton Hotbar2 => _hotbarSlots[1];
        public InputButton Hotbar3 => _hotbarSlots[2];
        public InputButton Hotbar4 => _hotbarSlots[3];
        public InputButton Hotbar5 => _hotbarSlots[4];
        public InputButton[] HotbarSlots => _hotbarSlots;
        public InputButton Temp => _temp;
        public InputButton Todo => _todo;
        public override bool HasPollable => true;

        public PlayerActionMap(InputActions action) : base(action)
        {
            _movement = new InputValue<Vector2>(action.Player.Move);
            _interact = new InputButton(action.Player.Interact);
            _inventory = new InputButton(action.Player.Inventory);
            _previous = new InputButton(action.Player.Previous);
            _next = new InputButton(action.Player.Next);
            _hotbarSlots = new InputButton[]
            {
                new InputButton(action.Player.Hotbar1),
                new InputButton(action.Player.Hotbar2),
                new InputButton(action.Player.Hotbar3),
                new InputButton(action.Player.Hotbar4),
                new InputButton(action.Player.Hotbar5),
            };
            
            //temp
            _temp = new InputButton(action.Player.Jump);
            _todo = new InputButton(action.Player.Todo);
        }


        public override void OnEnter()
        {
            InputActions.Player.Enable();
        }

        public override void OnExit()
        {
            InputActions.Player.Disable();
        }
        public override void OnUpdate()
        {
            _movement.ForcePoll();
        }
    }
    
}