import styled from 'styled-components';

import { IFormPageProps } from '../FormPage';

export const FormPage = styled.div<IFormPageProps>`
  background-color: white;
  min-height: 100%;
  min-width: ${(props) => !props.bypassMinWidth && '1200px'};
  padding: 0.5em 2em 0 2em;
  margin: 0px auto;
`;
