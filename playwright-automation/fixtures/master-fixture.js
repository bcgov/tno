import { AppPage } from "../pages/appPage";
import { EditorHomePage } from "../pages/editorHomePage";
import { HeadlinesDetailsPage } from "../pages/headlinesDetailsPage";

import { SubscriberSearchResultPage } from "../pages/subscriberSearchResultPage";
import { ReportPage } from "../pages/reportPage";
import { SubscriberMyReportPage } from "../pages/subscriberMyReportPage";
import { SubscriberNavBarPage } from "../pages/subscriberNavBarPage";
import { NotificationalertPage } from "../pages/notificationalertPage";
import { AddMediaPage } from "../pages/addMediaPage";
import { PreviewPage } from "../pages/previewPage";
import { AddProductPage } from "../pages/addProductPage";

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
        this.notificationalertPage = new NotificationalertPage(this.page);
        this.addMediaPage = new AddMediaPage(this.page);
    
        this.previewPage = new PreviewPage(this.page);
        this.addProductPage = new AddProductPage(this.page);

    }
}