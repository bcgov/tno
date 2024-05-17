import styled from 'styled-components';

export const UserProfile = styled.div`
  display: flex;
  flex-direction: row;
  gap: 1rem;
  align-items: center;
  margin-left: auto;

  .username-info {
    display: flex;
    align-items: center;
    flex-flow: nowrap;
    font-size: 1rem;
    padding: 0.25rem 0.5rem;
    border-radius: 1rem;

    &:hover {
      cursor: pointer;
    }
    > div {
      display: flex;
      flex-direction: row;
      flex-wrap: no wrap;
      gap: 0.25rem;
      align-items: center;

      > div:last-child {
        flex: 1;
      }
    }
    svg {
      color: ${(props) => props.theme.css.iconTertiaryColor};
    }

    &.impersonate {
      background: ${(props) => props.theme.css.btnRedColor};
      color: ${(props) => props.theme.css.fPrimaryInvertColor};

      svg {
        color: ${(props) => props.theme.css.fPrimaryInvertColor};
      }
    }
  }

  .logout {
    display: flex;
    flex-flow: nowrap;
    max-height: fit-content;
    font-size: 1rem;
    color: ${(props) => props.theme.css.fRedColor};

    &:hover {
      cursor: pointer;
    }

    svg {
      margin-right: 0.5em;
      margin-top: 0.25em;
    }
  }

  h1 {
    color: ${(props) => props.theme.css.defaultBlack};
    font-size: 1rem;
  }

  ul {
    list-style-type: none;
    padding-left: 1.5rem;
  }

  a {
    color: inherit;
    text-decoration: none;

    &:hover {
      color: ${(props) => props.theme.css.linkPrimaryColor};
    }
  }
`;
