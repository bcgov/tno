import { IReportResultModel } from 'tno-core';

export const openPreviewInNewTab = (previewContent: IReportResultModel) => {
  const previewHTML = `
      <html>
      <head>
        <title>Report Preview</title>
      </head>
      <body>
        <h1> ${previewContent.subject}</h1>
        <div>${previewContent.body}</div>
      </body>
      </html>
    `;

  const blob = new Blob([previewHTML], { type: 'text/html' });
  const url = URL.createObjectURL(blob);
  const windowName = 'ReportPreviewWindow';
  const previewWindow = window.open(url, windowName);
  if (previewWindow) {
    previewWindow.focus();
  }
};
