import React from 'react';
import { Show } from 'tno-core';

import { ColleagueEdit } from './ColleagueEdit';
import { ColleagueActionEnum } from './constants/ColleagueActionEnum';
import { MyColleagues } from './MyColleagues';

export interface IMyColleaguesLandingProps {
  inFrame: boolean;
}

export const MyColleaguesLanding: React.FC<IMyColleaguesLandingProps> = ({ inFrame }) => {
  const [action, setAction] = React.useState<string>(ColleagueActionEnum.List);

  return (
    <>
      <Show visible={action === ColleagueActionEnum.List}>
        <MyColleagues
          inFrame={inFrame}
          changeAction={(action: string) => {
            setAction(action);
          }}
        />
      </Show>
      <Show visible={action === ColleagueActionEnum.Edit}>
        <ColleagueEdit
          inFrame={inFrame}
          changeAction={(action: string) => {
            setAction(action);
          }}
        />
      </Show>
    </>
  );
};
