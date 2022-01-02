using System;
using System.Collections.Generic;

namespace _5GuysLMS.Models
{
    public class GradedNotification
    {
        // Returns the src of the notification bell image depending on if there are notifications or not
        public static string RenderNotificationSrcAsHTML(bool isNotifications)
        {
            return isNotifications ? "/notification-bell-red.png" : "/notification-bell.png";
        }

        // Returns a string of GradedNotification objects rendered as HTML
        public static string RenderListAsHTML(List<GradedNotification> gns, string queryString)
        {
            // If there are any notifications, create the HTML for them
            if (gns != null && gns.Count > 0)
            {
                string HTML = "<div class='container'><div class='row'>";
                foreach (GradedNotification gn in gns)
                {
                    // Add the <p> tag that contains the notification information and the <span> tag that contains the red X to dismiss the notification
                    HTML += $"<p class='list-group-item notificationItem col-11'>{gn}</p><a href='{queryString}{(string.IsNullOrEmpty(queryString) ? "?" : "&")}notificationID={gn.Id}&type=graded&handler=RedX' class='btn btn-danger text-white redX cursor-pointer col-1'>X</a>";
                }
                HTML += "</div></div>";
                return HTML;
            }
            // If there are no notifications, create HTML that tells the user that they are all caught up
            return "<p class='list-group-item notificationItem text-success'>No new assignments have been graded. You're all caught up!</p>";
        }
        // Returns the GradedNotification in the format DD NNNN Assignment Title - Graded
        public override string ToString()
        {
            return IsFileSubmissionNotification ?
                $"{FileSubmission.Assignment.Course.CourseDepartment} {FileSubmission.Assignment.Course.CourseNumber} {FileSubmission.Assignment.AssignmentTitle} - Graded" :
                $"{TextSubmission.Assignment.Course.CourseDepartment} {TextSubmission.Assignment.Course.CourseNumber} {TextSubmission.Assignment.AssignmentTitle} - Graded";
        }
        public int Id { get; set; }
        public bool IsSeen { get; set; }
        public bool IsFileSubmissionNotification { get; set; }
        // Navigation Properties - FileSubmission and TextSubmission should both be nullable because it can only be one
        public FileSubmission FileSubmission { get; set; }
        public TextSubmission TextSubmission { get; set; }
    }
}
