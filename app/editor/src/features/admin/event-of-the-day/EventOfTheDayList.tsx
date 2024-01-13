import { getPreviewReportRoute } from 'features/content';
import React from 'react';
import { FaBinoculars } from 'react-icons/fa';
import { toast } from 'react-toastify';
import { useContent, useLookup } from 'store/hooks';
import { useFolders } from 'store/hooks/admin';
import { Button, FlexboxTable, FormPage, IFolderContentModel, Row, Settings } from 'tno-core';

import { useColumns } from './hooks';
import * as styled from './styled';

/**
 * Provides a list of all topics.
 * Provides CRUD form for topics.
 * @returns Component
 */
const EventOfTheDayList: React.FC = () => {
  const [{ isReady, settings }] = useLookup();

  const [, { getContentInFolder }] = useFolders();
  const [, { updateContentTopics }] = useContent();

  const [loading, setLoading] = React.useState(false);
  const [eventOfTheDayFolderId, setEventOfTheDayFolderId] = React.useState(0);
  const [eventOfTheDayReportId, setEventOfTheDayReportId] = React.useState(0);
  const [items, setItems] = React.useState<IFolderContentModel[]>([]);

  React.useEffect(() => {
    if (isReady) {
      const eventOfTheDayFolderId = settings.find(
        (s) => s.name === Settings.EventOfTheDayFolder,
      )?.value;
      if (eventOfTheDayFolderId) setEventOfTheDayFolderId(+eventOfTheDayFolderId);
      else toast.error(`${Settings.EventOfTheDayFolder} setting needs to be configured.`);
      const eventOfTheDayReportId = settings.find(
        (s) => s.name === Settings.EventOfTheDayReport,
      )?.value;
      if (eventOfTheDayReportId) setEventOfTheDayReportId(+eventOfTheDayReportId);
      else toast.error(`${Settings.EventOfTheDayReport} setting needs to be configured.`);
    }
  }, [isReady, settings]);

  React.useEffect(() => {
    if (eventOfTheDayFolderId) {
      setLoading(true);
      getContentInFolder(eventOfTheDayFolderId, true)
        .then((data) => {
          setItems(data);
        })
        .catch(() => {})
        .finally(() => {
          setLoading(false);
        });
    }
    // KGM - overridden to enforce only call once
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [eventOfTheDayFolderId]);

  const handleSubmit = async (values: IFolderContentModel) => {
    try {
      setLoading(true);
      const result = await updateContentTopics(values.contentId, values.content!.topics);
      let index = items.findIndex((el) => el.contentId === values.contentId);
      let results = [...items];
      results[index].content!.topics = result;
      setItems(results);
    } catch {
      // Ignore error as it's handled globally.
    } finally {
      setLoading(false);
    }
  };

  return (
    <styled.EventOfTheDayList>
      <FormPage>
        <Row className="page-header">
          <p className="list-title">Update Content for Event of the Day</p>
          <div className="buttons">
            <Button
              onClick={() =>
                window.open(
                  `${getPreviewReportRoute(eventOfTheDayReportId)}?showNav=false`,
                  '_blank',
                )
              }
            >
              Preview <FaBinoculars className="icon" />
            </Button>
          </div>
        </Row>
        <FlexboxTable
          rowId="contentId"
          data={items}
          columns={useColumns(handleSubmit, loading)}
          groupBy={(item) => {
            if (item.original.content?.series?.name) return item.original.content?.series?.name;
            else if (item.original.content?.source?.name)
              return item.original.content?.source?.name;
            else return ' ';
          }}
          showActive={false}
          showSort={false}
          pagingEnabled={false}
          isLoading={loading}
        />
      </FormPage>
    </styled.EventOfTheDayList>
  );
};

export default EventOfTheDayList;
