using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;
using TrexRunner.Entities;

namespace TrexRunner.System
{
    public class InputController
    {

        private bool _isBlocked;
        private Trex _trex;

        private KeyboardState _previousKeyboardState;

        public InputController(Trex trex)
        {
            _trex = trex;
        }

        public void ProcessControls(GameTime gameTime)
        {

            KeyboardState keyboardState = Keyboard.GetState();

            if(!_isBlocked)
            {
                bool isJumpKeyPressed = keyboardState.IsKeyDown(Keys.Up) || keyboardState.IsKeyDown(Keys.Space);
                bool wasJumpKeyPressed = _previousKeyboardState.IsKeyDown(Keys.Up) || _previousKeyboardState.IsKeyDown(Keys.Space);

                if(!wasJumpKeyPressed && isJumpKeyPressed)
                {

                    if (_trex.State != TrexState.Jumping)
                        _trex.BeginJump();

                }
                else if (_trex.State == TrexState.Jumping && !isJumpKeyPressed)
                {

                    _trex.CancelJump();

                }
                else if (keyboardState.IsKeyDown(Keys.Down))
                {

                    if (_trex.State == TrexState.Jumping || _trex.State == TrexState.Falling)
                        _trex.Drop();
                    else
                        _trex.Duck();

                }
                else if (_trex.State == TrexState.Ducking && !keyboardState.IsKeyDown(Keys.Down))
                {

                    _trex.GetUp();

                }

            }

            _previousKeyboardState = keyboardState;

            _isBlocked = false;

        }

        public void BlockInputTemporarily()
        {
            _isBlocked = true;
        }

    }
}
