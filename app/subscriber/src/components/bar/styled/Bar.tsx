import styled from 'styled-components';

export const Bar = styled.div`
  display: flex;
  flex-direction: row;
  flex-wrap: wrap;
  gap: 1rem;
  align-items: center;
  justify-items: stretch;
  background: ${(props) => props.theme.css.bkTertiary};
  padding: 0.25rem 1rem;
  min-height: 46px;
  box-shadow: ${(props) => props.theme.css.boxShadow};
  color: ${(props) => props.theme.css.fPrimaryColor};
  min-width: fit-content;

  .frm-in {
    padding: unset;
  }

  input {
    max-height: 32px;
  }
`;
