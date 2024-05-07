import styled from 'styled-components';

export const ToggleButton = styled.div`
  display: flex;
  flex-direction: row;

  &:hover {
    color: ${(props) => props.theme.css.primaryLightColor};
    cursor: pointer;
  }
`;
