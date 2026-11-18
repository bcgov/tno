import styled from 'styled-components';

export const Bar = styled.div<{ $vanilla?: boolean }>`
  display: flex;
  flex-direction: row;
  flex-wrap: wrap;
  gap: 1rem;
  align-items: center;
  justify-items: stretch;
  background: ${(props) =>
    props.$vanilla ? props.theme.css.bkStaticGray : props.theme.css.bkPrimary};
  padding: 0.25rem 1rem;
  min-height: ${(props) => (props.$vanilla ? 'unset' : '46px')};
  padding: ${(props) => (props.$vanilla ? '0.5em' : 'unset')}
  box-shadow: ${(props) => !props.$vanilla && props.theme.css.boxShadow};
  color: ${(props) => props.theme.css.fPrimaryColor};
  min-width: fit-content;

  .frm-in {
    padding: unset;
  }

  input {
    max-height: 32px;
  }
`;
