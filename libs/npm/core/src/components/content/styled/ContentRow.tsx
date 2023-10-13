import styled from 'styled-components';

export const ContentRow = styled.div`
  display: flex;
  flex-direction: row;
  flex-wrap: nowrap;
  align-items: center;
  gap: 0.5rem;
  padding: 0.15rem;

  & div:nth-child(3) {
    flex: 1 1 100%;
  }

  .frm-in {
    padding: 0;

    .number {
      text-align: right;
    }

    input {
      padding: 0.15rem 0.25rem;
    }
  }
`;
