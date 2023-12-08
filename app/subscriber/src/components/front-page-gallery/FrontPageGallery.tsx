import React from 'react';
import { useContent, useNavigateAndScroll } from 'store/hooks';
import { IContentModel, Row } from 'tno-core';

import * as styled from './styled';

interface IFrontPageGallery extends React.HTMLAttributes<HTMLDivElement> {
  /**
   * Content array.
   */
  frontpages: IContentModel[];
}

export const FrontPageGallery: React.FC<IFrontPageGallery> = ({ frontpages }) => {
  const [srcUrls, setSrcUrls] = React.useState<any[]>([]);
  const navigateAndScroll = useNavigateAndScroll();
  const [, { stream }] = useContent();

  React.useEffect(() => {
    setSrcUrls([]);
    if (!!frontpages) {
      frontpages.forEach((x) => {
        if (x.fileReferences?.length) {
          stream(x.fileReferences[0].path).then((result) => {
            setSrcUrls((srcUrls) => [...srcUrls, { url: result, id: x.id }]);
          });
        }
      });
    }
  }, [frontpages, stream]);

  return (
    <styled.FrontPageGallery>
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
    </styled.FrontPageGallery>
  );
};
