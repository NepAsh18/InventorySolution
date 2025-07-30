document.addEventListener('DOMContentLoaded', function () {
    initSidebar();
    if (isAuthenticated) initNotificationSystem();
    initDataTables();
});

function initSidebar() {
    const sidebar = document.getElementById('sidebar');
    const sidebarCollapse = document.getElementById('sidebarCollapse');
    const sidebarOverlay = document.getElementById('sidebarOverlay');
    const mobileToggle = document.querySelector('.mobile-sidebar-toggle');

    if (!sidebar || !sidebarCollapse) return;

    sidebarCollapse.addEventListener('click', function () {
        const isCollapsed = !sidebar.classList.contains('collapsed');
        sidebar.classList.toggle('collapsed', isCollapsed);
        document.cookie = `sidebarCollapsed=${isCollapsed}; path=/; max-age=${60 * 60 * 24 * 30}`;
    });

    if (mobileToggle) {
        mobileToggle.addEventListener('click', function () {
            sidebar.classList.add('show');
            if (sidebarOverlay) sidebarOverlay.style.display = 'block';
        });
    }

    if (sidebarOverlay) {
        sidebarOverlay.addEventListener('click', function () {
            sidebar.classList.remove('show');
            this.style.display = 'none';
        });
    }
}

function initNotificationSystem() {
    if (!notificationEndpoints) {
        console.error('Notification endpoints not defined');
        return;
    }

    const updateNotifications = function () {
        // Update notifications dropdown
        fetch(notificationEndpoints.getNotifications)
            .then(response => {
                if (!response.ok) throw new Error('Network response was not ok');
                return response.text();
            })
            .then(data => {
                const container = document.getElementById('notificationContainer');
                if (container) container.innerHTML = data;
            })
            .catch(error => {
                console.error('Error loading notifications:', error);
            });

        // Update badge count
        fetch(notificationEndpoints.getNotificationCount)
            .then(response => {
                if (!response.ok) throw new Error('Network response was not ok');
                return response.text();
            })
            .then(count => {
                const badge = document.getElementById('notificationBadge');
                if (badge) {
                    badge.textContent = count;
                    badge.style.display = count && count !== '0' ? 'block' : 'none';
                }
            })
            .catch(error => {
                console.error('Error loading notification count:', error);
            });
    };

    // Event delegation for notifications
    document.addEventListener('click', function (e) {
        // Mark all as read
        if (e.target.classList.contains('mark-all-read')) {
            e.preventDefault();
            fetch(notificationEndpoints.markAllAsRead, {
                method: 'POST',
                headers: {
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                }
            })
                .then(() => updateNotifications())
                .catch(err => console.error('Error marking all as read:', err));
        }

        // Mark single as read
        const notificationItem = e.target.closest('.notification-item');
        if (notificationItem) {
            e.preventDefault();
            const notificationId = notificationItem.dataset.id;

            if (notificationId) {
                fetch(`${notificationEndpoints.markAsRead}/${notificationId}`, {
                    method: 'POST',
                    headers: {
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                    }
                })
                    .then(() => {
                        // Update after marking as read
                        updateNotifications();
                        // Follow the link
                        window.location.href = notificationItem.href;
                    })
                    .catch(err => console.error('Error marking as read:', err));
            }
        }
    });

    // Initial update
    updateNotifications();
    setInterval(updateNotifications, 30000); // Poll every 30 seconds
}

function initDataTables() {
    if (typeof $ === 'function' && $.fn.DataTable) {
        $('table').each(function () {
            if (!$.fn.DataTable.isDataTable(this)) {
                $(this).DataTable({
                    pageLength: 10,
                    lengthChange: true,
                    searching: true,
                    info: true,
                    responsive: true
                });
            }
        });
    }
}