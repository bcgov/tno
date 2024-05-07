import styled from 'styled-components';

export const Colours = styled.div`
  &.frm-in {
    .select {
      max-height: 2rem;
      min-height: 2rem;
      height: 2rem;
      .select__control {
        max-height: 2rem;
        min-height: 2rem;
        height: 2rem;
        .select__value-container {
          max-height: 2rem;
          padding: 0 0.5rem;
        }
        .select__indicators {
          max-height: 2rem;
        }
      }
    }
  }

  .picker-overlay {
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    z-index: 1;

    > div {
      position: absolute;
      z-index: 2;
      left: calc(50% - 110px);
      top: calc(50% - 151px);
    }
  }
`;
