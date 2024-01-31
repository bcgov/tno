import styled from 'styled-components';

export const ViewOptions = styled.div`
  display: flex;
  align-items: center;
  h3 {
    margin-bottom: 0;
  }
  .checkbox {
    font-size: 1rem;
    input {
      align-self: center;
    }
  }
  .group-by {
    input {
      align-self: center;
    }
    label {
      font-size: 1rem;
      align-self: center;
      margin-left: 0.25rem;
    }
  }

  .gear {
    margin-left: 1.5rem;
    cursor: pointer;
    color: ${({ theme }) => theme.css.btnBkPrimary};
    height: 1.5rem;
    width: 1.5rem;
  }
`;
