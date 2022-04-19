import React from 'react';
import { Footer, Header, Loading } from 'tno-core';

import * as styled from './styled';

interface ILayoutAnonymousProps extends React.HTMLAttributes<HTMLDivElement> {
  /**
   * Site name to display in header.
   */
  name: string;
}

/**
 * LayoutAnonymous provides a div structure to organize the page for anonymous users.
 * @param param0 Div element attributes.
 * @returns Layout component.
 */
export const LayoutAnonymous: React.FC<ILayoutAnonymousProps> = ({ name, children, ...rest }) => {
  const [isLoading] = React.useState(false);

  return (
    <styled.Layout {...rest}>
      <Header name={name} />
      <div className="main-window">
        <main>
          {children}
          {isLoading && <Loading />}
        </main>
      </div>
      <Footer />
    </styled.Layout>
  );
};
