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

interface IImage {
  url: string;
  id: number;
}

export const FrontPageGallery: React.FC<IFrontPageGallery> = ({ frontpages }) => {
  const [srcUrls, setSrcUrls] = React.useState<IImage[]>([]);
  const navigateAndScroll = useNavigateAndScroll();
  const [, { stream }] = useContent();

  React.useEffect(() => {
    const srcUrls = Promise.all(
      frontpages
        .filter((fp) => fp.fileReferences?.length)
        .map(async (fp) => {
          const result = await stream(fp.fileReferences![0].path);
          return { url: result, id: fp.id };
        }),
    );
    srcUrls.then((results) => setSrcUrls(results));
  }, [frontpages, stream]);

  return (
    <styled.FrontPageGallery>
      <Row justifyContent="center" className="content">
        {srcUrls.map((s) => (
          <img
            key={s.url}
            alt={`${s.id}`}
            className="front-page"
            src={s.url}
            onClick={() => navigateAndScroll(`/view/${s.id}`)}
          />
        ))}
      </Row>
    </styled.FrontPageGallery>
  );
};
