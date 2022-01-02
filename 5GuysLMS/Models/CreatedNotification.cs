using System;
using System.Collections.Generic;

namespace _5GuysLMS.Models
{
    public class CreatedNotification
    {
        // Returns a string of CreatedNotification objects rendered as HTML
        public static string RenderListAsHTML(List<CreatedNotification> cns, string queryString)
        {
            // If there are any notifications, create the HTML for them
            if (cns != null && cns.Count > 0)
            {
                string HTML = "<div class='container'><div class='row'>";
                foreach (CreatedNotification cn in cns)
                {
                    // Add the <p> tag that contains the notification information and the <span> tag that contains the red X to dismiss the notification
                    HTML += $"<p class='list-group-item notificationItem col-11'>{cn}</p><a href='{queryString}{(string.IsNullOrEmpty(queryString) ? "?" : "&")}notificationID={cn.Id}&type=created&handler=RedX' class='btn btn-danger text-white redX cursor-pointer col-1'>X</a>";
                }
                HTML += "</div></div>";
                return HTML;
            }
            // If there are no notifications, create HTML that tells the user that they are all caught up
            return "<p class='list-group-item notificationItem text-success'>No new assignments have been created. You're all caught up!</p>";
        }
        // Returns the CreatedNotification in the format DD NNNN Assignment Title - Created
        public override string ToString()
        {
            return $"{Assignment.Course.CourseDepartment} {Assignment.Course.CourseNumber} {Assignment.AssignmentTitle} - Created";
        }
        public int Id { get; set; }
        public bool IsSeen { get; set; }
        // Navigation Properties
        public Assignment Assignment { get; set; }
        public User User { get; set; }
    }
}
