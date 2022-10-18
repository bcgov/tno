import styled from 'styled-components';

import { IFormPageProps } from '../FormPage';

export const FormPage = styled.div<IFormPageProps>`
  background-color: white;
  min-height: 100%;
  ${(props) => (props.minWidth !== '' ? `min-width: ${props.minWidth ?? '1200px'}` : '')};
  ${(props) => (props.maxWidth !== '' ? `max-width: ${props.maxWidth ?? '1200px'}` : '')};
  padding: 0.5em 2em 0 2em;
  margin: 0px auto;
`;
