import { IReportResultModel } from 'tno-core';

export const openPreviewInNewTab = (previewContent: IReportResultModel) => {
  const previewHTML = `
    <html>
    <head>
      <title>Report Preview: ${previewContent.subject}</title>
      <style>
        body { font-family: Arial, sans-serif; margin: 20px; }
        .header { 
          background-color: #f3f3f3; 
          padding: 10px; 
          text-align: center; 
          position: relative;
        }
        .content { 
          margin-top: 20px;
          position: relative;
        }
        .close-btn {
          display: inline-block; 
          padding: 5px 15px; 
          background-color:transparent; 
          color: white; 
          text-decoration: none; 
          color: #007bff;
          border: 2px solid #007bff;
          border-radius:5px; 
          cursor: pointer;
          position: absolute;
          right: 10px;
          font-size: 16px;
        }        
        .close-btn-top {
          top: 10px;
        }
        .close-btn:hover {
          background-color: #0056b3;
        }
      </style>
    </head>
    <body>
      <div class="header">
        <h2>Report Preview: ${previewContent.subject}</h2>
        <button onclick="window.close();" class="close-btn close-btn-top">Close</button>
      </div>
      <div class="content">
        <div>${previewContent.body}</div>
      </div>
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
