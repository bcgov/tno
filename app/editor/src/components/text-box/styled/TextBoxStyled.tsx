import styled from 'styled-components';

import { ITextBoxProps } from '..';

export const TextBox = styled.div<ITextBoxProps>`
  background-color: ${(props) => props.backgroundColor};
  width: ${(props) => props.width};
  display: flex;
  height: ${(props) => props.height};
  border-radius: 20px;

  // add ability to control shadow
  box-shadow: 0px 0px 20px rgba(0, 0, 0, 0.15);

  // home page usage
  margin: ${(props) => props.className === 'home' && 0};
  position: ${(props) => props.className === 'home' && 'absolute'};
  top: ${(props) => props.className === 'home' && '50%'};
  left: ${(props) => props.className === 'home' && '50%'};
  -ms-transform: ${(props) => props.className === 'home' && 'translate(-50%, -50%)'};
  transform: ${(props) => props.className === 'home' && 'translate(-50%, -50%)'};
`;

export default TextBox;
