import { apiGet, apiPost, type ApiResult } from './client';

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
	status: 'Draft' | 'Generating' | 'Ready' | 'Active' | 'Completed' | 'Abandoned';
	updatedAt: string;
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
	}
};
