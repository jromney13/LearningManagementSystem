$(() => {
    var isPopoverOpen = false; // Keeps track of whether or not the popover is open
    $("[data-toggle='popover']").popover(); // Enables the popovers

    $("html").on("click", (e) => {
        // If the bell image is the target and the popover is not open, open it up
        if ($("#imgNotification").is(e.target) && !isPopoverOpen) {
            $("[data-toggle='popover']").popover("show");
            isPopoverOpen = true;
        }
        else {
            let isNotificationItemClicked = false; // Keeps track of whether or not a notification item was clicked
            notificationItems = $(".notificationItem");

            // Find out if one of the notification items was clicked
            for (let i = 0; i < notificationItems.length; i++) {
                if (isPopoverOpen && notificationItems[i] === e.target) {
                    isNotificationItemClicked = true;
                }
            }
            // If anything other than a notification item was clicked, close the popover
            if (!isNotificationItemClicked) {
                $("[data-toggle='popover']").popover("hide");
                isPopoverOpen = false;
            }
        }
    });
});
