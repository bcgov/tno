import styled from 'styled-components';

import { INavBarItemProps } from '..';

export const NavBarItem = styled.div<INavBarItemProps>`
  background-color: ${(props) => (props.active ? '#65799e' : '#38598a')};
  border-top-left-radius: ${(props) => (!!props.level && props.active ? '0.25em' : 'none')};
  border-top-right-radius: ${(props) => (!!props.level && props.active ? '0.25em' : 'none')};
  border-bottom-left-radius: ${(props) => (!props.level && props.active ? '0.25em' : 'none')};
  border-bottom-right-radius: ${(props) => (!props.level && props.active ? '0.25em' : 'none')};
  color: white;
  font-weight: 500;
  padding-top: 0.5rem;
  padding: 0.5em 1em 0.5em 1em;
  text-align: center;
  cursor: pointer;

  &:hover {
    background-color: #65799e;
    border-top-left-radius: ${(props) => (!!props.level ? '0.25em' : 'none')};
    border-top-right-radius: ${(props) => (!!props.level ? '0.25em' : 'none')};
    border-bottom-left-radius: ${(props) => (!props.level ? '0.25em' : 'none')};
    border-bottom-right-radius: ${(props) => (!props.level ? '0.25em' : 'none')};
  }
`;
