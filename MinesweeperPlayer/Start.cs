using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MinesweeperPlayer {
    class Start {
        public static void Main() {
            new Player().Play(new WindowsBoard());
        }
    }
}
