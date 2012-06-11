using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Threading;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace MinesweeperPlayer {
    delegate bool WindEnumCallback(IntPtr hwnd, int lParam);

    class WindowsBoard : IBoard {
        [DllImport("user32.dll", SetLastError=true)]
        private static extern IntPtr GetDC(IntPtr hdc);

        [DllImport("user32.dll")]
        private static extern IntPtr ReleaseDC(IntPtr hdc);

        [DllImport("user32.dll")]
        private static extern int SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int x, int y, int cx, int cy, uint flags);

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern IntPtr GetCurrentObject(IntPtr hdc, uint objectType);

        const int OBJ_BITMAP = 7;

        const uint SWP_NOOWNERZORDER = 0x200;
        const uint SWP_NOMOVE = 0x2;

        public WindowsBoard() {
            var minesweeperProcesses = Process.GetProcessesByName("MineSweeper");
            Process process;
            if (minesweeperProcesses.Length != 0) {
                process = minesweeperProcesses[0];
            }
            else {
                process = Process.Start(@"C:\Program Files\Microsoft Games\Minesweeper\Minesweeper.exe");
            }
            using (process) {
                Thread.Sleep(1000);
                var hWindow = process.MainWindowHandle;
                
                SetWindowPos(hWindow, new IntPtr(0), 0, 0, 50, 50, SWP_NOMOVE | SWP_NOOWNERZORDER);
               
                Thread.Sleep(1000);
                UpdateBoard(hWindow);
            }
        }

        private void UpdateBoard(IntPtr hWindow) {
            
            using (var processor = new ImageProcessor(GetImage(hWindow))) {
                processor.Process();
            }
        }

        private Bitmap GetImage(IntPtr hwnd) {
            var dc = GetDC(hwnd);
            if (dc == new IntPtr(0)) {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            try {
                var hBitmap = GetCurrentObject(dc, OBJ_BITMAP);
                if (hBitmap == new IntPtr(0)) {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
                return Bitmap.FromHbitmap(hBitmap);
            }
            finally {
                ReleaseDC(dc);
            }
        }

        private bool WindowCallback(IntPtr hwnd, int lParam) {
            GetImage(hwnd);
            return true;
        }

        public int getValue(int row, int column) {
            throw new NotImplementedException();
        }

        public LocationState getState(int row, int column) {
            throw new NotImplementedException();
        }

        public void setState(int row, int column, LocationState state) {
            throw new NotImplementedException();
        }
    }
}
