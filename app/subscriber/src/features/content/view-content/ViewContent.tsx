import { Bar } from 'components/bar';
import { Button } from 'components/button';
import { Sentiment } from 'components/sentiment';
import { ContentActionBar } from 'components/tool-bar';
import { formatSearch } from 'features/search-page/utils';
import { formatDate, showTranscription } from 'features/utils';
import parse from 'html-react-parser';
import React from 'react';
import { FaFeather } from 'react-icons/fa';
import { FaCopyright } from 'react-icons/fa6';
import { useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useApiHub, useContent, useWorkOrders } from 'store/hooks';
import useMobile from 'store/hooks/app/useMobile';
import { useMinisters } from 'store/hooks/subscriber/useMinisters';
import { useProfileStore } from 'store/slices';
import {
  Col,
  ContentTypeName,
  IContentModel,
  IMinisterModel,
  IQuoteModel,
  IWorkOrderMessageModel,
  IWorkOrderModel,
  MessageTargetKey,
  Row,
  Show,
  Spinner,
  useWindowSize,
  WorkOrderStatusName,
  WorkOrderTypeName,
} from 'tno-core';

import * as styled from './styled';
import { ToneValue } from './ToneValue';
import { isWorkOrderStatus } from './utils';

//Difference ratio
//const threshold = 0.1;

export interface IStream {
  url: string;
  type: string;
}

export interface IViewContentProps {
  /** set active content */
  setActiveContent?: (content: IContentModel[]) => void;
  activeContent?: IContentModel[];
}

/**
 * Component to display content when navigating to it from the landing page list view, responsive and adaptive to screen size
 * @returns ViewContent component
 */
export const ViewContent: React.FC<IViewContentProps> = ({ setActiveContent, activeContent }) => {
  const { id, popout } = useParams();
  const [
    {
      search: { filter },
    },
    { getContent, stream },
  ] = useContent();
  const { width } = useWindowSize();
  const [{ profile }] = useProfileStore();
  const [, api] = useMinisters();
  const [, { transcribe, findWorkOrders }] = useWorkOrders();
  const hub = useApiHub();
  const isMobile = useMobile();

  // flag to keep track of the bolding completion in my minister view
  const [boldingComplete, setBoldingComplete] = React.useState(false);
  const [aliases, setAliases] = React.useState<string[]>([]);
  const [workOrders, setWorkOrders] = React.useState<IWorkOrderModel[]>([]);
  const [content, setContent] = React.useState<IContentModel>();
  const [avStream, setAVStream] = React.useState<IStream>();
  const [ministers, setMinisters] = React.useState<IMinisterModel[]>([]);
  const [filteredQuotes, setFilteredQuotes] = React.useState<IQuoteModel[]>([]);
  const videoRef = React.useRef<HTMLVideoElement>(null);
  const radioRef = React.useRef<HTMLAudioElement>(null);
  const fileReference = content?.fileReferences ? content?.fileReferences[0] : undefined;

  const isAV = content?.contentType === ContentTypeName.AudioVideo;
  const isTranscribing =
    isAV &&
    !content?.isApproved &&
    isWorkOrderStatus(workOrders, WorkOrderTypeName.Transcription, [
      WorkOrderStatusName.Completed,
      WorkOrderStatusName.Submitted,
    ]);
  const isTranscriptRequestor = workOrders.some(
    (wo) =>
      wo.requestorId === profile?.id ||
      wo.userNotifications?.some((un) => un.userId === profile?.id),
  );
  const [isProcessing, setIsProcessing] = React.useState(
    workOrders.some(
      (wo) =>
        wo.contentId === content?.id &&
        wo.workType === WorkOrderTypeName.FFmpeg &&
        [WorkOrderStatusName.Submitted, WorkOrderStatusName.InProgress].includes(wo.status),
    ),
  );

  const handleTranscribe = React.useCallback(async () => {
    if (content?.isApproved) {
      toast.error('This content has already been approved.');
      return;
    }
    try {
      if (!!content) {
        const response = await transcribe(content);
        setWorkOrders([response.data, ...workOrders]);

        if (response.status === 200) toast.success('A transcript has been requested');
        if (response.status === 208)
          toast.success('You will receive an email when the transcript is available');
        // In case of failure no message will be displayed to the Subscriber application user.
      }
    } catch {
      // Ignore this failure it is handled by our global ajax requests.
    }
  }, [workOrders, transcribe, content]);

  React.useEffect(() => {
    if (isMobile) {
      const mainElement = document.querySelector('main');
      if (mainElement) {
        mainElement.scrollTo(0, 0);
      }
    }
    // only want to fire when id changes (user navigates to a new section)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id]);

  React.useEffect(() => {
    if (!ministers.length) {
      api.getMinisters().then((data) => {
        setMinisters(data);
      });
    }
  }, [api, ministers.length]);

  React.useEffect(() => {
    if (profile?.preferences?.myMinisters?.length > 0 && ministers.length > 0) {
      let selectedAliases: string[] = [];
      selectedAliases = ministers
        .filter((m) => profile?.preferences?.myMinisters?.includes(m.id))
        .flatMap((x) => [
          x.name,
          // first letter of first name whole last name separated by a period
          x.name.charAt(0) + '.' + x.name.split(' ').slice(-1),
        ]);
      setAliases(selectedAliases);
    }
  }, [ministers, profile?.preferences?.myMinisters]);

  React.useEffect(() => {
    // this will bold the ministers name in the summary or body, only when viewing from the my minister list when the content has been received
    if (window.location.href.includes('my-minister') && !boldingComplete && !!content) {
      let tempBody = content?.body;
      let tempSummary = content?.summary;
      aliases.length &&
        aliases.forEach((ministerAlias: string, index: number) => {
          // escape any special characters in the alias for the regex
          var ministerAliasRegex = ministerAlias.replace(/[/\-\\^$*+?.()|[\]{}]/g, '\\$&');
          const regex = new RegExp(ministerAliasRegex ?? '', 'gi');
          if (content?.summary && !content.summary.includes(`<b>${ministerAlias}</b>`)) {
            tempSummary = tempSummary?.replace(regex, `<b>${ministerAlias}</b>`);
          }

          if (
            content?.body &&
            !content.body.includes(`<b>${ministerAlias}</b>`) &&
            content.body.includes(ministerAlias)
          ) {
            tempBody = tempBody?.replace(regex, `<b>${ministerAlias}</b>`);
          }

          if (index === aliases.length - 1) {
            setContent({ ...content, body: tempBody, summary: tempSummary });
            setBoldingComplete(true);
          }
        });
    }
  }, [content, aliases, boldingComplete]);

  React.useEffect(() => {
    if (!!fileReference)
      stream(fileReference.path).then((result) => {
        setAVStream(
          !!result
            ? {
                url: result,
                type: fileReference.contentType,
              }
            : undefined,
        );
      });
    else setAVStream(undefined);
  }, [stream, fileReference]);

  React.useEffect(() => {
    if (avStream) {
      if (videoRef.current) videoRef.current.load();
      if (radioRef.current) radioRef.current.load();
    }
  }, [avStream]);

  const fetchContent = React.useCallback(
    (id: number) => {
      getContent(id)
        .then((content) => {
          if (!!content) {
            setActiveContent && setActiveContent([content]);
            setContent(content);
            if (!!content.quotes.length)
              setFilteredQuotes(content.quotes.filter((q) => q.isRelevant));
          }
        })
        .catch(() => {});
      findWorkOrders({ contentId: id })
        .then((res) => {
          setWorkOrders(res.items);
          setIsProcessing(
            res.items.some(
              (wo) =>
                wo.contentId === content?.id &&
                wo.workType === WorkOrderTypeName.FFmpeg &&
                [WorkOrderStatusName.Submitted, WorkOrderStatusName.InProgress].includes(wo.status),
            ),
          );
        })
        .catch(() => {});
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [getContent, findWorkOrders],
  );

  // if statement avoids unwanted fetch when navigating back to home view
  React.useEffect(() => {
    if (window.location.pathname.includes('view')) {
      id && fetchContent(+id);
    }
  }, [id, fetchContent]);

  const onWorkOrder = React.useCallback(
    (workOrder: IWorkOrderMessageModel) => {
      if (
        content &&
        workOrder.contentId === content?.id &&
        workOrder.workType === WorkOrderTypeName.FFmpeg
      ) {
        setIsProcessing(
          [WorkOrderStatusName.Submitted, WorkOrderStatusName.InProgress].includes(
            workOrder.status,
          ),
        );
        if (workOrder.status === WorkOrderStatusName.Completed) fetchContent(content.id);
      }
    },
    [fetchContent, content],
  );

  hub.useHubEffect(MessageTargetKey.WorkOrder, onWorkOrder);

  //Remove HTML tags, square brackets and line breaks before comparison.
  const cleanString = (str: string | undefined) => str?.replace(/<[^>]*>?|\[|\]|\n/gm, '').trim();

  const formattedBody = React.useMemo(
    () => formatSearch(content?.body?.replace(/\n+/g, '<br><br>') ?? '', filter),
    [content?.body, filter],
  );
  const formattedSummary = React.useMemo(
    () => formatSearch(content?.summary?.replace(/\n+/g, '<br><br>') ?? '', filter),
    [content?.summary, filter],
  );

  const cleanBody = cleanString(content?.body);
  const cleanSummary = cleanString(content?.summary);

  //Return true if length of cleanBody & cleanSummary is not same, or one of them does not exist
  const isDifferent = React.useMemo(() => {
    if (cleanBody === undefined || cleanSummary === undefined) {
      return true; // If either cleanBody or cleanSummary is undefined, return true
    }
    if (cleanBody.length !== cleanSummary.length) return true;
    if (cleanBody === cleanSummary) return false;
  }, [cleanBody, cleanSummary]);

  return (
    <styled.ViewContent className={`${!!popout && 'popout'}`}>
      <Bar className="info-bar" vanilla>
        <Show visible={!!content?.byline}>
          <div className="byline">{`BY ${content?.byline}`}</div>
        </Show>
        <div className="published-on">
          {content?.publishedOn && formatDate(content.publishedOn, true)}
        </div>
        <Row className="right-side">
          <div className="attributes">
            <div className="source-name attr">{content?.source?.name}</div>
            <div className="source-section attr">{`${content?.section} ${
              content?.page && `${content.page}`
            }`}</div>
            <Show visible={!!filteredQuotes.length}>
              <a href="#quotes-anchor" className="attr" title="go to Quotes">
                [{filteredQuotes.length}] Quotes
              </a>
            </Show>
          </div>
          {content?.tonePools && content?.tonePools.length && (
            <Row className="tone-group">
              <Sentiment value={content?.tonePools[0].value} />
              <div className="numeric-tone">
                <ToneValue tone={content?.tonePools[0].value} />
              </div>
            </Row>
          )}
        </Row>
      </Bar>
      {!!content && <ContentActionBar className="actions" content={[content]} viewingContent />}
      <Show visible={!!avStream && isAV}>
        <Row justifyContent="center">
          <Show visible={isProcessing}>
            <Col alignItems="center" gap="1rem">
              File is being converted.
              <Spinner />
            </Col>
          </Show>
          <Show visible={!isProcessing && fileReference?.contentType.startsWith('audio/')}>
            <audio controls ref={radioRef}>
              <source src={avStream?.url} type={fileReference?.contentType} />
              HTML5 Audio is required
            </audio>
          </Show>
          <Show visible={!isProcessing && fileReference?.contentType.startsWith('video/')}>
            <video
              controls
              height={width! > 500 ? '270' : 135}
              width={width! > 500 ? 480 : 240}
              preload="metadata"
              ref={videoRef}
            >
              <source src={avStream?.url} type={fileReference?.contentType} />
              HTML5 Audio is required
            </video>
          </Show>
        </Row>
      </Show>
      <Show visible={!!avStream && content?.contentType === ContentTypeName.Image}>
        <Row justifyContent="center">
          <img alt="media" src={!!avStream?.url ? avStream?.url : ''} />
        </Row>
      </Show>
      <Row id="summary" className="summary">
        <Show visible={isAV && !!content?.summary && isDifferent && !popout}>
          <Col className="summary-container">
            <span>{formattedSummary}</span>
            <Show visible={!!content?.sourceUrl}>
              <a rel="noreferrer" target="_blank" href={content?.sourceUrl}>
                More...
              </a>
            </Show>
          </Col>
        </Show>
        <Show visible={!isAV && !!content}>
          <Col>
            {!!content?.body?.length && !popout ? (
              <div>{formattedBody}</div>
            ) : (
              <span>{formattedSummary}</span>
            )}
            <Show visible={!!content?.sourceUrl}>
              <a rel="noreferrer" target="_blank" href={content?.sourceUrl}>
                More...
              </a>
            </Show>
          </Col>
        </Show>
        <Row className={`${!!popout && 'popout-transcribe-row'}`}>
          <Show
            visible={
              isAV &&
              !content?.source?.disableTranscribe &&
              !content.isApproved &&
              !isTranscriptRequestor &&
              !!content?.fileReferences.length
            }
          >
            <Button
              onClick={() => handleTranscribe()}
              variant={isTranscribing ? 'warn' : 'primary'}
              className="transcribe-button"
              disabled={
                (!!content?.fileReferences && !content?.fileReferences.length) ||
                (!!content?.fileReferences &&
                  content?.fileReferences.length > 0 &&
                  !content?.fileReferences[0].isUploaded)
              }
            >
              <Show visible={!isTranscribing}>
                <FaFeather /> Request Transcript
              </Show>
              <Show visible={isTranscribing && !isTranscriptRequestor}>
                Request Email when Transcript Complete
              </Show>
            </Button>
            {!!popout && (
              <ContentActionBar className="actions-popout" content={activeContent ?? []} />
            )}
          </Show>
        </Row>
      </Row>
      <Show visible={!!popout}>
        <div className="copyright-text">
          <hr />
          <FaCopyright />
          Copyright protected and owned by broadcaster. Your licence is limited to internal,
          non-commercial, government use. All reproduction, broadcast, transmission, or other use of
          this work is prohibited and subject to licence.
        </div>
      </Show>
      <Show visible={isAV && isTranscribing}>
        <hr />
        <h3>Transcription:</h3>
        <Col className="transcript-status">
          <p>
            Transcript request has been submitted. Once reviewed and approved it will be displayed.
          </p>
          {isTranscriptRequestor && <p>You will receive an email once available.</p>}
        </Col>
      </Show>
      <Show visible={isAV && !isTranscribing && !!content.body?.length && !popout}>
        <hr />
        <Row>
          <img
            className="transcript-feather"
            src={`${process.env.PUBLIC_URL}/assets/transcript_feather.svg`}
            alt="Transcript"
          />
          <h3 className="transcipt-heading">Transcript:</h3>
        </Row>
        <Col>{content && parse(showTranscription(content))}</Col>
      </Show>
      <Show
        visible={
          (content?.contentType === ContentTypeName.PrintContent ||
            content?.contentType === ContentTypeName.Internet) &&
          !!filteredQuotes.length
        }
      >
        <hr />
        <h3 id="quotes-anchor">Quotes:</h3>
        <Row>
          <ul className="quotes-container">
            {filteredQuotes.map((q) => {
              return (
                <li key={q.id}>
                  <q className="quote-statement">{q.statement}</q>
                  <br />
                  <label className="quote-byline">&mdash; {q.byline}</label>
                </li>
              );
            })}
          </ul>
        </Row>
      </Show>
    </styled.ViewContent>
  );
};
