import { useAppDispatch, useAppSelector } from 'store';
import { INavState, storeStatus } from 'store/slices';
import { MenuStatus } from 'tno-core';

/**
 * Manage navigation state with this hook.
 * @param initState Initial state
 * @returns Navigation state hook.
 */
export const useNavState = (initState?: INavState) => {
  const dispatch = useAppDispatch();
  const state = useAppSelector((store) => store.nav);

  /**
   * Get the navigation menu status from state.
   * @returns Navigation menu status.
   */
  const getStatus = () => {
    return state.status;
  };

  /**
   * Set the navigation menu status.
   * @param status The new menu status.
   */
  const setStatus = (status: MenuStatus) => {
    dispatch(storeStatus(status));
  };

  if (!!initState) {
    setStatus(initState.status);
  }

  return {
    getStatus,
    setStatus,
  };
};
