import React from 'react';
import { useKeycloakWrapper } from 'tno-core';

import { BrowserLogin, MobileLogin } from '.';
import * as styled from './styled';

export interface IUnauthenticatedHomeProps {
  /**
   * choose the height of the text box
   */
  height?: string;
  /**
   * choose the width of the text box
   */
  width?: string;
  /**
   * choose the background colour of the checkbox
   */
  backgroundColor?: string;
  /**
   * pass children to the text box
   */
  children?: React.ReactNode;
  /**
   * className for the component
   */
  className?: string;

  useMobileMode?: boolean;
}

/**
 * TextBox provides a customizable container to place informative information in.
 * @param height Div height element attribute.
 * @param width Div width element attribute.
 * @param backgroundColor Div background-color element attribute.
 * @returns TextBox component.
 */
export const UnauthenticatedHome: React.FC<IUnauthenticatedHomeProps> = (props) => {
  const keycloak = useKeycloakWrapper();

  const [isMobile, setIsMobile] = React.useState(false);

  const login = (hint?: string) => {
    const params = new URLSearchParams(window.location.search);
    const redirectTo = params.get('redirectTo');
    params.delete('redirectTo');
    const redirect = !redirectTo
      ? encodeURI(window.location.href)
      : encodeURI(
          window.location.href.split('?')[0].replace(window.location.pathname, redirectTo) +
            '?' +
            params.toString(),
        );
    keycloak.instance.login({ idpHint: hint, redirectUri: redirect, scope: 'openid' });
  };

  React.useEffect(() => {
    const mobileMediaQuery = window.matchMedia('(max-width: 767px)'); // Adjust the breakpoint as needed

    const handleMobileChange = (event: any) => {
      setIsMobile(event.matches);
    };

    mobileMediaQuery.addEventListener('change', handleMobileChange);
    setIsMobile(mobileMediaQuery.matches);

    return () => {
      mobileMediaQuery.removeEventListener('change', handleMobileChange);
    };
  }, []);

  return (
    <styled.UnauthenticatedHome>
      {isMobile ? <MobileLogin login={login} /> : <BrowserLogin login={login} />}
    </styled.UnauthenticatedHome>
  );
};
