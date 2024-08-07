import styled from 'styled-components';

export const LightweightModal = styled.dialog`
  border: 1px solid ${(props) => props.theme.css.btnBkPrimary};
  border-radius: 0.5em;
  .close-button {
    background-color: ${(props) => props.theme.css.btnBkPrimary};
    margin-top: 1em;
    color: ${(props) => props.theme.css.fPrimaryInvertColor};
    border: none;
    border-radius: 0.5em;
    padding: 0.5em 1em;
    float: right;
    cursor: pointer;
  }
`;
