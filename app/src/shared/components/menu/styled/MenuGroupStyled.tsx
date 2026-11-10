import styled from 'styled-components';

export const MenuGroup = styled.div`
  align-self: self-start;
  width: 100%;
  position: relative;
  z-index: 100;

  & > div {
    left: calc(100%);
    position: absolute;
    background-color: ${(props) => props.theme.css.primaryLightColor};
    border: solid 1px ${(props) => props.theme.css.primaryColor};
    min-width: 150px;
    align-items: flex-start;
    padding: 2px 5px 2px 5px;
    border-radius: 0.25em;

    div {
    }

    a {
      color: #ffffff;
      text-decoration: none;
    }

    a:hover {
      color: #ffffff;
      text-decoration: underline;
    }

    a:active {
      color: #ffffff;
    }

    a:focus {
      color: #ffffff;
    }
  }

  .up {
    bottom: 0;
  }

  .dn,
  .check {
    top: 0;
  }
`;

export default MenuGroup;
