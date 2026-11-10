import styled from 'styled-components';

export const Menu = styled.nav`
  display: flex;
  flex-direction: column;
  flex-grow: 0;
  padding: 2px;
  background-color: rgb(${(props) => props.theme.css.primaryLightColorRgb}, 0.85);
  color: #ffffff;
  align-items: flex-end;
  border-right: solid 1px ${(props) => props.theme.css.primaryLightColorRgb};

  svg.icon {
    width: 38px;
    border: solid 1px #ffffff;
    padding: 5px;
    border-radius: 0.25rem;
    cursor: pointer;

    &:hover {
      border-color: ${(props) => props.theme.css.primaryColor};
      background-color: #ffffff;

      path {
        stroke: ${(props) => props.theme.css.primaryColor};
      }
    }
  }

  button {
    text-align: left;
    list-style-type: none;
    display: block;
    width: 100%;
    margin: 0px;
    margin: 0px;
    color: #ffffff;
    text-decoration: none;
    background-color: transparent;
    border: none;
    cursor: pointer;

    div {
      margin: 0px;
      padding: 5px 0px;
    }

    svg {
      margin-left: 2px;
      margin-right: 10px;
    }

    &:hover {
      color: #ffffff;
      text-decoration: underline;
      background-color: ${(props) => props.theme.css.primaryLightColor};
      border-radius: 0.25em;
      svg {
        fill: ${(props) => props.theme.css.primaryColor};
      }
    }

    &:active {
      color: #ffffff;
    }

    &:visited {
      color: #ffffff;
    }

    &:focus {
      color: #ffffff;
    }
  }
`;
