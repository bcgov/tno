import 'react-reflex/styles.css';

import React from 'react';
import { ReflexContainer, ReflexElement } from 'react-reflex';

import { CondensedContentForm } from '../form';
import { ListViewPanel } from './ListViewPanel';
import * as styled from './styled';

export const CombinedView: React.FC<any> = () => {
  const [updated, setUpdated] = React.useState(false);

  return (
    <styled.CombinedView>
      <ReflexContainer orientation="vertical">
        <ReflexElement className="left-pane">
          <ListViewPanel updated={updated} setUpdated={setUpdated} />
        </ReflexElement>

        {/* TODO: Responsive table and form for resize, can save preferences to cache */}
        {/* <ReflexSplitter /> */}

        <ReflexElement maxSize={1325} minSize={1000} className="right-pane">
          <CondensedContentForm setUpdated={setUpdated} />
        </ReflexElement>
      </ReflexContainer>
    </styled.CombinedView>
  );
};
