/* eslint-disable simple-import-sort/imports */
import { FormikForm } from 'components/formik';
import moment from 'moment';
import React from 'react';
import { DragDropContext, Draggable, type DropResult } from 'react-beautiful-dnd';
import {
  FaChevronDown,
  FaChevronRight,
  FaCopy,
  FaEdit,
  FaGripLines,
  FaHistory,
  FaPlus,
  FaTrash,
} from 'react-icons/fa';
import { FaArrowRotateLeft, FaCircleInfo } from 'react-icons/fa6';
import { useLocation, useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useApiHub, useLookup } from 'store/hooks';
import {
  useAutomationProfiles,
  useFilters,
  useLLMs,
  useNotifications,
  useReports,
} from 'store/hooks/admin';
import { defaultFilter } from 'features/admin/filters/constants/defaultFilter';
import { defaultLLM } from 'features/admin/llms/constants/defaultLLM';
import {
  Button,
  ButtonVariant,
  Checkbox,
  Col,
  FieldSize,
  FlexboxTable,
  FormikCheckbox,
  FormikSelect,
  FormikText,
  FormikTextArea,
  IconButton,
  type IOptionItem,
  LabelPosition,
  Modal,
  Row,
  Select,
  Show,
  Tab,
  Tabs,
  Text,
  TextArea,
  useModal,
  Wysiwyg,
} from 'tno-core';

import {
  actionTypeDefaults,
  actionTypeOptionItems,
  ADD_ACTION_ACTION,
  AUTO_EXECUTE_ACTION_TYPES,
  AutomationSchema,
  buildDefaultActionPrompt,
  buildDefaultContentStepPrompt,
  contentFieldOptionItems,
  createDefaultAction,
  createDefaultStep,
  DEDUPLICATION_ACTION,
  deduplicateBatchDefaults,
  deduplicateModeOptions,
  DEFAULT_COLLECTION_MAX_ITEMS,
  DEFAULT_DEDUPLICATE_BATCH_SIZE,
  defaultAutomationProfile,
  FETCH_CONTENT_ACTION,
  getRunColumns,
  noneTargetOptions,
  RUN_NOTIFICATION_ACTION,
  RUN_REPORT_ACTION,
  SCORE_CONTENT_ACTION,
  sectionDocs,
  SELECT_TOP_ACTION,
  stepTargetOptions,
  UPDATE_CONTENT_FIELD_ACTION,
} from './constants';
import {
  type IAutomationProfileModel,
  type IAutomationRuleActionModel,
  type IAutomationRunDiffModel,
  type IAutomationRunModel,
  type IAutomationScheduleModel,
  type IAutomationStepModel,
} from './interfaces';
import {
  type IV2ActionDescriptor,
  parseV2Definition,
  parseV2RunSummary,
  RunLogViewer,
  serializeV2Definition,
  v2SaveModeOptions,
  V2Designer,
  V2RunOutcome,
} from './v2';
import {
  type ActionModalMode,
  type IActionDeleteState,
  type IActionModalState,
  type IStepDeleteState,
  type IStepModalState,
  type SectionDocKey,
  type StepModalMode,
} from './types';
import {
  applyContentField,
  applyObjective,
  buildProfileForExport,
  buildProfileForSave,
  cloneAction,
  cloneStep,
  createDefaultSchedule,
  createOption,
  effectivePromptTarget,
  findOptionByValue,
  formatRunTime,
  formatScheduleWeekDays,
  getActionSettingsGroup,
  getStepFilterLabel,
  hasEnrichmentFilter,
  normalizeProfile,
  normalizeSteps,
  scheduleWeekDayOptions,
  setActionSettingsGroup,
  syncActionDefaults,
  syncDefaultPrompt,
  toNumberOrUndefined,
  validateStepTargets,
} from './utils';
import { AutomationDebugging } from './AutomationDebugging';
import { StrictModeDroppable } from './StrictModeDroppable';
import * as styled from './styled';

const AutomationProfileForm: React.FC = () => {
  const [, api] = useAutomationProfiles();
  const [{ llms, actions }, { getLLMs }] = useLookup();
  const [, { findAllReportsHeadersOnly }] = useReports();
  const [, { findNotifications }] = useNotifications();
  const [{ filters }, { findFilters, getFilter, addFilter }] = useFilters();
  const [, { getLLM, addLLM, findAllLLMs }] = useLLMs();
  const { state } = useLocation();
  const { id } = useParams();
  const navigate = useNavigate();
  const { toggle, isShowing } = useModal();
  const { toggle: toggleSectionDoc, isShowing: isSectionDocShowing } = useModal();
  const { toggle: toggleStepModal, isShowing: isStepModalShowing } = useModal();
  const { toggle: toggleStepDeleteModal, isShowing: isStepDeleteModalShowing } = useModal();
  const { toggle: toggleActionModal, isShowing: isActionModalShowing } = useModal();
  const { toggle: toggleScheduleModal, isShowing: isScheduleModalShowing } = useModal();
  const { toggle: toggleActionDeleteModal, isShowing: isActionDeleteModalShowing } = useModal();

  const [activeSectionDoc, setActiveSectionDoc] = React.useState<SectionDocKey>('profile');
  const [llmOptions, setLLMOptions] = React.useState<IOptionItem[]>([]);
  const [filterOptions, setFilterOptions] = React.useState<IOptionItem[]>([]);
  const [actionOptions, setActionOptions] = React.useState<IOptionItem[]>([]);
  const [reportOptions, setReportOptions] = React.useState<IOptionItem[]>([]);
  const [notificationOptions, setNotificationOptions] = React.useState<IOptionItem[]>([]);
  const [stepModalState, setStepModalState] = React.useState<IStepModalState | null>(null);
  const [stepDeleteState, setStepDeleteState] = React.useState<IStepDeleteState | null>(null);
  const [actionModalState, setActionModalState] = React.useState<IActionModalState | null>(null);
  const [scheduleModalState, setScheduleModalState] = React.useState<{
    mode: 'add' | 'edit';
    index?: number;
    schedule: IAutomationScheduleModel;
  } | null>(null);
  const [actionDeleteState, setActionDeleteState] = React.useState<IActionDeleteState | null>(null);
  const [profile, setProfile] = React.useState<IAutomationProfileModel>(
    normalizeProfile(state?.profile),
  );
  const [activeTab, setActiveTab] = React.useState<'profile' | 'schedules' | 'runs' | 'debugging'>(
    'profile',
  );
  // Indexes of steps whose actions sub-grid is collapsed; remapped when steps reorder.
  const [collapsedSteps, setCollapsedSteps] = React.useState<Set<number>>(new Set());

  const toggleStepCollapsed = (index: number) => {
    setCollapsedSteps((collapsed) => {
      const updated = new Set(collapsed);
      if (updated.has(index)) updated.delete(index);
      else updated.add(index);
      return updated;
    });
  };
  const [runs, setRuns] = React.useState<IAutomationRunModel[]>([]);
  const [lastRun, setLastRun] = React.useState<IAutomationRunModel | null>(null);
  const [isRunning, setIsRunning] = React.useState(false);
  const importFileRef = React.useRef<HTMLInputElement>(null);
  const [runDetail, setRunDetail] = React.useState<IAutomationRunModel | null>(null);
  const [runDiff, setRunDiff] = React.useState<IAutomationRunDiffModel | null>(null);
  const { toggle: toggleRunDetailModal, isShowing: isRunDetailModalShowing } = useModal();
  // The run pending deletion; also drives the confirmation modal's visibility.
  const [runDeleteState, setRunDeleteState] = React.useState<IAutomationRunModel | null>(null);
  // The v2 action catalog (fetched once); the v2 designer renders action forms from it.
  const [v2Descriptors, setV2Descriptors] = React.useState<IV2ActionDescriptor[]>([]);
  // What the run detail modal shows: the outcome summary or the decision log (v2 runs).
  const [runDetailView, setRunDetailView] = React.useState<'outcome' | 'log'>('outcome');

  const profileId = Number(id);
  const hub = useApiHub();
  const DragDropContextAny = DragDropContext as any;
  const DroppableAny = StrictModeDroppable as any;
  const DraggableAny = Draggable as any;

  const refreshFilters = React.useCallback(() => {
    // Silent: refreshed when opening step/action modals; must not trigger the page loading overlay
    // (the overlay repaint can swallow the click that opened the modal).
    findFilters({}, true).catch(() => {});
  }, [findFilters]);

  const refreshRuns = React.useCallback(
    async (id: number): Promise<IAutomationRunModel[]> => {
      try {
        const results = await api.findRuns(id);
        setRuns(results);
        return results;
      } catch {
        return [];
      }
    },
    [api],
  );

  const handleRun = async (id: number, isDryRun = false) => {
    setIsRunning(true);
    try {
      const run = await api.runProfile(id, { isDryRun });
      setLastRun(run);
      toast.success(`Automation ${isDryRun ? 'dry ' : ''}run #${run.id} started.`);
      await refreshRuns(id);
    } catch {
      toast.error('Unable to start the automation run.');
    } finally {
      setIsRunning(false);
    }
  };

  // Create a disabled v2 copy of a v1 profile (same prompts, same order); the v1 profile keeps
  // running until the copy is proven.
  const handleMigrate = async (id: number) => {
    try {
      const result = await api.migrateProfile(id);
      toast.success(
        `Created '${result.profile.name}' (disabled).${
          result.warnings.length > 0 ? ` ${result.warnings.length} item(s) need review.` : ''
        }`,
      );
      result.warnings.forEach((warning) => toast.warn(warning, { autoClose: false }));
      navigate(`/admin/automations/${result.profile.id}`);
    } catch {
      // The ajax wrapper reports the error globally.
    }
  };

  // Download the current profile as a JSON file, including the definitions of every referenced
  // filter and LLM (so the profile is self-contained), without primary keys. The LLM api_key is
  // intentionally excluded - a credential must not be written to an export file.
  const handleExport = async (values: IAutomationProfileModel) => {
    try {
      const filterIds = new Set<number>();
      const llmIds = new Set<number>();
      if (values.filterId) filterIds.add(values.filterId);
      if (values.llmId) llmIds.add(values.llmId);
      (values.steps ?? []).forEach((step) => {
        if (step.filterId) filterIds.add(step.filterId);
        if (step.llmId) llmIds.add(step.llmId);
        (step.actions ?? []).forEach((action) => {
          if (action.llmId) llmIds.add(action.llmId);
          if (action.filterId) filterIds.add(action.filterId);
        });
      });

      const [filterDefs, llmDefs] = await Promise.all([
        Promise.all(Array.from(filterIds).map((id) => getFilter(id))),
        Promise.all(Array.from(llmIds).map((id) => getLLM(id))),
      ]);

      const data = {
        profile: buildProfileForExport(values),
        filters: filterDefs.map((f) => ({
          id: f.id,
          name: f.name,
          description: f.description,
          isEnabled: f.isEnabled,
          sortOrder: f.sortOrder,
          query: f.query,
          settings: f.settings,
        })),
        llms: llmDefs.map((l) => ({
          id: l.id,
          name: l.name,
          description: l.description,
          isEnabled: l.isEnabled,
          sortOrder: l.sortOrder,
          deploymentName: l.deploymentName,
          agentName: l.agentName,
          isPublic: l.isPublic,
          systemPrompt: l.systemPrompt,
          userPrompt: l.userPrompt,
          minTemperature: l.minTemperature,
          maxTemperature: l.maxTemperature,
          projectEndpoint: l.projectEndpoint,
        })),
      };

      const slug =
        (values.name || 'profile')
          .trim()
          .toLowerCase()
          .replace(/[^a-z0-9]+/g, '-')
          .replace(/(^-|-$)/g, '') || 'export';
      const uri = window.URL.createObjectURL(
        new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' }),
      );
      const link = document.createElement('a');
      link.href = uri;
      link.setAttribute('download', `automation-profile-${slug}.json`);
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(uri);
    } catch {
      toast.error('Failed to export the automation profile.');
    }
  };

  // Read an exported JSON file and load it into the form as a new (unsaved) profile. Referenced
  // filters and LLMs are recreated - matched by name when they already exist, otherwise created (a
  // new LLM has no api key) - so the references resolve in this environment. The profile itself is
  // not persisted; the user reviews the form and clicks Save to create it.
  const handleImportFile = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    event.target.value = ''; // allow re-selecting the same file
    if (!file) return;
    try {
      const raw = JSON.parse(await file.text());
      const rawProfile = raw?.profile ?? raw; // support the wrapped export and a bare profile
      const filterDefs: any[] = Array.isArray(raw?.filters) ? raw.filters : [];
      const llmDefs: any[] = Array.isArray(raw?.llms) ? raw.llms : [];
      const imported = normalizeProfile(rawProfile);

      // Resolve each referenced filter: reuse an existing filter with the same name, else create it.
      const existingFilters = await findFilters({});
      const filterMap = new Map<number, number>();
      for (const def of filterDefs) {
        const match = existingFilters.find((f) => f.name === def.name);
        const resolved = match
          ? match
          : await addFilter({
              ...defaultFilter,
              name: def.name,
              description: def.description ?? '',
              isEnabled: def.isEnabled ?? true,
              sortOrder: def.sortOrder ?? 0,
              query: def.query ?? {},
              settings: def.settings ?? defaultFilter.settings,
            });
        filterMap.set(def.id, resolved.id);
      }

      // Resolve each referenced LLM: reuse by name, else create WITHOUT an api key.
      const existingLLMs = await findAllLLMs();
      const llmMap = new Map<number, number>();
      let createdLLMWithoutKey = false;
      for (const def of llmDefs) {
        const match = existingLLMs.find((l) => l.name === def.name);
        let id: number;
        if (match) id = match.id;
        else {
          const created = await addLLM({
            ...defaultLLM,
            name: def.name,
            description: def.description ?? '',
            isEnabled: def.isEnabled ?? true,
            sortOrder: def.sortOrder ?? 0,
            deploymentName: def.deploymentName ?? '',
            agentName: def.agentName ?? '',
            isPublic: def.isPublic ?? defaultLLM.isPublic,
            systemPrompt: def.systemPrompt ?? '',
            userPrompt: def.userPrompt ?? '',
            minTemperature: def.minTemperature,
            maxTemperature: def.maxTemperature,
            projectEndpoint: def.projectEndpoint ?? '',
            apiKey: '',
          });
          id = created.id;
          createdLLMWithoutKey = true;
        }
        llmMap.set(def.id, id);
      }

      const mapFilter = (id?: number) => (id ? filterMap.get(id) ?? id : id);
      const mapLLM = (id?: number) => (id ? llmMap.get(id) ?? id : id);

      setProfile({
        ...imported,
        id: 0,
        // Suffix the name so it does not collide with the source profile on Save (editable).
        name: imported.name ? `${imported.name} (imported)` : imported.name,
        filterId: mapFilter(imported.filterId),
        llmId: mapLLM(imported.llmId),
        schedules: (imported.schedules ?? []).map((schedule) => ({ ...schedule, id: 0 })),
        steps: (imported.steps ?? []).map((step) => ({
          ...step,
          id: 0,
          filterId: mapFilter(step.filterId),
          llmId: mapLLM(step.llmId),
          actions: (step.actions ?? []).map((action) => ({
            ...action,
            id: 0,
            // Dedupe prior-action links reference action ids that no longer exist; re-link after save.
            priorActionId: undefined,
            filterId: mapFilter(action.filterId ?? undefined) ?? null,
            llmId: mapLLM(action.llmId),
          })),
        })),
      });
      setActiveTab('profile');
      toast.success('Profile loaded from file. Review the values and click Save to create it.');
      if (createdLLMWithoutKey)
        toast.warning(
          'A referenced LLM was created without an API key. Set its key in Admin > LLM before running.',
        );
    } catch {
      toast.error(
        'Failed to import the profile. Ensure the file is a valid automation profile export.',
      );
    }
  };

  // Poll for status updates while the latest manual run is in progress.
  React.useEffect(() => {
    if (!lastRun || !!lastRun.completedOn || !profileId) return;
    const timer = window.setInterval(async () => {
      const results = await refreshRuns(profileId);
      const updated = results.find((run) => run.id === lastRun.id);
      if (
        updated &&
        (updated.completedOn !== lastRun.completedOn || updated.status !== lastRun.status)
      ) {
        setLastRun(updated);
        const status = String(updated.status ?? 'Unknown');
        if (updated.completedOn) {
          if (status === 'Failed') toast.error(`Run #${updated.id} failed. ${updated.note ?? ''}`);
          else toast.success(`Run #${updated.id} completed successfully.`);
        } else {
          toast.info(`Run #${updated.id} status: ${status}.`);
        }
      }
    }, 3000);
    return () => window.clearInterval(timer);
  }, [lastRun, profileId, refreshRuns]);

  // Real-time: a run can be queued by the scheduler (not this page), so listen for the
  // "run begun / status changed" SignalR signal and refresh the runs list without a manual
  // page refresh. Setting it as the latest run also drives the in-progress poll above.
  hub.useHubEffect(
    'automation-run-updated',
    (message: { id?: number; profileId?: number } = {}) => {
      if (!profileId || message.profileId !== profileId) return;
      refreshRuns(profileId).then((results) => {
        const run = results.find((r) => r.id === message.id) ?? results[0];
        if (run) setLastRun(run);
      });
    },
  );

  const openRunDetail = (run: IAutomationRunModel) => {
    setRunDetail(run);
    setRunDiff(null);
    setRunDetailView('outcome');
    if (!isRunDetailModalShowing) toggleRunDetailModal();
    api
      .getRunDiff(run.id)
      .then((diff) => setRunDiff(diff))
      .catch(() => {});
  };

  const closeRunDetail = () => {
    setRunDetail(null);
    setRunDiff(null);
    if (isRunDetailModalShowing) toggleRunDetailModal();
  };

  const openRunDelete = React.useCallback((run: IAutomationRunModel) => {
    setRunDeleteState(run);
  }, []);

  const closeRunDelete = () => setRunDeleteState(null);

  // FlexboxTable rebuilds its internal table whenever the columns array identity changes, so this
  // must stay referentially stable - hence the stable (state setter only) delete handler above.
  const runColumns = React.useMemo(() => getRunColumns(openRunDelete), [openRunDelete]);

  React.useEffect(() => {
    if (!!profileId && profile.id !== profileId) {
      setProfile({ ...defaultAutomationProfile, id: profileId });
      api
        .getProfile(profileId)
        .then((data) => {
          // The API returns 204 (no body) when the profile does not exist.
          if (!data || !data.id) {
            toast.error(`Automation profile ${profileId} does not exist.`);
            navigate('/admin/automations');
            return;
          }
          setProfile(normalizeProfile(data));
        })
        .catch(() => {
          // The ajax wrapper reports the error globally; return to the list.
          navigate('/admin/automations');
        });
    }
  }, [api, navigate, profile.id, profileId]);

  React.useEffect(() => {
    if (!llms.length) {
      getLLMs().then((values) => {
        setLLMOptions(values.map((value) => createOption(value.name, value.id)));
      });
    } else {
      setLLMOptions(llms.map((value) => createOption(value.name, value.id)));
    }
  }, [getLLMs, llms]);

  React.useEffect(() => {
    setActionOptions(actions.map((value) => createOption(value.name, value.id)));
  }, [actions]);

  React.useEffect(() => {
    findAllReportsHeadersOnly()
      .then((reports) => {
        setReportOptions(reports.map((report) => createOption(report.name, report.id)));
      })
      .catch(() => {});
    findNotifications()
      .then((notifications) => {
        setNotificationOptions(
          notifications.map((notification) => createOption(notification.name, notification.id)),
        );
      })
      .catch(() => {});
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  React.useEffect(() => {
    if (!filters.length) {
      refreshFilters();
    }
  }, [filters.length, refreshFilters]);

  React.useEffect(() => {
    api
      .getV2Descriptors()
      .then(setV2Descriptors)
      .catch(() => {});
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  React.useEffect(() => {
    const handleFocus = () => {
      refreshFilters();
    };

    const handleVisibilityChange = () => {
      if (document.visibilityState === 'visible') refreshFilters();
    };

    window.addEventListener('focus', handleFocus);
    document.addEventListener('visibilitychange', handleVisibilityChange);

    return () => {
      window.removeEventListener('focus', handleFocus);
      document.removeEventListener('visibilitychange', handleVisibilityChange);
    };
  }, [refreshFilters]);

  React.useEffect(() => {
    setFilterOptions(filters.map((value) => createOption(value.name, value.id)));
  }, [filters]);

  const openSectionDoc = (section: SectionDocKey) => {
    setActiveSectionDoc(section);
    if (!isSectionDocShowing) toggleSectionDoc();
  };

  const closeSectionDoc = () => {
    if (isSectionDocShowing) toggleSectionDoc();
  };

  const openStepModal = (
    mode: StepModalMode,
    steps: IAutomationStepModel[],
    profileFilterId?: number,
    index?: number,
  ) => {
    refreshFilters();
    const selectedStep =
      index !== undefined
        ? steps[index]
        : {
            ...createDefaultStep(),
            target: profileFilterId ? ('content' as const) : ('none' as const),
          };
    setStepModalState({
      mode,
      index,
      step: cloneStep({ ...selectedStep, priority: index ?? steps.length }),
    });
    if (!isStepModalShowing) toggleStepModal();
  };

  const closeStepModal = () => {
    setStepModalState(null);
    if (isStepModalShowing) toggleStepModal();
  };

  const openStepDeleteModal = (index: number, name: string) => {
    setStepDeleteState({ index, name });
    if (!isStepDeleteModalShowing) toggleStepDeleteModal();
  };

  const closeStepDeleteModal = () => {
    setStepDeleteState(null);
    if (isStepDeleteModalShowing) toggleStepDeleteModal();
  };

  const openActionModal = (
    mode: ActionModalMode,
    steps: IAutomationStepModel[],
    stepIndex: number,
    actionIndex?: number,
  ) => {
    const step = steps[stepIndex];
    const defaultAction = createDefaultAction();
    // Chat-conversation steps default action prompts to include the News Story/Actions sections.
    if (step.useChatCompletions)
      defaultAction.prompt = buildDefaultActionPrompt(defaultAction.actionType, true);
    const selectedAction =
      actionIndex !== undefined ? step.actions[actionIndex] ?? defaultAction : defaultAction;
    setActionModalState({
      mode,
      stepIndex,
      actionIndex,
      action: cloneAction(selectedAction),
    });
    if (!isActionModalShowing) toggleActionModal();
  };

  const closeActionModal = () => {
    setActionModalState(null);
    if (isActionModalShowing) toggleActionModal();
  };

  const openActionDeleteModal = (stepIndex: number, actionIndex: number, actionType: string) => {
    setActionDeleteState({
      stepIndex,
      actionIndex,
      name: actionType,
    });
    if (!isActionDeleteModalShowing) toggleActionDeleteModal();
  };

  const closeActionDeleteModal = () => {
    setActionDeleteState(null);
    if (isActionDeleteModalShowing) toggleActionDeleteModal();
  };

  const updateActionDraft = (
    updater: (action: IAutomationRuleActionModel) => IAutomationRuleActionModel,
  ) => {
    setActionModalState((state) => {
      if (!state) return state;
      return {
        ...state,
        action: updater(state.action),
      };
    });
  };

  const updateStepDraft = (updater: (step: IAutomationStepModel) => IAutomationStepModel) => {
    setStepModalState((state) => {
      if (!state) return state;
      return { ...state, step: updater(state.step) };
    });
  };

  const handleSubmit = async (values: IAutomationProfileModel) => {
    const stepTargetError = validateStepTargets(values);
    if (stepTargetError) {
      toast.error(stepTargetError);
      return;
    }

    try {
      const profileToSave = buildProfileForSave(values);

      const originalId = values.id;
      const result = !values.id
        ? await api.addProfile(profileToSave)
        : await api.updateProfile(profileToSave);
      setProfile(normalizeProfile(result));
      toast.success(`${result.name} has successfully been saved.`);
      if (!originalId) navigate(`/admin/automations/${result.id}`);
    } catch {
      toast.error('Unable to save automation profile.');
    }
  };

  return (
    <styled.AutomationProfileForm>
      <styled.AutomationModalStyles />
      <IconButton
        iconType="back"
        label="Back to automation profiles"
        className="back-button"
        onClick={() => {
          navigate('/admin/automations');
        }}
      />
      <FormikForm
        initialValues={profile}
        validationSchema={AutomationSchema}
        onSubmit={(values, { setSubmitting }) => {
          handleSubmit(values);
          setSubmitting(false);
        }}
      >
        {({ isSubmitting, values, setFieldValue }) => {
          const orderedSteps = [...values.steps].sort(
            (left, right) => left.priority - right.priority,
          );

          // All actions ordered before the action being edited (earlier steps, then earlier
          // actions in the same step). Only saved actions (id > 0) can be referenced by a
          // 'deduplicate' action.
          const priorActionOptions: IOptionItem[] = !actionModalState
            ? []
            : orderedSteps.flatMap((step, stepIndex) => {
                if (stepIndex > actionModalState.stepIndex) return [];
                const limit =
                  stepIndex === actionModalState.stepIndex
                    ? actionModalState.mode === 'edit' &&
                      typeof actionModalState.actionIndex === 'number'
                      ? actionModalState.actionIndex
                      : step.actions.length
                    : step.actions.length;
                return step.actions
                  .slice(0, limit)
                  .filter((action) => !!action.id)
                  .map((action) =>
                    createOption(`${step.name}: ${action.name || action.actionType}`, action.id!),
                  );
              });

          // "Works on" targets for the action being edited: the original iterated item plus any
          // content created earlier in the same step by a 'create-content' action (only honoured
          // when the step sends separate prompts).
          const worksOnStep = actionModalState
            ? orderedSteps[actionModalState.stepIndex]
            : undefined;
          const worksOnLimit =
            actionModalState?.mode === 'edit' && typeof actionModalState?.actionIndex === 'number'
              ? actionModalState.actionIndex
              : worksOnStep?.actions.length ?? 0;
          const worksOnOptions: IOptionItem[] = [
            createOption('Original content item', 'original'),
            ...(worksOnStep?.actions ?? [])
              .slice(0, worksOnLimit)
              .filter(
                (action) => action.actionType === 'create-content' && !!action.createIdentifier,
              )
              .map((action) => createOption(action.createIdentifier!, action.createIdentifier!)),
          ];

          // Content properties supported by Extract Data default rows and the Create Content
          // mapping grid (fixed left column). Covers scalar, enum, foreign-key and collection
          // fields; the engine resolves each back to the content model when creating.
          const contentProperties = [
            'headline',
            'byline',
            'summary',
            'body',
            'edition',
            'section',
            'page',
            'status',
            'contentType',
            'source',
            'mediaType',
            'series',
            'tags',
            'topics',
            'sentiment',
          ];
          // The default Extract Data value (right column) for a property: foreign keys copy the
          // original's id; tags/topics/sentiment copy their aggregate token (resolved to a
          // code/name list or tone value); everything else copies its content token. Any of these
          // can be replaced with a prompt to generate the value instead.
          const defaultExtractValue = (property: string): string => {
            if (property === 'source') return '{content.sourceId}';
            if (property === 'mediaType') return '{content.mediaTypeId}';
            if (property === 'series') return '{content.seriesId}';
            return `{content.${property}}`;
          };
          const extractRows: { key: string; value: string }[] =
            actionModalState?.action.settings?.extract ?? [];
          const contentMapping: Record<string, string> =
            actionModalState?.action.settings?.mapping ?? {};

          return (
            <div className="form-container">
              <Tabs
                className="profile-tabs"
                tabs={
                  <>
                    <Tab
                      label="Profile"
                      className={activeTab === 'profile' ? 'tab-active' : undefined}
                      active={activeTab === 'profile'}
                      onClick={() => setActiveTab('profile')}
                    />
                    <Tab
                      label="Schedules"
                      className={activeTab === 'schedules' ? 'tab-active' : undefined}
                      active={activeTab === 'schedules'}
                      onClick={() => setActiveTab('schedules')}
                    />
                    <Tab
                      label="History"
                      className={activeTab === 'runs' ? 'tab-active' : undefined}
                      active={activeTab === 'runs'}
                      disabled={!values.id}
                      onClick={() => {
                        setActiveTab('runs');
                        if (values.id) refreshRuns(values.id);
                      }}
                    />
                    <Tab
                      label="Debugging"
                      className={activeTab === 'debugging' ? 'tab-active' : undefined}
                      active={activeTab === 'debugging'}
                      disabled={!values.id}
                      onClick={() => setActiveTab('debugging')}
                    />
                    <div className="tab-header-actions">
                      <Show visible={!!values.id}>
                        <Button
                          type="button"
                          className="header-btn-run"
                          variant={ButtonVariant.success}
                          disabled={isSubmitting || isRunning}
                          onClick={() => handleRun(values.id)}
                        >
                          Run
                        </Button>
                        <Show visible={values.schemaVersion >= 2}>
                          <Button
                            type="button"
                            className="header-btn-outline"
                            variant={ButtonVariant.secondary}
                            disabled={isSubmitting || isRunning}
                            tooltip="Compute and log every decision and change without writing anything."
                            onClick={() => handleRun(values.id, true)}
                          >
                            Dry Run
                          </Button>
                        </Show>
                        <Button
                          type="button"
                          className="header-btn-outline"
                          variant={ButtonVariant.secondary}
                          disabled={isSubmitting}
                          tooltip="Download this profile (steps, actions, and filters) as JSON."
                          onClick={() => handleExport(values)}
                        >
                          Export
                        </Button>
                      </Show>
                      <Button
                        type="button"
                        className="header-btn-outline"
                        variant={ButtonVariant.secondary}
                        disabled={isSubmitting}
                        tooltip="Create a new profile from an exported JSON file."
                        onClick={() => importFileRef.current?.click()}
                      >
                        Import
                      </Button>
                      <input
                        ref={importFileRef}
                        type="file"
                        accept="application/json,.json"
                        style={{ display: 'none' }}
                        onChange={handleImportFile}
                      />
                      <Show visible={!!values.id && values.schemaVersion < 2}>
                        <Button
                          type="button"
                          className="header-btn-outline"
                          variant={ButtonVariant.secondary}
                          disabled={isSubmitting}
                          tooltip="Create a disabled v2 copy of this profile (same prompts, same order); this profile keeps running until the copy is proven."
                          onClick={() => handleMigrate(values.id)}
                        >
                          Create v2 copy
                        </Button>
                      </Show>
                      <Show visible={!!values.id}>
                        <Button
                          type="button"
                          className="header-btn-delete"
                          onClick={toggle}
                          variant={ButtonVariant.secondary}
                          disabled={isSubmitting}
                        >
                          Delete
                        </Button>
                      </Show>
                      <Button type="submit" className="header-btn-save" disabled={isSubmitting}>
                        Save
                      </Button>
                    </div>
                  </>
                }
              >
                <div className="tab-panels">
                  <div className={`tab-panel${activeTab === 'runs' ? ' active' : ''}`}>
                    <Col className="form-inputs runs-tab">
                      <Row className="section-header" nowrap>
                        <h2>Run History</h2>
                      </Row>
                      <p className="section-help-text">
                        Click a run to view its responses and a summary of action outcomes.
                      </p>
                      <Show visible={runs.length === 0}>
                        <div className="rules-grid-empty">No runs have been recorded.</div>
                      </Show>
                      <Show visible={runs.length > 0}>
                        <FlexboxTable
                          rowId="id"
                          data={runs}
                          columns={runColumns}
                          showSort={true}
                          pagingEnabled={false}
                          onRowClick={(row) => openRunDetail(row.original)}
                        />
                      </Show>
                    </Col>
                  </div>
                  <div className={`tab-panel${activeTab === 'debugging' ? ' active' : ''}`}>
                    <Col className="form-inputs">
                      <Row className="section-header" nowrap>
                        <h2>Debugging</h2>
                      </Row>
                      <Show visible={!!values.id}>
                        <AutomationDebugging profileId={values.id} />
                      </Show>
                    </Col>
                  </div>
                  <div className={`tab-panel${activeTab === 'schedules' ? ' active' : ''}`}>
                    <Col className="form-inputs">
                      <Row className="section-header" nowrap>
                        <Row className="section-header-title" nowrap>
                          <h2>Schedules</h2>
                          <button
                            type="button"
                            className="section-doc-button"
                            aria-label="Schedule help"
                            title="Schedule help"
                            onClick={() => openSectionDoc('schedule')}
                          >
                            <FaCircleInfo />
                          </button>
                        </Row>
                      </Row>
                      <p className="section-help-text">
                        The Scheduler service queues a run once per day at (or after) each
                        schedule's Run At time on its selected week days.
                      </p>
                      <div className="schedules-grid">
                        <Row className="schedules-grid-header" nowrap>
                          <Col className="name-col">Name</Col>
                          <Col className="state-col">Run At</Col>
                          <Col className="condition-col">Run On</Col>
                          <Col className="date-col">Start After</Col>
                          <Col className="state-col">Enabled</Col>
                          <Col className="actions-col">
                            <button
                              type="button"
                              className="rule-icon-button"
                              aria-label="Add Schedule"
                              title="Add Schedule"
                              onClick={() => {
                                setScheduleModalState({
                                  mode: 'add',
                                  schedule: createDefaultSchedule(),
                                });
                                toggleScheduleModal();
                              }}
                            >
                              <FaPlus />
                            </button>
                          </Col>
                        </Row>
                        <Show visible={(values.schedules ?? []).length === 0}>
                          <div className="rules-grid-empty">
                            No schedules configured; the profile runs manually only.
                          </div>
                        </Show>
                        {(values.schedules ?? []).map((schedule, index) => (
                          <Row key={index} className="schedules-grid-row" nowrap>
                            <Col className="name-col">{schedule.name || '-'}</Col>
                            <Col className="state-col">
                              {(schedule.startAt ?? '').slice(0, 5) || 'Any time'}
                            </Col>
                            <Col className="condition-col">
                              {formatScheduleWeekDays(schedule.runOnWeekDays)}
                            </Col>
                            <Col className="date-col">
                              {schedule.runOn ? formatRunTime(schedule.runOn) : 'Any time'}
                            </Col>
                            <Col className="state-col">{schedule.isEnabled ? 'Yes' : 'No'}</Col>
                            <Col className="actions-col">
                              <button
                                type="button"
                                className="rule-icon-button"
                                disabled={!schedule.id}
                                aria-label={`Clear last run for schedule ${
                                  schedule.name || index + 1
                                }`}
                                title={
                                  schedule.id
                                    ? 'Clear last run (makes the schedule eligible to run again for testing)'
                                    : 'Save the profile before clearing last run'
                                }
                                onClick={async () => {
                                  if (!schedule.id || !id) return;
                                  await api.clearScheduleLastRun(+id, schedule.id);
                                  toast.success(
                                    `Cleared last run for schedule "${
                                      schedule.name || index + 1
                                    }".`,
                                  );
                                }}
                              >
                                <FaHistory />
                              </button>
                              <button
                                type="button"
                                className="rule-icon-button"
                                aria-label={`Edit schedule ${schedule.name || index + 1}`}
                                title={`Edit schedule ${schedule.name || index + 1}`}
                                onClick={() => {
                                  setScheduleModalState({
                                    mode: 'edit',
                                    index,
                                    schedule: { ...schedule },
                                  });
                                  toggleScheduleModal();
                                }}
                              >
                                <FaEdit />
                              </button>
                              <button
                                type="button"
                                className="rule-icon-button delete"
                                aria-label={`Delete schedule ${schedule.name || index + 1}`}
                                title={`Delete schedule ${schedule.name || index + 1}`}
                                onClick={() => {
                                  setFieldValue(
                                    'schedules',
                                    (values.schedules ?? []).filter(
                                      (_, scheduleIndex) => scheduleIndex !== index,
                                    ),
                                  );
                                }}
                              >
                                <FaTrash />
                              </button>
                            </Col>
                          </Row>
                        ))}
                      </div>
                    </Col>
                  </div>
                  <div className={`tab-panel${activeTab === 'profile' ? ' active' : ''}`}>
                    <Col className="form-inputs">
                      <Row className="section-header section-header-inline" nowrap>
                        <h2>Profile</h2>
                        <button
                          type="button"
                          className="section-doc-button"
                          aria-label="Profile section help"
                          title="Profile section help"
                          onClick={() => openSectionDoc('profile')}
                        >
                          <FaCircleInfo />
                        </button>
                      </Row>
                      <p className="section-help-text">
                        {values.schemaVersion >= 2
                          ? 'Configure the profile, shared prompts and schedule, then define steps by phase: init runs once, process runs per item, complete runs after all steps.'
                          : 'Configure the profile filter and schedule, then define ordered steps and actions.'}
                      </p>
                      <Row className="field-grid" gap="1rem">
                        <FormikText width={FieldSize.Big} name="name" label="Name" required />
                        <FormikSelect
                          width={FieldSize.Medium}
                          name="llmId"
                          label="LLM"
                          value={findOptionByValue(llmOptions, values.llmId) ?? ''}
                          options={llmOptions}
                          onChange={(newValue) => {
                            setFieldValue(
                              'llmId',
                              toNumberOrUndefined(newValue as { value?: unknown } | null),
                            );
                          }}
                          isClearable={false}
                        />
                        <Show visible={values.schemaVersion >= 2}>
                          <Select
                            width={FieldSize.Small}
                            name="definition-save-mode"
                            label="Save Mode"
                            isClearable={false}
                            options={v2SaveModeOptions}
                            value={
                              findOptionByValue(
                                v2SaveModeOptions,
                                parseV2Definition(values.definition).saveMode,
                              ) ?? ''
                            }
                            onChange={(newValue) => {
                              const option = newValue as IOptionItem;
                              const definition = parseV2Definition(values.definition);
                              definition.saveMode = `${option?.value ?? 'end-of-run'}`;
                              setFieldValue('definition', serializeV2Definition(definition));
                            }}
                          />
                        </Show>
                        <FormikCheckbox
                          labelPosition={LabelPosition.Right}
                          label="Is Enabled"
                          name="isEnabled"
                        />
                      </Row>
                      <Row className="field-grid description-row" gap="1rem">
                        <FormikTextArea
                          name="description"
                          label="Description"
                          width={FieldSize.Stretch}
                        />
                      </Row>
                      <Show visible={values.schemaVersion < 2}>
                        <Row className="field-grid filter-row" gap="1rem">
                          <FormikSelect
                            width={FieldSize.Big}
                            name="filterId"
                            label="Automation Filter"
                            value={findOptionByValue(filterOptions, values.filterId) ?? ''}
                            options={filterOptions}
                            onChange={(newValue) => {
                              setFieldValue(
                                'filterId',
                                toNumberOrUndefined(newValue as { value?: unknown } | null),
                              );
                            }}
                            isClearable
                          >
                            <div className="filter-controls">
                              <Button
                                type="button"
                                className="filter-icon-button"
                                variant={ButtonVariant.secondary}
                                disabled={!values.filterId}
                                aria-label="Edit Filter"
                                title="Edit Filter"
                                onClick={() => {
                                  if (!values.filterId) return;
                                  window.open(
                                    `/admin/filters/${values.filterId}`,
                                    '_blank',
                                    'noopener',
                                  );
                                }}
                              >
                                <FaEdit />
                              </Button>
                              <Button
                                type="button"
                                className="filter-icon-button"
                                variant={ButtonVariant.secondary}
                                aria-label="New Filter"
                                title="New Filter"
                                onClick={() => {
                                  window.open('/admin/filters/0', '_blank', 'noopener');
                                }}
                              >
                                <FaPlus />
                              </Button>
                            </div>
                          </FormikSelect>
                        </Row>
                      </Show>
                      <Show visible={values.schemaVersion >= 2}>
                        <V2Designer
                          value={values.definition}
                          onChange={(definition) => setFieldValue('definition', definition)}
                          descriptors={v2Descriptors}
                          filterOptions={filterOptions}
                          llmOptions={llmOptions}
                          reportOptions={reportOptions}
                          notificationOptions={notificationOptions}
                          actionOptions={actionOptions}
                          onValidate={async (definition) =>
                            api.validateProfile({ ...values, definition })
                          }
                        />
                      </Show>
                      <Show visible={values.schemaVersion < 2}>
                        <Row className="section-header" nowrap>
                          <Row className="section-header-title" nowrap>
                            <h2>Steps</h2>
                            <button
                              type="button"
                              className="section-doc-button"
                              aria-label="Steps section help"
                              title="Steps section help"
                              onClick={() => openSectionDoc('steps')}
                            >
                              <FaCircleInfo />
                            </button>
                          </Row>
                        </Row>
                        <p className="section-help-text">
                          Each step will be executed in the order they are listed.
                          <br />
                          If this automation profile includes a filter the processes will iterate
                          over the search results and execute all steps on each content item.
                        </p>
                        <div className="rules-grid">
                          <Row className="rules-grid-header" nowrap>
                            <Col className="drag-col" />
                            <Col className="collapse-col">
                              <button
                                type="button"
                                className="rule-icon-button step-collapse-toggle"
                                aria-label={
                                  collapsedSteps.size >= orderedSteps.length &&
                                  orderedSteps.length > 0
                                    ? 'Show all step actions'
                                    : 'Hide all step actions'
                                }
                                title={
                                  collapsedSteps.size >= orderedSteps.length &&
                                  orderedSteps.length > 0
                                    ? 'Show all step actions'
                                    : 'Hide all step actions'
                                }
                                onClick={() => {
                                  setCollapsedSteps((collapsed) =>
                                    collapsed.size >= orderedSteps.length && orderedSteps.length > 0
                                      ? new Set()
                                      : new Set(orderedSteps.map((_, index) => index)),
                                  );
                                }}
                              >
                                {collapsedSteps.size >= orderedSteps.length &&
                                orderedSteps.length > 0 ? (
                                  <FaChevronRight />
                                ) : (
                                  <FaChevronDown />
                                )}
                              </button>
                            </Col>
                            <Col className="name-col">Step Name</Col>
                            <Col className="state-col">Target</Col>
                            <Col className="condition-col">Filter</Col>
                            <Col className="state-col">Enabled</Col>
                            <Col className="actions-col">
                              <button
                                type="button"
                                className="rule-icon-button"
                                aria-label="Add Step"
                                title="Add Step"
                                onClick={() => openStepModal('add', orderedSteps, values.filterId)}
                              >
                                <FaPlus />
                              </button>
                            </Col>
                          </Row>
                          <Show visible={orderedSteps.length === 0}>
                            <div className="rules-grid-empty">No steps configured.</div>
                          </Show>
                          <Show visible={orderedSteps.length > 0}>
                            <DragDropContextAny
                              onDragEnd={(result: DropResult) => {
                                const { source, destination } = result;
                                if (!destination) return;
                                // Only reordering within the same list is supported; distinct droppable
                                // types keep steps and actions (and actions across steps) from mixing.
                                if (source.droppableId !== destination.droppableId) return;
                                if (source.index === destination.index) return;

                                if (source.droppableId === 'automation-steps-grid') {
                                  const reorderedSteps = [...orderedSteps];
                                  const [movedStep] = reorderedSteps.splice(source.index, 1);
                                  reorderedSteps.splice(destination.index, 0, movedStep);
                                  setFieldValue('steps', normalizeSteps(reorderedSteps));
                                  // Move the collapsed flags the same way so they follow their steps.
                                  setCollapsedSteps((collapsed) => {
                                    const flags = orderedSteps.map((_, i) => collapsed.has(i));
                                    const [movedFlag] = flags.splice(source.index, 1);
                                    flags.splice(destination.index, 0, movedFlag);
                                    return new Set(
                                      flags.flatMap((isCollapsed, i) => (isCollapsed ? [i] : [])),
                                    );
                                  });
                                  return;
                                }

                                if (source.droppableId.startsWith('automation-step-actions-')) {
                                  const stepIndex = Number(
                                    source.droppableId.replace('automation-step-actions-', ''),
                                  );
                                  if (Number.isNaN(stepIndex)) return;
                                  const updatedSteps = [...orderedSteps];
                                  const step = updatedSteps[stepIndex];
                                  if (!step) return;

                                  const reorderedActions = [...step.actions];
                                  const [movedAction] = reorderedActions.splice(source.index, 1);
                                  reorderedActions.splice(destination.index, 0, movedAction);

                                  updatedSteps[stepIndex] = {
                                    ...step,
                                    actions: reorderedActions,
                                  };
                                  setFieldValue('steps', normalizeSteps(updatedSteps));
                                }
                              }}
                            >
                              <DroppableAny
                                droppableId="automation-steps-grid"
                                type="automation-step"
                              >
                                {(provided: any) => (
                                  <div
                                    className="rules-grid-body"
                                    ref={provided.innerRef}
                                    {...provided.droppableProps}
                                  >
                                    {orderedSteps.map((step, index) => (
                                      <DraggableAny
                                        key={`${step.id}-${index}`}
                                        draggableId={`step-${step.id}-${index}`}
                                        index={index}
                                      >
                                        {(dragProvided: any, dragSnapshot: any) => (
                                          <div
                                            className={`step-row-container${
                                              dragSnapshot.isDragging ? ' is-dragging' : ''
                                            }`}
                                            ref={dragProvided.innerRef}
                                            {...dragProvided.draggableProps}
                                          >
                                            <Row className="rules-grid-row" nowrap>
                                              <Col
                                                className="drag-col"
                                                {...dragProvided.dragHandleProps}
                                              >
                                                <FaGripLines />
                                              </Col>
                                              <Col className="collapse-col">
                                                <button
                                                  type="button"
                                                  className="rule-icon-button step-collapse-toggle"
                                                  aria-label={
                                                    collapsedSteps.has(index)
                                                      ? `Expand ${step.name}`
                                                      : `Collapse ${step.name}`
                                                  }
                                                  title={
                                                    collapsedSteps.has(index)
                                                      ? `Expand ${step.name}`
                                                      : `Collapse ${step.name}`
                                                  }
                                                  onClick={() => toggleStepCollapsed(index)}
                                                >
                                                  {collapsedSteps.has(index) ? (
                                                    <FaChevronRight />
                                                  ) : (
                                                    <FaChevronDown />
                                                  )}
                                                </button>
                                              </Col>
                                              <Col className="name-col">
                                                {step.name}
                                                <Show visible={collapsedSteps.has(index)}>
                                                  <span className="step-action-count">
                                                    ({step.actions.length}{' '}
                                                    {step.actions.length === 1
                                                      ? 'action'
                                                      : 'actions'}
                                                    )
                                                  </span>
                                                </Show>
                                              </Col>
                                              <Col className="state-col">
                                                {[...stepTargetOptions, ...noneTargetOptions].find(
                                                  (option) => option.value === step.target,
                                                )?.label ?? step.target}
                                              </Col>
                                              <Col className="condition-col">
                                                {getStepFilterLabel(filterOptions, step.filterId)}
                                              </Col>
                                              <Col className="state-col">
                                                {step.isEnabled ? 'Yes' : 'No'}
                                              </Col>
                                              <Col className="actions-col">
                                                <button
                                                  type="button"
                                                  className="rule-icon-button"
                                                  aria-label={`Edit ${step.name}`}
                                                  title={`Edit ${step.name}`}
                                                  onClick={() =>
                                                    openStepModal(
                                                      'edit',
                                                      orderedSteps,
                                                      values.filterId,
                                                      index,
                                                    )
                                                  }
                                                >
                                                  <FaEdit />
                                                </button>
                                                <button
                                                  type="button"
                                                  className="rule-icon-button"
                                                  aria-label={`Duplicate ${step.name}`}
                                                  title={`Duplicate ${step.name}`}
                                                  onClick={() => {
                                                    const updatedSteps = [...orderedSteps];
                                                    const copy = {
                                                      ...cloneStep(step),
                                                      id: 0,
                                                      name: `${step.name} (copy)`,
                                                      // Copied actions are new rows; they must not
                                                      // carry the original action ids.
                                                      actions: step.actions.map((action) => ({
                                                        ...action,
                                                        id: 0,
                                                      })),
                                                    };
                                                    updatedSteps.splice(index + 1, 0, copy);
                                                    setFieldValue(
                                                      'steps',
                                                      normalizeSteps(updatedSteps),
                                                    );
                                                    // Shift collapsed flags for steps after the insert.
                                                    setCollapsedSteps(
                                                      (collapsed) =>
                                                        new Set(
                                                          Array.from(collapsed).map((i) =>
                                                            i > index ? i + 1 : i,
                                                          ),
                                                        ),
                                                    );
                                                  }}
                                                >
                                                  <FaCopy />
                                                </button>
                                                <button
                                                  type="button"
                                                  className="rule-icon-button delete"
                                                  aria-label={`Delete ${step.name}`}
                                                  title={`Delete ${step.name}`}
                                                  onClick={() =>
                                                    openStepDeleteModal(index, step.name)
                                                  }
                                                >
                                                  <FaTrash />
                                                </button>
                                              </Col>
                                            </Row>
                                            <Show visible={!collapsedSteps.has(index)}>
                                              <div className="actions-sub-grid">
                                                <Row className="actions-sub-grid-header" nowrap>
                                                  <Col className="drag-col" />
                                                  <Col className="name-col">Action Name</Col>
                                                  <Col className="condition-col">Action Type</Col>
                                                  <Col className="state-col">Max Calls</Col>
                                                  <Col className="state-col">Enabled</Col>
                                                  <Col className="actions-col">
                                                    <button
                                                      type="button"
                                                      className="rule-icon-button"
                                                      aria-label={`Add action to ${step.name}`}
                                                      title={`Add action to ${step.name}`}
                                                      onClick={() =>
                                                        openActionModal('add', orderedSteps, index)
                                                      }
                                                    >
                                                      <FaPlus />
                                                    </button>
                                                  </Col>
                                                </Row>
                                                <DroppableAny
                                                  droppableId={`automation-step-actions-${index}`}
                                                  type={`automation-step-actions-${index}`}
                                                >
                                                  {(actionProvided: any) => (
                                                    <div
                                                      className="actions-sub-grid-body"
                                                      ref={actionProvided.innerRef}
                                                      {...actionProvided.droppableProps}
                                                    >
                                                      {step.actions.map((action, actionIndex) => (
                                                        <DraggableAny
                                                          key={`step-${index}-action-${actionIndex}`}
                                                          draggableId={`step-${index}-action-${actionIndex}`}
                                                          index={actionIndex}
                                                        >
                                                          {(
                                                            actionDragProvided: any,
                                                            actionDragSnapshot: any,
                                                          ) => (
                                                            <Row
                                                              className={`actions-sub-grid-row${
                                                                actionDragSnapshot.isDragging
                                                                  ? ' is-dragging'
                                                                  : ''
                                                              }`}
                                                              ref={actionDragProvided.innerRef}
                                                              {...actionDragProvided.draggableProps}
                                                              nowrap
                                                            >
                                                              <Col
                                                                className="drag-col"
                                                                {...actionDragProvided.dragHandleProps}
                                                              >
                                                                <FaGripLines />
                                                              </Col>
                                                              <Col className="name-col">
                                                                {action.name || '-'}
                                                              </Col>
                                                              <Col className="condition-col">
                                                                {actionTypeOptionItems.find(
                                                                  (option) =>
                                                                    option.value ===
                                                                    action.actionType,
                                                                )?.label ?? action.actionType}
                                                              </Col>
                                                              <Col className="state-col">
                                                                {action.maxCalls ?? '-'}
                                                              </Col>
                                                              <Col className="state-col">
                                                                {action.isEnabled ? 'Yes' : 'No'}
                                                              </Col>
                                                              <Col className="actions-col">
                                                                <button
                                                                  type="button"
                                                                  className="rule-icon-button"
                                                                  aria-label={`Edit action ${
                                                                    action.name || action.actionType
                                                                  }`}
                                                                  title={`Edit action ${
                                                                    action.name || action.actionType
                                                                  }`}
                                                                  onClick={() =>
                                                                    openActionModal(
                                                                      'edit',
                                                                      orderedSteps,
                                                                      index,
                                                                      actionIndex,
                                                                    )
                                                                  }
                                                                >
                                                                  <FaEdit />
                                                                </button>
                                                                <button
                                                                  type="button"
                                                                  className="rule-icon-button delete"
                                                                  aria-label={`Delete action ${
                                                                    action.name || action.actionType
                                                                  }`}
                                                                  title={`Delete action ${
                                                                    action.name || action.actionType
                                                                  }`}
                                                                  onClick={() =>
                                                                    openActionDeleteModal(
                                                                      index,
                                                                      actionIndex,
                                                                      action.name ||
                                                                        action.actionType,
                                                                    )
                                                                  }
                                                                >
                                                                  <FaTrash />
                                                                </button>
                                                              </Col>
                                                            </Row>
                                                          )}
                                                        </DraggableAny>
                                                      ))}
                                                      {actionProvided.placeholder}
                                                    </div>
                                                  )}
                                                </DroppableAny>
                                              </div>
                                            </Show>
                                          </div>
                                        )}
                                      </DraggableAny>
                                    ))}
                                    {provided.placeholder}
                                  </div>
                                )}
                              </DroppableAny>
                            </DragDropContextAny>
                          </Show>
                        </div>
                      </Show>
                    </Col>
                  </div>
                </div>
              </Tabs>
              <Modal
                headerText="Confirm Removal"
                body="Are you sure you wish to remove this profile?"
                isShowing={isShowing}
                hide={toggle}
                type="delete"
                confirmText="Yes, Remove It"
                onConfirm={async () => {
                  try {
                    if (profile.id) {
                      await api.deleteProfile(profile.id);
                      toast.success(`${profile.name} has successfully been deleted.`);
                      navigate('/admin/automations');
                    }
                  } finally {
                    toggle();
                  }
                }}
              />
              <Modal
                headerText={sectionDocs[activeSectionDoc].title}
                isShowing={isSectionDocShowing}
                hide={closeSectionDoc}
                type="custom"
                component={
                  <div className="section-doc-content">{sectionDocs[activeSectionDoc].content}</div>
                }
                customButtons={
                  <Button variant={ButtonVariant.secondary} onClick={closeSectionDoc}>
                    Close
                  </Button>
                }
              />
              <Modal
                headerText={stepModalState?.mode === 'edit' ? 'Edit Step' : 'Add Step'}
                isShowing={isStepModalShowing}
                hide={closeStepModal}
                type="custom"
                component={
                  <div className="rule-modal-content">
                    <p className="modal-intro-text">
                      A step defines one stage in the automation flow. It controls when execution
                      happens (target), what optional filter context is available, and the prompt
                      instructions passed to its actions.
                    </p>
                    <Row className="field-grid step-name-row" gap="1rem">
                      <Text
                        width={FieldSize.Big}
                        name="step-name"
                        label="Name"
                        value={stepModalState?.step.name ?? ''}
                        onChange={(event) => {
                          const name = event.target.value;
                          updateStepDraft((step) => ({ ...step, name }));
                        }}
                      />
                      <styled.ModalEnabledCheckbox>
                        <Checkbox
                          label="Enabled"
                          name="step-enabled"
                          checked={stepModalState?.step.isEnabled ?? false}
                          onChange={(event) => {
                            const isEnabled = event.target.checked;
                            updateStepDraft((step) => ({ ...step, isEnabled }));
                          }}
                        />
                      </styled.ModalEnabledCheckbox>
                    </Row>
                    <styled.ModalPromptField>
                      <TextArea
                        name="step-description"
                        label="Description"
                        width="100%"
                        rows={3}
                        value={stepModalState?.step.description ?? ''}
                        onChange={(event) => {
                          const description = event.target.value;
                          updateStepDraft((step) => ({ ...step, description }));
                        }}
                      />
                    </styled.ModalPromptField>
                    <Row className="field-grid action-wysiwyg-row">
                      <styled.StepTargetWithHelp>
                        <Select
                          name="step-target"
                          label="Target"
                          isClearable={false}
                          options={values.filterId ? stepTargetOptions : noneTargetOptions}
                          value={
                            (values.filterId ? stepTargetOptions : noneTargetOptions).find(
                              (option) => option.value === stepModalState?.step.target,
                            ) ?? null
                          }
                          onChange={(newValue) => {
                            const target = (
                              newValue as { value?: IAutomationStepModel['target'] } | null
                            )?.value;
                            if (!target) return;
                            updateStepDraft((step) =>
                              syncDefaultPrompt({
                                ...step,
                                target,
                                // The gate only applies to 'content' targets; iteration only
                                // applies to 'start'/'end' targets.
                                applyToAutomationFilter:
                                  target === 'content' ? step.applyToAutomationFilter : false,
                                iterateStepFilter:
                                  target === 'start' || target === 'end'
                                    ? step.iterateStepFilter
                                    : false,
                              }),
                            );
                          }}
                        />
                        <styled.StepTargetHelpButton
                          type="button"
                          aria-label="Target and filter behavior help"
                          title="Target and filter behavior help"
                          onClick={() => openSectionDoc('stepFilters')}
                        >
                          <FaCircleInfo />
                        </styled.StepTargetHelpButton>
                      </styled.StepTargetWithHelp>
                      <Select
                        name="step-filter"
                        label="Step Filter"
                        width="25rem"
                        options={filterOptions}
                        value={findOptionByValue(filterOptions, stepModalState?.step.filterId)}
                        onChange={(newValue) => {
                          const filterId = toNumberOrUndefined(
                            newValue as { value?: unknown } | null,
                          );
                          updateStepDraft((step) =>
                            syncDefaultPrompt({
                              ...step,
                              filterId,
                              iterateStepFilter: filterId ? step.iterateStepFilter : false,
                            }),
                          );
                        }}
                        isClearable
                      >
                        <div className="filter-controls step-filter-controls">
                          <Button
                            type="button"
                            className="filter-icon-button"
                            variant={ButtonVariant.secondary}
                            disabled={!stepModalState?.step.filterId}
                            aria-label="Edit Step Filter"
                            title="Edit Step Filter"
                            onClick={() => {
                              if (!stepModalState?.step.filterId) return;
                              window.open(
                                `/admin/filters/${stepModalState.step.filterId}`,
                                '_blank',
                                'noopener',
                              );
                            }}
                          >
                            <FaEdit />
                          </Button>
                          <Button
                            type="button"
                            className="filter-icon-button"
                            variant={ButtonVariant.secondary}
                            aria-label="New Step Filter"
                            title="New Step Filter"
                            onClick={() => {
                              window.open('/admin/filters/0', '_blank', 'noopener');
                            }}
                          >
                            <FaPlus />
                          </Button>
                        </div>
                      </Select>
                    </Row>
                    <Row className="field-grid action-main-row" gap="1rem">
                      <Show visible={stepModalState?.step.target === 'content'}>
                        <Checkbox
                          label="Apply filter to profile content"
                          name="step-apply-to-profile"
                          checked={stepModalState?.step.applyToAutomationFilter ?? false}
                          disabled={!stepModalState?.step.filterId}
                          onChange={(event) => {
                            const applyToAutomationFilter = event.target.checked;
                            updateStepDraft((step) =>
                              syncDefaultPrompt({ ...step, applyToAutomationFilter }),
                            );
                          }}
                        />
                      </Show>
                      <Show
                        visible={
                          !!stepModalState?.step.filterId &&
                          (stepModalState?.step.target === 'start' ||
                            stepModalState?.step.target === 'end')
                        }
                      >
                        <Checkbox
                          label="Iterate over content from step filter"
                          name="step-iterate-step-filter"
                          checked={stepModalState?.step.iterateStepFilter ?? false}
                          onChange={(event) => {
                            const iterateStepFilter = event.target.checked;
                            updateStepDraft((step) =>
                              syncDefaultPrompt({ ...step, iterateStepFilter }),
                            );
                          }}
                        />
                      </Show>
                      <Checkbox
                        label="Send separate prompt for each action"
                        name="step-send-separate-prompts"
                        tooltip="Each action sends its own prompt (step prompt + that action's prompt). An abort stops later actions before their prompts are sent."
                        checked={stepModalState?.step.sendSeparatePrompts ?? false}
                        onChange={(event) => {
                          const sendSeparatePrompts = event.target.checked;
                          updateStepDraft((step) => ({ ...step, sendSeparatePrompts }));
                        }}
                      />
                      <Checkbox
                        label="Use chat completions"
                        name="step-use-chat-completions"
                        tooltip="Runs as a conversation: the step prompt becomes the system prompt and each action is its own message that builds on earlier responses. Requires a deployment-based (API key) LLM."
                        checked={stepModalState?.step.useChatCompletions ?? false}
                        onChange={(event) => {
                          const useChatCompletions = event.target.checked;
                          updateStepDraft((step) =>
                            syncDefaultPrompt({ ...step, useChatCompletions }),
                          );
                        }}
                      />
                    </Row>
                    <Row className="field-grid" gap="1rem">
                      <div className="schedule-field-group">
                        <Select
                          name="step-llm"
                          label="LLM"
                          width="18rem"
                          isClearable
                          options={llmOptions}
                          value={findOptionByValue(llmOptions, stepModalState?.step.llmId) ?? null}
                          onChange={(newValue) => {
                            const llmId = toNumberOrUndefined(
                              newValue as { value?: unknown } | null,
                            );
                            updateStepDraft((step) => ({ ...step, llmId }));
                          }}
                        />
                        <p className="schedule-help-text">
                          Optional; overrides the profile LLM for this step.
                        </p>
                      </div>
                    </Row>
                    <styled.ModalPromptField>
                      <styled.PromptResetButton
                        type="button"
                        aria-label="Reset to the default prompt"
                        title="Reset to the default prompt"
                        onClick={() =>
                          updateStepDraft((step) => ({
                            ...step,
                            prompt: buildDefaultContentStepPrompt(
                              hasEnrichmentFilter(step),
                              effectivePromptTarget(step),
                              step.useChatCompletions,
                            ),
                          }))
                        }
                      >
                        <FaArrowRotateLeft />
                      </styled.PromptResetButton>
                      <Wysiwyg
                        className="modal-wysiwyg"
                        name="step-prompt"
                        label="Prompt"
                        value={stepModalState?.step.prompt ?? ''}
                        onChange={(prompt) => {
                          updateStepDraft((step) => ({ ...step, prompt: prompt ?? '' }));
                        }}
                      />
                      <p className="modal-help-text">
                        <span
                          className="step-modal-info-icon"
                          role="button"
                          tabIndex={0}
                          aria-label="Step prompt keyword help"
                          title="Step prompt keyword help"
                          onClick={() => openSectionDoc('stepPrompt')}
                          onKeyDown={(event) => {
                            if (event.key === 'Enter' || event.key === ' ') {
                              event.preventDefault();
                              openSectionDoc('stepPrompt');
                            }
                          }}
                        >
                          <FaCircleInfo />
                        </span>{' '}
                        The step prompt is the runtime instruction for this step. Use template
                        tokens (for example <code>{`{content}`}</code>, <code>{`{actions}`}</code>{' '}
                        or <code>{`{content.headline}`}</code>) to inject dynamic values. Click the
                        reset icon beside the Prompt label to reapply the default prompt.
                      </p>
                    </styled.ModalPromptField>
                  </div>
                }
                customButtons={
                  <Row justifyContent="flex-end" width="100%" gap="0.5rem">
                    <Button variant={ButtonVariant.secondary} onClick={closeStepModal}>
                      Cancel
                    </Button>
                    <Button
                      onClick={() => {
                        if (!stepModalState) return;
                        if (!values.filterId && stepModalState.step.target !== 'none') {
                          toast.error(
                            "The step target must be 'None' when the profile does not include a filter.",
                          );
                          return;
                        }
                        if (!!values.filterId && stepModalState.step.target === 'none') {
                          toast.error(
                            "Select a step target of 'Content', 'Start', or 'End' when the profile includes a filter.",
                          );
                          return;
                        }
                        const updatedSteps = [...orderedSteps];
                        if (
                          stepModalState.mode === 'edit' &&
                          typeof stepModalState.index === 'number'
                        ) {
                          updatedSteps[stepModalState.index] = cloneStep(stepModalState.step);
                        } else {
                          updatedSteps.push(cloneStep(stepModalState.step));
                        }
                        setFieldValue('steps', normalizeSteps(updatedSteps));
                        closeStepModal();
                      }}
                      disabled={!stepModalState?.step.name.trim()}
                    >
                      Done
                    </Button>
                  </Row>
                }
              />
              <Modal
                headerText={actionModalState?.mode === 'edit' ? 'Edit Action' : 'Add Action'}
                isShowing={isActionModalShowing}
                hide={closeActionModal}
                type="custom"
                component={
                  <div className="rule-modal-content">
                    <p className="modal-intro-text">
                      An action is an executable operation within a step. It defines the action
                      type, optional call limit, and model instructions used to produce a
                      confirmable result.
                    </p>
                    <Row className="field-grid action-header-row" gap="1rem">
                      <Text
                        width={FieldSize.Medium}
                        name="action-name"
                        label="Name"
                        required
                        value={actionModalState?.action.name ?? ''}
                        onChange={(event) => {
                          const name = event.target.value;
                          updateActionDraft((action) => ({ ...action, name }));
                        }}
                      />
                      <Select
                        name="action-type"
                        label="Action Type"
                        required
                        isClearable={false}
                        options={actionTypeOptionItems}
                        value={actionTypeOptionItems.find(
                          (option) => option.value === actionModalState?.action.actionType,
                        )}
                        onChange={(newValue) => {
                          const actionType = (newValue as { value?: string } | null)?.value;
                          if (!actionType) return;
                          updateActionDraft((action) => {
                            const synced = syncActionDefaults(
                              {
                                ...action,
                                // 'Always run' only applies to value-less action types.
                                autoExecute: AUTO_EXECUTE_ACTION_TYPES.includes(actionType)
                                  ? action.autoExecute
                                  : false,
                              },
                              actionType,
                              orderedSteps[actionModalState?.stepIndex ?? -1]?.useChatCompletions ??
                                false,
                            );
                            // Seed default configuration for the data-driven action types.
                            const settings: Record<string, any> = { ...(synced.settings ?? {}) };
                            if (actionType === 'extract-data' && !settings.extract) {
                              settings.extract = contentProperties.map((property) => ({
                                key: property,
                                value: defaultExtractValue(property),
                              }));
                            }
                            if (actionType === 'create-content' && !settings.mapping) {
                              settings.mapping = Object.fromEntries(
                                contentProperties.map((property) => [property, property]),
                              );
                            }
                            return {
                              ...synced,
                              // The filter only belongs to 'fetch-content'; leaving it set on
                              // another type would persist a reference nothing reads.
                              filterId:
                                actionType === FETCH_CONTENT_ACTION
                                  ? synced.filterId ?? null
                                  : null,
                              settings,
                            };
                          });
                        }}
                      />
                      <Show
                        visible={
                          actionModalState?.action.actionType === UPDATE_CONTENT_FIELD_ACTION
                        }
                      >
                        <Select
                          name="action-content-field"
                          label="Content Field"
                          required
                          isClearable={false}
                          options={contentFieldOptionItems}
                          value={
                            contentFieldOptionItems.find(
                              (option) => option.value === actionModalState?.action.contentField,
                            ) ?? null
                          }
                          onChange={(newValue) => {
                            const contentField =
                              (newValue as { value?: string } | null)?.value ?? null;
                            updateActionDraft((action) => applyContentField(action, contentField));
                          }}
                        />
                      </Show>
                      <Show
                        visible={
                          actionModalState?.action.actionType === ADD_ACTION_ACTION ||
                          actionModalState?.action.actionType === SELECT_TOP_ACTION
                        }
                      >
                        <Select
                          name="action-content-action"
                          label="Content Action"
                          options={actionOptions}
                          value={
                            findOptionByValue(
                              actionOptions,
                              actionModalState?.action.contentActionId,
                            ) ?? null
                          }
                          onChange={(newValue) => {
                            const contentActionId = toNumberOrUndefined(
                              newValue as { value?: unknown } | null,
                            );
                            updateActionDraft((action) => ({
                              ...action,
                              contentActionId: contentActionId ?? null,
                            }));
                          }}
                        />
                      </Show>
                      <Show
                        visible={
                          actionModalState?.action.actionType === SCORE_CONTENT_ACTION ||
                          actionModalState?.action.actionType === SELECT_TOP_ACTION
                        }
                      >
                        <Text
                          width={FieldSize.Small}
                          name="action-objective"
                          label="Objective"
                          required
                          value={actionModalState?.action.objective ?? ''}
                          onChange={(event) => {
                            const objective = event.target.value;
                            updateActionDraft((action) => applyObjective(action, objective));
                          }}
                        />
                      </Show>
                      {/* Create Content: identifier for later actions to reference. */}
                      <Show visible={actionModalState?.action.actionType === 'create-content'}>
                        <Text
                          width={FieldSize.Small}
                          name="action-create-identifier"
                          label="Identifier"
                          required
                          value={actionModalState?.action.createIdentifier ?? ''}
                          onChange={(event) => {
                            const createIdentifier = event.target.value;
                            updateActionDraft((action) => ({ ...action, createIdentifier }));
                          }}
                        />
                      </Show>
                    </Row>
                    {/* Extract Data: key/value grid. A token-only value is copied; a prompt generates it. */}
                    <Show visible={actionModalState?.action.actionType === 'extract-data'}>
                      <div className="extract-grid">
                        <p className="modal-intro-text">
                          Each row assigns a value to a key. A value that is only a content token
                          (e.g. {'{content.body}'}) is copied directly; otherwise it is sent as a
                          prompt to generate the value.
                        </p>
                        {extractRows.map((row, rowIndex) => (
                          <Row
                            key={rowIndex}
                            className="field-grid"
                            gap="0.5rem"
                            alignItems="flex-end"
                            nowrap
                          >
                            <Text
                              width={FieldSize.Small}
                              name={`extract-key-${rowIndex}`}
                              label={rowIndex === 0 ? 'Key' : ''}
                              value={row.key ?? ''}
                              onChange={(event) => {
                                const key = event.target.value;
                                updateActionDraft((action) => {
                                  const rows = [
                                    ...((action.settings?.extract as {
                                      key: string;
                                      value: string;
                                    }[]) ?? []),
                                  ];
                                  rows[rowIndex] = { ...rows[rowIndex], key };
                                  return {
                                    ...action,
                                    settings: { ...(action.settings ?? {}), extract: rows },
                                  };
                                });
                              }}
                            />
                            <Text
                              width={FieldSize.Big}
                              name={`extract-value-${rowIndex}`}
                              label={rowIndex === 0 ? 'Value (content token or prompt)' : ''}
                              value={row.value ?? ''}
                              onChange={(event) => {
                                const value = event.target.value;
                                updateActionDraft((action) => {
                                  const rows = [
                                    ...((action.settings?.extract as {
                                      key: string;
                                      value: string;
                                    }[]) ?? []),
                                  ];
                                  rows[rowIndex] = { ...rows[rowIndex], value };
                                  return {
                                    ...action,
                                    settings: { ...(action.settings ?? {}), extract: rows },
                                  };
                                });
                              }}
                            />
                            <button
                              type="button"
                              className="rule-icon-button delete"
                              aria-label="Remove key"
                              title="Remove key"
                              onClick={() =>
                                updateActionDraft((action) => {
                                  const rows = [...((action.settings?.extract as any[]) ?? [])];
                                  rows.splice(rowIndex, 1);
                                  return {
                                    ...action,
                                    settings: { ...(action.settings ?? {}), extract: rows },
                                  };
                                })
                              }
                            >
                              <FaTrash />
                            </button>
                          </Row>
                        ))}
                        <button
                          type="button"
                          className="rule-icon-button"
                          aria-label="Add key"
                          title="Add key"
                          onClick={() =>
                            updateActionDraft((action) => ({
                              ...action,
                              settings: {
                                ...(action.settings ?? {}),
                                extract: [
                                  ...((action.settings?.extract as any[]) ?? []),
                                  { key: '', value: '' },
                                ],
                              },
                            }))
                          }
                        >
                          <FaPlus /> Add key
                        </button>
                      </div>
                    </Show>
                    {/* Create Content: clone toggle + property-to-key mapping grid. */}
                    <Show visible={actionModalState?.action.actionType === 'create-content'}>
                      <Row className="field-grid" gap="1rem">
                        <Checkbox
                          label="Clone the original content item"
                          name="action-create-clone"
                          tooltip="Start the new item from a copy of the iterated item, then apply the mapped values. Otherwise the item is built only from the mapped extracted data."
                          checked={actionModalState?.action.createClone ?? false}
                          onChange={(event) => {
                            const createClone = event.target.checked;
                            updateActionDraft((action) => ({ ...action, createClone }));
                          }}
                        />
                      </Row>
                      <div className="mapping-grid">
                        <p className="modal-intro-text">
                          Apply extracted data to the new content item. Set each content property to
                          the Extract Data key to copy from (defaults to the same name).
                        </p>
                        {contentProperties.map((property) => (
                          <Row
                            key={property}
                            className="field-grid"
                            gap="0.5rem"
                            alignItems="center"
                            nowrap
                          >
                            <Col className="name-col">{property}</Col>
                            <Text
                              width={FieldSize.Medium}
                              name={`map-${property}`}
                              value={contentMapping[property] ?? property}
                              onChange={(event) => {
                                const mappedKey = event.target.value;
                                updateActionDraft((action) => ({
                                  ...action,
                                  settings: {
                                    ...(action.settings ?? {}),
                                    mapping: {
                                      ...(action.settings?.mapping ?? {}),
                                      [property]: mappedKey,
                                    },
                                  },
                                }));
                              }}
                            />
                          </Row>
                        ))}
                      </div>
                    </Show>
                    {/* Which content this action operates on (separate-prompt steps only). */}
                    <Show
                      visible={
                        !!worksOnStep?.sendSeparatePrompts &&
                        actionModalState?.action.actionType !== 'create-content' &&
                        actionModalState?.action.actionType !== 'extract-data' &&
                        actionModalState?.action.actionType !== FETCH_CONTENT_ACTION &&
                        worksOnOptions.length > 1
                      }
                    >
                      <Row className="field-grid" gap="1rem">
                        <Select
                          name="action-works-on"
                          label="Works on"
                          isClearable={false}
                          options={worksOnOptions}
                          value={
                            worksOnOptions.find(
                              (option) =>
                                option.value === (actionModalState?.action.worksOn || 'original'),
                            ) ?? worksOnOptions[0]
                          }
                          onChange={(newValue) => {
                            const worksOn =
                              (newValue as { value?: string } | null)?.value ?? 'original';
                            updateActionDraft((action) => ({ ...action, worksOn }));
                          }}
                        />
                      </Row>
                    </Show>
                    <Show visible={actionModalState?.action.actionType === RUN_NOTIFICATION_ACTION}>
                      <Row className="field-grid action-wide-select-row" gap="1rem">
                        <Select
                          name="action-notification"
                          label="Notification"
                          required
                          width="100%"
                          styles={{
                            // Replaces the component default; keep the portal above the modal.
                            menuPortal: (base: any) => ({ ...base, zIndex: 9999 }),
                            container: (base: any) => ({ ...base, width: '100%' }),
                            control: (base: any) => ({
                              ...base,
                              height: 'auto',
                              minHeight: '2.375rem',
                            }),
                            valueContainer: (base: any) => ({
                              ...base,
                              whiteSpace: 'normal',
                            }),
                            singleValue: (base: any) => ({
                              ...base,
                              whiteSpace: 'normal',
                              overflow: 'visible',
                              textOverflow: 'unset',
                            }),
                            option: (base: any) => ({
                              ...base,
                              whiteSpace: 'normal',
                              wordBreak: 'break-word',
                            }),
                          }}
                          options={notificationOptions}
                          value={
                            findOptionByValue(
                              notificationOptions,
                              actionModalState?.action.notificationId,
                            ) ?? null
                          }
                          onChange={(newValue) => {
                            const notificationId = toNumberOrUndefined(
                              newValue as { value?: unknown } | null,
                            );
                            updateActionDraft((action) => ({
                              ...action,
                              notificationId: notificationId ?? null,
                            }));
                          }}
                        />
                      </Row>
                    </Show>
                    <Show visible={actionModalState?.action.actionType === RUN_REPORT_ACTION}>
                      <Row className="field-grid action-wide-select-row" gap="1rem">
                        <Select
                          name="action-report"
                          label="Report"
                          required
                          width="100%"
                          styles={{
                            // Replaces the component default; keep the portal above the modal.
                            menuPortal: (base: any) => ({ ...base, zIndex: 9999 }),
                            container: (base: any) => ({ ...base, width: '100%' }),
                            control: (base: any) => ({
                              ...base,
                              height: 'auto',
                              minHeight: '2.375rem',
                            }),
                            valueContainer: (base: any) => ({
                              ...base,
                              whiteSpace: 'normal',
                            }),
                            singleValue: (base: any) => ({
                              ...base,
                              whiteSpace: 'normal',
                              overflow: 'visible',
                              textOverflow: 'unset',
                            }),
                            option: (base: any) => ({
                              ...base,
                              whiteSpace: 'normal',
                              wordBreak: 'break-word',
                            }),
                          }}
                          options={reportOptions}
                          value={
                            findOptionByValue(reportOptions, actionModalState?.action.reportId) ??
                            null
                          }
                          onChange={(newValue) => {
                            const reportId = toNumberOrUndefined(
                              newValue as { value?: unknown } | null,
                            );
                            updateActionDraft((action) => ({
                              ...action,
                              reportId: reportId ?? null,
                            }));
                          }}
                        />
                      </Row>
                    </Show>
                    <Show visible={actionModalState?.action.actionType === FETCH_CONTENT_ACTION}>
                      <Row className="field-grid action-wide-select-row" gap="1rem">
                        <Select
                          name="action-filter"
                          label="Filter"
                          required
                          width="100%"
                          styles={{
                            // Replaces the component default; keep the portal above the modal.
                            menuPortal: (base: any) => ({ ...base, zIndex: 9999 }),
                            container: (base: any) => ({ ...base, width: '100%' }),
                          }}
                          options={filterOptions}
                          value={
                            findOptionByValue(filterOptions, actionModalState?.action.filterId) ??
                            null
                          }
                          onChange={(newValue) => {
                            const filterId = toNumberOrUndefined(
                              newValue as { value?: unknown } | null,
                            );
                            updateActionDraft((action) => ({
                              ...action,
                              filterId: filterId ?? null,
                            }));
                          }}
                        />
                      </Row>
                      <Row className="field-grid" gap="1rem">
                        <Text
                          width={FieldSize.Small}
                          name="action-collection-max-items"
                          label="Max Items"
                          type="number"
                          value={
                            getActionSettingsGroup(
                              actionModalState?.action,
                              'collection',
                            ).maxItems?.toString() ?? ''
                          }
                          placeholder={`${DEFAULT_COLLECTION_MAX_ITEMS}`}
                          onChange={(event) => {
                            const raw = event.target.value;
                            const parsed = Number.parseInt(raw || '0', 10);
                            const maxItems =
                              raw === '' || Number.isNaN(parsed) || parsed < 1 ? undefined : parsed;
                            updateActionDraft((action) =>
                              setActionSettingsGroup(action, 'collection', { maxItems }),
                            );
                          }}
                        />
                      </Row>
                      <p className="schedule-help-text">
                        The filter runs once per run and its results are held in memory for later
                        actions to compare against. Only the fields a comparison needs are fetched
                        (headline, byline, summary, body, published date, source) and long text is
                        truncated, so keep Max Items no larger than the comparison actually
                        requires.
                      </p>
                    </Show>
                    <Show visible={actionModalState?.action.actionType === DEDUPLICATION_ACTION}>
                      <Row className="field-grid action-wide-select-row" gap="1rem">
                        <Select
                          name="action-prior-action"
                          label="Prior Action"
                          required
                          width="100%"
                          styles={{
                            // Replaces the component default; keep the portal above the modal.
                            menuPortal: (base: any) => ({ ...base, zIndex: 9999 }),
                            container: (base: any) => ({ ...base, width: '100%' }),
                            control: (base: any) => ({
                              ...base,
                              height: 'auto',
                              minHeight: '2.375rem',
                            }),
                            valueContainer: (base: any) => ({
                              ...base,
                              whiteSpace: 'normal',
                            }),
                            singleValue: (base: any) => ({
                              ...base,
                              whiteSpace: 'normal',
                              overflow: 'visible',
                              textOverflow: 'unset',
                            }),
                            option: (base: any) => ({
                              ...base,
                              whiteSpace: 'normal',
                              wordBreak: 'break-word',
                            }),
                          }}
                          options={priorActionOptions}
                          value={
                            findOptionByValue(
                              priorActionOptions,
                              actionModalState?.action.priorActionId,
                            ) ?? null
                          }
                          onChange={(newValue) => {
                            const priorActionId = toNumberOrUndefined(
                              newValue as { value?: unknown } | null,
                            );
                            updateActionDraft((action) => ({
                              ...action,
                              priorActionId: priorActionId ?? null,
                            }));
                          }}
                        />
                      </Row>
                      <Row className="field-grid" gap="1rem">
                        <Select
                          name="action-deduplicate-mode"
                          label="Comparison"
                          isClearable={false}
                          width={FieldSize.Medium}
                          styles={{
                            // Replaces the component default; keep the portal above the modal.
                            menuPortal: (base: any) => ({ ...base, zIndex: 9999 }),
                          }}
                          options={deduplicateModeOptions}
                          value={
                            deduplicateModeOptions.find(
                              (option) =>
                                option.value ===
                                (getActionSettingsGroup(actionModalState?.action, 'deduplicate')
                                  .mode ?? 'iterate'),
                            ) ?? deduplicateModeOptions[0]
                          }
                          onChange={(newValue) => {
                            const mode =
                              (newValue as { value?: string } | null)?.value ?? 'iterate';
                            updateActionDraft((action) => {
                              const updated = setActionSettingsGroup(action, 'deduplicate', {
                                mode,
                              });
                              // The two modes need different prompts: batch mode has to name which
                              // candidate matched, so its confirmation captures the content id.
                              // Only replace values the admin has not customized.
                              const iterateDefaults = actionTypeDefaults[DEDUPLICATION_ACTION];
                              const from =
                                mode === 'batch' ? iterateDefaults : deduplicateBatchDefaults;
                              const to =
                                mode === 'batch' ? deduplicateBatchDefaults : iterateDefaults;
                              return {
                                ...updated,
                                prompt:
                                  !action.prompt ||
                                  action.prompt === '<p><br></p>' ||
                                  action.prompt === from.prompt
                                    ? to.prompt
                                    : action.prompt,
                                confirmationStatement:
                                  !action.confirmationStatement ||
                                  action.confirmationStatement === from.confirmationStatement
                                    ? to.confirmationStatement
                                    : action.confirmationStatement,
                              };
                            });
                          }}
                        />
                        <Show
                          visible={
                            getActionSettingsGroup(actionModalState?.action, 'deduplicate').mode ===
                            'batch'
                          }
                        >
                          <Text
                            width={FieldSize.Small}
                            name="action-deduplicate-batch-size"
                            label="Batch Size"
                            type="number"
                            value={
                              getActionSettingsGroup(
                                actionModalState?.action,
                                'deduplicate',
                              ).batchSize?.toString() ?? ''
                            }
                            placeholder={`${DEFAULT_DEDUPLICATE_BATCH_SIZE}`}
                            onChange={(event) => {
                              const raw = event.target.value;
                              const parsed = Number.parseInt(raw || '0', 10);
                              const batchSize =
                                raw === '' || Number.isNaN(parsed) || parsed < 1
                                  ? undefined
                                  : parsed;
                              updateActionDraft((action) =>
                                setActionSettingsGroup(action, 'deduplicate', { batchSize }),
                              );
                            }}
                          />
                        </Show>
                        <Text
                          width={FieldSize.Small}
                          name="action-deduplicate-max-comparisons"
                          label="Max Candidates"
                          type="number"
                          value={
                            getActionSettingsGroup(
                              actionModalState?.action,
                              'deduplicate',
                            ).maxComparisons?.toString() ?? ''
                          }
                          placeholder="No max"
                          onChange={(event) => {
                            const raw = event.target.value;
                            const parsed = Number.parseInt(raw || '0', 10);
                            const maxComparisons =
                              raw === '' || Number.isNaN(parsed) || parsed < 1 ? undefined : parsed;
                            updateActionDraft((action) =>
                              setActionSettingsGroup(action, 'deduplicate', { maxComparisons }),
                            );
                          }}
                        />
                      </Row>
                      <p className="schedule-help-text">
                        One prompt per item is the most precise but costs one LLM call for every
                        candidate. Batching sends many candidates in a single prompt, so comparing
                        against a fetched collection takes a fraction of the calls. Max Candidates
                        caps how many are examined per content item.
                      </p>
                    </Show>
                    <Row className="field-grid" gap="1rem">
                      <Show
                        visible={
                          !!actionModalState &&
                          (orderedSteps[actionModalState.stepIndex]?.sendSeparatePrompts ?? false)
                        }
                      >
                        <div className="schedule-field-group">
                          <Select
                            name="action-llm"
                            label="LLM"
                            width="18rem"
                            isClearable
                            options={llmOptions}
                            value={
                              findOptionByValue(llmOptions, actionModalState?.action.llmId) ?? null
                            }
                            onChange={(newValue) => {
                              const llmId = toNumberOrUndefined(
                                newValue as { value?: unknown } | null,
                              );
                              updateActionDraft((action) => ({ ...action, llmId }));
                            }}
                          />
                          <p className="schedule-help-text">
                            Optional; used for this action's separate prompt.
                          </p>
                        </div>
                      </Show>
                      {/* 'Fetch Content Collection' runs once per run regardless of how often the
                          step reaches it, so an execution cap has no meaning for it. */}
                      <Show visible={actionModalState?.action.actionType !== FETCH_CONTENT_ACTION}>
                        <Text
                          width={FieldSize.Small}
                          name="action-max-calls"
                          label="Max Calls"
                          type="number"
                          value={actionModalState?.action.maxCalls?.toString() ?? ''}
                          placeholder="No max"
                          onChange={(event) => {
                            const raw = event.target.value;
                            const parsed = Number.parseInt(raw || '0', 10);
                            const maxCalls =
                              raw === '' ? null : Math.max(0, Number.isNaN(parsed) ? 0 : parsed);
                            updateActionDraft((action) => ({ ...action, maxCalls }));
                          }}
                        />
                      </Show>
                      <styled.ModalEnabledCheckbox className="no-label-offset">
                        <Checkbox
                          label="Enabled"
                          name="action-enabled"
                          checked={actionModalState?.action.isEnabled ?? false}
                          onChange={(event) => {
                            const isEnabled = event.target.checked;
                            updateActionDraft((action) => ({ ...action, isEnabled }));
                          }}
                        />
                      </styled.ModalEnabledCheckbox>
                      <Show visible={!(actionModalState?.action.autoExecute ?? false)}>
                        <styled.ModalEnabledCheckbox className="no-label-offset">
                          <Checkbox
                            label="Abort if no confirmation"
                            name="action-abort-if-no-confirmation"
                            tooltip="Stop the remaining actions on this step if this action's confirmation is not received (e.g. stop further actions when content was not published)."
                            checked={actionModalState?.action.abortIfNoConfirmation ?? false}
                            onChange={(event) => {
                              const abortIfNoConfirmation = event.target.checked;
                              updateActionDraft((action) => ({ ...action, abortIfNoConfirmation }));
                            }}
                          />
                        </styled.ModalEnabledCheckbox>
                      </Show>
                      <Show
                        visible={AUTO_EXECUTE_ACTION_TYPES.includes(
                          actionModalState?.action.actionType ?? '',
                        )}
                      >
                        <styled.ModalEnabledCheckbox className="no-label-offset">
                          <Checkbox
                            label="Always run"
                            name="action-auto-execute"
                            tooltip="Execute unconditionally; no LLM confirmation is required and the action prompt is not sent to the model."
                            checked={actionModalState?.action.autoExecute ?? false}
                            onChange={(event) => {
                              const autoExecute = event.target.checked;
                              updateActionDraft((action) => ({ ...action, autoExecute }));
                            }}
                          />
                        </styled.ModalEnabledCheckbox>
                      </Show>
                    </Row>
                    {/* 'Extract Data' and 'Create Content' are deterministic (driven by their
                        grid/mapping) and 'Fetch Content Collection' only runs its filter; the
                        engine ignores the action prompt and confirmation statement for these
                        types, so the fields are hidden to avoid confusion. */}
                    <Show
                      visible={
                        actionModalState?.action.actionType !== 'create-content' &&
                        actionModalState?.action.actionType !== 'extract-data' &&
                        actionModalState?.action.actionType !== FETCH_CONTENT_ACTION
                      }
                    >
                      <styled.ModalPromptField>
                        <styled.ActionPromptHelpButton
                          type="button"
                          aria-label="Action prompt help"
                          title="Action prompt help"
                          onClick={() => openSectionDoc('actionPrompt')}
                        >
                          <FaCircleInfo />
                        </styled.ActionPromptHelpButton>
                        <Wysiwyg
                          className="modal-wysiwyg"
                          name="action-prompt"
                          label="Action Prompt"
                          value={actionModalState?.action.prompt ?? ''}
                          onChange={(prompt) => {
                            updateActionDraft((action) => ({ ...action, prompt }));
                          }}
                        />
                        <p className="modal-help-text">
                          Action prompt is sent to the model for this action and can use output from
                          the step context. Keep it explicit about expected response format.
                        </p>
                      </styled.ModalPromptField>
                      <Show visible={!actionModalState?.action.autoExecute}>
                        <styled.ModalPromptField>
                          <TextArea
                            name="action-confirmation"
                            label="Confirmation Statement"
                            width="100%"
                            rows={2}
                            value={actionModalState?.action.confirmationStatement ?? ''}
                            onChange={(event) => {
                              const confirmationStatement = event.target.value;
                              updateActionDraft((action) => ({ ...action, confirmationStatement }));
                            }}
                          />
                          <p className="modal-help-text">
                            Confirmation Statement is the exact phrase the response must include
                            before the action is treated as confirmed and eligible to execute.
                          </p>
                        </styled.ModalPromptField>
                      </Show>
                    </Show>
                  </div>
                }
                customButtons={
                  <Row justifyContent="flex-end" width="100%" gap="0.5rem">
                    <Button variant={ButtonVariant.secondary} onClick={closeActionModal}>
                      Cancel
                    </Button>
                    <Button
                      onClick={() => {
                        if (!actionModalState) return;
                        if (!actionModalState.action.name.trim()) {
                          toast.error('The action Name is required.');
                          return;
                        }
                        if (
                          actionModalState.action.actionType === UPDATE_CONTENT_FIELD_ACTION &&
                          !actionModalState.action.contentField
                        ) {
                          toast.error(
                            "The Content Field is required when the action type is 'Update Content Field'.",
                          );
                          return;
                        }
                        if (
                          (actionModalState.action.actionType === SCORE_CONTENT_ACTION ||
                            actionModalState.action.actionType === SELECT_TOP_ACTION) &&
                          !actionModalState.action.objective?.trim()
                        ) {
                          toast.error(
                            "The Objective is required when the action type is 'Score Content' or 'Select Top Content'.",
                          );
                          return;
                        }
                        if (
                          actionModalState.action.actionType === SELECT_TOP_ACTION &&
                          !actionModalState.action.contentActionId
                        ) {
                          toast.error(
                            "The Content Action is required when the action type is 'Select Top Content'.",
                          );
                          return;
                        }
                        if (
                          actionModalState.action.actionType === RUN_REPORT_ACTION &&
                          !actionModalState.action.reportId
                        ) {
                          toast.error(
                            "The Report is required when the action type is 'Publish Report'.",
                          );
                          return;
                        }
                        if (
                          actionModalState.action.actionType === RUN_NOTIFICATION_ACTION &&
                          !actionModalState.action.notificationId
                        ) {
                          toast.error(
                            "The Notification is required when the action type is 'Publish Notification'.",
                          );
                          return;
                        }
                        if (
                          actionModalState.action.actionType === DEDUPLICATION_ACTION &&
                          !actionModalState.action.priorActionId
                        ) {
                          toast.error(
                            "The Prior Action is required when the action type is 'Deduplication'. Save the profile first if the action you want to reference is new.",
                          );
                          return;
                        }
                        if (
                          actionModalState.action.actionType === FETCH_CONTENT_ACTION &&
                          !actionModalState.action.filterId
                        ) {
                          toast.error(
                            "The Filter is required when the action type is 'Fetch Content Collection'.",
                          );
                          return;
                        }
                        const updatedSteps = [...orderedSteps];
                        const step = updatedSteps[actionModalState.stepIndex];
                        if (!step) return;

                        const updatedActions = [...step.actions];
                        if (
                          actionModalState.mode === 'edit' &&
                          typeof actionModalState.actionIndex === 'number'
                        ) {
                          updatedActions[actionModalState.actionIndex] = cloneAction(
                            actionModalState.action,
                          );
                        } else {
                          updatedActions.push(cloneAction(actionModalState.action));
                        }

                        updatedSteps[actionModalState.stepIndex] = {
                          ...step,
                          actions: updatedActions,
                        };

                        setFieldValue('steps', normalizeSteps(updatedSteps));
                        closeActionModal();
                      }}
                    >
                      Done
                    </Button>
                  </Row>
                }
              />
              <Modal
                headerText="Confirm Step Removal"
                body={`Are you sure you wish to remove ${stepDeleteState?.name ?? 'this step'}?`}
                isShowing={isStepDeleteModalShowing}
                hide={closeStepDeleteModal}
                type="delete"
                confirmText="Yes, Remove Step"
                onConfirm={() => {
                  if (stepDeleteState) {
                    setFieldValue(
                      'steps',
                      normalizeSteps(
                        orderedSteps.filter((_, stepIndex) => stepIndex !== stepDeleteState.index),
                      ),
                    );
                    // Drop the removed step's collapsed flag and shift the ones after it.
                    setCollapsedSteps(
                      (collapsed) =>
                        new Set(
                          Array.from(collapsed)
                            .filter((i) => i !== stepDeleteState.index)
                            .map((i) => (i > stepDeleteState.index ? i - 1 : i)),
                        ),
                    );
                  }
                  closeStepDeleteModal();
                }}
              />
              <Modal
                headerText="Confirm Action Removal"
                body={`Are you sure you wish to remove ${
                  actionDeleteState?.name ?? 'this action'
                }?`}
                isShowing={isActionDeleteModalShowing}
                hide={closeActionDeleteModal}
                type="delete"
                confirmText="Yes, Remove Action"
                onConfirm={() => {
                  if (actionDeleteState) {
                    const updatedSteps = [...orderedSteps];
                    const step = updatedSteps[actionDeleteState.stepIndex];
                    if (step) {
                      updatedSteps[actionDeleteState.stepIndex] = {
                        ...step,
                        actions: step.actions.filter(
                          (_, actionIndex) => actionIndex !== actionDeleteState.actionIndex,
                        ),
                      };
                      setFieldValue('steps', normalizeSteps(updatedSteps));
                    }
                  }
                  closeActionDeleteModal();
                }}
              />
              <Modal
                headerText={scheduleModalState?.mode === 'edit' ? 'Edit Schedule' : 'Add Schedule'}
                isShowing={isScheduleModalShowing}
                hide={() => {
                  setScheduleModalState(null);
                  toggleScheduleModal();
                }}
                type="custom"
                component={
                  <div className="rule-modal-content">
                    <p className="modal-intro-text">
                      The Scheduler service queues a run once per day at (or after) the Run At time
                      on the selected week days. The schedule is not eligible to run before Start
                      After - set it to a future date/time so a schedule created after its Run At
                      time has already passed does not run on the day it is created.
                    </p>
                    <Row className="field-grid" gap="1rem">
                      <Text
                        width={FieldSize.Medium}
                        name="schedule-name"
                        label="Name"
                        required
                        value={scheduleModalState?.schedule.name ?? ''}
                        onChange={(event) => {
                          const name = event.target.value;
                          setScheduleModalState((state) =>
                            state ? { ...state, schedule: { ...state.schedule, name } } : state,
                          );
                        }}
                      />
                      <Text
                        width={FieldSize.Small}
                        name="schedule-start-at"
                        label="Run At"
                        type="time"
                        value={(scheduleModalState?.schedule.startAt ?? '').slice(0, 5)}
                        onChange={(event) => {
                          const time = event.target.value;
                          setScheduleModalState((state) =>
                            state
                              ? {
                                  ...state,
                                  schedule: {
                                    ...state.schedule,
                                    startAt: time ? `${time}:00` : null,
                                  },
                                }
                              : state,
                          );
                        }}
                      />
                      <Text
                        width={FieldSize.Medium}
                        name="schedule-run-on"
                        label="Start After"
                        type="datetime-local"
                        value={
                          scheduleModalState?.schedule.runOn
                            ? moment(scheduleModalState.schedule.runOn).format('YYYY-MM-DDTHH:mm')
                            : ''
                        }
                        onChange={(event) => {
                          // Sent as a UTC ISO string: the API binds it to a DateTime with
                          // Kind=Utc, which is what both the 'timestamp with time zone' column
                          // and the Scheduler's ToTimeZone conversion require.
                          const runOn = event.target.value
                            ? moment(event.target.value).toISOString()
                            : null;
                          setScheduleModalState((state) =>
                            state ? { ...state, schedule: { ...state.schedule, runOn } } : state,
                          );
                        }}
                      />
                    </Row>
                    <p className="schedule-help-text">
                      Leave Start After empty to run at any time.
                    </p>
                    <div className="schedule-field-group">
                      <span className="schedule-label" title="Select none to run every day.">
                        Run On
                      </span>
                      <Row gap="0.5rem" nowrap>
                        {scheduleWeekDayOptions.map((day) => (
                          <Checkbox
                            key={day.value}
                            label={day.label}
                            name={`schedule-day-${day.value}`}
                            checked={(scheduleModalState?.schedule.runOnWeekDays ?? []).includes(
                              day.value,
                            )}
                            onChange={(event) => {
                              const checked = event.target.checked;
                              setScheduleModalState((state) => {
                                if (!state) return state;
                                const current = state.schedule.runOnWeekDays ?? [];
                                const runOnWeekDays = checked
                                  ? [...current, day.value]
                                  : current.filter((value) => value !== day.value);
                                return { ...state, schedule: { ...state.schedule, runOnWeekDays } };
                              });
                            }}
                          />
                        ))}
                      </Row>
                    </div>
                    <Checkbox
                      label="Enabled"
                      name="schedule-enabled"
                      checked={scheduleModalState?.schedule.isEnabled ?? false}
                      onChange={(event) => {
                        const isEnabled = event.target.checked;
                        setScheduleModalState((state) =>
                          state ? { ...state, schedule: { ...state.schedule, isEnabled } } : state,
                        );
                      }}
                    />
                  </div>
                }
                customButtons={
                  <Row justifyContent="flex-end" width="100%" gap="0.5rem">
                    <Button
                      className="header-btn-outline"
                      variant={ButtonVariant.secondary}
                      onClick={() => {
                        setScheduleModalState(null);
                        toggleScheduleModal();
                      }}
                    >
                      Cancel
                    </Button>
                    <Button
                      className="header-btn-save"
                      onClick={() => {
                        if (!scheduleModalState) return;
                        if (!scheduleModalState.schedule.name.trim()) {
                          toast.error('The schedule Name is required.');
                          return;
                        }
                        const schedules = [...(values.schedules ?? [])];
                        if (
                          scheduleModalState.mode === 'edit' &&
                          typeof scheduleModalState.index === 'number'
                        ) {
                          schedules[scheduleModalState.index] = scheduleModalState.schedule;
                        } else {
                          schedules.push(scheduleModalState.schedule);
                        }
                        setFieldValue('schedules', schedules);
                        setScheduleModalState(null);
                        toggleScheduleModal();
                      }}
                    >
                      {scheduleModalState?.mode === 'edit' ? 'Save' : 'Add'}
                    </Button>
                  </Row>
                }
              />
              <Modal
                headerText={`Run #${runDetail?.id ?? ''}${runDetail?.isDryRun ? ' (dry run)' : ''}`}
                isShowing={isRunDetailModalShowing}
                hide={closeRunDetail}
                type="custom"
                component={
                  <div className="rule-modal-content run-detail-content">
                    <div className="run-detail-summary">
                      <div>
                        <label>Status:</label> <span>{String(runDetail?.status ?? '-')}</span>
                      </div>
                      <div>
                        <label>Trigger:</label>{' '}
                        <span>
                          {runDetail?.trigger ?? '-'}
                          {runDetail?.isDryRun ? ' • DRY' : ''}
                        </span>
                      </div>
                      <div>
                        <label>Started:</label> <span>{formatRunTime(runDetail?.startedOn)}</span>
                      </div>
                      <div>
                        <label>Completed:</label>{' '}
                        <span>{formatRunTime(runDetail?.completedOn)}</span>
                      </div>
                    </div>
                    <Show visible={!!runDetail?.note}>
                      <div className="run-detail-note">
                        <label>Note:</label> <span>{runDetail?.note}</span>
                      </div>
                    </Show>
                    {(() => {
                      const v2Summary = parseV2RunSummary(runDiff?.run?.summary);
                      if (!v2Summary) return null;
                      return (
                        <>
                          <Row gap="0.5rem" className="v2-run-detail-toggle">
                            <Button
                              variant={
                                runDetailView === 'outcome'
                                  ? ButtonVariant.primary
                                  : ButtonVariant.secondary
                              }
                              onClick={() => setRunDetailView('outcome')}
                            >
                              Outcome
                            </Button>
                            <Button
                              variant={
                                runDetailView === 'log'
                                  ? ButtonVariant.primary
                                  : ButtonVariant.secondary
                              }
                              onClick={() => setRunDetailView('log')}
                            >
                              Decision Log
                            </Button>
                          </Row>
                          <Show visible={runDetailView === 'outcome'}>
                            <V2RunOutcome summary={v2Summary} />
                          </Show>
                          <Show visible={runDetailView === 'log' && !!runDetail}>
                            <RunLogViewer
                              runId={runDetail!.id}
                              onFetch={api.findRunLogs}
                              onExplain={api.explainRunLog}
                              promptNames={Object.keys(
                                parseV2Definition(values.definition).prompts,
                              )}
                              onApplyPrompt={(name, text) => {
                                const definition = parseV2Definition(values.definition);
                                definition.prompts[name] = { ...definition.prompts[name], text };
                                setFieldValue('definition', serializeV2Definition(definition));
                              }}
                            />
                          </Show>
                        </>
                      );
                    })()}
                    <Show visible={!parseV2RunSummary(runDiff?.run?.summary)}>
                      <h2>LLM Responses</h2>
                      <Show visible={!!runDiff && (runDiff.responses ?? []).length > 0}>
                        <div className="run-detail-responses">
                          {(runDiff?.responses ?? []).map((response, index) => (
                            <div key={index} className="run-detail-response">
                              <label>
                                {response.stepName}
                                {response.actionName ? ` / ${response.actionName}` : ''}
                                {response.contentId ? ` — content ${response.contentId}` : ''}:
                              </label>
                              <Show visible={!!response.prompt}>
                                <details className="run-detail-prompt">
                                  <summary>Prompt</summary>
                                  <pre>{response.prompt}</pre>
                                </details>
                              </Show>
                              <pre>{response.response}</pre>
                            </div>
                          ))}
                        </div>
                      </Show>
                      <Show visible={!runDiff || (runDiff.responses ?? []).length === 0}>
                        <p className="modal-help-text">
                          No LLM responses have been recorded for this run.
                        </p>
                      </Show>
                      <h2>Action Outcomes</h2>
                      <Show
                        visible={
                          !!runDiff && (runDiff.changes.length > 0 || runDiff.stepHits.length > 0)
                        }
                      >
                        <pre className="run-detail-outcomes">
                          {JSON.stringify(
                            { changes: runDiff?.changes, stepHits: runDiff?.stepHits },
                            null,
                            2,
                          )}
                        </pre>
                      </Show>
                      <Show
                        visible={
                          !runDiff ||
                          (runDiff.changes.length === 0 && runDiff.stepHits.length === 0)
                        }
                      >
                        <p className="modal-help-text">
                          No action outcomes have been recorded for this run.
                        </p>
                      </Show>
                    </Show>
                  </div>
                }
                customButtons={
                  <Row justifyContent="flex-end" width="100%">
                    <Button variant={ButtonVariant.secondary} onClick={closeRunDetail}>
                      Close
                    </Button>
                  </Row>
                }
              />
              <Modal
                headerText="Confirm Run Removal"
                body={`Are you sure you wish to delete run #${
                  runDeleteState?.id ?? ''
                } from the run history? This cannot be undone.`}
                isShowing={!!runDeleteState}
                hide={closeRunDelete}
                type="delete"
                confirmText="Yes, Delete Run"
                onConfirm={async () => {
                  const run = runDeleteState;
                  if (!run) return closeRunDelete();
                  try {
                    await api.deleteRun(run.id);
                    setRuns((runs) => runs.filter((r) => r.id !== run.id));
                    // The in-progress poll keys off the latest run; drop it so it does not keep
                    // polling for a run that no longer exists.
                    setLastRun((last) => (last?.id === run.id ? null : last));
                    toast.success(`Run #${run.id} has been deleted.`);
                  } finally {
                    closeRunDelete();
                  }
                }}
              />
            </div>
          );
        }}
      </FormikForm>
    </styled.AutomationProfileForm>
  );
};

export default AutomationProfileForm;
