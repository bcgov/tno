import { FormPage } from 'components/formpage';
import moment from 'moment-timezone';
import React from 'react';
import {
  FaAngleDoubleLeft,
  FaAngleDoubleRight,
  FaAngleLeft,
  FaAngleRight,
  FaSyncAlt,
  FaTrash,
} from 'react-icons/fa';
import {
  IOrphanedContent,
  IOrphanScan,
  OrphanIndex,
  OrphanSide,
  useOrphanedContent,
} from 'store/hooks/admin/useOrphanedContent';
import styled from 'styled-components';
import {
  Button,
  ButtonVariant,
  Col,
  FieldSize,
  Modal,
  OptionItem,
  Row,
  Select,
  SelectDate,
  Show,
} from 'tno-core';

import { OrphanScanResponseError, parseOrphanScan } from './parseOrphanScan';

const sideOptions = [
  new OptionItem('Only in Elasticsearch', 'elasticsearch'),
  new OptionItem('Only in PostgreSQL', 'postgres'),
];

const indexOptions = [
  new OptionItem('All-content index', 'content'),
  new OptionItem('Published-content index', 'published'),
];

/** The posted-date bounds travel as calendar days (yyyy-MM-dd), read and written in local time. */
const DAY_FORMAT = 'YYYY-MM-DD';
const toDate = (day: string) => (day ? moment(day, DAY_FORMAT).toDate() : null);
const toDay = (date: Date | null) => (date ? moment(date).format(DAY_FORMAT) : '');

const Container = styled.div`
  display: flex;
  flex: 1;
  min-height: 0;
  padding: 1rem;
  > .form-page {
    display: flex;
    flex-direction: column;
    min-height: 0;
  }
  .scan-controls {
    flex: 0 1 auto;
    min-height: 0;
    overflow: auto;
    padding-bottom: 0.5rem;
  }
  .results-toolbar {
    flex-shrink: 0;
  }
  .results-panel {
    flex: 1 0 min(12rem, 35%);
    min-height: 0;
    overflow: auto;
    margin-top: 1rem;
    border: 1px solid #ddd;
  }
  table {
    width: 100%;
    border-collapse: collapse;
  }
  th,
  td {
    text-align: left;
    padding: 0.75rem;
    border-bottom: 1px solid #ddd;
    vertical-align: middle;
  }
  th {
    background: #f2f2f2;
    position: sticky;
    top: 0;
    z-index: 1;
  }
  .pagination {
    margin-left: auto;
  }
  .actions,
  .pagination {
    display: flex;
    gap: 0.5rem;
    align-items: center;
    .button {
      width: 2rem;
      height: 2rem;
      padding: 0.25rem;
      flex-shrink: 0;
      > div {
        justify-content: center;
      }
    }
    .delete-content {
      color: ${(props) => props.theme.css.dangerColor};
    }
  }
  .selection {
    width: 1%;
    white-space: nowrap;
  }
  .status {
    margin: 1rem 0;
  }
`;

type Resolution = {
  items: IOrphanedContent[];
  action: 'delete' | 'restore' | 'index';
  bulk?: boolean;
};

const OrphanedContent: React.FC = () => {
  const api = useOrphanedContent();
  const [side, setSide] = React.useState<OrphanSide>('elasticsearch');
  const [index, setIndex] = React.useState<OrphanIndex>('content');
  const [items, setItems] = React.useState<IOrphanedContent[]>([]);
  const [selected, setSelected] = React.useState<Set<number>>(() => new Set());
  const [job, setJob] = React.useState<IOrphanScan | null>(null);
  const [pageAfter, setPageAfter] = React.useState(0);
  const [pageBefore, setPageBefore] = React.useState<number>();
  const [pageLast, setPageLast] = React.useState(false);
  const [hasPrevious, setHasPrevious] = React.useState(false);
  const [nextAfter, setNextAfter] = React.useState(0);
  const [hasMore, setHasMore] = React.useState(false);
  // New-scan defaults use Pacific calendar dates, independent of the previous scan.
  const [endPostedOn, setEndPostedOn] = React.useState(() =>
    moment.tz('America/Vancouver').format(DAY_FORMAT),
  );
  const [startPostedOn, setStartPostedOn] = React.useState(() =>
    moment(endPostedOn, DAY_FORMAT).subtract(30, 'days').format(DAY_FORMAT),
  );
  const [busy, setBusy] = React.useState(false);
  const [loaded, setLoaded] = React.useState(false);
  const [notice, setNotice] = React.useState('');
  const [resolution, setResolution] = React.useState<Resolution>();
  const active = !!job && !job.completedOn;
  const currentJobId = React.useRef<string>();
  const refreshVersion = React.useRef(0);
  const selectableItems = items.filter((item) => !item.resolution);
  const selectedItems = selectableItems.filter((item) => selected.has(item.id));
  const allSelected = selectableItems.length > 0 && selectedItems.length === selectableItems.length;
  const someSelected = selectedItems.length > 0 && !allSelected;

  // Poll saved state, never drive the scan from the browser. Only one results page is retained.
  React.useEffect(() => {
    let disposed = false;
    let timer: ReturnType<typeof setTimeout>;
    const refresh = async () => {
      const version = refreshVersion.current;
      try {
        const latest = parseOrphanScan(await api.latest());
        if (disposed || version !== refreshVersion.current) return;
        const changed = latest?.id !== currentJobId.current;
        currentJobId.current = latest?.id;
        setJob(latest);
        setLoaded(true);
        if (changed && latest) {
          setItems([]);
          setSelected(new Set());
          setHasMore(false);
          setSide(latest.side);
          setIndex(latest.index);
          setPageAfter(0);
          setPageBefore(undefined);
          setPageLast(false);
          setHasPrevious(false);
          setResolution(undefined);
        }
        if (latest) {
          const result = await api.results(
            latest.id,
            changed ? 0 : pageAfter,
            changed ? undefined : pageBefore,
            changed ? false : pageLast,
          );
          if (disposed || version !== refreshVersion.current) return;
          setItems(result.items);
          setSelected((previous) => {
            const eligibleIds = new Set(
              result.items.filter((item) => !item.resolution).map((item) => item.id),
            );
            return new Set(Array.from(previous).filter((id) => eligibleIds.has(id)));
          });
          setNextAfter(result.nextAfter);
          setHasMore(result.hasMore);
          setHasPrevious(!!result.hasPrevious);
        }
      } catch (error) {
        if (!disposed && version === refreshVersion.current) {
          if (error instanceof OrphanScanResponseError) {
            setJob(null);
            setItems([]);
            setSelected(new Set());
            setHasMore(false);
            setLoaded(false);
            setResolution(undefined);
            currentJobId.current = undefined;
          }
          setNotice(
            error instanceof OrphanScanResponseError
              ? error.message
              : 'Unable to refresh scan status. Retrying automatically.',
          );
        }
      } finally {
        if (!disposed) timer = setTimeout(refresh, 5000);
      }
    };
    if (!busy) refresh();
    return () => {
      disposed = true;
      clearTimeout(timer);
    };
  }, [api, pageAfter, pageBefore, pageLast, busy]);

  const start = async () => {
    if (startPostedOn && endPostedOn && endPostedOn < startPostedOn) {
      setNotice('End posted date must be on or after start posted date.');
      return;
    }
    refreshVersion.current++;
    setBusy(true);
    try {
      const result = parseOrphanScan(
        await api.start(side, index, startPostedOn || undefined, endPostedOn || undefined),
      );
      if (!result) throw new OrphanScanResponseError();
      currentJobId.current = result.id;
      setJob(result);
      setItems([]);
      setSelected(new Set());
      setPageAfter(0);
      setPageBefore(undefined);
      setPageLast(false);
      setHasPrevious(false);
      setHasMore(false);
      setNotice('Scan started. You can leave this page and return later.');
    } catch (error) {
      setNotice(
        error instanceof OrphanScanResponseError
          ? error.message
          : 'Unable to start the scan. Another scan may already be running.',
      );
    } finally {
      setBusy(false);
    }
  };

  const cancel = async () => {
    if (!job) return;
    refreshVersion.current++;
    setBusy(true);
    try {
      await api.cancel(job.id);
      setJob({ ...job, cancelRequested: true });
      setNotice('Cancellation requested. The current batch will finish first.');
    } catch {
      setNotice('Unable to cancel the scan. Try again.');
    } finally {
      setBusy(false);
    }
  };

  const resolve = async () => {
    if (!resolution || !job || busy) return;
    refreshVersion.current++;
    setBusy(true);
    let succeeded = 0;
    let failed = 0;
    let message = '';
    setNotice(`Processed 0 of ${resolution.items.length} selected items.`);
    try {
      // Process the selected page sequentially to avoid flooding the API/indexer.
      for (const item of resolution.items) {
        try {
          const result = await api.resolve(item.id, resolution.action, job.side, job.index, job.id);
          succeeded++;
          message = result.message;
          const status =
            resolution.action === 'index'
              ? 'Indexing queued'
              : resolution.action === 'restore'
              ? 'Restored'
              : 'Deleted';
          setItems((previous) =>
            previous.map((row) => (row.id === item.id ? { ...row, resolution: status } : row)),
          );
          setSelected((previous) => {
            const next = new Set(previous);
            next.delete(item.id);
            return next;
          });
        } catch {
          failed++;
        }
        setNotice(`Processed ${succeeded + failed} of ${resolution.items.length} selected items.`);
      }
      if (resolution.bulk) {
        const outcome =
          resolution.action === 'index' ? 'queued for indexing' : 'restored to PostgreSQL';
        setNotice(
          `${succeeded} ${succeeded === 1 ? 'item' : 'items'} ${outcome}.` +
            (failed
              ? ` ${failed} ${
                  failed === 1 ? 'item failed and remains' : 'items failed and remain'
                } selected for retry.`
              : ''),
        );
      } else {
        setNotice(
          failed ? 'The action failed. Review the error and scan again before retrying.' : message,
        );
      }
    } finally {
      setResolution(undefined);
      setBusy(false);
    }
  };

  const changePage = (after: number, before?: number, last = false) => {
    refreshVersion.current++;
    setSelected(new Set());
    setItems([]);
    setHasMore(false);
    setResolution(undefined);
    setPageAfter(after);
    setPageBefore(before);
    setPageLast(last);
    setHasPrevious(false);
  };

  return (
    <Container>
      <FormPage>
        <div className="scan-controls">
          <h1>Orphaned Content</h1>
          <p>
            Find content that exists in PostgreSQL or Elasticsearch but is missing from the other
            store.
          </p>
          <Row className="field-row" gap="1rem">
            <Select
              name="side"
              label="Find content"
              width={FieldSize.Big}
              options={sideOptions}
              value={sideOptions.find((o) => o.value === side)}
              isClearable={false}
              isDisabled={busy || active || !loaded}
              onChange={(newValue) => {
                const option = newValue as OptionItem | undefined;
                if (option) setSide(option.value as OrphanSide);
              }}
            />
            <Select
              name="index"
              label="Compare with"
              width={FieldSize.Big}
              options={indexOptions}
              value={indexOptions.find((o) => o.value === index)}
              isClearable={false}
              isDisabled={busy || active || !loaded}
              onChange={(newValue) => {
                const option = newValue as OptionItem | undefined;
                if (option) setIndex(option.value as OrphanIndex);
              }}
            />
            <SelectDate
              id="txt-startPostedOn"
              name="startPostedOn"
              label="Start posted date (optional)"
              width={FieldSize.Medium}
              dateFormat="yyyy-MM-dd"
              portalId="orphaned-content-datepicker"
              placeholderText="YYYY-MM-DD"
              selected={toDate(startPostedOn)}
              isClearable
              disabled={busy || active || !loaded}
              onChange={(date) => setStartPostedOn(toDay(date))}
            />
            <SelectDate
              id="txt-endPostedOn"
              name="endPostedOn"
              label="End posted date (optional)"
              width={FieldSize.Medium}
              dateFormat="yyyy-MM-dd"
              portalId="orphaned-content-datepicker"
              placeholderText="YYYY-MM-DD"
              selected={toDate(endPostedOn)}
              isClearable
              disabled={busy || active || !loaded}
              onChange={(date) => setEndPostedOn(toDay(date))}
            />
            <Button
              variant={ButtonVariant.primary}
              disabled={busy || active || !loaded}
              onClick={start}
            >
              Start new scan
            </Button>
            <Show visible={active}>
              <Button disabled={busy || job?.cancelRequested} onClick={cancel}>
                Cancel scan
              </Button>
            </Show>
          </Row>
          <p>
            Private content is excluded from the PostgreSQL scan. The published index is compared
            only with content marked for publication or already published.
          </p>
          <p>
            Posted dates use Pacific time and include the entire end date. Leave either date blank
            for an open-ended range. Content without a posted date is included only when both dates
            are blank. Scans run in the background. Potential orphans are checked again after a
            grace period to allow indexing to catch up. Recent PostgreSQL changes are excluded from
            this run. Results reflect the time they were checked; each repair checks the content
            again.
          </p>
          <div className="status" role="status" aria-live="polite">
            {notice}
            <Show visible={!!job}>
              <p>
                {job?.status}: {job?.scanned.toLocaleString()} records checked;{' '}
                {job?.pending.toLocaleString()} awaiting verification; {job?.found.toLocaleString()}{' '}
                confirmed orphans.
              </p>
              <p>
                Results for {job?.side} compared with the {job?.index} index. Started{' '}
                {job && new Date(job.createdOn).toLocaleString()}. Posted dates (Pacific):{' '}
                {job?.startPostedOn || 'Any'} through {job?.endPostedOn || 'Any'}.
              </p>
              <Show visible={!!job?.error}>
                <p>{job?.error}</p>
              </Show>
            </Show>
          </div>
        </div>
        <Row className="results-toolbar" gap="1rem" alignItems="center">
          <Button
            disabled={busy || !job || selectedItems.length === 0}
            onClick={() =>
              setResolution({
                items: selectedItems,
                action: job?.side === 'postgres' ? 'index' : 'restore',
                bulk: true,
              })
            }
          >
            Sync all
          </Button>
          <span>{selectedItems.length} selected on this page</span>
          <nav className="pagination" aria-label="Results pages">
            <Button
              variant={ButtonVariant.secondary}
              title="First page"
              aria-label="First results page"
              disabled={busy || !job || !items.length || !hasPrevious}
              onClick={() => changePage(0)}
            >
              <FaAngleDoubleLeft aria-hidden="true" />
            </Button>
            <Button
              variant={ButtonVariant.secondary}
              title="Previous page"
              aria-label="Previous results page"
              disabled={busy || !job || !items.length || !hasPrevious}
              onClick={() => changePage(0, items[0]?.id)}
            >
              <FaAngleLeft aria-hidden="true" />
            </Button>
            <Button
              variant={ButtonVariant.secondary}
              title="Next page"
              aria-label="Next results page"
              disabled={busy || !job || !items.length || !hasMore}
              onClick={() => changePage(nextAfter)}
            >
              <FaAngleRight aria-hidden="true" />
            </Button>
            <Button
              variant={ButtonVariant.secondary}
              title="Last page"
              aria-label="Last results page"
              disabled={busy || !job || !items.length || !hasMore}
              onClick={() => changePage(0, undefined, true)}
            >
              <FaAngleDoubleRight aria-hidden="true" />
            </Button>
          </nav>
        </Row>
        <Show visible={items.length > 0}>
          <div
            className="results-panel"
            role="region"
            aria-label="Orphaned content results"
            tabIndex={0}
          >
            <table>
              <thead>
                <tr>
                  <th className="selection" scope="col">
                    <input
                      className="form-check-input"
                      type="checkbox"
                      aria-label="Select all on this page"
                      title="Select all unresolved content on this page"
                      checked={allSelected}
                      ref={(input) => {
                        if (input) input.indeterminate = someSelected;
                      }}
                      disabled={busy || selectableItems.length === 0}
                      onChange={(event) =>
                        setSelected(
                          event.target.checked
                            ? new Set(selectableItems.map((item) => item.id))
                            : new Set(),
                        )
                      }
                    />
                  </th>
                  <th>ID</th>
                  <th>Source</th>
                  <th>Headline</th>
                  <th>Content status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {items.map((item) => (
                  <tr key={item.id}>
                    <td className="selection">
                      <input
                        className="form-check-input"
                        type="checkbox"
                        aria-label={`Select content ${item.id}`}
                        checked={!item.resolution && selected.has(item.id)}
                        disabled={busy || !!item.resolution}
                        onChange={(event) => {
                          const checked = event.target.checked;
                          setSelected((previous) => {
                            const next = new Set(previous);
                            if (checked) next.add(item.id);
                            else next.delete(item.id);
                            return next;
                          });
                        }}
                      />
                    </td>
                    <td>{item.id}</td>
                    <td>{item.otherSource || '—'}</td>
                    <td>{item.headline}</td>
                    <td>{item.status}</td>
                    <td>
                      <div className="actions">
                        <Show visible={job?.side === 'elasticsearch' && !item.resolution}>
                          <Button
                            variant={ButtonVariant.link}
                            title="Add to PostgreSQL"
                            aria-label="Add to PostgreSQL"
                            disabled={busy}
                            onClick={() => setResolution({ items: [item], action: 'restore' })}
                          >
                            <FaSyncAlt aria-hidden="true" />
                          </Button>
                        </Show>
                        <Show visible={job?.side === 'postgres' && !item.resolution}>
                          <Button
                            variant={ButtonVariant.link}
                            title="Add to Elasticsearch"
                            aria-label="Add to Elasticsearch"
                            disabled={busy}
                            onClick={() => setResolution({ items: [item], action: 'index' })}
                          >
                            <FaSyncAlt aria-hidden="true" />
                          </Button>
                        </Show>
                        <Show visible={!!item.resolution && item.resolution !== 'Deleted'}>
                          <Button
                            variant={ButtonVariant.link}
                            disabled
                            title="Synced"
                            aria-label="Synced"
                          >
                            <FaSyncAlt aria-hidden="true" />
                          </Button>
                        </Show>
                        <Button
                          disabled={busy || item.resolution === 'Deleted'}
                          variant={ButtonVariant.link}
                          className="delete-content"
                          title={item.resolution === 'Deleted' ? 'Deleted' : 'Delete content'}
                          aria-label="Delete content"
                          onClick={() => setResolution({ items: [item], action: 'delete' })}
                        >
                          <FaTrash aria-hidden="true" />
                        </Button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </Show>
        <Show visible={job?.status === 'Completed' && job.found === 0}>
          <p>No orphaned content found for this comparison.</p>
        </Show>
        <Modal
          headerText="Resolve orphaned content"
          isShowing={!!resolution}
          hide={() => {
            if (!busy) setResolution(undefined);
          }}
          onConfirm={resolve}
          confirmText="Confirm"
          enableConfirm={!busy}
          component={
            <Col>
              <Show visible={!!resolution?.bulk}>
                <p>
                  Sync {resolution?.items.length} selected content{' '}
                  {resolution?.items.length === 1 ? 'item' : 'items'}?
                </p>
              </Show>
              <Show visible={!resolution?.bulk}>
                <p>
                  {resolution?.items[0]?.id}: {resolution?.items[0]?.headline}
                </p>
              </Show>
              <Show visible={busy}>
                <p>{notice}</p>
              </Show>
              <Show visible={resolution?.action === 'delete'}>
                <p>
                  Delete this content from PostgreSQL and Elasticsearch wherever it exists? Deleting
                  from PostgreSQL also removes related content records, including report membership.
                  This cannot be undone.
                </p>
              </Show>
              <Show visible={resolution?.action === 'restore'}>
                <p>
                  Restore the original content ID using the full Elasticsearch document when
                  available. A published-only copy may lack an unapproved transcript. Restoration
                  cannot recover deleted files, report membership, delivery records, or analysis
                  history. Missing lookup records must be restored first.
                </p>
              </Show>
              <Show visible={resolution?.action === 'index'}>
                <p>Queue the selected content for indexing using the current PostgreSQL records?</p>
              </Show>
            </Col>
          }
        />
      </FormPage>
    </Container>
  );
};

export default OrphanedContent;
