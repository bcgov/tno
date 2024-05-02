import { Bar } from 'components/bar';
import { Sentiment } from 'components/sentiment';
import { formatDate, showTranscription } from 'features/utils';
import parse from 'html-react-parser';
import React from 'react';
import { useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useApp, useContent, useWorkOrders } from 'store/hooks';
import { useMinisters } from 'store/hooks/subscriber/useMinisters';
import {
  Button,
  ButtonVariant,
  Col,
  ContentTypeName,
  IContentModel,
  IMinisterModel,
  IQuoteModel,
  IWorkOrderModel,
  Row,
  Show,
  useWindowSize,
  WorkOrderStatusName,
  WorkOrderTypeName,
} from 'tno-core';

import * as styled from './styled';
import { isWorkOrderStatus } from './utils';

export interface IStream {
  url: string;
  type: string;
}
export interface IViewContentProps {
  /** set active content */
  setActiveContent?: (content: IContentModel[]) => void;
}
/**
 * Component to display content when navigating to it from the landing page list view, responsive and adaptive to screen size
 * @returns ViewContent component
 */
export const ViewContent: React.FC<IViewContentProps> = ({ setActiveContent }) => {
  const { id } = useParams();
  const [, { getContent, stream }] = useContent();
  const { width } = useWindowSize();
  const [{ userInfo }] = useApp();
  const [, api] = useMinisters();
  const [, { transcribe, findWorkOrders }] = useWorkOrders();

  // flag to keep track of the bolding completion in my minister view
  const [boldingComplete, setBoldingComplete] = React.useState(false);
  const [aliases, setAliases] = React.useState<string[]>([]);
  const [workOrders, setWorkOrders] = React.useState<IWorkOrderModel[]>([]);
  const [content, setContent] = React.useState<IContentModel>();
  const [avStream, setAVStream] = React.useState<IStream>();
  const [ministers, setMinisters] = React.useState<IMinisterModel[]>([]);
  const [filteredQuotes, setFilteredQuotes] = React.useState<IQuoteModel[]>([]);

  const fileReference = content?.fileReferences ? content?.fileReferences[0] : undefined;

  const handleTranscribe = React.useCallback(async () => {
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
    if (!ministers.length) {
      api.getMinisters().then((data) => {
        setMinisters(data);
      });
    }
  }, [api, ministers.length]);

  React.useEffect(() => {
    if (userInfo?.preferences?.myMinisters?.length > 0 && ministers.length > 0) {
      let selectedAliases: string[] = [];
      selectedAliases = ministers
        .filter((m) => userInfo?.preferences?.myMinisters?.includes(m.id))
        .flatMap((x) => [
          x.name,
          // first letter of first name whole last name separated by a period
          x.name.charAt(0) + '.' + x.name.split(' ').slice(-1),
        ]);
      setAliases(selectedAliases);
    }
  }, [ministers, userInfo?.preferences?.myMinisters]);

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

  // add classname for colouring as well as formatting the tone value (+ sign for positive)
  const showToneValue = (tone: number) => {
    if (tone > 0) return <span className="pos">+{tone}</span>;
    if (tone < 0) return <span className="neg">{tone}</span>;
    if (tone === 0) return <span className="neut">{tone}</span>;
  };
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
      wo.requestorId === userInfo?.id ||
      wo.userNotifications?.some((un) => un.userId === userInfo?.id),
  );

  return (
    <styled.ViewContent>
      <div className="headline">{content?.headline}</div>
      <Bar className="info-bar" vanilla>
        <Show visible={!!content?.byline}>
          <div className="byline">{`BY ${content?.byline}`}</div>
        </Show>
        <div className="published-on">
          {content?.publishedOn && formatDate(content.publishedOn, true)}
        </div>
        <Row className="right-side">
          <div className="source-name">{content?.source?.name}</div>
          <span className="divider">|</span>
          <div className="source-section">{`${content?.section} ${
            content?.page && `:${content.page}`
          }`}</div>
          <Show visible={!!filteredQuotes.length}>
            <span className="divider">|</span>
            <a href="#quotes-anchor" title="go to Quotes">
              [{filteredQuotes.length}] Quotes
            </a>
          </Show>
          {content?.tonePools && content?.tonePools.length && (
            <Row className="tone-group">
              <Sentiment value={content?.tonePools[0].value} />
              <div className="numeric-tone">{showToneValue(content?.tonePools[0].value)}</div>
            </Row>
          )}
        </Row>
      </Bar>
      <Show visible={!!avStream && isAV}>
        <Row justifyContent="center">
          <Show visible={fileReference?.contentType.startsWith('audio/')}>
            <audio controls>
              <source src={avStream?.url} type={fileReference?.contentType} />
              HTML5 Audio is required
            </audio>
          </Show>
          <Show visible={fileReference?.contentType.startsWith('video/')}>
            <video
              controls
              height={width! > 500 ? '270' : 135}
              width={width! > 500 ? 480 : 240}
              preload="metadata"
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
        <Col>
          {!!content?.body?.length ? (
            <div>{parse(content?.body?.replace(/\n+/g, '<br><br>') ?? '')}</div>
          ) : (
            <span>{parse(content?.summary?.replace(/\n+/g, '<br><br>') ?? '')}</span>
          )}
          <Show visible={!!content?.sourceUrl}>
            <a rel="noreferrer" target="_blank" href={content?.sourceUrl}>
              More...
            </a>
          </Show>
        </Col>
        <Show
          visible={
            isAV &&
            !content?.source?.disableTranscribe &&
            !content.isApproved &&
            !isTranscriptRequestor
          }
        >
          <Button
            onClick={() => handleTranscribe()}
            variant={isTranscribing ? ButtonVariant.warning : ButtonVariant.primary}
            className="transcribe-button"
            disabled={
              (!!content?.fileReferences && !content?.fileReferences.length) ||
              (!!content?.fileReferences &&
                content?.fileReferences.length > 0 &&
                !content?.fileReferences[0].isUploaded)
            }
          >
            <Show visible={!isTranscribing}>Request Transcript</Show>
            <Show visible={isTranscribing && !isTranscriptRequestor}>
              Request Email when Transcript Complete
            </Show>
          </Button>
        </Show>
      </Row>
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
      <Show visible={isAV && !isTranscribing && !!content.body?.length}>
        <hr />
        <h3>Transcription:</h3>
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
