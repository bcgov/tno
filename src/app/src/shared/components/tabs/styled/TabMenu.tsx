import styled from 'styled-components';

export const TabMenu = styled.div`
  width: 100%;
  border-bottom: solid 2px ${(props) => props.theme.css.primaryColor};

  & > div div:not(:last-child) {
    margin-right: 0.5em;
  }
`;
