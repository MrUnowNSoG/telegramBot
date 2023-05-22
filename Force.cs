using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anti_war_mini_bot {
    public class Force {
        public string ForceMain;
        public string Commentar;

        public int index = 0;
        public int countWin = 0;
        public int countProblem = 0;

        public bool statsForseWin = false;
        public bool statsProblem = false;

        public long idHuman;

        public enum TypeForce { TikTok = 0, YouTube = 1, Insta = 2, Telegram = 3,  Other = 4};
        public TypeForce typeForce = TypeForce.Other;

        public Force(string force, int index, long idHm) {
            ForceMain = force;
            this.index = index;
            idHuman = idHm;
        }

        public void TestWin(int count) {
            if (this.countWin >= count) statsForseWin = true;
            else statsForseWin = false;
        }

        public bool BanTest(int count) {
            if (this.countProblem >= count) {
                statsProblem = true;
                return true;
            } else { 
                statsProblem = false;
                return false;
            }
        }
    }
}
