import styled from 'styled-components';

import { IUnauthenticatedHomeProps } from '..';

export const UnauthenticatedHome = styled.div<IUnauthenticatedHomeProps>`
  position: relative;
  overflow-x: auto;
  background: ${(props) => props.theme.css.bkPrimary};
  width: 100%;
  height: 100dvh;

  .containing-row {
    overflow-x: auto;
    height: auto;
  }

  a {
    margin-bottom: 2%;
  }

  .mm-logo {
    height: 8%;
    @media (max-width: 768px) {
      height: 6%;
      right: 1%;
    }
    position: fixed;
    right: 5%;
    bottom: 1%;
    z-index: 100;
  }
  .logo-row {
    justify-content: flex-end;
    img {
    }
  }
`;
