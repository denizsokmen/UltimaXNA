﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using UltimaXNA.Input;

namespace UltimaXNA.UILegacy
{
    internal delegate void MouseButtonEvent(int x, int y, MouseButtons button);
    internal delegate void MouseEvent(int x, int y);

    public class Control : iControl
    {
        bool _enabled = false;
        bool _visible = false;
        bool _isInitialized = false;
        bool _isDisposed = false;
        public bool Enabled { get { return _enabled; } set { _enabled = value; } }
        public bool Visible { get { return _visible; } set { _visible = value; } }
        public bool IsInitialized { get { return _isInitialized; } set { _isInitialized = value; } }
        public bool IsDisposed { get { return _isDisposed; } set { _isDisposed = value; } }
        public bool IsMovable = false;

        bool _handlesMouseInput = false;
        public bool HandlesMouseInput { get { return _handlesMouseInput; } set { _handlesMouseInput = value; } }
        bool _handlesKeyboardFocus = false;
        public bool HandlesKeyboardFocus { get { return _handlesKeyboardFocus; } set { _handlesKeyboardFocus = value; } }

        protected bool _renderFullScreen = false;

        internal MouseButtonEvent OnMouseClick;
        internal MouseButtonEvent OnMouseDoubleClick;
        internal MouseButtonEvent OnMouseDown;
        internal MouseButtonEvent OnMouseUp;
        internal MouseEvent OnMouseOver;
        internal MouseEvent OnMouseOut;

        float _inputMultiplier = 1.0f;
        public float InputMultiplier
        {
            set { _inputMultiplier = value; }
            get
            {
                if (_renderFullScreen)
                    return _inputMultiplier;
                else
                    return 1.0f;
            }
        }

        int _page = 0;
        public int Page { get { return _page; } set { _page = value; } }
        int _activePage = 0; // we always draw _activePage and Page 0.
        public int ActivePage
        {
            get { return _activePage; }
            set
            {
                _activePage = value;
                // Clear the current keyboardfocus if we own it and it's page != 0
                // If the page = 0, then it will still exist so it should maintain focus.
                if (_manager.KeyboardFocusControl != null)
                {
                    if (_controls.Contains(_manager.KeyboardFocusControl))
                    {
                        if (_manager.KeyboardFocusControl.Page == 0)
                            _manager.AnnounceNewKeyboardHandler(_manager.KeyboardFocusControl);
                        else
                            _manager.AnnounceNewKeyboardHandler(null);
                    }
                }
                // When you SET ActivePage to something, it announces to the inputmanager that there may be newly popped up
                // text boxes that want keyboard input.
                foreach (Control c in _controls)
                {
                    if (c.HandlesKeyboardFocus && (c.Page == 0 || c.Page == _activePage))
                    {
                        _manager.AnnounceNewKeyboardHandler(c);
                    }
                }
            }
        }

        Rectangle _area = Rectangle.Empty;
        Vector2 _position = Vector2.Zero;
        public int X { get { return (int)(_position.X); } set { _position.X = (int)value; } }
        public int Y { get { return (int)(_position.Y); } set { _position.Y = (int)value; } }
        public int Width
        {
            get { return _area.Width; }
            set
            {
                _area.Width = (int)value;
            }
        }
        public int Height
        {
            get { return _area.Height; }
            set
            {
                _area.Height = (int)value;
            }
        }
        public Vector2 Position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;
            }
        }
        public Vector2 Size
        {
            get { return new Vector2(_area.Width, _area.Height); }
            set
            {
                _area.Width = (int)value.X;
                _area.Height = (int)value.Y;
            }
        }
        public Rectangle Area
        {
            get { return _area; }
        }

        protected Control _owner = null;
        public Control Owner { get { return _owner; } }
        protected UIManager _manager = null;
        protected List<Control> _controls = null;

        protected string getTextEntry(int entryID)
        {
            foreach (Control c in _controls)
            {
                if (c.GetType() == typeof(Gumplings.TextEntry))
                {
                    Gumplings.TextEntry g = (Gumplings.TextEntry)c;
                    if (g.EntryID == entryID)
                        return g.Text;
                }
            }
            return string.Empty;
        }

#if DEBUG
        protected Texture2D _debugTexture;
#endif

        public Control(Control owner, int page)
        {
            _owner = owner;
            _page = page;
        }

        public void Initialize(UIManager manager)
        {
            _manager = manager;
            _isInitialized = true;
            _isDisposed = false;
        }

        public virtual void Dispose()
        {
            if (_controls != null)
            {
                foreach (Control c in _controls)
                {
                    c.Dispose();
                }
            }
            _isDisposed = true;
        }

        public Control[] HitTest(Vector2 position)
        {
            List<Control> focusedControls = new List<Control>();

            // offset the mouse position if we are rendering full screen...
            position /= InputMultiplier;

            // If we're owned by something, make sure we increment our hitArea to show this.
            if (_owner != null)
            {
                position.X -= _owner.X;
                position.Y -= _owner.Y;
            }

            bool inBounds = Area.Contains((int)position.X, (int)position.Y);
            if (inBounds)
            {
                if (_hitTest((int)position.X - X, (int)position.Y - Y))
                {
                    // FIXME!!!
                    // This MAY double include nested controls that can handle input... :(
                    // Since I have not nested controls yet, I have no way of knowing, but it looks suspect.
                    if (this.HandlesMouseInput)
                        focusedControls.Insert(0, this);
                    if (_controls != null)
                    {
                        foreach (Control c in _controls)
                        {
                            if ((c.Page == 0) || (c.Page == ActivePage))
                            {
                                Control[] c1 = c.HitTest(position);
                                if (c1 != null)
                                {
                                    for (int i = c1.Length - 1; i >= 0; i--)
                                    {
                                        focusedControls.Insert(0, c1[i]);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (focusedControls.Count == 0)
                return null;
            else
                return focusedControls.ToArray();
        }

        protected virtual bool _hitTest(int x, int y)
        {
            return true;
        }

        virtual public void Update(GameTime gameTime)
        {
            if (!_isInitialized)
                return;

            // update our area X and Y to reflect any movement.
            _area.X = X;
            _area.Y = Y;

            if (_controls != null)
            {
                foreach (Control c in _controls)
                {
                    if (!c.IsInitialized)
                        c.Initialize(_manager);
                    c.Update(gameTime);
                }

                List<Control> disposedControls = new List<Control>();
                foreach (Control c in _controls)
                {
                    if (c.IsDisposed)
                        disposedControls.Add(c);
                }
                foreach (Control c in disposedControls)
                {
                    _controls.Remove(c);
                }
            }
        }

        virtual public void Draw(ExtendedSpriteBatch spriteBatch)
        {
            if (!_isInitialized)
                return;

#if DEBUG
            // DEBUG_DrawBounds(spriteBatch);
#endif
        
            if (_controls != null)
            {
                foreach (Control c in _controls)
                {
                    if ((c.Page == 0) || (c.Page == ActivePage))
                    {
                        if (c.IsInitialized)
                        {
                            c.Position += Position;
                            c.Draw(spriteBatch);
                            c.Position -= Position;
                        }
                    }
                }
            }
        }

#if DEBUG
        void DEBUG_DrawBounds(ExtendedSpriteBatch spriteBatch)
        {
            if (_debugTexture == null)
            {
                Color[] data = new Color[] { Color.White };

                _debugTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                _debugTexture.SetData<Color>(data);
            }

            int Hue = 31;
            if (_manager.MouseOverControl == this)
                Hue = 11;

            Rectangle drawArea = _area;
            if (_owner == null)
            {
                _area.X -= X;
                _area.Y -= Y;
            }
            spriteBatch.Draw(_debugTexture, new Rectangle(_area.X, _area.Y, _area.Width, 1), Hue, false);
            spriteBatch.Draw(_debugTexture, new Rectangle(_area.X, _area.Y + _area.Height - 1, _area.Width, 1), Hue, false);
            spriteBatch.Draw(_debugTexture, new Rectangle(_area.X, _area.Y, 1, _area.Height), Hue, false);
            spriteBatch.Draw(_debugTexture, new Rectangle(_area.X + _area.Width - 1, _area.Y, 1, _area.Height), Hue, false);
        }
#endif

        public virtual void ActivateByButton(int buttonID)
        {
            if (_owner != null)
                _owner.ActivateByButton(buttonID);
        }

        public virtual void ActivateByHREF(string href)
        {
            if (_owner != null)
                _owner.ActivateByHREF(href);
        }

        public virtual void ActivateByKeyboardReturn(int textID, string text)
        {
            if (_owner != null)
                _owner.ActivateByKeyboardReturn(textID, text);
        }

        public virtual void ChangePage(int pageIndex)
        {
            if (_owner != null)
                _owner.ChangePage(pageIndex);
        }

        public void MouseDown(Vector2 position, MouseButtons button)
        {
            lastClickPosition = position;
            int x = (int)position.X - X - ((_owner != null) ? _owner.X : 0);
            int y = (int)position.Y - Y - ((_owner != null) ? _owner.Y : 0);
            _mouseDown(x, y, button);
            if (OnMouseDown != null)
                OnMouseDown(x, y, button);
        }

        public void MouseUp(Vector2 position, MouseButtons button)
        {
            int x = (int)position.X - X - ((_owner != null) ? _owner.X : 0);
            int y = (int)position.Y - Y - ((_owner != null) ? _owner.Y : 0);
            _mouseUp(x, y, button);
            if (OnMouseUp != null)
                OnMouseUp(x, y, button);
        }

        public void MouseOver(Vector2 position)
        {
            // Does not double-click if you move your mouse more than x pixels from where you first clicked.
            if (Math.Abs(lastClickPosition.X - position.X) + Math.Abs(lastClickPosition.Y - position.Y) > 3)
                maxTimeForDoubleClick = 0.0f;

            int x = (int)position.X - X - ((_owner != null) ? _owner.X : 0);
            int y = (int)position.Y - Y - ((_owner != null) ? _owner.Y : 0);
            _mouseOver(x, y);
            if (OnMouseOver != null)
                OnMouseOver(x, y);
        }

        public void MouseOut(Vector2 position)
        {
            int x = (int)position.X - X - ((_owner != null) ? _owner.X : 0);
            int y = (int)position.Y - Y - ((_owner != null) ? _owner.Y : 0);
            _mouseOut(x, y);
            if (OnMouseOut != null)
                OnMouseOut(x, y);
        }

        float maxTimeForDoubleClick = 0f;
        Vector2 lastClickPosition;

        public void MouseClick(Vector2 position, MouseButtons button)
        {
            int x = (int)position.X - X - ((_owner != null) ? _owner.X : 0);
            int y = (int)position.Y - Y - ((_owner != null) ? _owner.Y : 0);

            bool doubleClick = false;
            if (maxTimeForDoubleClick != 0f)
            {
                if (ClientVars.TheTime <= maxTimeForDoubleClick)
                    doubleClick = true;
            }
            maxTimeForDoubleClick = ClientVars.TheTime + ClientVars.SecondsForDoubleClick;

            _mouseClick(x, y, button);
            if (OnMouseClick != null)
                OnMouseClick(x, y, button);

            if (doubleClick)
            {
                _mouseDoubleClick(x, y, button);
                if (OnMouseDoubleClick != null)
                    OnMouseDoubleClick(x, y, button);
            }
        }

        public void KeyboardInput(string keys, List<Keys> specialKeys)
        {
            _keyboardInput(keys, specialKeys);
        }

        protected virtual void _mouseDown(int x, int y, MouseButtons button)
        {

        }

        protected virtual void _mouseUp(int x, int y, MouseButtons button)
        {

        }

        protected virtual void _mouseOver(int x, int y)
        {

        }

        protected virtual void _mouseOut(int x, int y)
        {

        }

        protected virtual void _mouseClick(int x, int y, MouseButtons button)
        {

        }

        protected virtual void _mouseDoubleClick(int x, int y, MouseButtons button)
        {

        }

        protected virtual void _keyboardInput(string keys, List<Keys> specialKeys)
        {

        }

        internal void Center()
        {
            Position = new Vector2(
                (_manager.Width - Width) / 2,
                (_manager.Height - Height) / 2);
        }

        internal Color GumpColorHue(int hue, bool hueOnlyGreyPixels)
        {
            if (hue == 0)
                return Color.White;
            else
            {
                // max hue is 0xFFF, 12 bits. Pack these 12 bits into RG. B is the flag byte.
                Color c = new Color(0, 0, 0, 255);
                c.R = (byte)((hue & 0x003F) << 2);
                c.G = (byte)((hue & 0x0FC0) >> 4);
                if (hueOnlyGreyPixels)
                    c.B |= 0x1;
                return c;
            }
        }

        internal Color GumpColorReal(Color color)
        {
            if (color == Color.White)
                return Color.White;
            else
            {
                // pack the color into RGB565
                int packed = ((color.R & 0xE0) >> 3) + ((color.G & 0xF0) << 3) + ((color.B & 0xE0) << 8);

                Color c = new Color(0, 0, 0, 255);
                c.R = (byte)(packed & 0x000000FF);
                c.G = (byte)((packed & 0x0000FF00) >> 8);
                c.B |= 0x2; // flag for unpacking a color
                // if (hueOnlyGreyPixels)
                //     c.B |= 0x1;
                return c;
            }
        }

        internal void ReleaseKeyboardInput(Control c)
        {
            if (_controls != null)
            {
                int startIndex = _controls.IndexOf(c);
                for (int i = startIndex + 1; i < _controls.Count; i++)
                {
                    if (_controls[i].HandlesKeyboardFocus)
                    {
                        _manager.KeyboardFocusControl = _controls[i];
                        return;
                    }
                }
                for (int i = 0; i < startIndex; i++)
                {
                    if (_controls[i].HandlesKeyboardFocus)
                    {
                        _manager.KeyboardFocusControl = _controls[i];
                        return;
                    }
                }
            }
        }
    }
}