import React from 'react';
import { useAjaxWrapper } from 'store/hooks';

import { useApiSubscriberAI } from './useApiSubscriberAI';

interface IAIController {
  analyze: (prompt: string) => Promise<string>;
}

export const useAI = (): IAIController => {
  const api = useApiSubscriberAI();
  const dispatch = useAjaxWrapper();

  return React.useMemo(
    () => ({
      analyze: async (prompt: string) => {
        const response = await dispatch<string>('ai-analyze', () => api.analyze(prompt));
        return response.data ?? '';
      },
    }),
    [api, dispatch],
  );
};
