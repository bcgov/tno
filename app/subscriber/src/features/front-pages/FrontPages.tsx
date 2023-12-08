import { FrontPageGallery } from 'components/front-page-gallery';
import React from 'react';
import { useContent } from 'store/hooks';
import { IContentModel } from 'tno-core';

import * as styled from './styled';

/** simple component used to display front pages on the landing page */
export const FrontPages: React.FC = () => {
  const [frontPages, setFrontPages] = React.useState<IContentModel[]>([]);
  const [, { getFrontPages }] = useContent();

  React.useEffect(() => {
    getFrontPages().then((data) => {
      setFrontPages(data.items);
    });
  }, [getFrontPages]);

  return (
    <styled.FrontPages>
      <div className="title">Front Pages</div>
      <FrontPageGallery frontpages={frontPages} />
    </styled.FrontPages>
  );
};
