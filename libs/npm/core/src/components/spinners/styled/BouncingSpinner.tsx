import styled from 'styled-components';

export const BouncingSpinner = styled.div`
  @keyframes bouncing-loader {
    from {
      opacity: 0.1;
      transform: translate3d(0, -4px, 0);
    }
    to {
      transform: translate3d(0, +8px, 0);
    }
  }

  display: flex;
  justify-content: center;

  & > div {
    width: 16px;
    height: 16px;
    margin: 0;
    background: #8385aa;
    border-radius: 50%;
    animation: bouncing-loader 0.6s infinite alternate;

    &:first-child {
      margin-left: 0.75em;
    }
  }

  & > div:nth-child(2) {
    animation-delay: 0.2s;
  }

  & > div:nth-child(3) {
    animation-delay: 0.4s;
  }
`;
