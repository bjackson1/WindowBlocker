using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace NHS.NeoIdentityAgent.UI
{
    public partial class WatermarkTextBox : TextBox
    {
        private const uint ECM_FIRST = 0x1500;
        private const uint EM_SETCUEBANNER = ECM_FIRST + 1;

        private string watermarkText = string.Empty;

        private bool showWatermarkWithFocus = false;

        public string WatermarkText
        {
            get
            {
                return this.watermarkText;
            }
            set
            {
                if (!this.watermarkText.Equals(value))
                {
                    this.watermarkText = value;
                    this.UpdateWatermark();
                    this.OnWatermarkTextChanged(EventArgs.Empty);
                }
            }
        }


        //private void SetWatermark(string watermarkText)
        //{
        //    SendMessage(this.Handle, EM_SETCUEBANNER, 0, watermarkText);
        //}

        public event EventHandler WatermarkTextChanged;

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnWatermarkTextChanged(EventArgs e)
        {
            EventHandler handler = WatermarkTextChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public bool ShowWatermarkWithFocus
        {
            get
            {
                return this.showWatermarkWithFocus;
            }
            set
            {
                if (this.showWatermarkWithFocus != value)
                {
                    this.showWatermarkWithFocus = value;
                    this.UpdateWatermark();
                    this.OnShowWatermarkTextWithFocusChanged(EventArgs.Empty);
                }
            }
        }

        public event EventHandler ShowWatermarkTextWithFocusChanged;

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnShowWatermarkTextWithFocusChanged(EventArgs e)
        {
            EventHandler handler = ShowWatermarkTextWithFocusChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            UpdateWatermark();

            base.OnHandleCreated(e);
        }



        private void UpdateWatermark()
        {
            // If the handle isn't yet created, 
            // this will be called when it is created
            if (this.IsHandleCreated)
            {
                NativeMethods.SendMessage(/*new HandleRef(this, this.Handle)*/ this.Handle, EM_SETCUEBANNER, (this.showWatermarkWithFocus) ? new IntPtr(1) : IntPtr.Zero, this.watermarkText);
            }
        }
    }
}
