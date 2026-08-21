/* eslint-disable simple-import-sort/imports */
import { FormikForm } from 'components/formik';
import moment from 'moment';
import React from 'react';
import { FaEdit, FaHistory, FaPlus, FaTrash } from 'react-icons/fa';
import { FaRegCalendar, FaRegClock } from 'react-icons/fa6';
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
  SelectDate,
  Show,
  Tab,
  Tabs,
  Text,
  useModal,
} from 'tno-core';

import {
  AutomationSchema,
  DATE_PICKER_PORTAL_ID,
  defaultAutomationProfile,
  getRunColumns,
} from './constants';
import {
  type IAutomationProfileModel,
  type IAutomationRunDiffModel,
  type IAutomationRunModel,
  type IAutomationScheduleModel,
} from './interfaces';
import {
  type IAutomationActionDescriptor,
  parseDefinition,
  parseRunSummary,
  RunLogViewer,
  serializeDefinition,
  AutomationDesigner,
  RunOutcome,
  collectFilterIds,
  collectLlmIds,
  remapDefinition,
} from './designer';
import {
  buildProfileForExport,
  buildProfileForSave,
  createDefaultSchedule,
  createOption,
  findOptionByValue,
  formatRunTime,
  formatScheduleWeekDays,
  normalizeProfile,
  scheduleWeekDayOptions,
  toNumberOrUndefined,
} from './utils';
import { AutomationDebugging } from './AutomationDebugging';
import { SectionInfoButton } from './SectionInfoButton';
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
  const { toggle: toggleScheduleModal, isShowing: isScheduleModalShowing } = useModal();

  const [llmOptions, setLLMOptions] = React.useState<IOptionItem[]>([]);
  const [filterOptions, setFilterOptions] = React.useState<IOptionItem[]>([]);
  const [actionOptions, setActionOptions] = React.useState<IOptionItem[]>([]);
  const [reportOptions, setReportOptions] = React.useState<IOptionItem[]>([]);
  const [notificationOptions, setNotificationOptions] = React.useState<IOptionItem[]>([]);
  const [scheduleModalState, setScheduleModalState] = React.useState<{
    mode: 'add' | 'edit';
    index?: number;
    schedule: IAutomationScheduleModel;
  } | null>(null);
  const [profile, setProfile] = React.useState<IAutomationProfileModel>(
    normalizeProfile(state?.profile),
  );
  const [activeTab, setActiveTab] = React.useState<
    'profile' | 'schedules' | 'runs' | 'livelog' | 'debugging'
  >('profile');
  const [runs, setRuns] = React.useState<IAutomationRunModel[]>([]);
  const [lastRun, setLastRun] = React.useState<IAutomationRunModel | null>(null);
  const [isRunning, setIsRunning] = React.useState(false);
  const importFileRef = React.useRef<HTMLInputElement>(null);
  const [runDetail, setRunDetail] = React.useState<IAutomationRunModel | null>(null);
  const [runDiff, setRunDiff] = React.useState<IAutomationRunDiffModel | null>(null);
  const { toggle: toggleRunDetailModal, isShowing: isRunDetailModalShowing } = useModal();
  // The run pending deletion; also drives the confirmation modal's visibility.
  const [runDeleteState, setRunDeleteState] = React.useState<IAutomationRunModel | null>(null);
  // The action catalog (fetched once); the designer renders action forms from it.
  const [actionDescriptors, setActionDescriptors] = React.useState<IAutomationActionDescriptor[]>(
    [],
  );
  // What the run detail modal shows: the outcome summary or the decision log.
  const [runDetailView, setRunDetailView] = React.useState<'outcome' | 'log'>('outcome');

  const profileId = Number(id);
  const hub = useApiHub();
  const refreshFilters = React.useCallback(() => {
    // Silent: must not trigger the page loading overlay (the repaint can swallow clicks).
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

  // Download the current profile as a JSON file, including the definitions of every referenced
  // filter and LLM (so the profile is self-contained), without primary keys. The LLM api_key is
  // intentionally excluded - a credential must not be written to an export file.
  const handleExport = async (values: IAutomationProfileModel) => {
    try {
      const filterIds = new Set<number>();
      const llmIds = new Set<number>();
      if (values.llmId) llmIds.add(values.llmId);
      // Filters and LLM overrides live inside the definition document.
      if (values.definition) {
        const definition = parseDefinition(values.definition);
        collectFilterIds(definition).forEach((id) => filterIds.add(id));
        collectLlmIds(definition).forEach((id) => llmIds.add(id));
      }

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
      if (imported.schemaVersion < 2 || !imported.definition) {
        toast.error('Only definition (schema version 2) profile exports can be imported.');
        return;
      }

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

      // Rewrite the filter/LLM ids inside the definition document through the same maps.
      const importedDefinition = serializeDefinition(
        remapDefinition(parseDefinition(imported.definition), mapFilter, mapLLM),
      );

      setProfile({
        ...imported,
        definition: importedDefinition,
        id: 0,
        // Suffix the name so it does not collide with the source profile on Save (editable).
        name: imported.name ? `${imported.name} (imported)` : imported.name,
        llmId: mapLLM(imported.llmId),
        schedules: (imported.schedules ?? []).map((schedule) => ({ ...schedule, id: 0 })),
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
          if ((data.schemaVersion ?? 1) < 2) {
            toast.error('This page only supports definition (schema version 2) profiles.');
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
      .getDescriptors()
      .then(setActionDescriptors)
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

  const handleSubmit = async (values: IAutomationProfileModel) => {
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
                      label="Live Log"
                      className={activeTab === 'livelog' ? 'tab-active' : undefined}
                      active={activeTab === 'livelog'}
                      disabled={!values.id}
                      onClick={() => {
                        setActiveTab('livelog');
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
                        <Button
                          type="button"
                          className="header-btn-outline header-btn-gap"
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
                      <Show visible={!!values.id}>
                        <Button
                          type="button"
                          className="header-btn-delete header-btn-gap"
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
                  <div className={`tab-panel${activeTab === 'livelog' ? ' active' : ''}`}>
                    <Col className="form-inputs">
                      <Row className="section-header" nowrap>
                        <Row className="section-header-title" nowrap>
                          <h2>Live Log</h2>
                        </Row>
                      </Row>
                      <Show visible={runs.length === 0}>
                        <p className="section-help-text">No runs have been recorded.</p>
                      </Show>
                      <Show visible={runs.length > 0}>
                        <p className="section-help-text">
                          Run #{(lastRun ?? runs[0])?.id} — {(lastRun ?? runs[0])?.status}
                          {(lastRun ?? runs[0])?.status === 'Running'
                            ? ' (streaming, refreshes every 5s)'
                            : ''}
                        </p>
                        <RunLogViewer
                          runId={(lastRun ?? runs[0])?.id ?? 0}
                          live={(lastRun ?? runs[0])?.status === 'Running'}
                          onFetch={api.findRunLogs}
                          onExplain={api.explainRunLog}
                          promptNames={Object.keys(parseDefinition(values.definition).prompts)}
                          onApplyPrompt={(name, text) => {
                            const definition = parseDefinition(values.definition);
                            definition.prompts[name] = { ...definition.prompts[name], text };
                            setFieldValue('definition', serializeDefinition(definition));
                          }}
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
                          <SectionInfoButton doc="schedule" />
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
                        <SectionInfoButton doc="profile" />
                      </Row>
                      <p className="section-help-text">
                        Configure the profile, shared prompts and schedule, then define steps by
                        phase: init runs once, process runs per item, complete runs after all steps.
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
                        <div className="checkbox-inline">
                          <FormikCheckbox
                            labelPosition={LabelPosition.Right}
                            label="Is Enabled"
                            name="isEnabled"
                          />
                        </div>
                      </Row>
                      <Row className="field-grid description-row" gap="1rem">
                        <FormikTextArea
                          name="description"
                          label="Description"
                          width={FieldSize.Stretch}
                        />
                      </Row>
                      <AutomationDesigner
                        value={values.definition}
                        onChange={(definition) => setFieldValue('definition', definition)}
                        descriptors={actionDescriptors}
                        filterOptions={filterOptions}
                        llmOptions={llmOptions}
                        reportOptions={reportOptions}
                        notificationOptions={notificationOptions}
                        actionOptions={actionOptions}
                        onValidate={async (definition) =>
                          api.validateProfile({ ...values, definition })
                        }
                      />
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
                      <div className="schedule-picker">
                        <SelectDate
                          width={FieldSize.Small}
                          name="schedule-start-at"
                          label="Run At"
                          dateFormat="h:mm aa"
                          showTimeSelect
                          showTimeSelectOnly
                          timeIntervals={30}
                          timeCaption=""
                          portalId={DATE_PICKER_PORTAL_ID}
                          selectedDate={
                            scheduleModalState?.schedule.startAt
                              ? moment(
                                  scheduleModalState.schedule.startAt,
                                  'HH:mm:ss',
                                ).toISOString()
                              : undefined
                          }
                          onChange={(date) => {
                            const startAt = date ? moment(date as Date).format('HH:mm:00') : null;
                            setScheduleModalState((state) =>
                              state
                                ? { ...state, schedule: { ...state.schedule, startAt } }
                                : state,
                            );
                          }}
                        />
                        <span className="schedule-picker-icon">
                          <FaRegClock />
                        </span>
                      </div>
                      <div className="schedule-picker">
                        <SelectDate
                          width={FieldSize.Medium}
                          name="schedule-run-on"
                          label="Start After"
                          placeholderText="yyyy-mm-dd --:-- --"
                          dateFormat="yyyy-MM-dd h:mm aa"
                          showTimeSelect
                          timeIntervals={30}
                          isClearable
                          // The modal clips its popup and scrolls its body, so the calendar
                          // renders into a body-level portal instead of being cut off.
                          portalId={DATE_PICKER_PORTAL_ID}
                          selectedDate={scheduleModalState?.schedule.runOn ?? undefined}
                          onChange={(date) => {
                            // Sent as a UTC ISO string: the API binds it to a DateTime with
                            // Kind=Utc, which is what both the 'timestamp with time zone' column
                            // and the Scheduler's ToTimeZone conversion require.
                            const runOn = date ? moment(date as Date).toISOString() : null;
                            setScheduleModalState((state) =>
                              state ? { ...state, schedule: { ...state.schedule, runOn } } : state,
                            );
                          }}
                        />
                        <span className="schedule-picker-icon">
                          <FaRegCalendar />
                        </span>
                      </div>
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
                      const runSummary = parseRunSummary(runDiff?.run?.summary);
                      if (!runSummary) return null;
                      return (
                        <>
                          <div className="automation-modal-tabs">
                            <button
                              type="button"
                              className={`automation-modal-tab${
                                runDetailView === 'outcome' ? ' active' : ''
                              }`}
                              onClick={() => setRunDetailView('outcome')}
                            >
                              Outcome
                            </button>
                            <button
                              type="button"
                              className={`automation-modal-tab${
                                runDetailView === 'log' ? ' active' : ''
                              }`}
                              onClick={() => setRunDetailView('log')}
                            >
                              Decision Log
                            </button>
                          </div>
                          <Show visible={runDetailView === 'outcome'}>
                            <RunOutcome summary={runSummary} />
                          </Show>
                          <Show visible={runDetailView === 'log' && !!runDetail}>
                            <RunLogViewer
                              runId={runDetail!.id}
                              onFetch={api.findRunLogs}
                              onExplain={api.explainRunLog}
                              promptNames={Object.keys(parseDefinition(values.definition).prompts)}
                              onApplyPrompt={(name, text) => {
                                const definition = parseDefinition(values.definition);
                                definition.prompts[name] = { ...definition.prompts[name], text };
                                setFieldValue('definition', serializeDefinition(definition));
                              }}
                            />
                          </Show>
                        </>
                      );
                    })()}
                    <Show visible={!parseRunSummary(runDiff?.run?.summary)}>
                      <p className="modal-help-text">
                        No outcome summary was recorded for this run.
                      </p>
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
