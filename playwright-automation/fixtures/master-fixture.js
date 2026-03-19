import { AppPage } from "../pages/appPage";
import { EditorHomePage } from "../pages/editorHomePage";
import { HeadlinesDetailsPage } from "../pages/headlinesDetailsPage";

import { SubscriberSearchResultPage } from "../pages/subscriberSearchResultPage";
import { ReportPage } from "../pages/reportPage";
import { SubscriberMyReportPage } from "../pages/subscriberMyReportPage";
import { SubscriberNavBarPage } from "../pages/subscriberNavBarPage";
import { EditorOnlineStoryPage } from "../pages/editorOnlineStoryPage";
import { MinisterPage } from "../pages/ministerPage";
import { SettingsPage } from "../pages/settingsPage";


export class MasterFixture {

    constructor(page) {
        this.page = page;
        this.appPage = new AppPage(this.page);
        this.editorHomePage = new EditorHomePage(this.page);
        this.headlinesDetailsPage = new HeadlinesDetailsPage(this.page);
        this.subscriberSearchResultPage = new SubscriberSearchResultPage(this.page);
        this.reportPage = new ReportPage(this.page);
        this.subscriberMyReportPage = new SubscriberMyReportPage(this.page);
        this.subscriberNavBarPage = new SubscriberNavBarPage(this.page);
        this.editorOnlineStoryPage = new EditorOnlineStoryPage(this.page);
        this.ministerPage = new MinisterPage(this.page);
        this.settingsPage = new SettingsPage(this.page);    

    }
}