import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useContent } from 'store/hooks';
import { IContentModel } from 'tno-core';

import * as styled from './styled';

/** simple component used to display front pages on the landing page */
export const FrontPages = () => {
  const [frontPages, setFrontPages] = React.useState<IContentModel[]>([]);
  const [srcUrls, setSrcUrls] = React.useState<any[]>([]);
  const navigate = useNavigate();
  const [, { getFrontPages, stream }] = useContent();
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
            const mimeType = 'img/png';
            setSrcUrls((srcUrls) => [
              ...srcUrls,
              { url: `data:${mimeType};base64,` + result, id: x.id },
            ]);
          });
        }
      });
    }
  }, [frontPages, stream]);

  return (
    <styled.FrontPages>
      <div className="title">Front Pages</div>
      <div className="content">
        {srcUrls.map((s) => (
          <img
            key={s.url}
            alt={s.id}
            className="front-page"
            src={s.url}
            onClick={() => navigate(`/view/${s.id}`)}
          />
        ))}
      </div>
    </styled.FrontPages>
  );
};
