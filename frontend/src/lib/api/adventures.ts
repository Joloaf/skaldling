import { apiGet, apiPost, apiPut, type ApiResult } from './client';

// GET /api/themes
export type ThemeDto = {
	id: string;
	name: string;
	description: string;
	isDefault: boolean;
};

export type ListThemesResponse = {
	themes: ThemeDto[];
};

// POST /api/adventures
export type NarrativeStyle = 'TaskIntegrated' | 'TaskIndependent';

export type TaskCommand = {
	description: string;
	difficulty: 'Easy' | 'Medium' | 'Hard';
	pointValue: number;
};

export type DayCommand = {
	dayNumber: number;
	tasks: TaskCommand[];
};

export type CreateAdventureCommand = {
	heroId: string;
	themeId: string;
	title: string;
	tone: 'Cozy' | 'Epic' | 'Mysterious' | 'Comedic' | 'Spooky';
	narrativeStyle: NarrativeStyle;
	moral: string | null;
	finaleReward: string | null;
	days: DayCommand[];
};

export type CreateAdventureResponse = {
	id: string;
	heroId: string;
	title: string;
	createdAt: string;
};

// POST /api/adventures/{id}/generate
export type GenerateStoryCommand = Record<string, never>;

export type GenerateStoryResponse = {
	adventureId: string;
	status: AdventureStatus;
	updatedAt: string;
};

// Adventure status type shared by all adventure responses
export type AdventureStatus =
	| 'Draft'
	| 'Generating'
	| 'Ready'
	| 'Active'
	| 'Completed'
	| 'Abandoned'

// GET /api/adventures/{id}
export type GetAdventureResponse = {
	id: string;
	heroId: string;
	heroName: string;
	heroAchievementPoints: number;
	title: string;
	themeName: string;
	tone: 'Cozy' | 'Epic' | 'Mysterious' | 'Comedic' | 'Spooky';
	narrativeStyle: NarrativeStyle;
	moral: string | null;
	finaleReward: string | null;
	status: AdventureStatus;
	createdAt: string;
	updatedAt: string;
	days: PlayDayDto[];
};

export type PlayDayDto = {
	id: string;
	dayNumber: number;
	narrativeIntro: string | null;
	narrativeConvergence: string | null;
	nodes: PlayNodeDto[];
};

export type PlayNodeDto = {
	id: string;
	order: number;
	narrativeText: string | null;
	sceneType: 'Quest' | 'Encounter' | 'Discovery' | 'Reflection' | 'Climax' | 'Resolution' | null;
	adventureTask: PlayAdventureTaskDto;
};

export type PlayAdventureTaskDto = {
	id: string;
	description: string;
	difficulty: 'Easy' | 'Medium' | 'Hard';
	pointValue: number;
	isCompleted: boolean;
};

// POST /api/adventures/{id}/start
export type StartAdventureResponse = {
	adventureId: string;
	status: AdventureStatus;
	updatedAt: string;
};

// PUT /api/adventures/{adventureId}/tasks/{taskId}/completion
export type UpdateTaskCompletionCommand = {
	isCompleted: boolean;
};

export type UpdateTaskCompletionResponse = {
	taskId: string;
	isCompleted: boolean;
	adventureScore: number;
	updatedAt: string;
};

// POST /api/adventures/{id}/confirm-finale
export type ConfirmFinaleRewardResponse = {
	adventureId: string;
	status: AdventureStatus;
	heroAchievementPoints: number;
	earnedScore: number;
	completedAt: string;
};

export const adventuresApi = {
	listThemes(): Promise<ApiResult<ListThemesResponse>> {
		return apiGet<ListThemesResponse>('/api/themes');
	},
	create(command: CreateAdventureCommand): Promise<ApiResult<CreateAdventureResponse>> {
		return apiPost<CreateAdventureResponse, CreateAdventureCommand>('/api/adventures', command);
	},
	generate(id: string): Promise<ApiResult<GenerateStoryResponse>> {
		return apiPost<GenerateStoryResponse, GenerateStoryCommand>(`/api/adventures/${id}/generate`, {});
	},
	get(id: string): Promise<ApiResult<GetAdventureResponse>> {
		return apiGet<GetAdventureResponse>(`/api/adventures/${id}`);
	},
	start(id: string): Promise<ApiResult<StartAdventureResponse>> {
		return apiPost<StartAdventureResponse, Record<string, never>>(`/api/adventures/${id}/start`, {});
	},
	updateTaskCompletion(adventureId: string, taskId: string, command: UpdateTaskCompletionCommand
	): Promise<ApiResult<UpdateTaskCompletionResponse>> {
		return apiPut<UpdateTaskCompletionResponse, UpdateTaskCompletionCommand>
		(`/api/adventures/${adventureId}/tasks/${taskId}/completion`, command);
	},
	confirmFinale(id: string): Promise<ApiResult<ConfirmFinaleRewardResponse>> {
		return apiPost<ConfirmFinaleRewardResponse, Record<string, never>>(`/api/adventures/${id}/confirm-finale`, {});
	}
};
