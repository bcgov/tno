import { Footer } from 'components/footer';
import { Header } from 'components/header';

import * as styled from './LayoutStyled';

interface ILayoutProps extends React.HTMLAttributes<HTMLDivElement> {
  /**
   * Whether keycloak authClient has been set (default: false).
   * The enables using the header component that uses the keycloak authClient.
   */
  authReady?: boolean;
}

/**
 * Layout provides a div structure to organize the page.
 * @param param0 Div element attributes.
 * @returns Layout component.
 */
export const Layout: React.FC<ILayoutProps> = ({ children, authReady = false, ...rest }) => {
  return (
    <styled.Layout {...rest}>
      <Header authReady={authReady} />
      <main>{children}</main>
      <Footer />
    </styled.Layout>
  );
};
