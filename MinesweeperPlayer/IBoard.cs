using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MinesweeperPlayer {
    public enum LocationState {
        Hidden,
        Flagged,
        Visible,
        Bomb
    }

    public struct LocationValue {
        public LocationState State;
        public int Value;
    }

    public interface IBoard {
        int getValue(int row, int column);
        LocationState getState(int row, int column);
        void setState(int row, int column, LocationState state);
    }
}
