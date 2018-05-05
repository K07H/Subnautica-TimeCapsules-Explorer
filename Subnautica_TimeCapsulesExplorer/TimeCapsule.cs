using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Subnautica_TimeCapsulesExplorer
{
    public class TimeCapsule
    {
        // Attributes (with default getters/setters).
        public string _id { get; set; }
        public string platform { get; set; }
        public string platform_user_id { get; set; }
        public string user_name { get; set; }
        public string title { get; set; }
        public string text { get; set; }
        public string language { get; set; }
        public string items { get; set; }
        public bool is_active { get; set; }
        public int votes_up { get; set; }
        public int votes_down { get; set; }
        public int copies_found { get; set; }
        public string class_id { get; set; }
        public string modified_at { get; set; }
        public string updated_at { get; set; }
        public string created_at { get; set; }
        public string image { get; set; }
        public string time_ago { get; set; }
        public string item_list { get; set; }

        // Default constructor.
        public TimeCapsule()
        {
            this._id = "";
            this.platform = "";
            this.platform_user_id = "";
            this.user_name = "";
            this.title = "";
            this.text = "";
            this.language = "";
            this.items = "";
            this.is_active = false;
            this.votes_up = 0;
            this.votes_down = 0;
            this.copies_found = 0;
            this.class_id = "";
            this.modified_at = "";
            this.updated_at = "";
            this.created_at = "";
            this.image = "";
            this.time_ago = "";
            this.item_list = "";
        }

        // Generates human-readable string from TimeCapsule.
        public string toStr()
        {
            return "ID=[" + this._id
                + "] Platform=[" + this.platform
                + "] UserID=[" + this.platform_user_id
                + "] UserName=[" + this.user_name
                + "] Title=[" + this.title
                + "] Text=[" + this.text
                + "] Language=[" + this.language
                + "] Items=[" + this.items
                + "] IsActive=[" + this.is_active
                + "] VotesUp=[" + this.votes_up
                + "] VotesDown=[" + this.votes_down
                + "] CopiesFound=[" + this.copies_found
                + "] ClassID=[" + this.class_id
                + "] ModifiedAt=[" + this.modified_at
                + "] UpdatedAt=[" + this.updated_at
                + "] CreatedAt=[" + this.created_at
                + "] Image=[" + this.image
                + "] TimeAgo=[" + this.time_ago
                + "] ItemList=[" + this.item_list
                + "]";
        }

        public string toSimpleStr()
        {
            return "ID: " + this._id + " Author: " + this.user_name + " Created at: " + this.created_at + " Up votes: " + this.votes_up + " Down votes: " + this.votes_down + " Is active: " + Convert.ToString(this.is_active);
        }

        // Returns the TimeCapsule image full URL.
        public string getImageUrl()
        {
            return "https://s3.amazonaws.com/subnautica-unknownworlds-com/time-capsule-images/" + this.image;
        }

        // Returns the TimeCapsule official web page URL.
        public string getWebPageUrl()
        {
            return "https://subnautica.unknownworlds.com/time-capsules/" + this._id;
        }
    }
}
