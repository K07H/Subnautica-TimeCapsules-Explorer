using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Subnautica_TimeCapsulesExplorer
{
    /// <summary>
    /// Logique d'interaction pour TimeCapsuleWindow.xaml
    /// </summary>
    public partial class TimeCapsuleWindow : Window
    {
        private TimeCapsule _tc = null;

        public TimeCapsuleWindow()
        {
            InitializeComponent();
        }

        public TimeCapsuleWindow(TimeCapsule tc)
        {
            InitializeComponent();

            this._tc = tc;

            // Set capsule title if there is one, otherwise remove it.
            if (tc.title.Length > 0)
                this.lbl_CapsuleTitle.Content = tc.title;
            else
                rootStackPanel.Children.RemoveAt(0);

            // Set capsule image.
            string imgURL = tc.getImageUrl();
            if (imgURL.Length > 0)
                this.img_CapsuleImage.Source = new BitmapImage(new Uri(imgURL));

            // Set capsule text.
            this.tb_CapsuleText.Text = tc.text;

            // Set capsule items list.
            this.tb_CapsuleItems.Text = "TimeCapsule items: " + tc.item_list;

            // Set capsule author.
            string authorStr = "Created";
            if (tc.platform.Length > 0 && tc.platform.CompareTo("Null") != 0)
                authorStr += " on " + tc.platform;
            authorStr += " by ";
            if (tc.user_name.Length > 0)
            {
                authorStr += tc.user_name;
                if (tc.platform_user_id.Length > 0)
                    authorStr += " (ID: " + tc.platform_user_id + ")";
            }
            else if (tc.platform_user_id.Length > 0)
                authorStr += tc.platform_user_id;
            else
                authorStr += "?";
            if (tc.time_ago.Length > 0)
                authorStr += " " + tc.time_ago;
            authorStr += ".";
            this.tb_CapsuleAuthor.Text = authorStr;

            // Set capsule upvotes.
            this.tb_CapsuleUpVotes.Text = "Up votes: " + Convert.ToString(tc.votes_up);

            // Set capsule downvotes.
            this.tb_CapsuleDownVotes.Text = "Down votes: " + Convert.ToString(tc.votes_down);

            // Set capsule creation datetime.
            this.tb_CapsuleCreated.Text = "Created at: " + tc.created_at;

            // Set capsule update datetime.
            this.tb_CapsuleUpdated.Text = "Updated at: " + tc.updated_at;

            // Set capsule modification datetime.
            this.tb_CapsuleModified.Text = "Modified at: " + tc.modified_at;

            // Set capsule language.
            this.tb_CapsuleLanguage.Text = "Language: " + tc.language;

            // Set number of capsule copies found.
            this.tb_CapsuleCopies.Text = "Copies found: " + Convert.ToString(tc.copies_found);

            // Set capsule "is active?".
            this.tb_CapsuleIsActive.Text = "Is active: " + Convert.ToString(tc.is_active);
        }

        private void lbl_OfficialWebsite_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start(this._tc.getWebPageUrl());
        }
    }
}
