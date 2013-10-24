using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace WindowBlocker
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll")]
        static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr
           hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess,
           uint idThread, uint dwFlags);


        Dictionary<IntPtr, BlockForm> windows = new Dictionary<IntPtr, BlockForm>();

        /// <summary>
        /// An object's Name property has changed. The system sends this event for the following user interface elements: check box, cursor, list-view control, push button, radio button, status bar control, tree view control, and window object. Server applications send this event for their accessible objects.
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/dd318066(v=vs.85).aspx
        /// </summary>
        private const uint EventObjectNameChange = 0x800C;

        /// <summary>
        /// A hidden object is shown. The system sends this event for the following user interface elements: caret, cursor, and window object. Server applications send this event for their accessible objects.<para/>Clients assume that when this event is sent by a parent object, all child objects are already displayed. Therefore, server applications do not send this event for the child objects.<para/>Hidden objects include the STATE_SYSTEM_INVISIBLE flag; shown objects do not include this flag. The EVENT_OBJECT_SHOW event also indicates that the STATE_SYSTEM_INVISIBLE flag is cleared. Therefore, servers do not send the EVENT_STATE_CHANGE event in this case.
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/dd318066(v=vs.85).aspx
        /// </summary>
        private const uint EventObjectShow = 0x8002;

        /// <summary>
        /// The callback function is not mapped into the address space of the process that generates the event. Because the hook function is called across process boundaries, the system must queue events. Although this method is asynchronous, events are guaranteed to be in sequential order. For more information, see Out-of-Context Hook Functions.
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/dd373611(v=vs.85).aspx
        /// </summary>
        private const uint WinEventOutOfContext = 0;

        /// <summary>
        /// Copies the text that corresponds to a window into a buffer provided by the caller.
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/ms632627(v=vs.85).aspx
        /// </summary>
        private const uint WMGetText = 0x000D;

        /// <summary>
        /// Determines the length, in characters, of the text associated with a window.
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/ms632628(v=vs.85).aspx
        /// </summary>
        private const int WMGetTextLength = 0x000E;

        /// <summary>
        /// Need to ensure delegate is not collected while we're using it, storing it in a class field is simplest way to do this.
        /// </summary>
        private WinEventDelegate procDelegate;

        private delegate void WinEventDelegate(IntPtr windowHandleEventHook, uint eventType, IntPtr windowHandle, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        const uint EVENT_SYSTEM_DRAGDROPSTART = 0x000E;

        const uint EVENT_SYSTEM_DRAGDROPEND = 0x000F;

        const uint EVENT_SYSTEM_MOVESIZEEND = 0x000B;

        const uint EVENT_OBJECT_LOCATIONCHANGE = 0x800B; // hwnd ID idChild is moved/sized item

        protected override void OnMaximizedBoundsChanged(EventArgs e)
        {
            //base.OnMaximizedBoundsChanged(e);
        }


        private IntPtr eventHook = IntPtr.Zero;


        public Form1()
        {
            InitializeComponent();

            this.procDelegate = new WinEventDelegate(WinEventProc);
            this.eventHook = SetWinEventHook(EVENT_OBJECT_LOCATIONCHANGE, EVENT_OBJECT_LOCATIONCHANGE, IntPtr.Zero, procDelegate, 0, 0, WinEventOutOfContext);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            BlockForm bf;

            foreach (string proc in this.textBox1.Lines)
            {
                if (!string.IsNullOrEmpty(proc))
                {
                    foreach (int pid in this.GetRunningProcesses(this.textBox1.Lines))
                    {
                        Process p = Process.GetProcessById(pid);

                        IntPtr hwnd = p.MainWindowHandle;

                        p.han

                        if (!this.windows.ContainsKey(hwnd))
                        {
                            bf = new BlockForm(hwnd, p);

                            this.windows.Add(hwnd, bf);
                        }
                    }
                }
            }
        }

        private List<int> GetRunningProcesses(string[] processes)
        {
            List<int> ret = new List<int>();
            int pid;

            ManagementObjectSearcher mos = new ManagementObjectSearcher("select * from Win32_Process");
            ManagementObjectCollection moc = mos.Get();

            foreach (ManagementObject mo in moc)
            {
                if (processes.Contains(mo["name"].ToString(), StringComparer.InvariantCultureIgnoreCase))
                {
                    if (int.TryParse(mo["ProcessId"].ToString(), out pid))
                    {
                        if (pid > 4)
                        {
                            ret.Add(pid);
                        }
                    }
                }
            }
            
            return ret;
        }

        private void WinEventProc(IntPtr windowHandleEventHook, uint eventType, IntPtr windowHandle, int idObject, int idChild, uint eventThread, uint dwmsEventTime)
        {
            Console.WriteLine(windowHandle.ToInt32());
            // filter out non-HWND namechanges... (eg. items within a listbox)
            if (idObject != 0 || idChild != 0)
            {
                return;
            }

            BlockForm bf;
            //Console.WriteLine(string.Format("{0} == {1}", windowHandle, windowHandleEventHook));

            if (this.windows.TryGetValue(windowHandle, out bf))
            {
                bf.MoveWindow();
            }
            
            //string windowTitle = GetControlText(windowHandle);

            //if (detectionStrings.ContainsKey(windowTitle))
            //{
            //    SetForegroundWindow(windowHandle);
            //}
        }



    }
}
