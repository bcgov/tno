import { defaultEnvelope, ILifecycleToasts, useSummon } from '../summon';
import { Settings } from '.';

/**
 * Common hook to make requests to the APi.
 * @returns CustomAxios object setup for the API.
 */
export const useApi = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
  useAuth: boolean = true,
) => {
  const summon = useSummon({ ...options, baseURL: options.baseURL ?? Settings.ApiPath }, useAuth);
  return summon;
};
