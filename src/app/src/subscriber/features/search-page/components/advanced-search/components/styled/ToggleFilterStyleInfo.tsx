import styled from 'styled-components';

export const ToggleFilterStyleInfo = styled.div`
  svg {
    cursor: pointer;
    color: ${(props) => props.theme.css.btnBkPrimary};
    margin-right: 0.25em;
  }

  .row {
    max-height: 2.5em;
  }
`;
