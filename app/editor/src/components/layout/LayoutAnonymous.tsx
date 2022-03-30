import React from 'react';
import { Footer, Header } from 'tno-core';

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
  return (
    <styled.Layout {...rest}>
      <Header name={name} />
      <div className="main-window">
        <main>{children}</main>
      </div>
      <Footer />
    </styled.Layout>
  );
};
