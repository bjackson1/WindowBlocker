using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace WindowBlocker
{
    public partial class BlockForm : Form
    {
        // I added a comment 

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }

        private IntPtr obscuredWindowHandle;
        private Process obscuredProcess;
        private Point obscuredWindowLocation;
        private Size obscuredWindowSize;
        private bool blockingActive = false;

        public BlockForm(IntPtr obscuredWindowHandle, Process obscuredProcess)//(string title, IntPtr obscuredWindowHandle, Process obscuredProcess, Point initialPosition, Size initialSize)
        {
            InitializeComponent();
            this.obscuredWindowHandle = obscuredWindowHandle;

            this.AllowTransparency = true;
            this.Opacity = 0.975;
            //this.TopMost = true;

            this.titleLabel.Font = SystemInformation.MenuFont;

            //this.MoveWindow();

            int windowTextLength = GetWindowTextLength(this.obscuredWindowHandle);
            StringBuilder sb = new StringBuilder(windowTextLength);
            GetWindowText(this.obscuredWindowHandle, sb, windowTextLength + 1);

            this.titleLabel.Text = sb.ToString();

            this.obscuredProcess = obscuredProcess;
            //this.Location = initialPosition;
            //this.Size = initialSize;
            
            this.Show();
        }

        void BlockForm_Move(object sender, EventArgs e)
        {
            MoveWindow(this.obscuredWindowHandle, this.Left, this.Top, this.Width, this.Height, true);
        }

        private void BlockForm_Load(object sender, EventArgs e)
        {
            //this.GetObscuredWindow();
            blockingActive = true;
            this.MoveWindow();
            this.Move += BlockForm_Move;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            //Graphics g = this.CreateGraphics();
            //TextureBrush tb = new TextureBrush(Image.FromFile( "checks.png"));
            //g.FillRectangle(tb, 0, 0, this.Width, this.Height);
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 0x84:
                    base.WndProc(ref m);
                    if ((int)m.Result == 0x1)
                        m.Result = (IntPtr)0x2;
                    return;
            }

            base.WndProc(ref m);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "Your application will be closed.  Any unsaved work will be lost.\r\n\r\nDo you want to continue?", this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
            {
                this.obscuredProcess.Exited += obscuredProcess_Exited;
                this.obscuredProcess.EnableRaisingEvents = true;
                this.obscuredProcess.Kill();
            }
        }

        void obscuredProcess_Exited(object sender, EventArgs e)
        {
            this.Exit();
        }

        private delegate void ExitDelegate();
        private void Exit()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new ExitDelegate(this.Exit), new object[] { });
            }
            else
            {
                this.Close();
            }
        }

        private void GetObscuredWindow()
        {
            RECT bounds;
            GetWindowRect(this.obscuredWindowHandle, out bounds);

            this.obscuredWindowLocation = new Point(bounds.Left, bounds.Top);
            this.obscuredWindowSize = new Size(bounds.Right - bounds.Left, bounds.Bottom - bounds.Top);
        }

        internal void MoveWindow()
        {
            this.GetObscuredWindow();

            if (this.blockingActive)
            {
                this.Size = this.obscuredWindowSize;
                this.Location = this.obscuredWindowLocation;
                //SetWindowPos(this.Handle, this.obscuredWindowHandle, this.obscuredWindowLocation.X - 5, this.obscuredWindowLocation.Y - 5, this.Width, this.Height, 0x0210);
                SetWindowPos(this.obscuredWindowHandle, this.Handle, this.Location.X, this.Location.Y, this.Width, this.Height, 0x0210);
                //this.blockingActive = false;
            }

            //SetWindowPos(this.Handle, this.obscuredWindowHandle, this.obscuredWindowLocation.X, this.obscuredWindowLocation.Y, this.Width, this.Height, 0x0210);
        }

        private Point MouseDownLocation;


        private void titleLabel_MouseDown(object sender, MouseEventArgs e)
        {
            //base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                dragOffset = this.PointToScreen(e.Location);
                var formLocation = FindForm().Location;
                dragOffset.X -= formLocation.X;
                dragOffset.Y -= formLocation.Y;
            } 
            //if (e.Button == System.Windows.Forms.MouseButtons.Left)
            //{
            //    MouseDownLocation = e.Location;
            //}
        }

        private void titleLabel_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Point newLocation = this.PointToScreen(e.Location);

                newLocation.X -= dragOffset.X;
                newLocation.Y -= dragOffset.Y;

                FindForm().Location = newLocation;
            }            //if (e.Button == System.Windows.Forms.MouseButtons.Left)
            //{
            //    this.Left = e.X + titleLabel.Left - MouseDownLocation.X;
            //    this.Top = e.Y + titleLabel.Top - MouseDownLocation.Y;
            //}
        }

        Point dragOffset;

        protected override void OnMouseDown(MouseEventArgs e)
        {

        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);


        }
    }
}
