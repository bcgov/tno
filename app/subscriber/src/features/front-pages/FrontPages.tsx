import React from 'react';
import { useContent, useNavigateAndScroll } from 'store/hooks';
import { IContentModel, Row } from 'tno-core';

import * as styled from './styled';

/** simple component used to display front pages on the landing page */
export const FrontPages = () => {
  const [frontPages, setFrontPages] = React.useState<IContentModel[]>([]);
  const [srcUrls, setSrcUrls] = React.useState<any[]>([]);
  const [, { getFrontPages, stream }] = useContent();
  const navigateAndScroll = useNavigateAndScroll();

  React.useEffect(() => {
    getFrontPages().then((data) => {
      setFrontPages(data.items);
    });
  }, [getFrontPages]);

  React.useEffect(() => {
    if (!!frontPages) {
      frontPages.forEach((x) => {
        if (x.fileReferences?.length) {
          stream(x.fileReferences[0].path).then((result) => {
            setSrcUrls((srcUrls) => [...srcUrls, { url: result, id: x.id }]);
          });
        }
      });
    }
  }, [frontPages, stream]);

  return (
    <styled.FrontPages>
      <div className="title">Front Pages</div>
      <Row justifyContent="center" className="content">
        {srcUrls.map((s) => (
          <img
            key={s.url}
            alt={s.id}
            className="front-page"
            src={s.url}
            onClick={() => navigateAndScroll(`/view/${s.id}`)}
          />
        ))}
      </Row>
    </styled.FrontPages>
  );
};
