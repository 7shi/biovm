using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Girl.Windows.Forms;

namespace BioVM
{
    public class ScrollBox : Control
    {
        public InternalVScrollBar VScrollBar;
        public InternalHScrollBar HScrollBar;

        public ScrollBox()
        {
            var style
                = ControlStyles.Selectable
                | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer;
            SetStyle(style, true);
            VScrollBar = new InternalVScrollBar(this);
            VScrollBar.Scroll += new EventHandler((sender, e) => Invalidate());
            HScrollBar = new InternalHScrollBar(this);
            HScrollBar.Scroll += new EventHandler((sender, e) => Invalidate());
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            VScrollBar.Page = (int)(ClientSize.Height / Font.GetHeight());
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            VScrollBar.Value -= e.Delta / 120;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Handled) return;

            switch (e.KeyCode)
            {
                case Keys.Home:
                    VScrollBar.MoveTop();
                    break;
                case Keys.End:
                    VScrollBar.MoveBottom();
                    break;
                case Keys.PageUp:
                    VScrollBar.PageUp();
                    break;
                case Keys.PageDown:
                    VScrollBar.PageDown();
                    break;
                case Keys.Up:
                    VScrollBar.LineUp();
                    break;
                case Keys.Down:
                    VScrollBar.LineDown();
                    break;
                case Keys.Left:
                    HScrollBar.LineUp();
                    break;
                case Keys.Right:
                    HScrollBar.LineDown();
                    break;
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == (int)VScrollBar.Msg)
                VScrollBar.WndProc(ref m);
            base.WndProc(ref m);
        }

        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Down:
                case Keys.Right:
                case Keys.Up:
                case Keys.Left:
                    return true;
            }
            return base.IsInputKey(keyData);
        }
    }
}
