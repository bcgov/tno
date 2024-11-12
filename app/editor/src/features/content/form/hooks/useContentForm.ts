import { AxiosError } from 'axios';
import { IStream } from 'features/storage/interfaces';
import { FormikHelpers, FormikProps } from 'formik';
import moment from 'moment';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';
import {
  useApiHub,
  useApp,
  useContent,
  useLookup,
  useLookupOptions,
  useWorkOrders,
} from 'store/hooks';
import {
  ContentStatusName,
  ContentTypeName,
  IContentActionMessageModel,
  IContentMessageModel,
  IContentModel,
  IResponseErrorModel,
  IWorkOrderMessageModel,
  MessageTargetKey,
  WorkOrderStatusName,
  WorkOrderTypeName,
} from 'tno-core';

import { IContentFormProps } from '..';
import { IFile } from '../components/upload';
import { defaultFormValues } from '../constants';
import { IContentForm } from '../interfaces';
import { getContentPath, toForm, toModel, triggerFormikValidate } from '../utils';

export const useContentForm = ({
  contentType: initContentType = ContentTypeName.AudioVideo,
}: IContentFormProps) => {
  const hub = useApiHub();
  const [{ userInfo }] = useApp();
  const navigate = useNavigate();
  const [
    ,
    {
      getContent,
      addContent,
      download,
      updateContent,
      deleteContent,
      upload,
      attach,
      stream: getStream,
    },
  ] = useContent();
  const [, { findWorkOrders, transcribe, nlp, ffmpeg }] = useWorkOrders();
  const [{ series }, { getSeries }] = useLookupOptions();
  const [{ settings }] = useLookup();

  // TODO: The stream shouldn't be reset every time the users changes the tab.
  const [stream, setStream] = React.useState<IStream>(); // TODO: Remove dependency coupling with storage component.
  const [isSubmitting, setIsSubmitting] = React.useState(false);

  const [contentType, setContentType] = React.useState(initContentType);
  const [form, setForm] = React.useState<IContentForm>(defaultFormValues(contentType));
  const [isProcessing, setIsProcessing] = React.useState(
    form.workOrders.some((wo) => wo.status === WorkOrderStatusName.InProgress),
  );

  const userId = userInfo?.id ?? '';
  const fileReference = form.fileReferences.length ? form.fileReferences[0] : undefined;
  const path = fileReference?.path;
  const file = !!fileReference
    ? ({
        name: fileReference.fileName,
        size: fileReference.size,
      } as IFile)
    : undefined;

  const updateForm = React.useCallback(
    async (content: IContentModel | undefined) => {
      if (!!content) {
        setForm(toForm(content));
        const res = await findWorkOrders({ contentId: content.id });
        setForm({ ...toForm(content), workOrders: res.data.items });
        // If the form is loaded from the URL instead of clicking on the list view it defaults to the snippet form.
        setContentType(content.contentType);
      }
    },
    [findWorkOrders],
  );

  const fetchContent = React.useCallback(
    async (id: number) => {
      try {
        const content = await getContent(id);
        // Set alert to default false if the content is already published
        if (content?.status === 'Published') {
          const alert = settings.find((x) => x.name === 'AlertActionId');
          const alertAction = alert
            ? content.actions.find((x) => x.id === parseInt(alert.value))
            : null;
          if (alertAction) {
            alertAction.value = 'false';
          }
        }
        await updateForm(content);
      } catch (error) {
        const aError = error as AxiosError;
        if (!!aError && !!aError.response?.data) {
          const data = aError.response.data as IResponseErrorModel;
          if (data.type === 'NoContentException') navigate('/contents');
        }
      }
    },
    [getContent, updateForm, settings, navigate],
  );

  const onWorkOrder = React.useCallback(
    async (workOrder: IWorkOrderMessageModel) => {
      if (form.id === workOrder.contentId) {
        if (
          [
            WorkOrderTypeName.FFmpeg,
            WorkOrderTypeName.Transcription,
            WorkOrderTypeName.NaturalLanguageProcess,
          ].includes(workOrder.workType)
        ) {
          // TODO: Don't overwrite the user's edits.
          await fetchContent(workOrder.contentId);
          setIsProcessing(
            workOrder.workType === WorkOrderTypeName.FFmpeg &&
              workOrder.status === WorkOrderStatusName.InProgress,
          );
        }
      }
    },
    [fetchContent, form],
  );

  hub.useHubEffect(MessageTargetKey.WorkOrder, onWorkOrder);

  React.useEffect(() => {
    setIsProcessing(
      form.workOrders.some(
        (wo) =>
          wo.workType === WorkOrderTypeName.FFmpeg && wo.status === WorkOrderStatusName.InProgress,
      ),
    );
  }, [form.workOrders]);

  const onContentAction = React.useCallback(
    (action: IContentActionMessageModel) => {
      if (action.contentId === form.id) {
        setForm({
          ...form,
          actions: form.actions.map((a) =>
            a.id === action.actionId ? { ...a, value: action.value, version: action.version } : a,
          ),
        });
      }
    },
    [form],
  );

  hub.useHubEffect(MessageTargetKey.ContentActionUpdated, onContentAction);

  const onContentUpdated = React.useCallback(
    async (message: IContentMessageModel) => {
      if (form.id === message.id) {
        if (form.version !== message.version) {
          try {
            // TODO: Don't overwrite the user's edits.
            fetchContent(message.id);
          } catch {}
        } else if (message.reason) {
          getContent(form.id)
            .then((values) => {
              switch (message.reason) {
                case 'file':
                  setForm({ ...form, fileReferences: values?.fileReferences ?? [] });
                  break;
                case 'quotes':
                  setForm({ ...form, quotes: values?.quotes ?? [] });
                  break;
              }
            })
            .catch(() => {});
        }
      }
    },
    [fetchContent, form, getContent],
  );

  hub.useHubEffect(MessageTargetKey.ContentUpdated, onContentUpdated);

  const resetForm = React.useCallback((values: IContentForm) => {
    // Reset form for next record.
    const parsedDate = moment(values.publishedOn);
    const updatedDate = parsedDate.add(1, 'second');
    setForm({
      ...defaultFormValues(values.contentType),
      sourceId: values.sourceId,
      mediaTypeId: values.mediaTypeId,
      otherSource: values.otherSource,
      publishedOn: updatedDate.format('MMM D, yyyy HH:mm:ss'),
      publishedOnTime: updatedDate.format('HH:mm:ss'),
    });
    setStream(undefined);
  }, []);

  const setAvStream = React.useCallback(() => {
    if (!!path) {
      const encodedPath = encodeURIComponent(path);

      getStream(encodedPath)
        .then((result) => {
          setStream(
            !!result
              ? {
                  url: result,
                  type: fileReference?.contentType,
                }
              : undefined,
          );
        })
        .catch(() => {});
    }
  }, [getStream, fileReference, path]);

  const handleSubmit = React.useCallback(
    async (
      values: IContentForm,
      formikHelpers: FormikHelpers<IContentForm>,
    ): Promise<IContentForm> => {
      setIsSubmitting(true);
      let contentResult: IContentModel | null = null;
      const originalId = values.id;
      let result = form;
      try {
        if (!values.id) {
          // Only new content is initialized.
          values.contentType = contentType;
          values.ownerId = !!userId ? userId : '';
        }

        const model = toModel(values);
        contentResult = !form.id ? await addContent(model) : await updateContent(model);

        if (!!values.file) {
          // TODO: Make it possible to upload on the initial save instead of a separate request.
          // Upload the file if one has been added.
          const content = await upload(contentResult, values.file);
          result = toForm({ ...content, tonePools: values.tonePools });
        } else if (
          !originalId &&
          !!values.fileReferences.length &&
          !values.fileReferences[0].isUploaded
        ) {
          // TODO: Make it possible to upload on the initial save instead of a separate request.
          // A file was attached but hasn't been uploaded to the API.
          const fileReference = values.fileReferences[0];
          const content = await attach(contentResult.id, 0, fileReference.path);
          result = toForm({ ...content, tonePools: values.tonePools });
        } else {
          result = toForm({ ...contentResult, tonePools: values.tonePools });
        }
        setForm({ ...result, workOrders: form.workOrders });

        toast.success(`"${contentResult.headline}" has successfully been saved.`);

        if (!!contentResult?.seriesId) {
          // A dynamically added series has been added, fetch the latests series.
          const newSeries = series.find((s) => s.id === contentResult?.seriesId);
          if (!newSeries) getSeries();
        }

        if (!originalId) {
          navigate(getContentPath(contentResult.id, contentResult?.contentType));
          // resetForm(result);
        }
      } catch {
        // If the upload fails, we still need to update the form from the original update.
        if (!!contentResult) {
          result = toForm(contentResult);
          setForm({ ...result, workOrders: form.workOrders });
          if (!originalId) navigate(getContentPath(contentResult.id, contentResult?.contentType));
        }
      } finally {
        setIsSubmitting(false);
      }
      return result;
    },
    [
      addContent,
      attach,
      contentType,
      form,
      getSeries,
      navigate,
      series,
      updateContent,
      upload,
      userId,
    ],
  );

  const handlePublish = React.useCallback(
    async (
      values: IContentForm,
      formikHelpers: FormikHelpers<IContentForm>,
    ): Promise<IContentForm> => {
      if (
        [
          ContentStatusName.Draft,
          ContentStatusName.Unpublish,
          ContentStatusName.Unpublished,
        ].includes(values.status)
      )
        values.status = ContentStatusName.Publish;

      return await handleSubmit(values, formikHelpers);
    },
    [handleSubmit],
  );

  const handleUnpublish = React.useCallback(
    async (props: FormikProps<IContentForm>) => {
      if (
        props.values.status === ContentStatusName.Publish ||
        props.values.status === ContentStatusName.Published
      ) {
        triggerFormikValidate(props);
        if (props.isValid) {
          props.values.status = ContentStatusName.Unpublish;
          await handleSubmit(props.values, props);
        }
      }
    },
    [handleSubmit],
  );

  const handleSave = React.useCallback(
    async (props: FormikProps<IContentForm>) => {
      triggerFormikValidate(props);
      props.validateForm(props.values);
      if (props.isValid) {
        await handleSubmit(props.values, props);
      }
    },
    [handleSubmit],
  );

  const handleTranscribe = React.useCallback(
    async (values: IContentForm, formikHelpers: FormikHelpers<IContentForm>) => {
      try {
        // TODO: Only save when required.
        // Save before submitting request.
        const content = await handleSubmit(values, formikHelpers);
        const response = await transcribe(toModel(values));
        setForm({ ...content, workOrders: [response.data, ...form.workOrders] });

        if (response.status === 200) toast.success('A transcript has been requested');
        else if (response.status === 208) {
          if (response.data.status === WorkOrderStatusName.Completed)
            toast.warn('Content has already been transcribed');
          else toast.warn(`An active request for transcription already exists`);
        }
      } catch {
        // Ignore this failure it is handled by our global ajax requests.
      }
    },
    [form.workOrders, handleSubmit, transcribe],
  );

  const handleNLP = React.useCallback(
    async (values: IContentForm, formikHelpers: FormikHelpers<IContentForm>) => {
      try {
        // TODO: Only save when required.
        // Save before submitting request.
        const content = await handleSubmit(values, formikHelpers);
        const response = await nlp(toModel(values));
        setForm({ ...content, workOrders: [response.data, ...form.workOrders] });

        if (response.status === 200) toast.success('An NLP has been requested');
        else if (response.status === 208) {
          if (response.data.status === WorkOrderStatusName.Completed)
            toast.warn('Content has already been processed by NLP');
          else toast.warn(`An active request for NLP already exists`);
        }
      } catch {
        // Ignore this failure it is handled by our global ajax requests.
      }
    },
    [form.workOrders, handleSubmit, nlp],
  );

  const handleFFmpeg = React.useCallback(
    async (values: IContentForm, formikHelpers: FormikHelpers<IContentForm>) => {
      try {
        // Save before submitting request.
        const response = await ffmpeg(toModel(values));
        setForm({ ...values, workOrders: [response.data, ...form.workOrders] });

        if (response.status === 200) toast.success('A FFmpeg process has been requested');
        else if (response.status === 208) {
          if (response.data.status === WorkOrderStatusName.Completed)
            toast.warn('Content has already been processed by FFmpeg');
          else toast.warn(`An active request for FFmpeg already exists`);
        }
      } catch {
        // Ignore this failure it is handled by our global ajax requests.
      }
    },
    [form.workOrders, ffmpeg],
  );

  const goToNext = React.useCallback(
    (form: IContentForm) => {
      resetForm(form);
      navigate(getContentPath(0, form.contentType));
    },
    [navigate, resetForm],
  );

  return {
    userInfo,
    form,
    setForm,
    isSubmitting,
    isProcessing,
    setIsSubmitting,
    fetchContent,
    deleteContent,
    handleSubmit,
    handleSave,
    handlePublish,
    handleUnpublish,
    handleTranscribe,
    handleNLP,
    handleFFmpeg,
    goToNext,
    file,
    fileReference,
    stream,
    setAvStream,
    download,
    setStream,
  };
};
