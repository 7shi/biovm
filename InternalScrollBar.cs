using System;
using System.Windows.Forms;
using Girl.Windows.API;

namespace Girl.Windows.Forms
{
    /// <summary>
    /// ウィンドウ内部のスクロールバーをラップします。
    /// </summary>
    public class InternalScrollBar
    {
        private Control control;
        protected Win32API.SB fnBar;
        protected Win32API.WS style;
        public Win32API.WM Msg { get; protected set; }
        protected int min, max, page, value;
        public event EventHandler Scroll;
        private bool disposed = false;

        public InternalScrollBar(Control ctrl)
        {
            control = ctrl;
            control.Disposed += new EventHandler((sender, e) => disposed = true);
        }

        public Win32API.ScrollInfo GetScrollInfo(Win32API.SIF fMask)
        {
            Win32API.ScrollInfo ret = new Win32API.ScrollInfo();
            ret.cbSize = 28;
            ret.fMask = (uint)fMask;
            if (Visible) Win32API.GetScrollInfo(control.Handle, (int)fnBar, ref ret);
            return ret;
        }

        public void SetScrollInfo(Win32API.ScrollInfo si)
        {
            if (disposed) return;
            Win32API.SetScrollInfo(control.Handle, (int)fnBar, ref si, true);
        }

        public bool Visible
        {
            get
            {
                if (disposed) return false;
                int style = Win32API.GetWindowLong(control.Handle, (int)Win32API.GWL.Style);
                return (style & (int)style) != 0;
            }
        }

        public int Value
        {
            get
            {
                return GetScrollInfo(Win32API.SIF.Pos).nPos;
            }

            set
            {
                if (!Visible) return;

                Win32API.ScrollInfo si = new Win32API.ScrollInfo();
                si.cbSize = 28;
                si.fMask = (uint)Win32API.SIF.Pos;
                si.nPos = Math.Min(Math.Max(value, min), max - Math.Max(page - 1, 0));
                if (this.value == si.nPos) return;

                SetScrollInfo(si);
                this.value = si.nPos;
                if (Scroll != null) Scroll.Invoke(this, EventArgs.Empty);
            }
        }

        public int Page
        {
            get
            {
                return GetScrollInfo(Win32API.SIF.Range).nPage;
            }

            set
            {
                page = value;
                Win32API.ScrollInfo si = new Win32API.ScrollInfo();
                si.cbSize = 28;
                si.fMask = (uint)Win32API.SIF.Page;
                si.nPage = page;
                SetScrollInfo(si);
            }
        }

        public int Min
        {
            get
            {
                return GetScrollInfo(Win32API.SIF.Range).nMin;
            }

            set
            {
                min = value;
                Win32API.ScrollInfo si = new Win32API.ScrollInfo();
                si.cbSize = 28;
                si.fMask = (uint)Win32API.SIF.Range;
                si.nMin = min;
                si.nMax = max;
                SetScrollInfo(si);
            }
        }

        public int Max
        {
            get
            {
                return GetScrollInfo(Win32API.SIF.Range).nMax;
            }

            set
            {
                max = value;
                Min = min;
            }
        }

        public void MoveTop() { Value = min; }
        public void MoveBottom() { Value = max; }
        public void PageUp() { Value -= page; }
        public void PageDown() { Value += page; }
        public void LineUp() { Value--; }
        public void LineDown() { Value++; }

        public void WndProc(ref Message m)
        {
            if (m.Msg != (int)Msg) return;

            switch (m.WParam.ToInt32() & 0xffff)
            {
                case (int)Win32API.SB.Top:
                    MoveTop();
                    break;
                case (int)Win32API.SB.Bottom:
                    MoveBottom();
                    break;
                case (int)Win32API.SB.PageUp:
                    PageUp();
                    break;
                case (int)Win32API.SB.PageDown:
                    PageDown();
                    break;
                case (int)Win32API.SB.LineUp:
                    LineUp();
                    break;
                case (int)Win32API.SB.LineDown:
                    LineDown();
                    break;
                case (int)Win32API.SB.ThumbPosition:
                case (int)Win32API.SB.ThumbTrack:
                    Value = GetScrollInfo(Win32API.SIF.TrackPos).nTrackPos;
                    break;
            }
        }
    }

    /// <summary>
    /// ウィンドウ内部の横スクロールバーをラップします。
    /// </summary>
    public class InternalHScrollBar : InternalScrollBar
    {
        public InternalHScrollBar(Control ctrl)
            : base(ctrl)
        {
            fnBar = Win32API.SB.Horz;
            style = Win32API.WS.HScroll;
            Msg = Win32API.WM.HScroll;
        }
    }

    /// <summary>
    /// ウィンドウ内部の縦スクロールバーをラップします。
    /// </summary>
    public class InternalVScrollBar : InternalScrollBar
    {
        public InternalVScrollBar(Control ctrl)
            : base(ctrl)
        {
            fnBar = Win32API.SB.Vert;
            style = Win32API.WS.VScroll;
            Msg = Win32API.WM.VScroll;
        }
    }
}
