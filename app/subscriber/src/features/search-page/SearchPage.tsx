import { SearchWithLogout } from 'components/search-with-logout';
import {
  IContentListAdvancedFilter,
  IContentListFilter,
} from 'features/content/list-view/interfaces';
import { DetermineToneIcon, makeFilter } from 'features/home/utils';
import React from 'react';
import { FaPlay, FaSave, FaStop } from 'react-icons/fa';
import { useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useApp, useContent, useUsers } from 'store/hooks';
import { useAppStore } from 'store/slices';
import { Col, IContentModel, IUserInfoModel, IUserModel, Page, Row, Show, Text } from 'tno-core';

import { Player } from './player/Player';
import * as styled from './styled';
import { trimWords } from './utils';

// Simple component to display users search results
export const SearchPage: React.FC = () => {
  const [{ filter, filterAdvanced }, { findContent }] = useContent();
  const [searchItems, setSearchItems] = React.useState<IContentModel[]>([]);
  const [activeContent, setActiveContent] = React.useState<IContentModel | null>(null);
  const [playerOpen, setPlayerOpen] = React.useState<boolean>(false);
  const [searchName, setSearchName] = React.useState<string>('');
  const navigate = useNavigate();
  const [{ userInfo }] = useApp();
  const [, store] = useAppStore();

  const api = useUsers();

  const urlParams = new URLSearchParams(window.location.search);
  const queryText = urlParams.get('queryText');
  const fetch = React.useCallback(
    async (filter: IContentListFilter & Partial<IContentListAdvancedFilter>) => {
      try {
        const data = await findContent(
          makeFilter({
            ...filter,
            startDate: '',
            contentTypes: [],
            endDate: '',
          }),
        );
        setSearchItems(data.items);
        return new Page(data.page - 1, data.quantity, data?.items, data.total);
      } catch (error) {
        // TODO: Handle error
        throw error;
      }
    },
    [findContent],
  );

  const updateUserSearches = async () => {
    const urlParams = new URLSearchParams(window.location.search);
    const queryText = urlParams.get('queryText');
    const user = {
      ...userInfo,
      preferences: {
        myMinister: localStorage.getItem('myMinister') ?? userInfo?.preferences.myMinister,
        searches: [
          ...(userInfo?.preferences.searches ?? []),
          { name: searchName, queryText: queryText },
        ],
      },
      roles: userInfo?.roles ?? [],
    } as IUserModel;
    await api.updateUser(user, user.id ?? 0);
    store.storeUserInfo(user as IUserInfoModel);

    toast.success(`${searchName} has successfully been saved.`);
  };

  /** retrigger content fetch when change is applied */
  React.useEffect(() => {
    fetch({
      ...filter,
      ...filterAdvanced,
      keyword: queryText ?? '',
    });
  }, [filter, filterAdvanced, fetch, queryText]);

  return (
    <styled.SearchPage>
      <SearchWithLogout />
      <Row className="save-bar" justifyContent="center">
        <p className="label">Name this search: </p>
        <Text onChange={(e) => setSearchName(e.target.value)} name="searchName" />
        <button onClick={() => updateUserSearches()}>
          <FaSave />
        </button>
      </Row>
      <Row>
        <div className={playerOpen ? 'scroll minimized' : 'scroll'}>
          <Col className={'search-items'}>
            {searchItems.map((item) => {
              return (
                <Row key={item.id} className="rows">
                  <Col className="cols">
                    <Row className="tone-date">
                      <DetermineToneIcon
                        tone={item.tonePools?.length ? item.tonePools[0].value : 0}
                      />
                      <p className="date text-content">
                        {new Date(item.publishedOn).toDateString()}
                      </p>
                    </Row>
                    <p
                      className="headline text-content"
                      onClick={() => navigate(`/view/${item.id}`)}
                    >
                      {item.headline}
                    </p>
                    {/* TODO: Extract text around keyword searched and preview that text rather than the first 50 words */}
                    <p className="summary text-content">
                      {item.body ? trimWords(item.body, 50) : trimWords(item.summary, 50)}
                    </p>
                    <Show visible={!!item.fileReferences?.length}>
                      <button
                        onClick={() => {
                          !playerOpen && setPlayerOpen(true);
                          item.fileReferences && setActiveContent(item);
                        }}
                        className={
                          playerOpen && activeContent?.id === item.id
                            ? 'playing media-button'
                            : 'show media-button'
                        }
                      >
                        {playerOpen && activeContent?.id === item.id ? (
                          <Row>
                            <p>NOW PLAYING</p> <FaStop />
                          </Row>
                        ) : (
                          <Row>
                            <p>PLAY MEDIA</p> <FaPlay />
                          </Row>
                        )}
                      </button>
                    </Show>
                  </Col>
                </Row>
              );
            })}
          </Col>
        </div>
        <Show visible={playerOpen}>
          <Col className="player">
            <Player setPlayerOpen={setPlayerOpen} content={activeContent} />
          </Col>
        </Show>
      </Row>
    </styled.SearchPage>
  );
};
