import { FaExternalLinkAlt, FaExternalLinkSquareAlt, FaRegClone } from 'react-icons/fa';
import { useApp } from 'store/hooks';
import { Show } from 'tno-core';

import { NavigateOptions } from './constants';

/**
 * Component to choose navigation options.
 * @returns Component.
 */
export const TabControl: React.FC = () => {
  const [{ options }, { storeUserOptions }] = useApp();

  return (
    <>
      <Show visible={options?.open === undefined || options?.open === NavigateOptions.NewTab}>
        <FaExternalLinkAlt
          className="button button-link"
          title="Open in new tab"
          onClick={() => storeUserOptions?.({ ...options, open: NavigateOptions.OnPage })}
        />
      </Show>
      <Show visible={options?.open === NavigateOptions.OnPage}>
        <FaExternalLinkSquareAlt
          className="button button-link"
          title="Open on page"
          onClick={() => storeUserOptions?.({ ...options, open: NavigateOptions.TwoTabView })}
        />
      </Show>
      <Show visible={options?.open === NavigateOptions.TwoTabView}>
        <FaRegClone
          className="button button-link"
          title="Two tabs"
          onClick={() => storeUserOptions?.({ ...options, open: NavigateOptions.NewTab })}
        />
      </Show>
    </>
  );
};
