import React from 'react';
import styled from 'styled-components';

const Body = styled.div`
  /* Report templates emit sibling blocks; defer their layout and paint until nearby.
     Keep the HTML mounted so find-in-page, selection and report anchors still work. */
  @media screen {
    > div,
    > div > div,
    > div > ul,
    > p,
    > ul,
    > section,
    article,
    .article {
      content-visibility: auto;
      contain-intrinsic-block-size: auto 300px;
    }
  }
`;

/** Shared by the editable preview and standalone report instance view. */
export const ReportBody = React.memo(({ html }: { html: string }) => (
  <Body className="preview-body" dangerouslySetInnerHTML={{ __html: html }} />
));
