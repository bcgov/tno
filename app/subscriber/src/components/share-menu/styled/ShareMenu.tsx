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
`;
