import styled from 'styled-components';

export const ChartAdvanced = styled.div`
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 1rem;

  > div {
    border-radius: 0.5rem;
    border: solid 1px ${(props) => props.theme.css.tableEvenRow};
    padding: 0.25rem 0.5rem;

    textarea {
      min-height: 250px;
    }
  }
`;
