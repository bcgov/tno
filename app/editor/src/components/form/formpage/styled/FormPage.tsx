import styled from 'styled-components';

import { IFormPageProps } from '../FormPage';

export const FormPage = styled.div<IFormPageProps>`
  background-color: white;
  height: ${(props) => props.height ?? '100%'};
  padding: 2em;
  width: 70em;
  min-width: fit-content;
`;
