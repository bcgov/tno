import 'react-reflex/styles.css';

import React from 'react';
import { ReflexContainer, ReflexElement } from 'react-reflex';
import { useContent } from 'store/hooks';
import { Page, Row } from 'tno-core';

import { CondensedContentForm } from '../form';
import { ContentFilter } from '../list-view';
import { IContentListFilter } from '../list-view/interfaces';
import { makeFilter } from '../list-view/utils';
import { ListViewPanel } from './ListViewPanel';
import * as styled from './styled';

export const CombinedView: React.FC<any> = () => {
  const [{ filterAdvanced }, { findContent }] = useContent();
  const [updated, setUpdated] = React.useState(false);

  const fetch = React.useCallback(
    async (filter: IContentListFilter) => {
      try {
        const data = await findContent(
          makeFilter({
            ...filter,
            ...filterAdvanced,
          }),
        );
        const page = new Page(data.page - 1, data.quantity, data?.items, data.total);

        // setPage(page);
        return page;
      } catch (error) {
        // TODO: Handle error
        throw error;
      }
    },
    [filterAdvanced, findContent],
  );

  return (
    <styled.CombinedView>
      <Row className="filter-area">
        <ContentFilter setUpdated={setUpdated} updated={updated} search={fetch} />
      </Row>
      <ReflexContainer orientation="vertical">
        <ReflexElement className="left-pane">
          <ListViewPanel />
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
