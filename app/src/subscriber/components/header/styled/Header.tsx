import styled from 'styled-components';

import env from '../env.json';

export const Header = styled.div`
  display: flex;
  flex-direction: row;
  gap: 1rem;
  padding: 0.75rem;

  background-color: ${(props) => {
    if (env.dev.includes(window.location.hostname))
      return props.theme.css.developmentBackgroundColor;
    else if (env.test.includes(window.location.hostname))
      return props.theme.css.testBackgroundColor;
    else return props.theme.css.bkMain;
  }};

  @media (max-width: 700px) {
    .mm-logo {
      display: none;
    }
  }

  @media (min-width: 700px) {
    .mm-logo-no-text {
      display: none;
    }
  }

  .logo-container {
    padding-left: 0;
    @media (max-width: 700px) {
      width: fit-content;
    }
    @media (min-width: 700px) {
      width: 15em;
    }
    display: flex;
  }
`;
