import styled from 'styled-components';

import { IUnauthenticatedHomeProps } from '..';

export const UnauthenticatedHome = styled.div<IUnauthenticatedHomeProps>`
  background-color: ${(props) => props.backgroundColor};
  display: flex;
  border-radius: 20px;

  width: 100%;

  // add ability to control shadow
  box-shadow: 0px 0px 20px rgba(0, 0, 0, 0.15);

  // home page usage
  margin: ${(props) => props.className === 'home' && 0};
  position: ${(props) => props.className === 'home' && 'absolute'};
  top: ${(props) => props.className === 'home' && '50%'};
  left: ${(props) => props.className === 'home' && '50%'};
  -ms-transform: ${(props) => props.className === 'home' && 'translate(-50%, -50%)'};
  transform: ${(props) => props.className === 'home' && 'translate(-50%, -50%)'};

  @media only screen and (max-width: 1024px) {
    flex-direction: column;
    top: ${(props) =>
      props.className === 'home' &&
      ((props.useMobileMode && '87%') || (!props.useMobileMode && '54%'))};
  }
  @media only screen and (max-width: 912px) {
    top: ${(props) =>
      props.className === 'home' &&
      ((props.useMobileMode && '39%') || (!props.useMobileMode && '56%'))};
  }
  @media only screen and (max-width: 820px) {
    top: ${(props) =>
      props.className === 'home' &&
      ((props.useMobileMode && '46%') || (!props.useMobileMode && '56%'))};
  }
  @media only screen and (max-width: 768px) {
    top: ${(props) =>
      props.className === 'home' &&
      ((props.useMobileMode && '52%') || (!props.useMobileMode && '61%'))};
  }
  @media only screen and (max-width: 540px) {
    top: ${(props) =>
      props.className === 'home' &&
      ((props.useMobileMode && '82%') || (!props.useMobileMode && '62%'))};
  }
  @media only screen and (max-width: 414px) {
    top: ${(props) => props.className === 'home' && '70%'};
  }
  @media only screen and (max-width: 393px) {
    top: ${(props) => props.className === 'home' && '80%'};
  }
  @media only screen and (max-width: 375px) {
    top: ${(props) => props.className === 'home' && '103%'};
  }
  @media only screen and (max-width: 360px) {
    top: ${(props) => props.className === 'home' && '95%'};
  }
  @media only screen and (max-width: 280px) {
    top: ${(props) => props.className === 'home' && '141%'};
  }
  @media only screen and (min-width: 1200px) {
    width: ${(props) => props.width};
    height: ${(props) => props.height};
  }

  .system-message {
    background-color: ${(props) => props.theme.css.accentColor};
    padding: 0.5rem;
    border-radius: 0.5rem;
  }
`;
