import { DetermineToneIcon } from 'features/home/utils';
import { showTranscription } from 'features/utils';
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
  IWorkOrderModel,
  Row,
  Show,
  useWindowSize,
  WorkOrderStatusName,
  WorkOrderTypeName,
} from 'tno-core';

import * as styled from './styled';
import { formatTime, isWorkOrderStatus } from './utils';
import { WorkOrderStatus } from './utils/WorkOrderStatus';
import { ViewContentToolbar } from './ViewContentToolbar';

export interface IStream {
  url: string;
  type: string;
}

/**
 * Component to display content when navigating to it from the landing page list view, responsive and adaptive to screen size
 * @returns ViewContent component
 */
export const ViewContent: React.FC = () => {
  const { id } = useParams();
  const [content, setContent] = React.useState<IContentModel>();
  const [, { getContent, stream }] = useContent();
  const [avStream, setAVStream] = React.useState<IStream>();
  // flag to keep track of the bolding completion in my minister view
  const [boldingComplete, setBoldingComplete] = React.useState(false);
  const { width } = useWindowSize();
  const [{ userInfo }] = useApp();
  const [, api] = useMinisters();
  const [aliases, setAliases] = React.useState<string[]>([]);
  const [, { transcribe, findWorkOrders }] = useWorkOrders();
  const [workOrders, setWorkOrders] = React.useState<IWorkOrderModel[]>([]);
  const handleTranscribe = React.useCallback(async () => {
    try {
      // TODO: Only save when required.
      // Save before submitting request.
      if (!!content) {
        const response = await transcribe(content);
        setWorkOrders([response.data, ...workOrders]);

        if (response.status === 200) toast.success('A transcript has been requested');
        else if (response.status === 208) {
          if (response.data.status === WorkOrderStatusName.Completed)
            toast.warn('Content has already been transcribed');
          else toast.warn(`An active request for transcription already exists`);
        }
      }
    } catch {
      // Ignore this failure it is handled by our global ajax requests.
    }
  }, [workOrders, transcribe, content]);

  const fileReference = content?.fileReferences ? content?.fileReferences[0] : undefined;

  const [ministers, setMinisters] = React.useState<IMinisterModel[]>([]);

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
          // first letter of first name whole last name seperated by a period
          x.name.charAt(0) + '.' + x.name.split(' ').slice(-1),
          // ...x.aliases
          //   .split(',')
          //   .filter((element) => element) // remove empty entries
          //   .map((a) => a.trim()),
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
      getContent(id).then((content) => {
        if (!!content) {
          setContent(content);
        } else {
        }
      });
      findWorkOrders({ contentId: id }).then((res) => {
        setWorkOrders(res.items);
      });
    },
    [getContent, findWorkOrders],
  );

  // add classname for colouring as well as formatting the tone value (+ sign for positive)
  const showToneValue = (tone: number) => {
    if (tone > 0) return <span className="pos">+{tone}</span>;
    if (tone < 0) return <span className="neg">{tone}</span>;
    if (tone === 0) return <span className="neut">{tone}</span>;
  };

  const formatDate = (date: string) => {
    if (date) {
      const d = new Date(date);
      const ye = new Intl.DateTimeFormat('en', { year: 'numeric' }).format(d);
      const mo = new Intl.DateTimeFormat('en', { month: 'short' }).format(d);
      const da = new Intl.DateTimeFormat('en', { day: '2-digit' }).format(d);
      return `${da}-${mo}-${ye} ${formatTime(d)}`;
    } else {
      return '';
    }
  };

  // if statement avoids unwanted fetch when navigating back to home view
  React.useEffect(() => {
    if (window.location.pathname.includes('view')) {
      id && fetchContent(+id);
    }
  }, [id, fetchContent]);

  return (
    <styled.ViewContent>
      <Show visible={!!content}>
        <ViewContentToolbar content={content!} tags={content?.tags ?? []} />
      </Show>
      <Row className="headline-container">
        <p>{content?.headline && content.headline}</p>
        <Row alignItems="center">
          <p className="tone-value">
            {showToneValue(content?.tonePools ? content?.tonePools[0]?.value : 0)}
          </p>
          <DetermineToneIcon tone={content?.tonePools ? content?.tonePools[0]?.value : 0} />
        </Row>
      </Row>
      <Row justifyContent="space-between">
        <Col>
          <p className="name-date">
            {content?.byline ? `by ${content?.byline} - ` : ''}
            {formatDate(content?.publishedOn ?? '')}
          </p>
          <Show visible={!!content?.source?.name && content?.contentType === ContentTypeName.Story}>
            <p className="source-name">{content?.source?.name}</p>
          </Show>
        </Col>
        <p className="source-section">
          <Show visible={content?.contentType === ContentTypeName.PrintContent}>
            {content?.source?.name} -{' '}
            {`${content?.section}${content?.page ? `: ${content?.page}` : ''}`}
          </Show>
          <Show visible={content?.contentType !== ContentTypeName.PrintContent}>
            {!!content?.series && `${content?.source?.name} / ${content?.series?.name}`}
          </Show>
        </p>
      </Row>
      <Show visible={!!avStream && content?.contentType === ContentTypeName.AudioVideo}>
        <Row justifyContent="center">
          <Show visible={fileReference?.contentType.startsWith('audio/')}>
            <audio src={avStream?.url} controls>
              HTML5 Audio is required
            </audio>
          </Show>
          <Show visible={fileReference?.contentType.startsWith('video/')}>
            <video
              controls
              height={width! > 500 ? '270' : 135}
              width={width! > 500 ? 480 : 240}
              src={!!avStream?.url ? avStream?.url : ''}
            />
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
          <Show
            visible={
              content?.contentType === ContentTypeName.PrintContent ||
              content?.contentType === ContentTypeName.Story
            }
          >
            <span>{parse(content?.body?.replace(/\n+/g, '<br><br>') ?? '')}</span>
          </Show>
          <Show
            visible={
              content?.contentType === ContentTypeName.AudioVideo ||
              content?.contentType === ContentTypeName.Image
            }
          >
            <span>{parse(content?.summary?.replace(/\n+/g, '<br><br>') ?? '')}</span>
          </Show>
          <Show visible={!!content?.sourceUrl}>
            <a rel="noreferrer" target="_blank" href={content?.sourceUrl}>
              More...
            </a>
          </Show>
        </Col>
        <Show visible={content?.contentType === ContentTypeName.AudioVideo}>
          <Button
            onClick={() => handleTranscribe()}
            variant={ButtonVariant.action}
            className="transcribe-button"
            disabled={
              (!!content?.fileReferences && !content?.fileReferences.length) ||
              (!!content?.fileReferences &&
                content?.fileReferences.length > 0 &&
                !content?.fileReferences[0].isUploaded) ||
              isWorkOrderStatus(workOrders, WorkOrderTypeName.Transcription, [
                WorkOrderStatusName.Completed,
              ])
            }
          >
            <div className="text">Transcribe</div>
            <WorkOrderStatus workOrders={workOrders} type={WorkOrderTypeName.Transcription} />
          </Button>
        </Show>
      </Row>
      <Show visible={content?.contentType === ContentTypeName.AudioVideo && !!content.body?.length}>
        <hr />
        <h3>Transcription:</h3>
        <Row>
          <span>{content && parse(showTranscription(content))}</span>
        </Row>
      </Show>
    </styled.ViewContent>
  );
};
