import { IReportResultModel } from 'tno-core';

export const openPreviewInNewTab = (previewContent: IReportResultModel) => {
  const previewHTML = `
      <html>
      <head>
        <title>Report Preview</title>
      </head>
      <body>
        <h1>Preview Subject: ${previewContent.subject}</h1>
        <div>${previewContent.body}</div>
      </body>
      </html>
    `;

  const blob = new Blob([previewHTML], { type: 'text/html' });
  const url = URL.createObjectURL(blob);
  window.open(url, '_blank');
};
