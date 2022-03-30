import styled from 'styled-components';

export const NavBarGroup = styled.div`
  display: flex;
  flex-direction: column;
  width: 100%;
  background-color: ${(props) => props.theme.css.primaryLightColor};

  & > div:first-child {
    margin-left: 5em;
  }

  & > div:not(:first-child) {
    border-top: solid 1px #65799e;

    & > div {
      border-right: solid 1px #65799e;
    }
  }
`;
