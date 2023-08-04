import styled from 'styled-components';

import { IFormPageProps } from '../FormPage';

export const FormPage = styled.div<IFormPageProps>`
  background-color: white;
  min-height: fit-content;
  margin: 0px auto;

  :not(.no-padding) {
    padding: 0.5em 2em 0 2em;
  }

  div[role='rowgroup'] {
    min-height: 100px;
    max-height: calc(100vh - 400px);
    overflow-y: scroll;
    overflow-x: hidden;
  }

  p[role='alert'] {
    font-size: 0.85em;
    color: ${(props) => props.theme.css.dangerColor};
  }

  @media only screen and (max-width: 1200px) {
    width: 100%;
  }

  @media only screen and (min-width: 1201px) {
    width: 90%;
  }
`;
