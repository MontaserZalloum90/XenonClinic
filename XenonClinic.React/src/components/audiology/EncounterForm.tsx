import { useState } from "react";
import { useForm } from "react-hook-form";
import { PlusIcon, TrashIcon } from "@heroicons/react/24/outline";
import type {
  CreateEncounterRequest,
  EncounterAssessment,
  EncounterPlan,
  Diagnosis,
  CreateEncounterTaskRequest,
} from "../../types/audiology";
import {
  EncounterType,
  TaskPriority,
  HearingAidStyle,
} from "../../types/audiology";

interface EncounterFormProps {
  patientId: number;
  appointmentId?: number;
  onSubmit: (
    data: CreateEncounterRequest,
    tasks: CreateEncounterTaskRequest[],
  ) => void;
  onCancel: () => void;
  isLoading?: boolean;
}

interface FormData {
  encounterDate: string;
  encounterType: string;
  chiefComplaint: string;
  historyOfPresentIllness: string;
  // Assessment
  hearingStatus: string;
  hearingChangeFromLast: string;
  currentHearingAids: string;
  hearingAidPerformance: string;
  otoscopyRight: string;
  otoscopyLeft: string;
  additionalFindings: string;
  clinicalImpression: string;
  // Plan
  treatmentPlan: string;
  followUpRequired: boolean;
  followUpInterval: string;
  followUpReason: string;
  hearingAidRecommended: boolean;
  hearingAidStyle: string;
  hearingAidTechnology: string;
  hearingAidBilateral: boolean;
  hearingAidRationale: string;
  // Notes
  notes: string;
  internalNotes: string;
}

const ENCOUNTER_TYPE_OPTIONS = [
  { value: EncounterType.InitialConsultation, label: "Initial Consultation" },
  { value: EncounterType.FollowUp, label: "Follow-Up" },
  { value: EncounterType.HearingTest, label: "Hearing Test" },
  { value: EncounterType.HearingAidFitting, label: "Hearing Aid Fitting" },
  {
    value: EncounterType.HearingAidAdjustment,
    label: "Hearing Aid Adjustment",
  },
  { value: EncounterType.HearingAidRepair, label: "Hearing Aid Repair" },
  { value: EncounterType.Counseling, label: "Counseling" },
  { value: EncounterType.TinnitusEvaluation, label: "Tinnitus Evaluation" },
  { value: EncounterType.BalanceAssessment, label: "Balance Assessment" },
  { value: EncounterType.CochlearImplantMapping, label: "CI Mapping" },
];

const HEARING_AID_STYLE_OPTIONS = Object.entries(HearingAidStyle).map(
  ([key, value]) => ({
    value,
    label: key,
  }),
);

export const EncounterForm = ({
  patientId,
  appointmentId,
  onSubmit,
  onCancel,
  isLoading,
}: EncounterFormProps) => {
  const [activeTab, setActiveTab] = useState<
    "complaint" | "assessment" | "plan" | "tasks"
  >("complaint");
  const [diagnoses, setDiagnoses] = useState<Diagnosis[]>([]);
  const [recommendations, setRecommendations] = useState<string[]>([]);
  const [tasks, setTasks] = useState<Partial<CreateEncounterTaskRequest>[]>([]);
  const [newDiagnosis, setNewDiagnosis] = useState({
    code: "",
    description: "",
    isPrimary: false,
  });
  const [newRecommendation, setNewRecommendation] = useState("");
  const [newTask, setNewTask] = useState({
    title: "",
    priority: TaskPriority.Normal,
    dueDate: "",
  });

  const { register, handleSubmit, watch } = useForm<FormData>({
    defaultValues: {
      encounterDate: new Date().toISOString().split("T")[0],
      encounterType: EncounterType.InitialConsultation,
      followUpRequired: false,
      hearingAidRecommended: false,
      hearingAidBilateral: true,
    },
  });

  const followUpRequired = watch("followUpRequired");
  const hearingAidRecommended = watch("hearingAidRecommended");

  const addDiagnosis = () => {
    if (newDiagnosis.code && newDiagnosis.description) {
      setDiagnoses([...diagnoses, { ...newDiagnosis }]);
      setNewDiagnosis({ code: "", description: "", isPrimary: false });
    }
  };

  const removeDiagnosis = (index: number) => {
    setDiagnoses(diagnoses.filter((_, i) => i !== index));
  };

  const addRecommendation = () => {
    if (newRecommendation.trim()) {
      setRecommendations([...recommendations, newRecommendation.trim()]);
      setNewRecommendation("");
    }
  };

  const removeRecommendation = (index: number) => {
    setRecommendations(recommendations.filter((_, i) => i !== index));
  };

  const addTask = () => {
    if (newTask.title.trim()) {
      setTasks([...tasks, { ...newTask, patientId, encounterId: 0 }]);
      setNewTask({ title: "", priority: TaskPriority.Normal, dueDate: "" });
    }
  };

  const removeTask = (index: number) => {
    setTasks(tasks.filter((_, i) => i !== index));
  };

  const handleFormSubmit = (data: FormData) => {
    const assessment: EncounterAssessment = {
      hearingStatus: data.hearingStatus || undefined,
      hearingChangeFromLast: (data.hearingChangeFromLast ||
        undefined) as EncounterAssessment["hearingChangeFromLast"],
      currentHearingAids: data.currentHearingAids || undefined,
      hearingAidPerformance: data.hearingAidPerformance || undefined,
      otoscopyRight: data.otoscopyRight || undefined,
      otoscopyLeft: data.otoscopyLeft || undefined,
      additionalFindings: data.additionalFindings || undefined,
      clinicalImpression: data.clinicalImpression || undefined,
      diagnoses: diagnoses.length > 0 ? diagnoses : undefined,
    };

    const plan: EncounterPlan = {
      treatmentPlan: data.treatmentPlan || undefined,
      recommendations: recommendations.length > 0 ? recommendations : undefined,
      followUpRequired: data.followUpRequired,
      followUpInterval: data.followUpInterval || undefined,
      followUpReason: data.followUpReason || undefined,
      hearingAidRecommendation: data.hearingAidRecommended
        ? {
            recommended: true,
            style: (data.hearingAidStyle as HearingAidStyle) || undefined,
            technology: data.hearingAidTechnology || undefined,
            bilateral: data.hearingAidBilateral,
            rationale: data.hearingAidRationale || undefined,
          }
        : undefined,
    };

    const encounterRequest: CreateEncounterRequest = {
      patientId,
      appointmentId,
      encounterDate: data.encounterDate,
      encounterType: data.encounterType as EncounterType,
      chiefComplaint: data.chiefComplaint || undefined,
      historyOfPresentIllness: data.historyOfPresentIllness || undefined,
      notes: data.notes || undefined,
    };

    // Add assessment and plan to the request (extending the type)
    const fullRequest = {
      ...encounterRequest,
      assessment,
      plan,
      internalNotes: data.internalNotes || undefined,
    };

    const taskRequests = tasks.map((t) => ({
      ...t,
      patientId,
      encounterId: 0, // Will be set after encounter creation
    })) as CreateEncounterTaskRequest[];

    onSubmit(fullRequest as CreateEncounterRequest, taskRequests);
  };

  const tabs = [
    { id: "complaint", label: "Complaint", step: 1 },
    { id: "assessment", label: "Assessment", step: 2 },
    { id: "plan", label: "Plan", step: 3 },
    { id: "tasks", label: "Tasks", step: 4 },
  ];

  return (
    <form onSubmit={handleSubmit(handleFormSubmit)} className="space-y-6">
      {/* Header */}
      <div className="grid grid-cols-2 gap-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Encounter Date
          </label>
          <input
            type="date"
            {...register("encounterDate", { required: true })}
            className="w-full px-3 py-2 border border-gray-300 rounded-md"
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Encounter Type
          </label>
          <select
            {...register("encounterType", { required: true })}
            className="w-full px-3 py-2 border border-gray-300 rounded-md"
          >
            {ENCOUNTER_TYPE_OPTIONS.map((opt) => (
              <option key={opt.value} value={opt.value}>
                {opt.label}
              </option>
            ))}
          </select>
        </div>
      </div>

      {/* Tab Navigation */}
      <div className="border-b border-gray-200">
        <nav className="flex space-x-8">
          {tabs.map((tab) => (
            <button
              key={tab.id}
              type="button"
              onClick={() =>
                setActiveTab(
                  tab.id as "complaint" | "assessment" | "plan" | "tasks",
                )
              }
              className={`py-2 px-1 border-b-2 font-medium text-sm ${
                activeTab === tab.id
                  ? "border-primary-500 text-primary-600"
                  : "border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300"
              }`}
            >
              <span className="inline-flex items-center">
                <span className="w-6 h-6 rounded-full bg-gray-200 text-gray-600 text-xs flex items-center justify-center mr-2">
                  {tab.step}
                </span>
                {tab.label}
              </span>
            </button>
          ))}
        </nav>
      </div>

      {/* Tab Content */}
      <div className="min-h-[400px]">
        {/* Complaint Tab */}
        {activeTab === "complaint" && (
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Chief Complaint
              </label>
              <textarea
                {...register("chiefComplaint")}
                rows={3}
                className="w-full px-3 py-2 border border-gray-300 rounded-md"
                placeholder="Patient's main concern or reason for visit..."
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                History of Present Illness
              </label>
              <textarea
                {...register("historyOfPresentIllness")}
                rows={5}
                className="w-full px-3 py-2 border border-gray-300 rounded-md"
                placeholder="Detailed history including onset, duration, severity, associated symptoms..."
              />
            </div>
          </div>
        )}

        {/* Assessment Tab */}
        {activeTab === "assessment" && (
          <div className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Hearing Status
                </label>
                <textarea
                  {...register("hearingStatus")}
                  rows={2}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md"
                  placeholder="Current hearing status..."
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Change from Last Visit
                </label>
                <select
                  {...register("hearingChangeFromLast")}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md"
                >
                  <option value="">Select...</option>
                  <option value="improved">Improved</option>
                  <option value="stable">Stable</option>
                  <option value="declined">Declined</option>
                  <option value="unknown">Unknown</option>
                </select>
              </div>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Otoscopy - Right Ear
                </label>
                <textarea
                  {...register("otoscopyRight")}
                  rows={2}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md"
                  placeholder="Right ear findings..."
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Otoscopy - Left Ear
                </label>
                <textarea
                  {...register("otoscopyLeft")}
                  rows={2}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md"
                  placeholder="Left ear findings..."
                />
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Clinical Impression
              </label>
              <textarea
                {...register("clinicalImpression")}
                rows={3}
                className="w-full px-3 py-2 border border-gray-300 rounded-md"
                placeholder="Summary of clinical findings..."
              />
            </div>

            {/* Diagnoses */}
            <div className="border rounded-lg p-4">
              <h4 className="font-medium text-gray-900 mb-3">
                Diagnoses (ICD-10)
              </h4>
              <div className="space-y-2 mb-3">
                {diagnoses.map((dx, index) => (
                  <div
                    key={index}
                    className="flex items-center justify-between bg-gray-50 p-2 rounded"
                  >
                    <span>
                      <strong>{dx.code}</strong>: {dx.description}
                      {dx.isPrimary && (
                        <span className="ml-2 text-xs text-primary-600">
                          (Primary)
                        </span>
                      )}
                    </span>
                    <button
                      type="button"
                      onClick={() => removeDiagnosis(index)}
                      className="text-red-500 hover:text-red-700"
                    >
                      <TrashIcon className="h-4 w-4" />
                    </button>
                  </div>
                ))}
              </div>
              <div className="flex gap-2">
                <input
                  type="text"
                  value={newDiagnosis.code}
                  onChange={(e) =>
                    setNewDiagnosis({ ...newDiagnosis, code: e.target.value })
                  }
                  placeholder="ICD-10 Code"
                  className="w-32 px-2 py-1 text-sm border border-gray-300 rounded"
                />
                <input
                  type="text"
                  value={newDiagnosis.description}
                  onChange={(e) =>
                    setNewDiagnosis({
                      ...newDiagnosis,
                      description: e.target.value,
                    })
                  }
                  placeholder="Description"
                  className="flex-1 px-2 py-1 text-sm border border-gray-300 rounded"
                />
                <label className="flex items-center text-sm">
                  <input
                    type="checkbox"
                    checked={newDiagnosis.isPrimary}
                    onChange={(e) =>
                      setNewDiagnosis({
                        ...newDiagnosis,
                        isPrimary: e.target.checked,
                      })
                    }
                    className="mr-1"
                  />
                  Primary
                </label>
                <button
                  type="button"
                  onClick={addDiagnosis}
                  className="px-3 py-1 bg-primary-600 text-white text-sm rounded hover:bg-primary-700"
                >
                  <PlusIcon className="h-4 w-4" />
                </button>
              </div>
            </div>
          </div>
        )}

        {/* Plan Tab */}
        {activeTab === "plan" && (
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Treatment Plan
              </label>
              <textarea
                {...register("treatmentPlan")}
                rows={3}
                className="w-full px-3 py-2 border border-gray-300 rounded-md"
                placeholder="Detailed treatment plan..."
              />
            </div>

            {/* Recommendations */}
            <div className="border rounded-lg p-4">
              <h4 className="font-medium text-gray-900 mb-3">
                Recommendations
              </h4>
              <ul className="space-y-1 mb-3">
                {recommendations.map((rec, index) => (
                  <li
                    key={index}
                    className="flex items-center justify-between bg-gray-50 p-2 rounded"
                  >
                    <span>{rec}</span>
                    <button
                      type="button"
                      onClick={() => removeRecommendation(index)}
                      className="text-red-500 hover:text-red-700"
                    >
                      <TrashIcon className="h-4 w-4" />
                    </button>
                  </li>
                ))}
              </ul>
              <div className="flex gap-2">
                <input
                  type="text"
                  value={newRecommendation}
                  onChange={(e) => setNewRecommendation(e.target.value)}
                  placeholder="Add recommendation..."
                  className="flex-1 px-2 py-1 text-sm border border-gray-300 rounded"
                  onKeyPress={(e) =>
                    e.key === "Enter" &&
                    (e.preventDefault(), addRecommendation())
                  }
                />
                <button
                  type="button"
                  onClick={addRecommendation}
                  className="px-3 py-1 bg-primary-600 text-white text-sm rounded hover:bg-primary-700"
                >
                  <PlusIcon className="h-4 w-4" />
                </button>
              </div>
            </div>

            {/* Hearing Aid Recommendation */}
            <div className="border rounded-lg p-4">
              <div className="flex items-center mb-3">
                <input
                  type="checkbox"
                  {...register("hearingAidRecommended")}
                  id="hearingAidRecommended"
                  className="mr-2"
                />
                <label
                  htmlFor="hearingAidRecommended"
                  className="font-medium text-gray-900"
                >
                  Recommend Hearing Aids
                </label>
              </div>

              {hearingAidRecommended && (
                <div className="grid grid-cols-2 gap-4 mt-4">
                  <div>
                    <label className="block text-sm text-gray-600 mb-1">
                      Style
                    </label>
                    <select
                      {...register("hearingAidStyle")}
                      className="w-full px-2 py-1 text-sm border border-gray-300 rounded"
                    >
                      <option value="">Select style...</option>
                      {HEARING_AID_STYLE_OPTIONS.map((opt) => (
                        <option key={opt.value} value={opt.value}>
                          {opt.label}
                        </option>
                      ))}
                    </select>
                  </div>
                  <div>
                    <label className="block text-sm text-gray-600 mb-1">
                      Technology Level
                    </label>
                    <select
                      {...register("hearingAidTechnology")}
                      className="w-full px-2 py-1 text-sm border border-gray-300 rounded"
                    >
                      <option value="">Select...</option>
                      <option value="Essential">Essential</option>
                      <option value="Standard">Standard</option>
                      <option value="Advanced">Advanced</option>
                      <option value="Premium">Premium</option>
                    </select>
                  </div>
                  <div className="flex items-center">
                    <input
                      type="checkbox"
                      {...register("hearingAidBilateral")}
                      id="bilateral"
                      className="mr-2"
                    />
                    <label
                      htmlFor="bilateral"
                      className="text-sm text-gray-600"
                    >
                      Bilateral fitting recommended
                    </label>
                  </div>
                  <div className="col-span-2">
                    <label className="block text-sm text-gray-600 mb-1">
                      Rationale
                    </label>
                    <textarea
                      {...register("hearingAidRationale")}
                      rows={2}
                      className="w-full px-2 py-1 text-sm border border-gray-300 rounded"
                      placeholder="Reason for recommendation..."
                    />
                  </div>
                </div>
              )}
            </div>

            {/* Follow-up */}
            <div className="border rounded-lg p-4">
              <div className="flex items-center mb-3">
                <input
                  type="checkbox"
                  {...register("followUpRequired")}
                  id="followUpRequired"
                  className="mr-2"
                />
                <label
                  htmlFor="followUpRequired"
                  className="font-medium text-gray-900"
                >
                  Follow-up Required
                </label>
              </div>

              {followUpRequired && (
                <div className="grid grid-cols-2 gap-4 mt-4">
                  <div>
                    <label className="block text-sm text-gray-600 mb-1">
                      Interval
                    </label>
                    <select
                      {...register("followUpInterval")}
                      className="w-full px-2 py-1 text-sm border border-gray-300 rounded"
                    >
                      <option value="">Select...</option>
                      <option value="1 week">1 Week</option>
                      <option value="2 weeks">2 Weeks</option>
                      <option value="1 month">1 Month</option>
                      <option value="3 months">3 Months</option>
                      <option value="6 months">6 Months</option>
                      <option value="1 year">1 Year</option>
                    </select>
                  </div>
                  <div>
                    <label className="block text-sm text-gray-600 mb-1">
                      Reason
                    </label>
                    <input
                      type="text"
                      {...register("followUpReason")}
                      className="w-full px-2 py-1 text-sm border border-gray-300 rounded"
                      placeholder="Reason for follow-up..."
                    />
                  </div>
                </div>
              )}
            </div>
          </div>
        )}

        {/* Tasks Tab */}
        {activeTab === "tasks" && (
          <div className="space-y-4">
            <p className="text-sm text-gray-600">
              Add tasks to be completed as part of this encounter's action plan.
            </p>

            <div className="space-y-2">
              {tasks.map((task, index) => (
                <div
                  key={index}
                  className="flex items-center justify-between bg-gray-50 p-3 rounded-lg"
                >
                  <div>
                    <span className="font-medium">{task.title}</span>
                    <span
                      className={`ml-2 text-xs px-2 py-0.5 rounded ${
                        task.priority === TaskPriority.Urgent
                          ? "bg-red-100 text-red-800"
                          : task.priority === TaskPriority.High
                            ? "bg-orange-100 text-orange-800"
                            : task.priority === TaskPriority.Normal
                              ? "bg-blue-100 text-blue-800"
                              : "bg-gray-100 text-gray-800"
                      }`}
                    >
                      {task.priority}
                    </span>
                    {task.dueDate && (
                      <span className="ml-2 text-xs text-gray-500">
                        Due: {task.dueDate}
                      </span>
                    )}
                  </div>
                  <button
                    type="button"
                    onClick={() => removeTask(index)}
                    className="text-red-500 hover:text-red-700"
                  >
                    <TrashIcon className="h-4 w-4" />
                  </button>
                </div>
              ))}
            </div>

            <div className="border rounded-lg p-4">
              <h4 className="font-medium text-gray-900 mb-3">Add Task</h4>
              <div className="grid grid-cols-3 gap-3">
                <div className="col-span-3">
                  <input
                    type="text"
                    value={newTask.title}
                    onChange={(e) =>
                      setNewTask({ ...newTask, title: e.target.value })
                    }
                    placeholder="Task title..."
                    className="w-full px-3 py-2 border border-gray-300 rounded-md"
                  />
                </div>
                <div>
                  <label className="block text-xs text-gray-500 mb-1">
                    Priority
                  </label>
                  <select
                    value={newTask.priority}
                    onChange={(e) =>
                      setNewTask({
                        ...newTask,
                        priority: e.target.value as TaskPriority,
                      })
                    }
                    className="w-full px-2 py-1 text-sm border border-gray-300 rounded"
                  >
                    <option value={TaskPriority.Low}>Low</option>
                    <option value={TaskPriority.Normal}>Normal</option>
                    <option value={TaskPriority.High}>High</option>
                    <option value={TaskPriority.Urgent}>Urgent</option>
                  </select>
                </div>
                <div>
                  <label className="block text-xs text-gray-500 mb-1">
                    Due Date
                  </label>
                  <input
                    type="date"
                    value={newTask.dueDate}
                    onChange={(e) =>
                      setNewTask({ ...newTask, dueDate: e.target.value })
                    }
                    className="w-full px-2 py-1 text-sm border border-gray-300 rounded"
                  />
                </div>
                <div className="flex items-end">
                  <button
                    type="button"
                    onClick={addTask}
                    className="w-full px-3 py-1 bg-primary-600 text-white text-sm rounded hover:bg-primary-700"
                  >
                    Add Task
                  </button>
                </div>
              </div>
            </div>

            {/* Notes */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Notes (visible to patient)
              </label>
              <textarea
                {...register("notes")}
                rows={2}
                className="w-full px-3 py-2 border border-gray-300 rounded-md"
                placeholder="Notes that may be shared with patient..."
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Internal Notes (staff only)
              </label>
              <textarea
                {...register("internalNotes")}
                rows={2}
                className="w-full px-3 py-2 border border-gray-300 rounded-md"
                placeholder="Internal notes not shown to patient..."
              />
            </div>
          </div>
        )}
      </div>

      {/* Form Actions */}
      <div className="flex justify-between items-center pt-4 border-t">
        <div className="flex space-x-2">
          {activeTab !== "complaint" && (
            <button
              type="button"
              onClick={() => {
                const currentIndex = tabs.findIndex((t) => t.id === activeTab);
                if (currentIndex > 0)
                  setActiveTab(
                    tabs[currentIndex - 1].id as
                      | "complaint"
                      | "assessment"
                      | "plan"
                      | "tasks",
                  );
              }}
              className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50"
            >
              Previous
            </button>
          )}
          {activeTab !== "tasks" && (
            <button
              type="button"
              onClick={() => {
                const currentIndex = tabs.findIndex((t) => t.id === activeTab);
                if (currentIndex < tabs.length - 1)
                  setActiveTab(
                    tabs[currentIndex + 1].id as
                      | "complaint"
                      | "assessment"
                      | "plan"
                      | "tasks",
                  );
              }}
              className="px-4 py-2 text-sm font-medium text-white bg-primary-600 rounded-md hover:bg-primary-700"
            >
              Next
            </button>
          )}
        </div>

        <div className="flex space-x-3">
          <button
            type="button"
            onClick={onCancel}
            className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50"
          >
            Cancel
          </button>
          <button
            type="submit"
            disabled={isLoading}
            className="px-4 py-2 text-sm font-medium text-white bg-green-600 rounded-md hover:bg-green-700 disabled:opacity-50"
          >
            {isLoading ? "Saving..." : "Complete Encounter"}
          </button>
        </div>
      </div>
    </form>
  );
};
