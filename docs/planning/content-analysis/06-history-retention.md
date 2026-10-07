# Report and Notification History Retention

Purge report and notification history at a configured age. Independent of the other phases. There is
no purge today; history grows indefinitely, dominated by the full email HTML stored in each
instance's `body`.

## Decisions

| Topic | Decision |
| --- | --- |
| Report retention | 90 days by default. |
| Notification retention | 30 days by default. |
| Configuration | `setting` table, editable at runtime in Admin → Settings; `0` disables a purge. |
| Protection | Age-based, but always keep the report instances that report logic depends on. |
| Runner | The scheduler service, once a day, through a new event schedule type. |
| Expired links | Emailed "view on web" links to a purged report show a "report expired" page. |
| Report scope | Report instances, evening overview (AV) instances, unsent draft instances, and Phase 1 cached AI results. |
| Private stories | Not purged. Subscriber-authored private content stays when its instances are purged (today's behavior on instance delete). |
| Notification window | Keep each notification's history for the longest of the retention, its filter's date window, and the notification service's published-before offset. |

## Settings

| Setting | Default |
| --- | --- |
| `ReportRetentionDays` | 90 |
| `NotificationRetentionDays` | 30 |

Seeded by migration PostUp SQL and added to `AdminConfigurableSettingNames`.

## Runner

- Add `EventScheduleType.PurgeHistory` and seed one daily event schedule, editable like other
  schedules.
- The scheduler forwards it to the event-handler service, as it does `CleanFolder`.
- The event-handler calls the admin purge endpoints. Each purge deletes in batches with
  `ExecuteDelete`, following the automation run prune
  (`libs/net/dal/Services/AutomationRunService.cs:96-104`), and logs counts per table.
- A dry-run endpoint returns the counts a purge would delete, for the settings page and testing.

## Report purge

Per report and owner, compute a protected set:

- the latest instance (sent or not);
- the most recent sent instances, keeping
  `max(largest IncludePreviousReports across the report's AI sections, 10 when any section uses
  RemoveDuplicateTitles3Days, otherwise 2)`.

This protects previous-instance AI input, `CopyPriorInstance`, `ClearOnStartNewReport=false`
carry-over, `OnlyNewContent`, `ExcludeHistorical`, `ExcludeReports`, duplicate-title removal, and
dashboard status — including weekly and monthly reports whose instances all exceed the age.

Outside the protected set, delete:

- sent instances with `sent_on` older than the retention;
- unsent instances with `created_on` older than the retention.

Deleting an instance cascades to `report_instance_content`, `user_report_instance`, and the
Phase 1 cached AI results. Content rows are untouched.

Evening overview: delete `av_overview_instance` rows with `published_on` older than the report
retention, cascading to sections, section items, and `user_av_overview_instance`.

## Notification purge

Per notification, the cutoff is `now − max(NotificationRetentionDays, filter date window,
IgnoreContentPublishedBeforeOffset)`:

- The filter date window comes from the notification's filter settings (`DateOffset`). A filter
  with a fixed `StartDate` keeps instances since that date.
- This keeps the history that `ResendOption.Never` and filter-based "already sent" exclusion rely
  on, so a purge cannot cause a resend.

Delete `notification_instance` rows older than the cutoff. Subscriptions (`user_notification`,
`user_content_notification`) are never purged.

## Expired report page

- The subscriber "view on web" route (`report/instances/:id/view`) shows a "report expired" page
  when the instance no longer exists, stating that reports are kept for the configured number of
  days.
- For a signed-in subscriber with access to the report, the page links to its latest instance.
- The API returns a distinct not-found response for a missing instance so the page can tell it
  apart from other errors.
- The evening overview page shows the same message for a purged date.

## Indexes

EF migration adding indexes that support date-only cutoffs:

- `report_instance (sent_on)` and `report_instance (created_on)`;
- `notification_instance (sent_on)`.

## Tests

- Protected instances survive regardless of age, for daily, weekly, and monthly reports.
- `IncludePreviousReports`, `OnlyNewContent`, and `ExcludeHistorical` behave identically before
  and after a purge.
- Notifications with long filter windows, and the `Never` resend option, do not resend after a
  purge.
- Private content referenced by purged instances remains.
- Settings of `0` disable each purge; the dry run matches the actual deletions.
- Expired links show the expired page; missing evening overview dates show the message.
