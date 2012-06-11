using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;

namespace MinesweeperPlayer {
    class ImageProcessor : IDisposable {

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        const int SM_CXSIZEFRAME = 32;
        const int BLACK_THRESHOLD = 300;
        const int EDGE_SEARCH_OFFSET = 4;
        const int MIN_SQUARE_WIDTH = 18;

        Bitmap image;
        int leftEdge;
        int topEdge;
        int rightEdge;
        int bottomEdge;
        int[] columnBoundaries;
        int[] rowBoundaries;

        public ImageProcessor(Bitmap image) {
            this.image = image;
        }

        private bool IsBlack(Color color) {
            return color.B < 150 && color.G < 100 && color.R < 100;
        }

        private bool IsLight(Color color) {
            return color.B > 150 && color.G > 150 && color.R > 150;
        }

        private void GetEdges() {
            int borderWidth = GetSystemMetrics(SM_CXSIZEFRAME);
            image.Save("blah.png");
            leftEdge = borderWidth;
            int midHeight = image.Height / 2;
            while (IsBlack(image.GetPixel(leftEdge, midHeight))) {
                leftEdge++;
            }
            while (!IsBlack(image.GetPixel(leftEdge, midHeight))) {
                leftEdge++;
            }
            topEdge = midHeight;
            while (IsBlack(image.GetPixel(leftEdge, topEdge))) {
                topEdge--;
            }
            topEdge++;
            bottomEdge = midHeight;
            while (IsBlack(image.GetPixel(leftEdge, bottomEdge))) {
                bottomEdge++;
            }
            bottomEdge--;
            rightEdge = leftEdge;
            while (IsBlack(image.GetPixel(rightEdge, topEdge))) {
                rightEdge++;
            }
            rightEdge--;
        }

        int GetLocationNumber(int top, int left) {
            int currentVal = 0;
            int minDiff = 20;
            for (int i = 1; i <= 8; i++) {
                int[,] mask = NumMasks.Masks[i];
                int diff = 0;
                for (int x = 3; x < MIN_SQUARE_WIDTH; x++) {
                    for (int y = 3; y < MIN_SQUARE_WIDTH; y++) {
                        var color = image.GetPixel(left + x, top + y);
                        int pixelVal = IsLight(color) ? 0 : 1;
                        diff += Math.Abs(mask[x, y] - pixelVal);
                    }
                }
                if (diff < minDiff) {
                    currentVal = i;
                    minDiff = diff;
                }
            }
            return currentVal;
        }

        bool IsLocationFlagged(int top, int left, int right, int bottom) {
            for (int y = top; y <= bottom; y++) {
                var color = image.GetPixel((left + right) / 2, y);
                if (color.R > 230 && color.G < 50) {
                    return true;
                }
            }
            return false;
        }

        LocationState GetLocationState(int top, int left, int right, int bottom) {
            var colorLowerRight = image.GetPixel(right - 2, bottom - 2);
            if (colorLowerRight.B - colorLowerRight.R > 60) {
                if (IsLocationFlagged(top, left, right, bottom)) {
                    return LocationState.Flagged;
                } else {
                    return LocationState.Hidden;
                }
            }

            int brightPixelCount = 0;
            for (int x = left; x <= right; x++) {
                var color = image.GetPixel(x, (top + bottom) / 2);
                if (IsLight(color)) {
                    brightPixelCount++;
                }
            }
            if (brightPixelCount < 8) {
                return LocationState.Bomb;
            }

            return LocationState.Visible;
        }

        LocationValue GetValue(int xSquare, int ySquare) {
            int left = columnBoundaries[xSquare];
            int right = columnBoundaries[xSquare + 1];
            int top = rowBoundaries[ySquare];
            int bottom = rowBoundaries[ySquare + 1];
            var state = GetLocationState(top, left, right, bottom);
            int value = 0;
            if (state == LocationState.Visible) {
                value = GetLocationNumber(top, left);
            }

            return new LocationValue {
                State = state,
                Value = value
            };
        }

        private int GetRowBrightness(int y) {
            int brightness = 0;
            for (int x = leftEdge; x <= rightEdge; x++) {
                var color = image.GetPixel(x, y);
                brightness += color.R;
                brightness += color.G;
                brightness += color.B;
            }
            return brightness;
        }

        private int GetColumnBrightness(int x) {
            int brightness = 0;
            for (int y = topEdge; y <= bottomEdge; y++) {
                var color = image.GetPixel(x, y);
                brightness += color.R;
                brightness += color.G;
                brightness += color.B;
            }
            return brightness;
        }

        private void UpdateRowBoundaries() {
            List<int> rowBoundaries = new List<int>();
            int nextRowBoundary = topEdge;
            int currentBrightness = GetRowBrightness(nextRowBoundary);
            while (nextRowBoundary <= bottomEdge) {
                int nextBrightness = GetRowBrightness(nextRowBoundary + 1);
                if (nextBrightness > currentBrightness) {
                    rowBoundaries.Add(nextRowBoundary);
                    nextRowBoundary += MIN_SQUARE_WIDTH;
                    currentBrightness = GetRowBrightness(nextRowBoundary);
                }
                else {
                    nextRowBoundary++;
                    currentBrightness = nextBrightness;
                }
            }
            if (bottomEdge - rowBoundaries[rowBoundaries.Count - 1] > 3) {
                rowBoundaries.Add(bottomEdge);
            } 
            this.rowBoundaries = rowBoundaries.ToArray();
        }

        private void UpdateColumnBoundaries() {
            List<int> columnBoundaries = new List<int>();
            int nextColumnBoundary = leftEdge;
            int currentBrightness = GetColumnBrightness(nextColumnBoundary);
            while (nextColumnBoundary <= rightEdge) {
                int nextBrightness = GetColumnBrightness(nextColumnBoundary + 1);
                if (nextBrightness > currentBrightness) {
                    columnBoundaries.Add(nextColumnBoundary);
                    nextColumnBoundary += MIN_SQUARE_WIDTH;
                    currentBrightness = GetColumnBrightness(nextColumnBoundary);
                }
                else {
                    nextColumnBoundary++;
                    currentBrightness = nextBrightness;
                }
            }
            if (rightEdge - columnBoundaries[columnBoundaries.Count - 1] > 3) {
                columnBoundaries.Add(rightEdge);
            }
            this.columnBoundaries = columnBoundaries.ToArray();
        }

        public LocationValue[,] Process() {
            GetEdges();
            UpdateColumnBoundaries();
            UpdateRowBoundaries();

            LocationValue[,] retVals = new LocationValue[columnBoundaries.Length - 1, rowBoundaries.Length - 1];
            for (int y = 0; y < retVals.GetLength(1); y++) {
                for (int x = 0; x < retVals.GetLength(0); x++) {
                    retVals[x, y] = GetValue(x, y);
                    Console.WriteLine(String.Format("{0},{1}: {2} {3}", x, y, retVals[x,y].State, retVals[x,y].Value));
                }
            }
            Console.WriteLine(String.Format("{0} {1}", leftEdge, topEdge));
            return retVals;
        }

        public void Dispose() {
            image.Dispose();
        }
    }
}
