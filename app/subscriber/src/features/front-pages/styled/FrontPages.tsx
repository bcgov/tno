import styled from 'styled-components';
import { Col } from 'tno-core';

export const FrontPages = styled(Col)`
  @media (min-width: 1702px) {
    max-width: fit-content;
    min-width: 35em;
  }

  background-color: ${(props) => props.theme.css.lightGray};
  img {
    padding: 0.5em;
    cursor: pointer;
    &:hover {
      filter: brightness(70%);
      -webkit-filter: brightness(70%);
      -webkit-transition: all 1s ease;
      -moz-transition: all 1s ease;
      -o-transition: all 1s ease;
      -ms-transition: all 1s ease;
      transition: all 1s ease;
    }
  }
  .front-page {
    @media (min-width: 1702px) {
      width: 8em;
      height: 8em;
    }

    @media (max-width: 1702px) {
      height: 10em;
      width: 10em;
    }

    object-fit: cover;
    object-position: 0% 0%;
  }
`;
