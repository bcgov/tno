import styled from 'styled-components';

export const ShareMenu = styled.div`
  /* share story button */
  .share-story {
    height: 2em;
    width: 2em;
    &:hover {
      cursor: pointer;
      transform: scale(1.1);
      color: ${(props) => props.theme.css.sideBarIconHoverColor};
    }
    color: ${(props) => props.theme.css.sideBarIconColor};
  }

  ul {
    list-style-type: none;
    padding-left: 1rem;
    cursor: pointer;
  }

  a {
    color: inherit;
    text-decoration: none;
  }

  input {
    font-size: 1em;
    border: none;
    border-bottom: 0.1rem solid;
    border-radius: 0;
    padding: 0;
    &:focus {
      box-shadow: none;
      outline: none;
    }
  }
  .share-email {
    display: flex;
    background-color: transparent;
    color: ${(props) => props.theme.css.btnBkPrimary};
    &:hover {
      cursor: pointer;
      color: ${(props) => props.theme.css.btnBkPrimary};
    }

    border: 0.1em solid ${(props) => props.theme.css.btnBkPrimary};
    height: 1.5em;
    margin-left: 0;
    min-width: fit-content;
  }
`;
