import styled from 'styled-components';

export const Menu = styled.div`
  padding-left: 2em;
  background-color: ${(props) => props.theme.css.primaryLightColor};
  color: white;

  .navbar {
    padding: 0;

    > .container,
    .container-fluid {
      > .navbar-brand {
        color: white;
        padding: 0 0.25em;
        &:hover {
          background-color: #65799e;
          border-top-left-radius: 0.25em;
          border-top-right-radius: 0.25em;
          border-bottom-left-radius: 0.25em;
          border-bottom-right-radius: 0.25em;
        }
      }

      > .navbar-collapse {
        > .navbar-nav {
          gap: 0.5rem;

          > .nav-item {
            > a {
              color: white;
            }
            &:hover {
              background-color: #65799e;
              border-top-left-radius: 0.25em;
              border-top-right-radius: 0.25em;
              border-bottom-left-radius: 0.25em;
              border-bottom-right-radius: 0.25em;
            }
          }
        }
      }
    }
    .navbar-toggler {
      border-color: white;
      > .navbar-toggler-icon {
        background-image: url("data:image/svg+xml,<svg viewBox='0 0 30 30' xmlns='http://www.w3.org/2000/svg'><path stroke='white' stroke-width='2' stroke-linecap='round' stroke-miterlimit='10' d='M4 7h22M4 15h22M4 23h22'/></svg>");
      }
    }
  }
`;
