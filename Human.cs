using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anti_war_mini_bot {
    public class Human {


        public long Chat_Id;
        public string Real_Name = "NoName";

        public string Fake_Name = "NoName";
        public int BanCount = 0;

        public bool TikTok = true;
        public bool YouTube = true;
        public bool Insta = true;

        public int indexForceAdd = -1;
        public int indexForce = 0;

        //Stan
        public enum States { NewUser = 0, NewNick = 1, SettingNet = 2, 
            Work = 3, SendMessage = 4, SendLinc = 5, SentType = 6, 
            SendTime = 7, SendComment = 8};
        public States Status_HM = States.NewUser;


        public int score = 0;
        public Human(long chat_id, string real_name) {
            Chat_Id = chat_id;
            Real_Name = real_name;
            Fake_Name = real_name;
        }

    }
}
