import { Element, Text } from 'domhandler';
import parse from 'html-react-parser';
import React from 'react';
import { FaScroll, FaVideo } from 'react-icons/fa6';
import { Link } from 'react-router-dom';
import { useAVOverviewInstances, useContent } from 'store/hooks';
import {
  Col,
  ContentTypeName,
  IAVOverviewInstanceModel,
  IReportResultModel,
  Loading,
  Show,
} from 'tno-core';

import * as styled from './styled';

interface ItemMetadata {
  [contentId: string]: {
    hasTranscript: boolean;
    hasVideo: boolean;
  };
}

const AVOverviewPreview: React.FC = () => {
  const [{ findAVOverview, viewAVOverview }] = useAVOverviewInstances();

  const [isLoading, setIsLoading] = React.useState(true);
  const [, { getContent }] = useContent();
  const [instance, setInstance] = React.useState<IAVOverviewInstanceModel>();
  const [preview, setPreview] = React.useState<IReportResultModel | undefined>();
  const [isPublished, setIsPublished] = React.useState(false);
  const [reactElements, setReactElements] = React.useState<string | JSX.Element | JSX.Element[]>();
  const [itemMetadata, setItemMetadata] = React.useState<ItemMetadata>({});

  const handlePreviewReport = React.useCallback(async () => {
    try {
      setIsLoading(true);
      const date = new Date();
      let instance = await findAVOverview(date.toISOString());
      if (!instance?.id || !instance.isPublished) {
        date.setDate(date.getDate() - 1);
        instance = await findAVOverview(date.toISOString());
      }
      setInstance(instance);
      if (!!instance?.id) {
        const preview = await viewAVOverview(instance.id);
        setIsPublished(instance.isPublished);
        setPreview(preview);
      }
    } catch {
    } finally {
      setIsLoading(false);
    }
  }, [findAVOverview, viewAVOverview]);

  React.useEffect(() => {
    handlePreviewReport();
  }, [handlePreviewReport]);

  // build reference object for AV items that contain transcripts/videos
  React.useEffect(() => {
    if (instance && instance.sections?.length) {
      instance.sections.forEach((section) => {
        if (section && section.items?.length) {
          section.items.forEach((item) => {
            if (item?.contentId) {
              const contentId = item?.contentId;
              getContent(item?.contentId).then((content) => {
                setItemMetadata({
                  [contentId]: {
                    hasTranscript:
                      content?.contentType === ContentTypeName.AudioVideo && !!content?.body,
                    hasVideo:
                      content?.fileReferences?.some((file) =>
                        file.contentType.startsWith('video/'),
                      ) ?? false,
                  },
                });
              });
            }
          });
        }
      });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [instance]);

  // enhance items that contain transcript/video with icons
  React.useEffect(() => {
    if (!preview?.body || !getContent) return;
    const htmlToReactElements = parse(preview?.body ?? '', {
      replace(domNode) {
        const element = domNode as Element;
        if (element?.type === 'tag' && element?.name === 'a' && element?.attribs?.href) {
          const hrefValue = element?.attribs?.href;
          const regex = new RegExp('(?<=(?:view\\/))\\d+', 'gm');
          const matches = hrefValue?.match(regex);

          let contentId;
          let hasTranscript = false,
            hasVideo = false;

          if (matches?.length && matches.length === 1) {
            contentId = matches[0];
            if (Object.keys(itemMetadata).includes(contentId)) {
              hasTranscript = itemMetadata[contentId].hasTranscript;
              hasVideo = itemMetadata[contentId].hasVideo;
            }
          }
          return (
            <div className="display-metadata">
              <a href={element?.attribs?.href}>{(element?.children?.[0] as Text)?.data}</a>
              <div>
                {hasTranscript && (
                  <Link to={`/view/${contentId}`}>
                    <FaScroll href={element?.attribs?.href} className="scroll-icon" />
                  </Link>
                )}
                {hasVideo && (
                  <Link to={`/view/${contentId}`}>
                    <FaVideo className="video-icon" />
                  </Link>
                )}
              </div>
            </div>
          );
        }
      },
    });
    setReactElements(htmlToReactElements);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [preview?.body, getContent, itemMetadata]);

  return (
    <styled.AVOverviewPreview>
      <Show visible={isLoading}>
        <Loading />
      </Show>
      <Show visible={!isLoading && !!isPublished}>
        <Col className="preview-report">
          <div className="danger">
            This TNO product is intended only for the use of the person to whom it is addressed.
            Please do not forward or redistribute.{' '}
          </div>
          <div className="preview-body">{reactElements}</div>
        </Col>
      </Show>
      <Show visible={!isPublished}>No report has been published yet. Please check back later.</Show>
    </styled.AVOverviewPreview>
  );
};

export default AVOverviewPreview;
