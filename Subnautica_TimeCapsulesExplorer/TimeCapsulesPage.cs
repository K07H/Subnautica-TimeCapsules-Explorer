using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Subnautica_TimeCapsulesExplorer
{
    public class TimeCapsulesPage
    {
        public int _pageID = -1;
        public List<TimeCapsule> _tcpCapsules = new List<TimeCapsule>();

        private TimeCapsulesPage() { }
        public TimeCapsulesPage(int pageID)
        {
            this._pageID = pageID;
        }

        public List<TimeCapsule> getTimeCapsulesByID(string ID)
        {
            List<TimeCapsule> res = new List<TimeCapsule>();
            foreach (TimeCapsule tc in this._tcpCapsules)
            {
                if (tc._id.CompareTo(ID) == 0)
                    res.Add(tc);
            }
            return res;
        }

        public List<TimeCapsule> getTimeCapsulesByUserName(string userName)
        {
            List<TimeCapsule> res = new List<TimeCapsule>();
            foreach (TimeCapsule tc in this._tcpCapsules)
            {
                if (tc.user_name.CompareTo(userName) == 0)
                    res.Add(tc);
            }
            return res;
        }

        public List<TimeCapsule> getTimeCapsulesByUserID(string userID)
        {
            List<TimeCapsule> res = new List<TimeCapsule>();
            foreach (TimeCapsule tc in this._tcpCapsules)
            {
                if (tc.platform_user_id.CompareTo(userID) == 0)
                    res.Add(tc);
            }
            return res;
        }

        public List<TimeCapsule> getTimeCapsulesContainingText(string text)
        {
            List<TimeCapsule> res = new List<TimeCapsule>();
            foreach (TimeCapsule tc in this._tcpCapsules)
            {
                if (tc.text.Contains(text))
                    res.Add(tc);
            }
            return res;
        }
    }
}
