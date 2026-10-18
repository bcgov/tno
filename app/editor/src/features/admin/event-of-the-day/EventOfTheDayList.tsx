import { FormPage } from 'components/formpage';
import { getPreviewReportRoute } from 'features/content';
import moment from 'moment-timezone';
import React from 'react';
import { FaBinoculars, FaPaperPlane, FaTimes } from 'react-icons/fa';
import { FaArrowsRotate } from 'react-icons/fa6';
import { useSearchParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useContent, useFolders, useLookup, useReportInstances, useReports } from 'store/hooks';
import { useTopics } from 'store/hooks/admin';
import {
  IEventOfDayInstance,
  IEventOfDayPreview,
  useEventOfDayReports,
} from 'store/hooks/editor/useEventOfDayReports';
import {
  Button,
  ButtonVariant,
  FlexboxTable,
  type IContentTopicModel,
  type IFolderContentModel,
  type IOptionItem,
  type IReportResultModel,
  type ITopicModel,
  Modal,
  Row,
  Select,
  Settings,
  Show,
  useModal,
} from 'tno-core';

import { TopicFormSmall } from '../topics';
import { useColumns } from './hooks';
import * as styled from './styled';

const zone = 'America/Vancouver';
const sortStories = (data: IFolderContentModel[]) =>
  [...data].sort((a, b) => {
    const source =
      (a.content?.source?.sortOrder ?? 99999) - (b.content?.source?.sortOrder ?? 99999);
    return source || (a.content?.page ?? 'ZZZ').localeCompare(b.content?.page ?? 'ZZZ');
  });
// A story counts in the report when it has topics, none is "Not Applicable", and each scores above zero.
// The API also requires its source to have topic scoring.
const countsInReport = (topics: IContentTopicModel[] = []) =>
  !!topics.length && topics.every((t) => !t.isSystem && t.score > 0);

/** Edit source topics by BC publication date and review historical report replacements. */
const EventOfTheDayList: React.FC = () => {
  const [{ isReady, settings }] = useLookup();
  const [, { findAllTopics, updateTopic, addTopic }] = useTopics();
  const [{ publishReport }] = useReports();
  const [{ viewReportInstance }] = useReportInstances();
  const history = useEventOfDayReports();
  const { getContentInFolder } = useFolders();
  const [, { updateContentTopics, resetContentTopicScore }] = useContent();
  const { toggle, isShowing } = useModal();
  const [params, setParams] = useSearchParams();
  const today = moment.tz(zone).format('YYYY-MM-DD');
  const requestedDate = params.get('date');
  const date =
    requestedDate && moment(requestedDate, 'YYYY-MM-DD', true).isValid() && requestedDate <= today
      ? requestedDate
      : today;
  const isToday = date === today;
  const folderId = Number(settings.find((s) => s.name === Settings.EventOfTheDayFolder)?.value);
  const reportId = Number(settings.find((s) => s.name === Settings.EventOfTheDayReport)?.value);

  const [items, setItems] = React.useState<IFolderContentModel[]>([]);
  // Every story published on the date that has a topic or is in the report: the pool to add from.
  const [dayStories, setDayStories] = React.useState<IFolderContentModel[]>([]);
  const [allTopics, setAllTopics] = React.useState<ITopicModel[]>([]);
  const [instances, setInstances] = React.useState<IEventOfDayInstance[]>([]);
  const [instanceId, setInstanceId] = React.useState(0);
  const [loading, setLoading] = React.useState(false);
  const [working, setWorking] = React.useState(false);
  const [isSent, setIsSent] = React.useState(false);
  const [reload, setReload] = React.useState(0);
  const [selected, setSelected] = React.useState<number[]>([]);
  const [preview, setPreview] = React.useState<IReportResultModel>();
  const [draft, setDraft] = React.useState<IEventOfDayPreview>();
  const [draftMode, setDraftMode] = React.useState<'regenerate' | 'recollect'>('regenerate');
  const [added, setAdded] = React.useState<number[]>([]);
  const changedIds = React.useRef(new Set<number>());
  const busy = loading || working;
  const live = isToday && instanceId === 0;
  const currentView = React.useRef('');
  currentView.current = `${date}:${instanceId}`;
  // Redux-backed controllers can change identity when a request updates the store.
  // Load on explicit inputs, using the latest callbacks without making them triggers.
  const loaders = React.useRef({ findAllTopics, findHistory: history.find, getContentInFolder });
  loaders.current = { findAllTopics, findHistory: history.find, getContentInFolder };

  React.useEffect(() => {
    let active = true;
    loaders.current
      .findAllTopics()
      .then((topics) => {
        if (active) setAllTopics(topics);
      })
      .catch(() => {});
    return () => {
      active = false;
    };
  }, [reload]);

  React.useEffect(() => {
    changedIds.current.clear();
  }, [date]);

  React.useEffect(() => {
    if (!isReady || !reportId) return;
    let active = true;
    setLoading(true);
    setItems([]);
    setDayStories([]);
    setInstances([]);
    setInstanceId(0);
    setSelected([]);
    setAdded([]);
    setPreview(undefined);
    setDraft(undefined);
    setIsSent(false);
    const { findHistory, getContentInFolder } = loaders.current;
    findHistory(date)
      .then(async (result) => {
        // Today's default remains the live folder used by the existing publishing flow.
        const content =
          isToday && folderId ? await getContentInFolder(folderId, true) : result.content;
        if (!active) return;
        setItems(sortStories(content));
        setDayStories(sortStories(result.content));
        setInstances(result.instances);
        setInstanceId(isToday ? 0 : result.instances[0]?.id ?? 0);
      })
      .catch(() => {})
      .finally(() => {
        if (active) setLoading(false);
      });
    return () => {
      active = false;
    };
  }, [date, folderId, isReady, isToday, reload, reportId]);

  const invalidatePreview = () => {
    setPreview(undefined);
    setDraft(undefined);
  };

  const changeDate = (value: string) => {
    if (!moment(value, 'YYYY-MM-DD', true).isValid() || value > today) return;
    invalidatePreview();
    setParams((current) => {
      current.set('date', value);
      return current;
    });
  };

  const selectInstance = async (id: number) => {
    invalidatePreview();
    setSelected([]);
    setAdded([]);
    setInstanceId(id);
    if (!isToday) return;
    setWorking(true);
    try {
      const content =
        id === 0 ? await getContentInFolder(folderId, true) : (await history.find(date)).content;
      setItems(sortStories(content));
    } catch {
      setItems([]);
    } finally {
      setWorking(false);
    }
  };

  const storeTopics = (contentId: number, topics: IContentTopicModel[]) => {
    setItems((current) =>
      current.map((item) =>
        item.contentId === contentId && item.content
          ? { ...item, content: { ...item.content, topics } }
          : item,
      ),
    );
  };

  // Today's report is sent from the folder, so a story that no longer counts leaves it.
  const leaveFolder = async (contentId: number, topics: IContentTopicModel[]) => {
    if (!live || countsInReport(topics)) return;
    await history.removeFromFolder(contentId);
    setItems((current) => current.filter((item) => item.contentId !== contentId));
  };

  const handleSubmit = async (values: IFolderContentModel) => {
    invalidatePreview();
    setWorking(true);
    try {
      const result = await updateContentTopics(values.contentId, values.content!.topics);
      changedIds.current.add(values.contentId);
      storeTopics(values.contentId, result);
      await leaveFolder(values.contentId, result);
    } catch {
      // Errors are displayed by the API wrapper; keep the last saved row.
    } finally {
      setWorking(false);
    }
  };

  const handleReset = async (values: IFolderContentModel, topic: IContentTopicModel) => {
    invalidatePreview();
    setWorking(true);
    try {
      const result = await resetContentTopicScore(values.contentId, topic.id);
      changedIds.current.add(values.contentId);
      storeTopics(values.contentId, result);
      await leaveFolder(values.contentId, result);
    } catch {
      // Errors are displayed by the API wrapper.
    } finally {
      setWorking(false);
    }
  };

  const handleAddOrUpdate = async (values: ITopicModel) => {
    try {
      // The small form only adds topics.
      if (values.id !== 0) return;

      // need case insensitive string compare here or we will end up with variations on names
      const topicNameMatch = allTopics.find(
        (x) => x.name.toUpperCase() === values.name.toUpperCase(),
      );

      if (!topicNameMatch) {
        const result = await addTopic(values);
        setAllTopics((topics) => [...topics, result]);
        toast.success(`Topic with name [${values.name}] has been added.`);
      } else if (topicNameMatch.isEnabled) {
        toast.warn(`Topic with name [${values.name}] already exists.`);
      } else {
        // Re-enable the disabled topic of the same name rather than adding a duplicate.
        const result = await updateTopic({
          ...topicNameMatch,
          isEnabled: values.isEnabled,
          topicType: values.topicType,
        });
        setAllTopics((topics) => topics.map((t) => (t.id === result.id ? result : t)));
        toast.success(`Topic with name [${values.name}] has been added.`);
      }
    } catch {
      // Ignore error as it's handled globally.
    }
  };

  const excludeSelected = async () => {
    const systemTopic = allTopics.find((t) => t.isSystem);
    if (!systemTopic) return;
    invalidatePreview();
    setWorking(true);
    try {
      // Persist each successful change in the UI, even if a later request fails.
      for (const item of items.filter((item) => selected.includes(item.contentId))) {
        const topics = await updateContentTopics(item.contentId, [{ ...systemTopic, score: 0 }]);
        changedIds.current.add(item.contentId);
        storeTopics(item.contentId, topics);
        await leaveFolder(item.contentId, topics);
        setSelected((ids) => ids.filter((id) => id !== item.contentId));
      }
      toast.success(
        live
          ? "Selected stories marked Not Applicable and removed from today's folder."
          : 'Selected stories marked Not Applicable.',
      );
    } catch {
      // The remaining selection can be retried after an error.
    } finally {
      setWorking(false);
    }
  };

  const regenerate = async (recollect = false) => {
    const view = currentView.current;
    invalidatePreview();
    setWorking(true);
    try {
      const result = await history.preview(
        instanceId,
        { changedContentIds: Array.from(changedIds.current), addContentIds: added, recollect },
        date,
      );
      if (currentView.current === view) {
        setDraftMode(recollect ? 'recollect' : 'regenerate');
        setDraft(result);
        setPreview(result);
      }
    } catch {
      // Includes a helpful retry message if indexing has not caught up yet.
    } finally {
      setWorking(false);
    }
  };

  const showSaved = async () => {
    const view = currentView.current;
    invalidatePreview();
    setWorking(true);
    try {
      const result = await viewReportInstance(instanceId);
      if (currentView.current === view) setPreview(result);
    } catch {
    } finally {
      setWorking(false);
    }
  };

  const savePreview = async () => {
    if (!draft || draft.instanceId !== instanceId) return;
    setWorking(true);
    try {
      const saved = await history.save(draft, date);
      setInstances((current) =>
        current.some((item) => item.id === saved.id)
          ? current.map((item) => (item.id === saved.id ? saved : item))
          : [saved, ...current],
      );
      setInstanceId(saved.id);
      setAdded([]);
      setDraft(undefined);
      toast.success('Report saved. The subscriber page now shows this version. No email was sent.');
    } catch {
    } finally {
      setWorking(false);
    }
  };

  const addStory = async (contentId: number) => {
    if (!contentId || added.includes(contentId)) return;
    invalidatePreview();
    if (!live) {
      setAdded((current) => [...current, contentId]);
      return;
    }
    // Today's report is sent from the folder, so the story goes straight into it.
    setWorking(true);
    try {
      await history.addToFolder(contentId);
      setItems(sortStories(await getContentInFolder(folderId, true)));
    } catch {
      // Errors are displayed by the API wrapper.
    } finally {
      setWorking(false);
    }
  };

  const removeStory = (contentId: number) => {
    invalidatePreview();
    setAdded((current) => current.filter((id) => id !== contentId));
  };

  const current = instances.find((instance) => instance.id === instanceId);
  const storyLabel = (item: IFolderContentModel) =>
    [item.content?.headline, item.content?.source?.code ?? item.content?.otherSource]
      .filter(Boolean)
      .join(' — ');
  // Only this date's stories that count in the report can join it; those it already holds are left out.
  const inReport = live
    ? items.map((item) => item.contentId)
    : [...(current?.contentIds ?? []), ...added];
  const addable: IOptionItem[] = dayStories
    .filter((item) => countsInReport(item.content?.topics) && !inReport.includes(item.contentId))
    .map((item) => ({
      discriminator: 'IOption',
      label: storyLabel(item),
      value: item.contentId,
      isDisabled: false,
    }));

  const columns = useColumns(allTopics, handleSubmit, handleReset);
  const included = items.filter((item) => countsInReport(item.content?.topics)).length;

  return (
    <styled.EventOfTheDayList>
      <FormPage>
        <Row className="page-header">
          <p className="list-title">Update Content for Event of the Day</p>
          <div className="buttons">
            <Button
              title="Refresh"
              variant={ButtonVariant.secondary}
              disabled={busy}
              onClick={() => setReload((value) => value + 1)}
            >
              <FaArrowsRotate className="icon-refresh" />
            </Button>
          </div>
        </Row>
        <Row className="date-navigation">
          <Button
            disabled={busy}
            onClick={() => changeDate(moment(date).subtract(1, 'day').format('YYYY-MM-DD'))}
          >
            Previous day
          </Button>
          <label>
            Publication date (BC)
            <input
              aria-label="Publication date (BC)"
              type="date"
              value={date}
              max={today}
              disabled={busy}
              onChange={(e) => changeDate(e.target.value)}
            />
          </label>
          <Button
            disabled={busy || isToday}
            onClick={() => changeDate(moment(date).add(1, 'day').format('YYYY-MM-DD'))}
          >
            Next day
          </Button>
          <Button disabled={busy || isToday} onClick={() => changeDate(today)}>
            Today
          </Button>
        </Row>
        <Show visible={instances.length > 0}>
          <Row className="date-navigation">
            <label>
              Report
              <select
                aria-label="Report instance"
                value={instanceId}
                disabled={busy}
                onChange={(e) => {
                  void selectInstance(Number(e.target.value));
                }}
              >
                <Show visible={isToday}>
                  <option value={0}>Current folder — new report</option>
                </Show>
                {instances.map((instance) => (
                  <option key={instance.id} value={instance.id}>
                    {moment(instance.publishedOn).tz(zone).format('YYYY-MM-DD HH:mm')} — #
                    {instance.id}
                  </option>
                ))}
              </select>
            </label>
          </Row>
        </Show>
        <p className="history-help">
          Topic and score changes save immediately and affect future reports. Mark a story Not
          Applicable to exclude it from the report and topic charts; stories scored zero are left
          out too. Regenerate and save to update a published report. Re-collect rebuilds the
          report&apos;s story list from every story that counts on its date, the way the folder was
          collected.
        </p>
        <Show visible={!loading && !live && !instanceId}>
          <p role="status">
            No report was generated on this date. Correct the source topics, then generate the
            report to review and save it.
          </p>
        </Show>
        <Show visible={loading}>
          <p role="status">Loading stories…</p>
        </Show>
        <fieldset disabled={busy} className="topic-editor">
          <Row className="topic-form-row">
            <TopicFormSmall onAddOrUpdate={handleAddOrUpdate} />
          </Row>
          <Row className="selection-actions">
            <span>
              {items.length} stories loaded; {included} with a report topic; {selected.length}{' '}
              selected
            </span>
            <Button
              variant={ButtonVariant.secondary}
              disabled={busy || !selected.length || !allTopics.some((t) => t.isSystem)}
              onClick={excludeSelected}
            >
              Mark selected Not Applicable
            </Button>
            <Button
              variant={ButtonVariant.secondary}
              disabled={busy || !selected.length}
              onClick={() => setSelected([])}
            >
              Clear selection
            </Button>
          </Row>
          <FlexboxTable
            rowId="contentId"
            data={items}
            columns={[
              {
                label: (
                  <input
                    type="checkbox"
                    aria-label="Select all stories"
                    disabled={busy || !items.length}
                    checked={!!items.length && selected.length === items.length}
                    onChange={(e) =>
                      setSelected(e.target.checked ? items.map((i) => i.contentId) : [])
                    }
                  />
                ),
                accessor: 'selected',
                width: '4ch',
                cell: (cell) => (
                  <input
                    type="checkbox"
                    aria-label={`Select story ${cell.original.contentId}`}
                    checked={selected.includes(cell.original.contentId)}
                    disabled={busy}
                    onChange={(e) =>
                      setSelected((ids) =>
                        e.target.checked
                          ? [...ids, cell.original.contentId]
                          : ids.filter((id) => id !== cell.original.contentId),
                      )
                    }
                  />
                ),
              },
              ...columns,
            ]}
            groupBy={(item) =>
              item.original.content?.series?.name ?? item.original.content?.source?.name ?? ' '
            }
            showActive={false}
            showSort={false}
            pagingEnabled={false}
          />
        </fieldset>
        <Row className="page-header">
          <div className="buttons">
            <Show visible={live}>
              <Button
                disabled={busy}
                onClick={() =>
                  window.open(`${getPreviewReportRoute(reportId)}?showNav=false`, '_blank')
                }
              >
                Preview <FaBinoculars className="icon" />
              </Button>
              <Button onClick={toggle} disabled={busy || isShowing || isSent}>
                Send <FaPaperPlane className="icon" />
              </Button>
            </Show>
            <Show visible={!!instanceId}>
              <Button variant={ButtonVariant.secondary} disabled={busy} onClick={showSaved}>
                View saved report
              </Button>
              <Button disabled={busy} onClick={() => regenerate()}>
                Regenerate and preview
              </Button>
              <Button disabled={busy} onClick={() => regenerate(true)}>
                Re-collect stories and preview
              </Button>
            </Show>
            <Show visible={!loading && !live && !instanceId}>
              <Button disabled={busy} onClick={() => regenerate()}>
                Generate report for this date
              </Button>
            </Show>
          </div>
        </Row>
        <Show visible={!loading}>
          <section className="add-stories" aria-label="Add stories to the report">
            <Select
              name="addStory"
              label={live ? 'Add a story to the folder' : 'Add a story to the report'}
              placeholder="Search this date's stories that count in the report"
              options={addable}
              value=""
              isDisabled={busy}
              isClearable={false}
              onChange={(option: any) => void addStory(Number(option?.value))}
            />
            <Show visible={!live && added.length > 0}>
              <ul className="added-stories" aria-label="Stories to add">
                {added.map((contentId) => {
                  const item = items.find((i) => i.contentId === contentId);
                  return (
                    <li key={contentId}>
                      <span>{item ? storyLabel(item) : `Story ${contentId}`}</span>
                      <Button
                        variant={ButtonVariant.link}
                        title={`Remove story ${contentId}`}
                        disabled={busy}
                        onClick={() => removeStory(contentId)}
                      >
                        <FaTimes />
                      </Button>
                    </li>
                  );
                })}
              </ul>
              <p className="history-help">
                Added stories are included when you generate or regenerate, and kept when you save.
              </p>
            </Show>
          </section>
        </Show>
        <Show visible={!!preview}>
          <section className="report-preview" aria-label="Report preview">
            <Row className="page-header">
              <h2>
                {draft
                  ? draft.instanceId
                    ? draftMode === 'recollect'
                      ? 'Re-collected preview — not saved'
                      : 'Regenerated preview — not saved'
                    : 'Generated preview — not saved'
                  : 'Saved report'}
              </h2>
              <div className="buttons">
                <Show visible={!!draft}>
                  <Button disabled={busy} onClick={savePreview}>
                    Save report
                  </Button>
                </Show>
                <Button
                  variant={ButtonVariant.secondary}
                  disabled={busy}
                  onClick={invalidatePreview}
                >
                  Close preview
                </Button>
              </div>
            </Row>
            <iframe title="Event of the Day report preview" sandbox="" srcDoc={preview?.body} />
          </section>
        </Show>
      </FormPage>
      <Modal
        headerText="Confirm Send"
        body="Are you sure you wish to send the Event of the Day report?"
        isShowing={isShowing}
        hide={toggle}
        type="default"
        confirmText="Yes, Send"
        onConfirm={async () => {
          setWorking(true);
          try {
            await publishReport(reportId);
            setIsSent(true);
            toast.success('Event of the Day report has been successfully requested.');
          } catch {
          } finally {
            setWorking(false);
            toggle();
          }
        }}
      />
    </styled.EventOfTheDayList>
  );
};

export default EventOfTheDayList;
