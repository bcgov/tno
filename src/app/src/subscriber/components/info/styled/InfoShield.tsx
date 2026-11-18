import styled from 'styled-components';

export const InfoShield = styled.div`
  color: ${(props) => props.theme.css.iconPrimaryColor};
  padding: 0.5em;
  svg {
    height: 1.25em;
    width: 1.25em;
  }
  position: absolute;
  bottom: 0;
  .react-tooltip {
    background-color: ${(props) => props.theme.css.bkPrimary};
    .tooltip-link {
      color: ${(props) => props.theme.css.fPrimaryColor};
      text-decoration: none;
    }
  }

  &:hover {
    cursor: pointer;
  }
`;
