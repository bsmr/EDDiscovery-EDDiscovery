﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EDDiscovery.Actions
{
    public partial class ActionVariableForm : Form
    {
        public Dictionary<string, string> result;      // only on OK

        public class Group
        {
            public Panel panel;
            public ExtendedControls.TextBoxBorder var;
            public ExtendedControls.TextBoxBorder value;
            public ExtendedControls.ButtonExt del;
        };

        List<Group> groups;
        EDDiscovery2.EDDTheme theme;

        int panelmargin = 3;
        int panelwidth = 800;
        const int vscrollmargin = 10;

        public ActionVariableForm()
        {
            InitializeComponent();
            groups = new List<Group>();
            CancelButton = buttonCancel;
            AcceptButton = buttonOK;
        }

        public void Init(string t, EDDiscovery2.EDDTheme th, Dictionary<string, string> vbs)
        {
            theme = th;

            bool winborder = theme.ApplyToForm(this, SystemFonts.DefaultFont);
            statusStripCustom.Visible = panelTop.Visible = panelTop.Enabled = !winborder;
            this.Text = label_index.Text = t;

            if (vbs != null)
            {
                foreach (KeyValuePair<string, string> ky in vbs)
                {
                    CreateEntry(ky.Key, ky.Value);
                }
            }
        }

        public Group CreateEntry(string var, string value)
        {
            Group g = new Group();

            g.panel = new Panel();
            g.panel.BorderStyle = BorderStyle.FixedSingle;

            g.var = new ExtendedControls.TextBoxBorder();
            g.var.Size = new Size(120, 24);
            g.var.Location = new Point(panelmargin, panelmargin);
            g.var.Text = var;
            g.panel.Controls.Add(g.var);

            g.value = new ExtendedControls.TextBoxBorder();
            g.value.Size = new Size(350, 24);
            g.value.Location = new Point(g.var.Location.X + g.var.Width + 8, panelmargin);
            g.value.Text = value;
            g.panel.Controls.Add(g.value);

            g.del = new ExtendedControls.ButtonExt();
            g.del.Size = new Size(24, 24);
            g.del.Location = new Point(g.value.Location.X + g.value.Width + 8, panelmargin);
            g.del.Text = "X";
            g.del.Tag = g;
            g.del.Click += Del_Clicked;
            g.panel.Controls.Add(g.del);

            groups.Add(g);

            panelVScroll1.Controls.Add(g.panel);
            theme.ApplyToControls(g.panel, SystemFonts.DefaultFont);

            panelwidth = g.del.Location.X + g.del.Width + 8;

            FixUpGroups();

            return g;
        }

        void FixUpGroups()      // fixes and positions groups.
        {
            int y = panelmargin;

            foreach (Group g in groups)
            {
                g.panel.Size = new Size(panelwidth, 32);
                g.panel.Location = new Point(panelmargin, y);
                y += g.panel.Height + 6;
            }

            buttonMore.Location = new Point(panelmargin, y);

            Rectangle screenRectangle = RectangleToScreen(this.ClientRectangle);
            int titleHeight = screenRectangle.Top - this.Top;

            y += buttonMore.Height + titleHeight + ((panelTop.Enabled) ? (panelTop.Height + statusStripCustom.Height) : 8) + 16 + panelOK.Height;

            this.MinimumSize = new Size(panelwidth + vscrollmargin * 2 + panelVScroll1.ScrollBarWidth + 8, y);
            this.MaximumSize = new Size(Screen.FromControl(this).WorkingArea.Width, Screen.FromControl(this).WorkingArea.Height);
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            result = new Dictionary<string, string>();
            foreach( Group g in groups)
            {
                string var = g.var.Text;
                string value = g.value.Text;
                if (var.Length > 0)
                    result[var] = value;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void Del_Clicked(object sender, EventArgs e)
        {
            ExtendedControls.ButtonExt b = sender as ExtendedControls.ButtonExt;
            Group g = (Group)b.Tag;

            g.panel.Controls.Clear();
            panelVScroll1.Controls.Remove(g.panel);
            groups.Remove(g);
            Invalidate(true);
            FixUpGroups();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void buttonMore_Click(object sender, EventArgs e)
        {
            CreateEntry("", "");
        }

        #region Window Control

        public const int WM_MOVE = 3;
        public const int WM_SIZE = 5;
        public const int WM_MOUSEMOVE = 0x200;
        public const int WM_LBUTTONDOWN = 0x201;
        public const int WM_LBUTTONUP = 0x202;
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int WM_NCLBUTTONUP = 0xA2;
        public const int WM_NCMOUSEMOVE = 0xA0;
        public const int HT_CLIENT = 0x1;
        public const int HT_CAPTION = 0x2;
        public const int HT_LEFT = 0xA;
        public const int HT_RIGHT = 0xB;
        public const int HT_BOTTOM = 0xF;
        public const int HT_BOTTOMRIGHT = 0x11;
        public const int WM_NCL_RESIZE = 0x112;
        public const int HT_RESIZE = 61448;
        public const int WM_NCHITTEST = 0x84;

        // Mono compatibility
        private bool _window_dragging = false;
        private Point _window_dragMousePos = Point.Empty;
        private Point _window_dragWindowPos = Point.Empty;

        private IntPtr SendMessage(int msg, IntPtr wparam, IntPtr lparam)
        {
            Message message = Message.Create(this.Handle, msg, wparam, lparam);
            this.WndProc(ref message);
            return message.Result;
        }

        protected override void WndProc(ref Message m)
        {
            bool windowsborder = this.FormBorderStyle == FormBorderStyle.Sizable;
            // Compatibility movement for Mono
            if (m.Msg == WM_LBUTTONDOWN && (int)m.WParam == 1 && !windowsborder)
            {
                int x = unchecked((short)((uint)m.LParam & 0xFFFF));
                int y = unchecked((short)((uint)m.LParam >> 16));
                _window_dragMousePos = new Point(x, y);
                _window_dragWindowPos = this.Location;
                _window_dragging = true;
                m.Result = IntPtr.Zero;
                this.Capture = true;
            }
            else if (m.Msg == WM_MOUSEMOVE && (int)m.WParam == 1 && _window_dragging)
            {
                int x = unchecked((short)((uint)m.LParam & 0xFFFF));
                int y = unchecked((short)((uint)m.LParam >> 16));
                Point delta = new Point(x - _window_dragMousePos.X, y - _window_dragMousePos.Y);
                _window_dragWindowPos = new Point(_window_dragWindowPos.X + delta.X, _window_dragWindowPos.Y + delta.Y);
                this.Location = _window_dragWindowPos;
                this.Update();
                m.Result = IntPtr.Zero;
            }
            else if (m.Msg == WM_LBUTTONUP)
            {
                _window_dragging = false;
                _window_dragMousePos = Point.Empty;
                _window_dragWindowPos = Point.Empty;
                m.Result = IntPtr.Zero;
                this.Capture = false;
            }
            // Windows honours NCHITTEST; Mono does not
            else if (m.Msg == WM_NCHITTEST)
            {
                base.WndProc(ref m);

                if ((int)m.Result == HT_CLIENT)
                {
                    int x = unchecked((short)((uint)m.LParam & 0xFFFF));
                    int y = unchecked((short)((uint)m.LParam >> 16));
                    Point p = PointToClient(new Point(x, y));

                    if (p.X > this.ClientSize.Width - statusStripCustom.Height && p.Y > this.ClientSize.Height - statusStripCustom.Height)
                    {
                        m.Result = (IntPtr)HT_BOTTOMRIGHT;
                    }
                    else if (p.Y > this.ClientSize.Height - statusStripCustom.Height)
                    {
                        m.Result = (IntPtr)HT_BOTTOM;
                    }
                    else if (p.X > this.ClientSize.Width - 5)       // 5 is generous.. really only a few pixels gets thru before the subwindows grabs them
                    {
                        m.Result = (IntPtr)HT_RIGHT;
                    }
                    else if (p.X < 5)
                    {
                        m.Result = (IntPtr)HT_LEFT;
                    }
                    else if (!windowsborder)
                    {
                        m.Result = (IntPtr)HT_CAPTION;
                    }
                }
            }
            else
            {
                base.WndProc(ref m);
            }
        }

        private void label_index_MouseDown(object sender, MouseEventArgs e)
        {
            ((Control)sender).Capture = false;
            SendMessage(WM_NCLBUTTONDOWN, (System.IntPtr)HT_CAPTION, (System.IntPtr)0);
        }

        private void panel_minimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void panel_close_Click(object sender, EventArgs e)
        {
            Close();
        }

        #endregion


    }
}
