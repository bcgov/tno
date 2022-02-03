import styled from 'styled-components';

import { ITabProps } from '.';

export const Tab = styled.button<ITabProps>`
  background-color: inherit;
  float: left;
  border: 1px solid #DDDDDD;
  box-sizing: border-box;
  border-radius: 4px 4px 0px 0px;
  cursor: pointer;
  padding: 14px 16px;
  transition: 0.3s;
  font-size: 16px;
  color: ${(props) => (props.active ? '#313132' : '#606060')};
  border-bottom: ${(props) => props.active && 'none'};
  font-weight: ${(props) => props.active && '700'};
}
`;

export default Tab;
