import styled from 'styled-components';

import { IFormPageProps } from '../FormPage';

export const FormPage = styled.div<IFormPageProps>`
  flex: 1 1 100%;
  background-color: white;

  &:not(.no-padding) {
    padding: 0.5em 2em 2em 2em;
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
`;
